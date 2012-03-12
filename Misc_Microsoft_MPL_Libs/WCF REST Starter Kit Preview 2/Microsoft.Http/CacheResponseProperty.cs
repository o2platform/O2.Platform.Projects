//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.ComponentModel;
    using System.Globalization;

    public class CacheResponseProperty
    {
        public int? AgeInSeconds
        {
            get;
            set;
        }


        public string CacheControl
        {
            get;
            set;
        }
        public DateTime? CacheSyncDate
        {
            get;
            set;
        }

        public bool IsFromCache
        {
            get;
            set;
        }

        public System.Net.Cache.HttpRequestCacheLevel? Level
        {
            get;
            set;
        }


        public TimeSpan? MaxAge
        {
            get;
            set;
        }

        public TimeSpan? MaxStale
        {
            get;
            set;
        }

        public TimeSpan? MinFresh
        {
            get;
            set;
        }


        public string Vary
        {
            get;
            set;
        }


        public string Via
        {
            get;
            set;
        }


        public string Warning
        {
            get;
            set;
        }

        public override string ToString()
        {
            return "CacheResponseProperty(" + this.IsFromCache + ", " + this.Level + ")";
        }

        internal static CacheResponseProperty LoadFrom(System.Net.Cache.RequestCachePolicy req, HttpWebResponse response)
        {
            var property = new CacheResponseProperty()
                {
                    IsFromCache = response.IsFromCache,
                };

            if (response.IsFromCache)
            {
                var policy = req as System.Net.Cache.HttpRequestCachePolicy;

                property.CacheSyncDate = policy.CacheSyncDate;
                property.Level = policy.Level;
                property.MaxAge = policy.MaxAge;
                property.MaxStale = policy.MaxStale;
                property.MinFresh = policy.MinFresh;

                property.AgeInSeconds = int.Parse(response.Headers[HttpResponseHeader.Age],CultureInfo.InvariantCulture);
                property.CacheControl = response.Headers[HttpResponseHeader.CacheControl];
                property.Vary = response.Headers[HttpResponseHeader.Vary];
                property.Via = response.Headers[HttpResponseHeader.Via];
                property.Warning = response.Headers[HttpResponseHeader.Warning];
            }
            return property;
        }
    }
}
