//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Net;
    using System.IO;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using Microsoft.Http.Headers;

    public sealed class HttpResponseMessage : IDisposable
    {
        readonly HttpMessageCore core;

        HttpRequestMessage request;

        public HttpResponseMessage()
        {
            this.core = new HttpMessageCore(this);
        }

        public HttpContent Content
        {
            get
            {
                return core.Content;
            }
            set
            {
                core.Content = value;
            }
        }
        ResponseHeaders headers;
        public ResponseHeaders Headers
        {
            get
            {
                if (headers == null)
                {
                    headers = new ResponseHeaders(core.Headers);
                }
                return headers;
            }
            set
            {
                headers = value;
            }
        }

        public string Method
        {
            get
            {
                return core.Method;
            }
            set
            {
                core.Method = value;
            }
        }
        public HttpRequestMessage Request
        {
            get
            {
                if (request == null)
                {
                    throw new InvalidOperationException();
                }
                return request;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (request != null && value != request)
                {
                    throw new InvalidOperationException(request + " " + value);
                }
                this.request = value;
            }
        }

        public ICollection<object> Properties
        {
            get
            {
                return core.Properties;
            }
        }

        internal bool HasProperties
        {
            get
            {
                return core.HasProperties;
            }
        }

        public HttpStatusCode StatusCode
        {
            get;
            set;
        }

        public Uri Uri
        {
            get
            {
                return core.Uri;
            }
            set
            {
                core.Uri = value;
            }
        }

        public void Dispose()
        {
            if (this.request != null)
            {
                this.request.Dispose();
            }
            this.core.Dispose();
        }

        public override string ToString()
        {
            return "HttpResponseMessage(" + this.Method + " " + this.Uri + " " + this.StatusCode + " " + (int) this.StatusCode + ")";
        }
    }
}
