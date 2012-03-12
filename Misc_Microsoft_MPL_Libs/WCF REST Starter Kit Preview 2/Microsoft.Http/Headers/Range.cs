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

    public class Range : Collection<ByteRange>
    {
        public Range()
        {
        }

        public static Range Parse(string value)
        {
            var b = new Range();
            foreach (var x in ParseToList(value))
            {
                b.Add(x);
            }
            return b;
        }

        public override string ToString()
        {
            return MakeValue();
        }

        static List<ByteRange> ParseToList(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return new List<ByteRange>();
            }
            string[] parts = value.Split('=');
            if (parts[0] != "bytes" || parts.Length != 2)
            {
                throw new ArgumentOutOfRangeException("Range: " + value);
            }
            List<ByteRange> spec = new List<ByteRange>();
            foreach (string x in parts[1].Split(','))
            {
                ByteRange br = ByteRange.Parse(x);
                spec.Add(br);
            }
            return spec;
        }

        string MakeValue()
        {
            if (this.Count == 0)
            {
                return "";
            }
            StringBuilder b = new StringBuilder();
            foreach (var r in this)
            {
                if (b.Length != 0)
                {
                    b.Append(", ");
                }

                b.Append(r);
            }
            return "bytes=" + b;
        }
    }
}
