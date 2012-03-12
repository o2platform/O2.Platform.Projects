//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Http
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using Microsoft.Http.Headers;

    sealed class HttpMessageCore : IDisposable
    {
        readonly object parent;

        HttpContent content;

        bool disposed;

        Collection<object> properties;
        public HttpMessageCore(object parent)
        {
            this.parent = parent;
#if DEBUG 
            this.stack = Environment.StackTrace;
#endif
        }
        public HttpContent Content
        {
            get
            {
                return content;
            }
            set
            {
                // is a null content ever okay?
                content = value;
            }
        }

        HeaderStore store;
        public HeaderStore Headers
        {
            get
            {
                if (store == null)
                {
                    store = new HeaderStore();
                }
                return store;
            }
            set
            {
                store = value;
            }
        }

        // the Method is needed on the response (processing GET xyz versus HEAD xyz)
        public string Method
        {
            get;
            set;
        }
        public ICollection<object> Properties
        {
            get
            {
                if (properties == null)
                {
                    properties = new Collection<object>();
                }
                return properties;
            }
        }

        public bool HasProperties
        {
            get
            {
                return this.properties != null && this.properties.Count != 0;
            }
        }

        public Uri Uri
        {
            get;
            set;
        }


        public static string CalculateEffectiveContentType(HttpRequestMessage request)
        {
            var header = request.Headers.ContentType;
            var c = request.Content;
            return CalculateEffectiveContentType(header, c);
        }

        public static string CalculateEffectiveContentType(string header, HttpContent c)
        {
            var body = HttpContent.IsNullOrEmpty(c) ? null : c.ContentType;

            return CalculateEffectiveContentType(header, body);
        }

        public static string CalculateEffectiveContentType(string header, string body)
        {
            if (string.IsNullOrEmpty(header) && string.IsNullOrEmpty(body))
            {
                // consider: check the method?
                return null;
            }

            if (string.IsNullOrEmpty(header) && !string.IsNullOrEmpty(body))
            {
                // request.Headers.ContentType = body;
                return body;
            }
            else if (!string.IsNullOrEmpty(header) && string.IsNullOrEmpty(body))
            {
                // okay
                return header;
            }
            else // both non-null, header wins
            {
                //if (header != body)
                //{
                //    throw new ArgumentOutOfRangeException("Content-Type header was \"" + header + "\" but body indicated content type was \"" + body + "\"");
                //}
                return header;
            }
        }
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            disposed = true;

            if (this.content != null)
            {
                this.content.Dispose();
            }
            // CONSIDER: MakeReadOnly?
#if DEBUG
            GC.SuppressFinalize(this);
#endif
        }

#if DEBUG
        string stack;
        internal static string filename;
        static readonly object sharedLock = new object();
        ~HttpMessageCore()
        {
            var req = this.parent as HttpRequestMessage;
            if (req != null && req.HasBeenSent)
            {
                return;
            }
            lock (sharedLock)
            {
                if (filename == null)
                {
                    filename = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Environment.TickCount + ".txt");
                    Debug.WriteLine(filename);
                }
                System.IO.File.AppendAllText(filename, this + Environment.NewLine + stack + Environment.NewLine + Environment.NewLine);
            }
            Debug.WriteLine("finalizer: " + this);
            Debug.WriteLine(stack);
        }
#endif

        public override string ToString()
        {
            return "HttpMessageCore(" + this.parent + ")";
        }
    }
}
