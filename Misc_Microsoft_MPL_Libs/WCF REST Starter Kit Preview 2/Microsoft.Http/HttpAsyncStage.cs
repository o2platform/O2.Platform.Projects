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


    public abstract class HttpAsyncStage : HttpStage
    {
        protected internal abstract IAsyncResult BeginProcessRequestAndTryGetResponse(HttpRequestMessage request, AsyncCallback callback, object state);

        protected internal abstract IAsyncResult BeginProcessResponse(HttpResponseMessage response, object state, AsyncCallback callback, object callbackState);
        protected internal abstract void EndProcessRequestAndTryGetResponse(IAsyncResult result, out HttpResponseMessage response, out object state);
        protected internal abstract void EndProcessResponse(IAsyncResult result);
    }
}
