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

    public class Cookie // : Dictionary<string, string>
    {
        static readonly string AttributeOnly = Guid.NewGuid().ToString();
        readonly Dictionary<string, string> dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Cookie()
        {
        }

        public DateTime? Expires
        {
            get
            {
                if (!dict.ContainsKey("Expires"))
                {
                    return null;
                }

                return HeaderFormatter.Default.FromString<DateTime>(dict["expires"]);
            }
            set
            {
                if (value == null)
                {
                    dict.Remove("Expires");
                }
                else
                {
                    dict["expires"] = HeaderFormatter.Default.ToString(value.Value);
                }
            }
        }

        public string Path
        {
            get
            {
                if (!dict.ContainsKey("path"))
                {
                    return null;
                }
                return dict["path"];
            }
            set
            {
                dict["path"] = value;
            }
        }

        public static Cookie Parse(string value)
        {
            var cookie = new Cookie();
            foreach (var pair in HeaderStore.ParseMultiValue(value, ';'))
            {
                var eq = pair.IndexOf('=');
                if (eq == -1)
                {
                    cookie.Add(pair, Cookie.AttributeOnly);
                    continue;
                }

                cookie.Add(pair.Substring(0, eq).Trim(), pair.Substring(eq + 1).Trim());
            }

            return cookie;
        }

        public void Add(string key, string value)
        {
            dict.Add(key, value);
        }

        public void Add(string attributeName)
        {
            dict.Add(attributeName, Cookie.AttributeOnly);
        }

        // not there == exception, attribute = null, value = value
        public string this[string key]
        {
            get
            {
                var v = dict[key];
                if (v == AttributeOnly)
                {
                    return null;
                }
                return v;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                dict[key] = value;
            }
        }

        public bool HasAttribute(string key)
        {
            return ContainsKey(key) && this[key] == null;
        }

        public bool ContainsKey(string key)
        {
            return dict.ContainsKey(key);
        }

        public override string ToString()
        {
            if (dict.Count == 0)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
            foreach (var pair in dict)
            {
                if (string.IsNullOrEmpty(pair.Value))
                {
                    continue;
                }
                if (sb.Length != 0)
                {
                    sb.Append("; ");
                }
                if (pair.Value == Cookie.AttributeOnly)
                {
                    sb.Append(pair.Key);
                    continue;
                }
                sb.AppendFormat("{0}={1}", pair.Key, pair.Value);
            }
            return sb.ToString();
        }
    }
}
