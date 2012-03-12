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

    public sealed class RequestHeaders : HttpHeaders
    {
        public RequestHeaders()
            : base(new HeaderStore())
        {
        }
        internal RequestHeaders(HeaderStore store)
            : base(store)
        {
        }

        public HeaderValues<StringWithOptionalQuality> Accept
        {
            get
            {
                return store.GetCollection<StringWithOptionalQuality>("Accept");
            }
            set
            {
                store.SetCollection("Accept", value);
            }
        }

        public HeaderValues<StringWithOptionalQuality> AcceptCharset
        {
            get
            {
                return store.GetCollection<StringWithOptionalQuality>("Accept-Charset");
            }
            set
            {
                store.SetCollection("Accept-Charset", value);
            }
        }


        public HeaderValues<StringWithOptionalQuality> AcceptEncoding
        {
            get
            {
                return store.GetCollection<StringWithOptionalQuality>("Accept-Encoding");
            }
            set
            {
                store.SetCollection("Accept-Encoding", value);
            }
        }
        public HeaderValues<StringWithOptionalQuality> AcceptLanguage
        {
            get
            {
                return store.GetCollection<StringWithOptionalQuality>("Accept-Language");
            }
            set
            {
                store.SetCollection("Accept-Language", value);
            }
        }

        public Credential Authorization
        {
            get
            {
                return store.GetOrNull<Credential>("Authorization");
            }
            set
            {
                store.SetOrNull("Authorization", value);
            }
        }

        public HeaderValues<Cookie> Cookie
        {
            get
            {
                return store.GetCollection<Cookie>("Cookie");
            }
            set
            {
                store.SetCollection<Cookie>("Cookie", value);
            }
        }

        public Expect Expect
        {
            get
            {
                return store.GetOrNull<Expect>("Expect");
            }
            set
            {
                store.SetOrNull("Expect", value);
            }
        }

        // note: could also be MailAddress
        public string From
        {
            get
            {
                return store.GetOrNull<string>("From");
            }
            set
            {
                store.SetOrNull("From", value);
            }
        }

        public Host Host
        {
            get
            {
                return store.GetOrNull<Host>("Host");
            }
            set
            {
                store.SetOrNull("Host", value);
            }
        }
        public HeaderValues<EntityTag> IfMatch
        {
            get
            {
                return store.GetCollection<EntityTag>("If-Match");
            }
            set
            {
                store.SetCollection("If-Match", value);
            }
        }
        public DateTime? IfModifiedSince
        {
            get
            {
                return store.GetOrNullable<DateTime>("If-Modified-Since");
            }
            set
            {
                store.SetOrNull("If-Modified-Since", value);
            }
        }


        public HeaderValues<EntityTag> IfNoneMatch
        {
            get
            {
                return store.GetCollection<EntityTag>("If-None-Match");
            }
            set
            {
                // The result of a request having both an If-None-Match header field and either an If-Match or an If-Unmodified-Since header fields is undefined by this specification. 
                store.SetCollection("If-None-Match", value);
            }
        }

        public DateOrEntityTag IfRange
        {
            get
            {
                return store.GetOrNull<DateOrEntityTag>("If-Range");
            }
            set
            {
                store.SetOrNull("If-Range", value);
            }
        }
        public DateTime? IfUnmodifiedSince
        {
            get
            {
                return store.GetOrNullable<DateTime>("If-Unmodified-Since");
            }
            set
            {
                store.SetOrNull("If-Unmodified-Since", value);
            }
        }

        public int? MaxForwards
        {
            get
            {
                return store.GetOrNullable<int>("Max-Forwards");
            }
            set
            {
                store.SetOrNull("Max-Forwards", value);
            }
        }

        public Credential ProxyAuthorization
        {
            get
            {
                return store.GetOrNull<Credential>("Proxy-Authorization");
            }
            set
            {
                store.SetOrNull("Proxy-Authorization", value);
            }
        }

        public Range Range
        {
            get
            {
                return store.GetOrNull<Range>("Range");
            }
            set
            {
                store.SetOrNull("Range", value);
            }
        }

        public Uri Referer
        {
            get
            {
                return store.GetOrNull<Uri>("Referer");
            }
            set
            {
                store.SetOrNull<Uri>("Referer", value);
            }
        }

        public HeaderValues<StringWithOptionalQuality> TE
        {
            get
            {
                return store.GetCollection<StringWithOptionalQuality>("TE");
            }
            set
            {
                store.SetCollection("TE", value);
            }
        }
        public HeaderValues<ProductOrComment> UserAgent
        {
            get
            {
                return store.GetCollection<ProductOrComment>("User-Agent", ' ', '(', ')');
            }
            set
            {
                store.SetCollection("User-Agent", value);
            }
        }
        public static RequestHeaders Parse(string lines)
        {
            return new RequestHeaders(HeaderStore.Parse(lines));
        }
    }
}
