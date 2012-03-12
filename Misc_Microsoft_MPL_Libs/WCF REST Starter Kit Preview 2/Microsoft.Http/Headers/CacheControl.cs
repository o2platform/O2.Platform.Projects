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


    public class CacheControl
    {
        Collection<string> extensions;
        Collection<string> noCacheHeaders;
        Collection<string> privateHeaders;

        public Collection<string> Extensions
        {
            get
            {
                if (extensions == null)
                {
                    extensions = new Collection<string>();
                }
                return extensions;
            }
        }
        public TimeSpan? MaxAge
        {
            get;
            set;
        }

        public bool MaxStale
        {
            get;
            set;
        }
        public TimeSpan? MaxStaleLimit
        {
            get;
            set;
        }
        public TimeSpan? MinFresh
        {
            get;
            set;
        }
        public bool MustRevalidate
        {
            get;
            set;
        }
        public bool NoCache
        {
            get;
            set;
        }
        public Collection<string> NoCacheHeaders
        {
            get
            {
                if (noCacheHeaders == null)
                {
                    noCacheHeaders = new Collection<string>();
                }
                return noCacheHeaders;
            }
        }
        public bool NoStore
        {
            get;
            set;
        }
        public bool NoTransform
        {
            get;
            set;
        }
        public bool OnlyIfCached
        {
            get;
            set;
        }
        public bool Private
        {
            get;
            set;
        }
        public Collection<string> PrivateHeaders
        {
            get
            {
                if (privateHeaders == null)
                {
                    privateHeaders = new Collection<string>();
                }
                return privateHeaders;
            }
        }
        public bool ProxyRevalidate
        {
            get;
            set;
        }
        public bool Public
        {
            get;
            set;
        }
        public TimeSpan? SharedMaxAge
        {
            get;
            set;
        }

        public static CacheControl Parse(string headerValue)
        {
            var c = new CacheControl();
            foreach (var d in HeaderStore.ParseMultiValue(headerValue, ','))
            {
                var directive = d.ToUpper(CultureInfo.InvariantCulture);
                switch (directive)
                {
                    case "NO-CACHE":
                        c.NoCache = true;
                        continue;
                    case "NO-STORE":
                        c.NoStore = true;
                        continue;
                    case "MAX-STALE":
                        c.MaxStale = true;
                        c.MaxStaleLimit = null;
                        continue;
                    case "NO-TRANSFORM":
                        c.NoTransform = true;
                        continue;
                    case "ONLY-IF-CACHED":
                        c.OnlyIfCached = true;
                        continue;
                    case "PUBLIC":
                        c.Public = true;
                        continue;
                    case "PRIVATE":
                        c.Private = true;
                        continue;
                    case "MUST-REVALIDATE":
                        c.MustRevalidate = true;
                        continue;
                    case "PROXY-REVALIDATE":
                        c.ProxyRevalidate = true;
                        continue;
                }

                if (directive.StartsWithInvariant("no-cache"))
                {
                    c.noCacheHeaders = LoadHeaders(d);
                    c.NoCache = true;
                    continue;
                }

                if (directive.StartsWithInvariant("max-age"))
                {
                    c.MaxAge = GetDeltaSeconds(directive);
                    continue;
                }

                if (directive.StartsWithInvariant("max-stale"))
                {
                    c.MaxStaleLimit = GetDeltaSeconds(directive);
                    c.MaxStale = true;
                    continue;
                }

                if (directive.StartsWithInvariant("min-fresh"))
                {
                    c.MinFresh = GetDeltaSeconds(directive);
                    continue;
                }

                if (directive.StartsWithInvariant("private"))
                {
                    c.privateHeaders = LoadHeaders(d);
                    c.Private = true;
                    continue;
                }

                if (directive.StartsWithInvariant("s-maxage"))
                {
                    c.SharedMaxAge = GetDeltaSeconds(directive);
                    continue;
                }

                c.Extensions.Add(d);
            }
            return c;
        }


        public override string ToString()
        {
            List<string> x = new List<string>();
            //"no-cache"
            //"no-cache" [ "=" <"> 1#field-name <">
            if (this.NoCache)
            {
                x.Add(Make("no-cache", noCacheHeaders));
            }
            //"no-store"
            if (this.NoStore)
            {
                x.Add("no-store");
            }

            //"max-age" "=" delta-seconds
            if (this.MaxAge.HasValue)
            {
                x.Add(Make("max-age", this.MaxAge.Value));
            }
            //"max-stale" [ "=" delta-seconds ]
            if (this.MaxStale)
            {
                if (this.MaxStaleLimit.HasValue)
                {
                    x.Add(Make("max-stale", this.MaxStaleLimit.Value));
                }
                else
                {
                    x.Add("max-stale");
                }
            }
            //"min-fresh" "=" delta-seconds
            if (this.MinFresh.HasValue)
            {
                x.Add(Make("min-fresh", this.MinFresh.Value));
            }
            //"must-revalidate"
            if (this.MustRevalidate)
            {
                x.Add("must-revalidate");
            }
            //"no-transform"
            if (this.NoTransform)
            {
                x.Add("no-transform");
            }
            //"only-if-cached"
            if (this.OnlyIfCached)
            {
                x.Add("only-if-cached");
            }
            //"private" [ "=" <"> 1#field-name <"> ]
            if (this.Private)
            {
                x.Add(Make("private", privateHeaders));
            }
            //"proxy-revalidate"
            if (this.ProxyRevalidate)
            {
                x.Add("proxy-revalidate");
            }
            //"public"
            if (this.Public)
            {
                x.Add("public");
            }
            if (this.SharedMaxAge.HasValue)
            {
                x.Add(Make("s-maxage", this.SharedMaxAge.Value));
            }

            //cache-extension
            if (this.extensions != null)
            {
                foreach (var e in this.extensions)
                {
                    x.Add(e);
                }
            }

            return string.Join(", ", x.ToArray());
        }
        static TimeSpan GetDeltaSeconds(string directive)
        {
            var t = directive.Split('=').Last().Trim();
            return TimeSpan.FromSeconds(int.Parse(t, CultureInfo.InvariantCulture));
        }

        static Collection<string> LoadHeaders(string s)
        {
            s = s.Substring(s.IndexOf('=') + 1).Trim();
            if (s.StartsWithInvariant("\""))
            {
                s = s.Substring(1).Trim();
            }
            if (s.EndsWithInvariant("\""))
            {
                s = s.Substring(0, s.Length - 1);
            }

            return HeaderStore.ParseMultiValue(s, ',');
        }

        static string Make(string prefix, TimeSpan delta)
        {
            if (delta == null)
            {
                return prefix;
            }
            return prefix + "=" + HeaderFormatter.Default.ToString(delta);
        }

        static string Make(string prefix, Collection<string> fields)
        {
            if (fields == null || fields.Count == 0)
            {
                return prefix;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(prefix);
            sb.Append('=');
            sb.Append('"');
            sb.Append(string.Join(", ", fields.ToArray()));
            sb.Append('"');

            return sb.ToString();
        }
    }
}
