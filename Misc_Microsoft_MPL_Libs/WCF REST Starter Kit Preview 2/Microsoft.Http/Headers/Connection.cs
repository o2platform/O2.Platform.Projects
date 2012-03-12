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


    public class Connection
    {
        Collection<string> headers;

        public Connection()
        {
        }
        public bool Close
        {
            get;
            set;
        }
        public Collection<string> Headers
        {
            get
            {
                if (headers == null)
                {
                    headers = new Collection<string>();
                }
                return headers;
            }
        }

        public static Connection Parse(string value)
        {
            var c = new Connection();
            foreach (var h in value.Split(','))
            {
                var t = h.Trim();
                if (string.IsNullOrEmpty(t))
                {
                    continue;
                }

                if (h.Equals("close", StringComparison.OrdinalIgnoreCase))
                {
                    c.Close = true;
                    continue;
                }

                c.Headers.Add(t);
            }

            return c;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (Close)
            {
                sb.Append("close");
            }

            foreach (var s in this.Headers)
            {
                if (sb.Length != 0)
                {
                    sb.Append(", ");
                }
                sb.Append(s);
            }
            return sb.ToString();
        }
    }
}
