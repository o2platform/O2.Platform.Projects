//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace Microsoft.Http.Headers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Text;


    public sealed class ResponseHeaders : HttpHeaders
    {
        public ResponseHeaders()
            : base(new HeaderStore())
        {
        }
        internal ResponseHeaders(HeaderStore store)
            : base(store)
        {
        }

        public AcceptRangeUnit? AcceptRanges
        {
            get
            {
                return store.GetOrNullable<AcceptRangeUnit>("Accept-Range");
            }
            set
            {
                store.SetOrNull("Accept-Range", value);
            }
        }

        public TimeSpan? Age
        {
            get
            {
                return store.GetOrNullable<TimeSpan>("Age");
            }
            set
            {
                store.SetOrNull("Age", value);
            }
        }

        public EntityTag ETag
        {
            get
            {
                return store.GetOrNull<EntityTag>("ETag");
            }
            set
            {
                store.SetOrNull("ETag", value);
            }
        }
        public Uri Location
        {
            get
            {
                return store.GetOrNull<Uri>("Location");
            }
            set
            {
                if (value != null && !value.IsAbsoluteUri)
                {
                    throw new ArgumentOutOfRangeException(value + " must be an absolute Uri");
                }
                store.SetOrNull("Location", value);
            }
        }
        public Challenge ProxyAuthenticate
        {
            get
            {
                return store.GetOrNull<Challenge>("Proxy-Authenticate");
            }
            set
            {
                store.SetOrNull("Proxy-Authenticate", value);
            }
        }

        public DateOrDeltaSeconds RetryAfter
        {
            get
            {
                return store.GetOrNull<DateOrDeltaSeconds>("Retry-After");
            }
            set
            {
                store.SetOrNull("Retry-After", value);
            }
        }

        public HeaderValues<ProductOrComment> Server
        {
            get
            {
                return store.GetCollection<ProductOrComment>("Server", ' ', '(', ')');
            }
            set
            {
                store.SetCollection("Server", value);
            }
        }
        public HeaderValues<Cookie> SetCookie
        {
            get
            {
                return store.GetCollection<Cookie>("Set-Cookie");
            }
            set
            {
                store.SetCollection("Set-Cookie", value);
            }
        }

        public HeaderValues<string> Vary
        {
            get
            {
                return store.GetCollection<string>("Vary");
            }
            set
            {
                store.SetCollection("Vary", value);
            }
        }

        // note: the spec says this is challenge*
        public Challenge WwwAuthenticate
        {
            get
            {
                return store.GetOrNull<Challenge>("WWW-Authenticate");
            }
            set
            {
                store.SetOrNull("WWW-Authenticate", value);
            }
        }

        public static ResponseHeaders Parse(string lines)
        {
            return new ResponseHeaders(HeaderStore.Parse(lines));
        }
    }
}
