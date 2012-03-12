//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Http
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;

    sealed class CancelManager
    {
        List<IHttpCancel> list;
        public void Add(IHttpCancel action)
        {
            if (list == null)
            {
                list = new List<IHttpCancel>();
            }
            list.Add(action);
        }

        public void Cancel()
        {
            if (list != null)
            {
                foreach (var a in list)
                {
                    a.Cancel();
                }
            }
        }

        public static void AddIfCancelManagerPresent(HttpRequestMessage request, IHttpCancel action)
        {
            var manager = request.GetPropertyOrDefault<CancelManager>();
            if (manager != null)
            {
                manager.Add(action);
            }
        }

        public static void EnableCancel(HttpRequestMessage request)
        {
            var manager = request.GetPropertyOrDefault<CancelManager>();
            if (manager == null)
            {
                manager = new CancelManager();
                request.Properties.Add(manager);
            }
        }
    }

    interface IHttpCancel
    {
        void Cancel();
    }

    class HttpStageProcessingAsyncState
    {
        bool cancelled;
        public void MarkCancelled()
        {
            if (cancelled)
            {
                return;
            }
            cancelled = true;
            var manager = request.GetPropertyOrDefault<CancelManager>();
            if (manager != null)
            {
                manager.Cancel();
            }
        }
        public bool Cancelled
        {
            get
            {
                return this.cancelled;
            }
        }
        public readonly ReadOnlyCollection<HttpStage> stages;
        public readonly HttpRequestMessage request;
        public readonly Stack<object> states;

        public void MarkAsAsync()
        {
            wentAsync = true;
        }
        bool wentAsync;
        public bool StayedSynchronous
        {
            get
            {
                return !wentAsync;
            }
        }
#if DEBUG
        internal readonly DateTime started = DateTime.UtcNow;
#endif
        public HttpStageProcessingAsyncState(ReadOnlyCollection<HttpStage> stages, HttpRequestMessage request)
        {
            this.stages = stages;
            this.request = request;
            this.states = new Stack<object>(this.stages.Count);
        }

        public bool AllowAsync
        {
            get
            {
                return !this.ForceSynchronous;
            }
        }

        public bool ForceSynchronous
        {
            get;
            set;
        }

        public HttpResponseMessage GetResponseOrNull()
        {
            return this.response;
        }

        HttpResponseMessage response;
        public HttpResponseMessage Response
        {
            get
            {
                Debug.Assert(this.response != null);
                return this.response;
            }
        }
        public void SetResponse(HttpResponseMessage response)
        {
            Debug.Assert(this.response == null);
            this.response = response;
        }

        Exception exception;
        public Exception GetExceptionOrNull()
        {
            return exception;
        }

        public void SetException(Exception e)
        {
            Debug.Assert(this.exception == null);
            this.exception = e;
            if (this.response != null)
            {
                this.response.Dispose();
            }
            if (this.request != null)
            {
                this.request.Dispose();
            }
        }
    }
}