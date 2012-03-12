//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Http
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography;


    public abstract class HttpProcessingStage : HttpStage
    {
        public abstract void ProcessRequest(HttpRequestMessage request);
        public abstract void ProcessResponse(HttpResponseMessage response);

        protected internal sealed override void ProcessRequestAndTryGetResponse(HttpRequestMessage request, out HttpResponseMessage response, out object state)
        {
            response = null;
            state = null;
            ProcessRequest(request);
        }

        protected internal sealed override void ProcessResponse(HttpResponseMessage response, object state)
        {
            ProcessResponse(response);
        }
    }

    public abstract class HttpProcessingStage<TState> : HttpStage
    {
        public abstract TState ProcessRequest(HttpRequestMessage request);
        public abstract void ProcessResponse(HttpResponseMessage response, TState state);

        protected internal sealed override void ProcessRequestAndTryGetResponse(HttpRequestMessage request, out HttpResponseMessage response, out object state)
        {
            response = null;
            state = ProcessRequest(request);
        }

        protected internal sealed override void ProcessResponse(HttpResponseMessage response, object state)
        {
            TState t = (TState) state;
            ProcessResponse(response, t);
        }
    }

}
