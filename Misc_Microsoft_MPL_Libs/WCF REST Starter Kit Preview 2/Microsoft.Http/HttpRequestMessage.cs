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

    public sealed class HttpRequestMessage : IDisposable
    {
        readonly HttpMessageCore core;
        public HttpRequestMessage()
        {
            this.core = new HttpMessageCore(this);
            this.Method = "GET";
        }

        public HttpRequestMessage(string method, Uri uri)
        {
            this.core = new HttpMessageCore(this);
            this.Method = method;
            this.Uri = uri;
        }

        public HttpRequestMessage(string method, string uri)
            : this(method, new Uri(uri, UriKind.RelativeOrAbsolute))
        {
        }
        public HttpRequestMessage(string method, Uri uri, RequestHeaders headers)
            : this(method, uri)
        {
            this.headers = headers;
        }
        public HttpRequestMessage(string method, Uri uri, HttpContent content)
            : this(method, uri)
        {
            this.Content = content;
        }
        public HttpRequestMessage(string method, Uri uri, RequestHeaders headers, HttpContent content)
            : this(method, uri,content)
        {
            this.headers = headers;
        }

        public HttpContent Content
        {
            get
            {
                return core.Content;
            }
            set
            {
                if (HasBeenSent)
                {
                    throw new InvalidOperationException("message has been sent");
                }
                core.Content = value;
            }
        }

        RequestHeaders headers;

        public RequestHeaders Headers
        {
            get
            {
                if (headers == null)
                {
                    headers = new RequestHeaders(this.core.Headers);
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

        internal bool HasBeenSent
        {
            get;
            set;
        }

        public void Dispose()
        {
            this.core.Dispose();
        }

        public override string ToString()
        {
            return "HttpRequestMessage(" + this.Method + " " + this.Uri + ")";
        }
    }
}
