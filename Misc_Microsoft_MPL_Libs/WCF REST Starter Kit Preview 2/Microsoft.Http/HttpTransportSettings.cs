//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Http
{
    using System;
    using System.Net;
    using System.Net.Cache;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;

    public class HttpWebRequestTransportSettings
    {
        public HttpWebRequestTransportSettings()
        {
        }

        HttpRequestCachePolicy cache;
        X509CertificateCollection certificates;
        IWebProxy proxy;

        public AuthenticationLevel? AuthenticationLevel
        {
            get;
            set;
        }

        public bool? AllowWriteStreamBuffering
        {
            get;
            set;
        }

        public DecompressionMethods? AutomaticDecompression
        {
            get;
            set;
        }

        public HttpRequestCachePolicy CachePolicy
        {
            get
            {
                return cache;
            }
            set
            {
                HasCachePolicy = true;
                cache = value;
            }
        }

        public X509CertificateCollection ClientCertificates
        {
            get
            {
                if (certificates == null)
                {
                    certificates = new X509CertificateCollection();
                }
                return certificates;
            }
        }

        public CookieContainer Cookies
        {
            get;
            set;
        }

        public ICredentials Credentials
        {
            get;
            set;
        }


        public TokenImpersonationLevel? ImpersonationLevel
        {
            get;
            set;
        }

        int redirects = 50;
        public int MaximumAutomaticRedirections
        {
            get
            {
                return this.redirects;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "MaximumAutomaticRedirections must be greater than or equal to zero");
                }
                this.redirects = value;
            }
        }

        public int? MaximumResponseHeaderKB
        {
            get;
            set;
        }

        public bool? PreAuthenticate
        {
            get;
            set;
        }

        public IWebProxy Proxy
        {
            get
            {
                return proxy;
            }
            set
            {
                HasProxy = true;
                proxy = value;
            }
        }

        public TimeSpan? ReadWriteTimeout
        {
            get;
            set;
        }

        public bool? SendChunked
        {
            get;
            set;
        }

        public TimeSpan? ConnectionTimeout
        {
            get;
            set;
        }

        public bool? UseDefaultCredentials
        {
            get;
            set;
        }

        internal bool HasCachePolicy
        {
            get;
            private set;
        }

        internal bool HasClientCertificates
        {
            get
            {
                return certificates != null && certificates.Count != 0;
            }
        }
        internal bool HasProxy
        {
            get;
            private set;
        }

        public void SetCacheLevel(System.Net.Cache.HttpRequestCacheLevel level)
        {
            this.CachePolicy = new System.Net.Cache.HttpRequestCachePolicy(level);
        }

    }
}
