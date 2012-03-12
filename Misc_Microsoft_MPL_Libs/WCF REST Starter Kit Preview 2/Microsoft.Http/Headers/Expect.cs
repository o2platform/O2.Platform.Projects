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


    public class Expect
    {
        Collection<string> extensions;

        public Expect()
        {
        }
        public bool Expect100Continue
        {
            get;
            set;
        }
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

        public static Expect Parse(string value)
        {
            var c = new Expect();
            foreach (var h in value.Split(','))
            {
                var t = h.Trim();
                if (string.IsNullOrEmpty(t))
                {
                    continue;
                }

                if (h.Equals("100-continue", StringComparison.OrdinalIgnoreCase))
                {
                    c.Expect100Continue = true;
                    continue;
                }

                c.Extensions.Add(t);
            }

            return c;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (Expect100Continue)
            {
                sb.Append("100-continue");
            }

            foreach (var s in this.Extensions)
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
