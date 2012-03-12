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


    public abstract class HttpStage
    {
        protected internal abstract void ProcessRequestAndTryGetResponse(HttpRequestMessage request, out HttpResponseMessage response, out object state);
        protected internal abstract void ProcessResponse(HttpResponseMessage response, object state);
    }
}
