//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Http
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Globalization;
    using Microsoft.Http.Headers;
    using System.Threading;

    public class HttpWebRequestTransportStage : HttpAsyncStage
    {
        public HttpWebRequestTransportSettings Settings
        {
            get;
            set;
        }

        protected internal override IAsyncResult BeginProcessRequestAndTryGetResponse(HttpRequestMessage request, AsyncCallback callback, object state)
        {
            return new HttpTransportAsyncResult(false, request, this.Settings, callback, state);
        }

        protected internal override IAsyncResult BeginProcessResponse(HttpResponseMessage response, object state, AsyncCallback callback, object callbackState)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected internal override void EndProcessRequestAndTryGetResponse(IAsyncResult result, out HttpResponseMessage response, out object state)
        {
            var resp = HttpTransportAsyncResult.End(result);
            Debug.Assert(resp != null);
            response = resp;
            state = null;
        }

        protected internal override void EndProcessResponse(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected internal override void ProcessRequestAndTryGetResponse(HttpRequestMessage request, out HttpResponseMessage response, out object state)
        {
            var result = new HttpTransportAsyncResult(true, request, this.Settings, null, null);
            EndProcessRequestAndTryGetResponse(result, out response, out state);
        }

        protected internal override void ProcessResponse(HttpResponseMessage response, object state)
        {
        }

        public static void CopyHeadersFromHttpWebResponse(WebHeaderCollection webResponseHeaders, ResponseHeaders responseHeaders)
        {
            var keys = webResponseHeaders.Keys;
            for (int i = 0; i < keys.Count; ++i)
            {
                var k = keys[i];
                var v = webResponseHeaders.GetValues(i);
                responseHeaders.Add(k, v);
            }
        }
        static void CopyHeadersToHttpWebRequest(RequestHeaders source, HttpWebRequest request)
        {
            // These are special cased in WHC and will throw if you try to set them on the request.Headers
            // var keys = source.Keys.ToArray();
            foreach (string key in source.Keys)
            {
                string value = source[key];
                switch (key.ToUpper(CultureInfo.InvariantCulture))
                {
                    case "ACCEPT":
                        request.Accept = value;
                        break;
                    case "CONTENT-TYPE":
                        request.ContentType = value;
                        break;
                    case "CONNECTION":
                        if (value.Equals("Keep-Alive", StringComparison.OrdinalIgnoreCase))
                        {
                            request.KeepAlive = true;
                        }
                        else if (value.Equals("close", StringComparison.OrdinalIgnoreCase))
                        {
                            request.KeepAlive = false;
                        }
                        else
                        {
                            request.Connection = value;
                        }
                        break;
                    case "CONTENT-LENGTH":
                        request.ContentLength = source.ContentLength.Value;
                        break;
                    case "EXPECT":
                        request.Expect = value;
                        break;
                    case "IF-MODIFIED-SINCE":
                        request.IfModifiedSince = source.IfModifiedSince.Value;
                        break;
                    case "DATE":
                    case "PROXY-CONNECTION":
                    case "HOST":
                        // this is not allowed according to HeaderInfoTable -- but there's nowhere I can find to put it on the HWR
                        throw new NotSupportedException(key);
                    case "RANGE":
                        foreach (var r in source.Range)
                        {
                            if (r.End.HasValue)
                            {
                                request.AddRange("bytes", r.Begin, r.End.Value);
                            }
                            else
                            {
                                request.AddRange("bytes", r.Begin);
                            }
                        }
                        break;
                    case "REFERER":
                        request.Referer = value;
                        break;
                    case "TRANSFER-ENCODING":
                        if (value.Equals("chunked", StringComparison.OrdinalIgnoreCase))
                        {
                            request.SendChunked = true;
                        }
                        else
                        {
                            request.TransferEncoding = value;
                        }
                        break;
                    case "USER-AGENT":
                        request.UserAgent = value;
                        break;

                    default:
                        request.Headers.Add(key, value);
                        break;
                }
            }
        }

        static void CopySettingsToHttpWebRequest(HttpWebRequestTransportSettings settings, HttpWebRequest request)
        {
            // skip: request.ConnectionGroupName;
            // skip: request.ContinueDelegate;
            // skip: request.KeepAlive;
            // skip: request.Pipelined;
            // skip: request.ProtocolVersion;
            // skip: request.ServicePoint
            // skip: request.UnsafeAuthenticatedConnectionSharing;

            if (settings.AutomaticDecompression != null)
            {
                request.AutomaticDecompression = settings.AutomaticDecompression.Value;
            }

            if (settings.AllowWriteStreamBuffering != null)
            {
                request.AllowWriteStreamBuffering = settings.AllowWriteStreamBuffering.Value;
            }

            if (settings.HasClientCertificates)
            {
                request.ClientCertificates = settings.ClientCertificates;
            }

            if (settings.AuthenticationLevel != null)
            {
                request.AuthenticationLevel = settings.AuthenticationLevel.Value;
            }

            if (settings.HasCachePolicy)
            {
                request.CachePolicy = settings.CachePolicy;
            }

            if (settings.MaximumAutomaticRedirections != 0)
            {
                request.AllowAutoRedirect = true;
                request.MaximumAutomaticRedirections = settings.MaximumAutomaticRedirections;
            }
            else
            {
                request.AllowAutoRedirect = false;
            }

            if (settings.ImpersonationLevel != null)
            {
                request.ImpersonationLevel = settings.ImpersonationLevel.Value;
            }

            if (settings.MaximumResponseHeaderKB != null)
            {
                request.MaximumResponseHeadersLength = settings.MaximumResponseHeaderKB.Value;
            }

            if (settings.ReadWriteTimeout != null)
            {
                request.ReadWriteTimeout = (int)settings.ReadWriteTimeout.Value.TotalMilliseconds;
            }

            if (settings.PreAuthenticate != null)
            {
                request.PreAuthenticate = settings.PreAuthenticate.Value;
            }

            if (settings.Credentials != null)
            {
                request.Credentials = settings.Credentials;
            }

            if (settings.HasProxy)
            {
                request.Proxy = settings.Proxy;
            }

            if (settings.Cookies != null)
            {
                request.CookieContainer = settings.Cookies;
            }

            if (settings.SendChunked != null)
            {
                request.SendChunked = settings.SendChunked.Value;
            }

            if (settings.ConnectionTimeout != null)
            {
                request.Timeout = (int)settings.ConnectionTimeout.Value.TotalMilliseconds;
            }

            if (settings.UseDefaultCredentials != null)
            {
                request.UseDefaultCredentials = settings.UseDefaultCredentials.Value;
            }
        }

        class HttpTransportAsyncResult : AsyncResult<HttpResponseMessage>, IHttpCancel
        {
            static readonly AsyncCallback EndGetRequestStreamAndWriteCallback = EndGetRequestStreamAndWrite;

            readonly HttpRequestMessage request;
            readonly HttpWebRequestTransportSettings settings;
            bool stayedSync;
            HttpWebRequest webRequest;
            HttpWebResponse webResponse;

            public HttpTransportAsyncResult(bool preferSync, HttpRequestMessage request, HttpWebRequestTransportSettings settings, AsyncCallback callback, object state)
                : base(callback, state)
            {
                if (request.Uri == null)
                {
                    throw new ArgumentNullException("request", "request.Uri is null");
                }
                if (!request.Uri.IsAbsoluteUri)
                {
                    throw new UriFormatException("\"" + request.Uri + "\" is not an absolute URI");
                }
                this.stayedSync = true;
                this.settings = settings;
                this.request = request;

                CancelManager.AddIfCancelManagerPresent(this.request, this);

                CreateAndPrepareWebRequest(this);

                if (!HttpContent.IsNullOrEmpty(request.Content))
                {
                    var writer = request.Content;
                    Stream stream;
                    if (!preferSync)
                    {
                        stayedSync = false;
                        Trace(this, "Going async");
                        this.timedOutReason = TimeoutReason.GetRequestStream;
                        var result = webRequest.BeginGetRequestStream(EndGetRequestStreamAndWriteCallback, this);
                        Trace(this, "called BeginGetRequestStream");
                        if (result.CompletedSynchronously)
                        {
                            Trace(this, "BeginGetRequestStream completed synchronously");
                            stream = webRequest.EndGetRequestStream(result);
                        }
                        else
                        {
                            Trace(this, "went async for BeginGetRequestStream");
                            RegisterAbortTimeout(result, TimeoutReason.GetRequestStream);
                            return;
                        }
                    }
                    else
                    {
                        stream = this.webRequest.GetRequestStream();
                    }
                    WriteToRequestStream(this, stream, writer);
                }

                this.timedOutReason = TimeoutReason.GetResponse;
                if (preferSync)
                {
                    PopulateWebResponse(this, null, PopulateWebResponseSyncFunc);
                }
                else
                {
                    var result = this.webRequest.BeginGetResponse(EndGetResponseCallback, this);
                    if (result.CompletedSynchronously)
                    {
                        this.stayedSync = true;
                        PopulateWebResponse(this, result, PopulateWebResponseEndSyncFunc);
                    }
                    else
                    {
                        this.stayedSync = false;
                        RegisterAbortTimeout(result, TimeoutReason.GetResponse);
                    }
                }

            }
            enum TimeoutReason
            {
                Unknown,
                GetRequestStream,
                WriteToRequestStream,
                GetResponse
            }

            void RegisterAbortTimeout(IAsyncResult result, TimeoutReason reason)
            {
                this.timedOutReason = reason;
                TimeoutHelper.Register(result.AsyncWaitHandle, MaybeAbortCallback, this, TimeSpan.FromMilliseconds(webRequest.Timeout));
            }

            TimeSpan TimeoutStarted
            {
                get;
                set;
            }

            static readonly Action<HttpTransportAsyncResult, bool> MaybeAbortCallback = MaybeAbort;
            static void MaybeAbort(HttpTransportAsyncResult self, bool timedOut)
            {
                Trace(self, (timedOut ? "timed out " : "completed ") + self.request.ToString());
                if (timedOut)
                {
                    self.timedOut = true;
                    self.timedOutAt = DateTime.UtcNow;
                    self.Cancel();
                }
            }
            bool timedOut;
            DateTime timedOutAt;
            TimeoutReason timedOutReason;
            bool cancelled;
            public void Cancel()
            {
                if (cancelled)
                {
                    return;
                }
                cancelled = true;
                if (this.webRequest != null)
                {
                    this.webRequest.Abort();
                }
                if (this.webResponse != null)
                {
                    this.webResponse.Close();
                }
            }

            static void CreateAndPrepareWebRequest(HttpTransportAsyncResult self)
            {
                Trace(self, "CreateAndPrepareRequest");
                var request = self.request;
                var settings = self.settings;
                var http = (HttpWebRequest)WebRequest.Create(request.Uri);
                http.Method = request.Method;

                CopySettingsToHttpWebRequest(settings, http);
                HttpWebRequestTransportSettings messageSettings = request.GetPropertyOrDefault<HttpWebRequestTransportSettings>();
                if (messageSettings != null)
                {
                    CopySettingsToHttpWebRequest(messageSettings, http);
                }

                var calc = HttpMessageCore.CalculateEffectiveContentType(request);
                if (calc != request.Headers.ContentType)
                {
                    request.Headers.ContentType = calc;
                }

                if (!HttpContent.IsNullOrEmpty(request.Content) && request.Content.HasLength())
                {
                    if (request.Headers.ContentLength == null)
                    {
                        request.Headers.ContentLength = request.Content.GetLength();
                    }
                }

                CopyHeadersToHttpWebRequest(request.Headers, http);

                if (http.Method == "GET" && http.ContentLength >= 0)
                {
                    throw new NotSupportedException("can't set Content-Length to " + http.ContentLength + " on " + http.Method);
                }

                if (http.Method == "GET" && !HttpContent.IsNullOrEmpty(request.Content))
                {
                    throw new NotSupportedException("can't set a non-IsEmpty content on a GET: " + self.request.Content);
                }

                self.webRequest = http;
            }

            static void EndGetRequestStreamAndWrite(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }
                HttpTransportAsyncResult self = (HttpTransportAsyncResult)result.AsyncState;
                Trace(self, "EndGetRequestStreamAndWrite");
                try
                {
                    self.stayedSync = false;
                    var stream = self.webRequest.EndGetRequestStream(result);
                    Debug.Assert(!self.stayedSync);
                    var content = self.request.Content;
                    WriteToRequestStream(self, stream, content);

                    self.timedOutReason = TimeoutReason.GetResponse;
                    var result2 = self.webRequest.BeginGetResponse(EndGetResponseCallback, self);
                    Trace(self, "started begingetresponse");
                    self.stayedSync = false;
                    if (result2.CompletedSynchronously)
                    {
                        Trace(self, "begingetresponse completed sync");
                        PopulateWebResponse(self, result2, PopulateWebResponseEndSyncFunc);
                    }
                    else
                    {
                        self.RegisterAbortTimeout(result2, TimeoutReason.GetResponse);
                    }
                }
                catch (Exception e)
                {
                    if (IsFatal(e))
                    {
                        throw;
                    }
                    self.Complete(e);
                }
            }

            static void WriteToRequestStream(HttpTransportAsyncResult self, Stream stream, HttpContent content)
            {
                if (self.timedOut)
                {
                    throw new TimeoutException();
                }

                if (self.cancelled)
                {
                    throw new OperationCanceledException();
                }

                self.timedOutReason = TimeoutReason.WriteToRequestStream;
                try
                {
                    Trace(self, "starting to writeto");
                    content.WriteTo(stream);
                    Trace(self, "ending writeto");
                }
                catch (Exception e)
                {
                    Trace(self, "got exception during writeto: " + e);
                    throw;
                }
                stream.Close();
                Trace(self, "closed request stream");
            }

            void Complete(Exception e)
            {
                e = MaybeWrap(this, e);
                // no-op if already cancelled
                this.Cancel();
                base.Complete(this.stayedSync, e);
            }
            static readonly AsyncCallback EndGetResponseCallback = EndGetResponse;
            static void EndGetResponse(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }
                var self = (HttpTransportAsyncResult)result.AsyncState;
                Trace(self, "EndGetResponseCallback");
                self.stayedSync = false;
                PopulateWebResponse(self, result, PopulateWebResponseEndAsyncFunc);
            }

            static readonly Func<WebRequest, IAsyncResult, WebResponse> PopulateWebResponseSyncFunc = PopulateWebResponseSync;
            static readonly Func<WebRequest, IAsyncResult, WebResponse> PopulateWebResponseEndSyncFunc = PopulateWebResponseEndSync;
            static readonly Func<WebRequest, IAsyncResult, WebResponse> PopulateWebResponseEndAsyncFunc = PopulateWebResponseEndAsync;

            static WebResponse PopulateWebResponseSync(WebRequest request, IAsyncResult result)
            {
                Debug.Assert(result == null);
                return request.GetResponse();
            }

            static WebResponse PopulateWebResponseEndSync(WebRequest request, IAsyncResult result)
            {
                Debug.Assert(result.CompletedSynchronously);
                return request.EndGetResponse(result);
            }

            static WebResponse PopulateWebResponseEndAsync(WebRequest request, IAsyncResult result)
            {
                Debug.Assert(!result.CompletedSynchronously);
                return request.EndGetResponse(result);
            }

            static void PopulateWebResponse(HttpTransportAsyncResult self, IAsyncResult result, Func<WebRequest, IAsyncResult, WebResponse> getResponse)
            {
                Trace(self, "PopulateWebResponse " + getResponse);
                Debug.Assert(getResponse == PopulateWebResponseSyncFunc || getResponse == PopulateWebResponseEndAsyncFunc || getResponse == PopulateWebResponseEndSyncFunc);

                HttpResponseMessage responseMessage = null;
                try
                {
                    self.webResponse = (HttpWebResponse)getResponse(self.webRequest, result);
                }
                catch (System.Net.WebException ex)
                {
                    self.webResponse = ex.Response as HttpWebResponse;
                    // if an SSL request fails, the inner exception will be populated
                    if (self.webResponse == null || ex.InnerException != null)
                    {
                        if (ex.Response != null)
                        {
                            try
                            {
                                ex.Response.Close();
                            }
                            catch (Exception duringClose)
                            {
                                Debug.WriteLine(duringClose);
                                if (IsFatal(duringClose))
                                {
                                    throw;
                                }
                                ex.Data["CloseException"] = duringClose;
                            }
                        }

                        self.Complete(ex);
                        return;
                    }
                    // we got a webexception because of a status code, so we treat this as normal
                    Debug.Assert(self.webResponse != null);
                }
                catch (Exception e)
                {
                    if (IsFatal(e))
                    {
                        throw;
                    }
                    self.Complete(e);
                    return;
                }

                try
                {
                    responseMessage = PopulateResponse(self);
                    self.Complete(self.stayedSync, responseMessage);
                }
                catch (Exception e)
                {
                    if (IsFatal(e))
                    {
                        throw;
                    }
                    self.Complete(e);
                }
            }

            static Exception MaybeWrap(HttpTransportAsyncResult self, Exception e)
            {
                var we = e as WebException;
                if (we != null)
                {
                    var status = we.Status;
                    if (we.Status == WebExceptionStatus.Timeout || self.timedOut)
                    {
                        self.timedOut = true;
                        status = WebExceptionStatus.Timeout;
                    }
                    else if (we.Status == WebExceptionStatus.RequestCanceled || self.cancelled)
                    {
                        self.cancelled = true;
                        status = WebExceptionStatus.RequestCanceled;
                    }

                    e = new WebException(null, we, status, null);
                }

                if (self.timedOut)
                {
                    return new TimeoutException(self.timedOutReason.ToString() + " timed out", e);
                }

                if (self.cancelled)
                {
                    return new OperationCanceledException(e.Message, e);
                }

                return e;
            }

            static HttpResponseMessage PopulateResponse(HttpTransportAsyncResult self)
            {
                HttpResponseMessage response;
                Trace(self, "PopulateResponse");
                var webResponse = self.webResponse;

                long? len = null;
                if (webResponse.ContentLength != -1)
                {
                    len = webResponse.ContentLength;
                }
                response = new HttpResponseMessage()
                   {
                       Request = self.request,
                       Content = HttpContent.Create(new HttpWebResponseInputStream(webResponse), webResponse.ContentType, len),
                       Uri = webResponse.ResponseUri,
                       StatusCode = webResponse.StatusCode,
                       Method = webResponse.Method,
                   };

                if (webResponse.IsFromCache)
                {
                    response.Properties.Add(CacheResponseProperty.LoadFrom(self.webRequest.CachePolicy, webResponse));
                }

                var webResponseHeaders = webResponse.Headers;
                var responseHeaders = response.Headers;
                CopyHeadersFromHttpWebResponse(webResponseHeaders, responseHeaders);

                if (response.Method == "HEAD")
                {
                    response.Content.LoadIntoBuffer();
                }

                var webRequestHeaders = self.webRequest.Headers;
                var requestHeaders = self.request.Headers;

                foreach (var newHeader in webRequestHeaders.AllKeys)
                {
                    if (!requestHeaders.ContainsKey(newHeader))
                    {
                        requestHeaders.Add(newHeader, webRequestHeaders[newHeader]);
                    }
                }

                if (self.settings.Cookies != null)
                {
                    self.settings.Cookies.Add(webResponse.Cookies);
                    response.Properties.Add(new CookieCollection() { webResponse.Cookies });
                }

                return response;
            }

            readonly TimeSpan started = DateTime.UtcNow.TimeOfDay;

            [Conditional("TRACE"), Conditional("DEBUG")]
            static void Trace(HttpTransportAsyncResult self, string s)
            {
#if DEBUG && TRACE
                Debug.WriteLine(DateTime.UtcNow.TimeOfDay - self.started + " " + string.Format("transport thread 0x{0} : {1}", System.Threading.Thread.CurrentThread.ManagedThreadId.ToString("x2"), s));
#endif
            }

            static class TimeoutHelper
            {
                public static void Register(WaitHandle handle, Action<HttpTransportAsyncResult, bool> callback, HttpTransportAsyncResult value, TimeSpan timeout)
                {
                    value.TimeoutStarted = DateTime.UtcNow.TimeOfDay;
                    var x = new TimeoutState(handle, callback, value);
                    var h = ThreadPool.RegisterWaitForSingleObject(handle, TimeoutState.StaticWaitOrTimerCallback, x, timeout, true);
                    x.Registered = h;
                }
                sealed class TimeoutState
                {
                    public RegisteredWaitHandle Registered
                    {
                        get;
                        set;
                    }
                    readonly HttpTransportAsyncResult value;
                    readonly WaitHandle handle;
                    readonly Action<HttpTransportAsyncResult, bool> callback;

                    public TimeoutState(WaitHandle handle, Action<HttpTransportAsyncResult, bool> callback, HttpTransportAsyncResult value)
                    {
                        this.handle = handle;
                        this.callback = callback;
                        this.value = value;
                    }

                    static void StaticWaitOrTimer(object value, bool timedOut)
                    {
                        var state = (TimeoutState)value;
                        try
                        {
                            state.callback(state.value, timedOut);
                        }
                        finally
                        {
                            if (state.Registered != null && state.handle != null)
                            {
                                state.Registered.Unregister(state.handle);
                            }

                            if (state.handle != null)
                            {
                                state.handle.Close();
                            }
                        }
                    }
                    public static readonly WaitOrTimerCallback StaticWaitOrTimerCallback = StaticWaitOrTimer;
                }
            }
        }

        sealed class HttpWebResponseInputStream : DetectEofStream
        {
            const int maxSocketRead = 65536; // 0x10000
            readonly HttpWebResponse webResponse;

            protected override void Dispose(bool disposing)
            {
                Close();
                base.Dispose(disposing);
            }

#if DEBUG
            ~HttpWebResponseInputStream()
            {
                Debug.WriteLine("finalizer: " + this.webResponse.ResponseUri + " " + (this.webResponse.Headers + "").Trim() + Environment.NewLine);
            }
#endif
            bool closed;
            bool responseClosed;
            // Methods
            public HttpWebResponseInputStream(HttpWebResponse httpWebResponse)
                : base(httpWebResponse.GetResponseStream())
            {
                this.webResponse = httpWebResponse;
            }

            public override string ToString()
            {
                return "HttpWebResponseInputStream(" + this.webResponse.Method + " " + this.webResponse.ResponseUri + " " + this.webResponse.StatusCode + ")";
            }

            public override bool CanRead
            {
                get
                {
                    return true;
                }
            }

            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                return base.BeginRead(buffer, offset, Math.Min(count, maxSocketRead), callback, state);
            }
            public override void Close()
            {
                if (closed)
                {
                    return;
                }
                closed = true;
                base.Close();
                this.CloseResponse();
            }

            public override int EndRead(IAsyncResult result)
            {
                return base.EndRead(result);
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (closed)
                {
                    throw new InvalidOperationException("closed");
                }
                if (!base.CanRead)
                {
                    return 0;
                }
                return base.Read(buffer, offset, Math.Min(count, maxSocketRead));
            }

            protected override void OnReceivedEof()
            {
                base.OnReceivedEof();
                this.CloseResponse();
            }

            void CloseResponse()
            {
                if (!this.responseClosed)
                {
                    this.responseClosed = true;
                    this.webResponse.Close();
#if DEBUG
                    GC.SuppressFinalize(this);
#endif
                }
            }
        }
    }
}

