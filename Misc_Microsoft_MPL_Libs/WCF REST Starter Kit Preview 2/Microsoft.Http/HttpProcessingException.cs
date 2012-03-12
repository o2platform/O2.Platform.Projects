//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    [Serializable]
    public class HttpProcessingException : SystemException
    {
        public HttpProcessingException()
        {
        }
        public HttpProcessingException(string message)
            : base(message)
        {
        }
        public HttpProcessingException(string message, Exception inner)
            : base(message, inner)
        {
        }
        public HttpProcessingException(string message, Exception inner, HttpRequestMessage request, HttpResponseMessage response)
            : this(message, inner)
        {
            this.request = request;
            this.response = response;
        }
        protected HttpProcessingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }

        [NonSerialized]
        HttpRequestMessage request;
        public HttpRequestMessage Request
        {
            get
            {
                return this.request;
            }
        }

        [NonSerialized]
        HttpResponseMessage response;
        public HttpResponseMessage Response
        {
            get
            {
                return this.response;
            }
        }
    }
}
