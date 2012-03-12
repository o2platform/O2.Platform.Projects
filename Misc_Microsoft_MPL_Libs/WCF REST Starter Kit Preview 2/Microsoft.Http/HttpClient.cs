//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Http
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.ComponentModel;
    using System.Diagnostics;
    using Microsoft.Http.Headers;

    public class HttpClient : IDisposable
    {
        bool disposed;

        HttpWebRequestTransportSettings settings;
        IList<HttpStage> stages;

        public HttpClient()
        {
        }

        public HttpClient(Uri baseAddress)
        {
            this.BaseAddress = baseAddress;
        }
        public HttpClient(string baseAddress)
            : this(new Uri(baseAddress, UriKind.Absolute))
        {
        }

        ~HttpClient()
        {
            Dispose(false);
        }

        RequestHeaders defaultHeaders;
        public RequestHeaders DefaultHeaders
        {
            get
            {
                if (defaultHeaders == null)
                {
                    defaultHeaders = new RequestHeaders();
                }
                return defaultHeaders;
            }
            set
            {
                this.defaultHeaders = value;
            }
        }

        Uri address;
        public Uri BaseAddress
        {   // null or an absolute uri
            get
            {
                return address;
            }
            set
            {
                // null is acceptable
                if (value != null)
                {
                    if (!value.IsAbsoluteUri)
                    {
                        throw new UriFormatException(value + " is not an absolute URI");
                    }
                }

                address = value;
            }
        }

        public IList<HttpStage> Stages
        {
            get
            {
                if (this.stages == null)
                {
                    this.stages = new Collection<HttpStage>();
                }
                return this.stages;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (pipeline != null)
                {
                    throw new InvalidOperationException("stages cannot be modified after first send");
                }
                this.stages = value;
            }
        }

        public HttpWebRequestTransportSettings TransportSettings
        {
            get
            {
                if (settings == null)
                {
                    settings = new HttpWebRequestTransportSettings();
                }
                return settings;
            }
            set
            {
                settings = value;
            }
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            disposed = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual HttpStage CreateTransportStage()
        {
            return new HttpWebRequestTransportStage()
                {
                    Settings = this.TransportSettings
                };
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        ReadOnlyCollection<HttpStage> pipeline;
        ReadOnlyCollection<HttpStage> GetPipeline()
        {
            if (pipeline == null)
            {
                var temp = new List<HttpStage>();

                if (this.stages != null)
                {
                    foreach (var stage in this.stages)
                    {
                        if (stage == null)
                        {
                            continue;
                        }
                        temp.Add(stage);
                    }
                }
                var transport = CreateTransportStage();
                if (transport != null)
                {
                    temp.Add(transport);
                }
                while (temp.Remove(null) && temp.Count != 0)
                {
                }
                if (temp.Count == 0)
                {
                    throw new InvalidOperationException("no stages available");
                }
                pipeline = temp.AsReadOnly();
                this.stages = new ReadOnlyCollection<HttpStage>(this.Stages);
            }
            return pipeline;
        }

        protected void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new InvalidOperationException("disposed");
            }
        }

        public IAsyncResult BeginSend(HttpRequestMessage request, AsyncCallback callback, object state)
        {
            PrepareRequest(ref request);
            return BeginSendCore(request, callback, state);
        }


        HttpStageProcessingAsyncResult BeginSendCore(HttpRequestMessage request, AsyncCallback callback, object state)
        {
            var async = new HttpStageProcessingAsyncState(GetPipeline(), request);
            var stage = new HttpStageProcessingAsyncResult(async, callback, state);
            return stage;
        }


        void PrepareRequest(ref HttpRequestMessage request)
        {
            // post-condition: request != null && !string.IsNullOrEmpty(request.Method) && request.Uri != null && request.Uri.IsAbsoluteUri && !request.HasBeenSent;
            if (disposed && request != null)
            {
                request.Dispose();
            }
            ThrowIfDisposed();

            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            bool hasDefaultBaseAddress = this.address != null;

            if (request == null)
            {
                if (!hasDefaultBaseAddress)
                {
                    throw new ArgumentNullException("request", "request is null and BaseAddress is null");
                }

                request = new HttpRequestMessage();
            }

            if (request.HasBeenSent)
            {
                request.Dispose();
                throw new InvalidOperationException(request + " has already been sent");
            }

            if (request.Uri == null)
            {
                if (!hasDefaultBaseAddress)
                {
                    request.Dispose();
                    throw new ArgumentNullException("request", "request.Uri is null and DefaultRequest.BaseAddress is null");
                }
                request.Uri = this.BaseAddress;
            }
            else if (!request.Uri.IsAbsoluteUri)
            {
                if (!hasDefaultBaseAddress)
                {
                    request.Dispose();
                    throw new UriFormatException("request.Uri is relative (" + request.Uri + ") and DefaultRequest.BaseAddress is null");
                }
                request.Uri = new Uri(this.BaseAddress, request.Uri);
            }

            if (string.IsNullOrEmpty(request.Method))
            {
                request.Dispose();
                throw new ArgumentOutOfRangeException("request", "request.Method is null or empty");
            }

            if (defaultHeaders != null)
            {
                foreach (var h in defaultHeaders.Keys)
                {
                    if (request.Headers.ContainsKey(h))
                    {
                        continue;
                    }
                    foreach (var v in defaultHeaders.GetValues(h))
                    {
                        request.Headers.Add(h, v);
                    }
                }
            }

            request.HasBeenSent = true;
        }

        public HttpResponseMessage EndSend(IAsyncResult result)
        {
            var state = HttpStageProcessingAsyncResult.End(result, true);
            return state.Response;
        }


        public HttpResponseMessage Send(HttpRequestMessage request)
        {
            PrepareRequest(ref request);

            var async = new HttpStageProcessingAsyncState(GetPipeline(), request)
            {
                ForceSynchronous = true
            };
            var result = new HttpStageProcessingAsyncResult(async, null, null);
            var xyz = HttpStageProcessingAsyncResult.End(result, true);
            if (!result.CompletedSynchronously)
            {
                throw new InvalidOperationException("didn't complete synchronously: " + result + " " + xyz.Response);
            }
            return xyz.Response;
        }

        class SendAsyncState
        {
            public HttpClient Client
            {
                get;
                private set;
            }
            public AsyncOperation Operation
            {
                get;
                private set;
            }
            public SendAsyncState(HttpClient c, AsyncOperation op)
            {
                this.Client = c;
                this.Operation = op;
            }
            public HttpStageProcessingAsyncState AsyncState
            {
                get;
                set;
            }
        }

        public void SendAsync(HttpRequestMessage request)
        {
            PrepareRequest(ref request);
            SendAsyncCore(request, null);
        }
        public void SendAsync(HttpRequestMessage request, object userState)
        {
            PrepareRequest(ref request);
            SendAsyncCore(request, userState);
        }

        void SendAsyncCore(HttpRequestMessage request, object userState)
        {
            if (userState != null)
            {
                CancelManager.EnableCancel(request);
                lock (pendingAsync)
                {
                    HttpStageProcessingAsyncResult pend;
                    if (pendingAsync.TryGetValue(userState, out pend))
                    {
                        if (pend == null)
                        {
                            throw new ArgumentException("userState is not unique", "userState");
                        }
                        else
                        {
                            throw new ArgumentException("userState is already being used for " + pend.HttpAsyncState.request, "userState");
                        }
                    }
                    pendingAsync.Add(userState, null);
                }
            }
            var operation = AsyncOperationManager.CreateOperation(userState);
            var state = new SendAsyncState(this, operation);
            var result = this.BeginSendCore(request, SendCompletedCallback, state);
            if (userState != null && !result.IsCompleted)
            {
                lock (pendingAsync)
                {
                    Debug.Assert(pendingAsync[userState] == null);
                    pendingAsync[userState] = result;
                }
            }
        }

        // this is needed because we need to map the user state to the asyncresult (so we can cancel)
        readonly Dictionary<object, HttpStageProcessingAsyncResult> pendingAsync = new Dictionary<object, HttpStageProcessingAsyncResult>();

        static readonly AsyncCallback SendCompletedCallback = SendCompletedCore;
        static void SendCompletedCore(IAsyncResult a)
        {
            var result = (HttpStageProcessingAsyncResult)a;
            var state = (SendAsyncState)result.AsyncState;
            var pend = state.Client.pendingAsync;
            if (state.Operation.UserSuppliedState != null)
            {
                lock (pend)
                {
#if DEBUG
                bool removed = pend.Remove(state.Operation.UserSuppliedState);
                Debug.WriteLine(state.Operation.UserSuppliedState + " removed " + removed);
#else
                    pend.Remove(state.Operation.UserSuppliedState);
#endif
                }
            }
            // false == don't throw exception
            state.AsyncState = HttpStageProcessingAsyncResult.End(result, false);

            state.Operation.PostOperationCompleted(OperationCompleted, state);
        }

        static void OperationCompleted(object o)
        {
            var state = (SendAsyncState)o;
            var client = state.Client;
            var handler = client.SendCompleted;
            bool dispose = true;
            try
            {
                if (handler != null)
                {
                    var args = new SendCompletedEventArgs(state.AsyncState.request, state.AsyncState.GetResponseOrNull(),
                        state.AsyncState.GetExceptionOrNull(), state.AsyncState.Cancelled, state.Operation.UserSuppliedState);
                    handler(client, args);
                    if (args.PreventAutomaticDispose)
                    {
                        dispose = false;
                    }
                }
            }
            finally
            {
                if (dispose)
                {
                    var x = state.AsyncState.GetResponseOrNull();
                    if (x != null)
                    {
                        x.Dispose();
                    }
                    var y = state.AsyncState.request;
                    if (y != null)
                    {
                        y.Dispose();
                    }
                }
            }
        }

        public event EventHandler<SendCompletedEventArgs> SendCompleted;

        public void SendAsyncCancel(object userState)
        {
            if (userState == null)
            {
                throw new ArgumentNullException("userState");
            }
            HttpStageProcessingAsyncResult result;
            lock (pendingAsync)
            {
                if (!pendingAsync.TryGetValue(userState, out result))
                {
                    Debug.WriteLine(userState + " not found");
                    return;
                }
                if (result == null)
                {
                    Debug.WriteLine(userState + " was null (pending cancel)");
                    return;
                }
                // disallow additional cancellations
                pendingAsync[userState] = null;
            }

            HttpStageProcessingAsyncResult stage = (HttpStageProcessingAsyncResult)result;
            stage.MarkCancelled();
        }

        public HttpResponseMessage Send(HttpMethod method)
        {
            return Send(method, this.BaseAddress, null, null);
        }
        public HttpResponseMessage Send(HttpMethod method, Uri uri)
        {
            return Send(method, uri, null, null);
        }
        public HttpResponseMessage Send(HttpMethod method, Uri uri, RequestHeaders headers)
        {
            return Send(method, uri, headers, null);
        }
        public HttpResponseMessage Send(HttpMethod method, Uri uri, HttpContent content)
        {
            return Send(method, uri, null, content);
        }
        public HttpResponseMessage Send(HttpMethod method, string uri)
        {
            return Send(method, uri, null, null);
        }
        public HttpResponseMessage Send(HttpMethod method, string uri, RequestHeaders headers)
        {
            return Send(method, uri, headers, null);
        }
        public HttpResponseMessage Send(HttpMethod method, string uri, HttpContent content)
        {
            return Send(method, uri, null, content);
        }
        public HttpResponseMessage Send(HttpMethod method, string uri, RequestHeaders headers, HttpContent content)
        {
            return Send(method, new Uri(uri, UriKind.RelativeOrAbsolute), headers, content);
        }
        public HttpResponseMessage Send(HttpMethod method, Uri uri, RequestHeaders headers, HttpContent content)
        {
            return Send(new HttpRequestMessage(method.ToString(), uri, headers, content));
        }

    }


}