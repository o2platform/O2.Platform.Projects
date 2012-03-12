//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;


    public sealed class HttpQueryString : Collection<KeyValuePair<string, string>>
    {
        public void Add(string name, string value)
        {
            this.Add(new KeyValuePair<string, string>(name, value));
        }
        public Uri MakeQueryString(Uri uri)
        {
            return MakeQueryString(uri, this);
        }
        public static Uri MakeQueryString(Uri uri, HttpQueryString queryString)
        {
            return MakeQueryString(uri, (IEnumerable<KeyValuePair<string, string>>)queryString);
        }
        public static Uri MakeQueryString(Uri uri, IEnumerable<KeyValuePair<string, string>> values)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var pair in values)
            {
                if (sb.Length != 0)
                {
                    sb.Append('&');
                }
                sb.Append(UrlEncodeWithUppercasePercent20(pair.Key));
                sb.Append('=');
                sb.Append(UrlEncodeWithUppercasePercent20(pair.Value));
            }

            if (!uri.IsAbsoluteUri)
            {
                var s = uri.ToString();
                if (s.Contains("?"))
                {
                    if (!s.EndsWithInvariant('&') && !s.EndsWithInvariant('?'))
                    {
                        return new Uri(s + "&" + sb, UriKind.Relative);
                    }
                    return new Uri(s + sb, UriKind.Relative);
                }

                return new Uri(s + "?" + sb, UriKind.Relative);
            }
            UriBuilder b = new UriBuilder(uri);
            string existing = b.Query;
            if (existing.StartsWithInvariant('?'))
            {
                existing = existing.Substring(1);
            }
            if (!string.IsNullOrEmpty(existing))
            {
                existing = existing + "&";
            }
            b.Query = existing + sb.ToString();
            return b.Uri;
        }
        const string unreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._~";

        static string UrlEncodeWithUppercasePercent20(string value)
        {
            StringBuilder result = new StringBuilder(value.Length);

            foreach (char symbol in value)
            {
                if (unreservedChars.IndexOf(symbol) != -1)
                {
                    result.Append(symbol);
                }
                else
                {
                    result.Append('%');
                    result.Append(((int)symbol).ToString("X2", CultureInfo.InvariantCulture));
                }
            }
            return result.ToString();
        }
    }
}