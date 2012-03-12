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


    public class ByteRange
    {

        public ByteRange(int from, int to)
        {
            this.Begin = from;
            this.End = to;
        }

        public ByteRange(int range)
        {
            this.Begin = range;
        }

        public int Begin
        {
            get;
            set;
        }

        public int? End
        {
            get;
            set;
        }
        public static ByteRange Parse(string value)
        {
            ByteRange br;
            string r = value.Trim();
            int minus = r.IndexOf('-');
            if (minus == -1 || minus == 0)
            {
                br = new ByteRange(ParseInt32(r));
            }
            else
            {
                var from = ParseInt32(r.Substring(0, minus));
                var to = ParseInt32(r.Substring(minus + 1));
                br = new ByteRange(from, to);
            }
            return br;
        }

        private static int ParseInt32(string r)
        {
            return int.Parse(r, CultureInfo.InvariantCulture);
        }

        public override string ToString()
        {
            if (this.End == null)
            {
                return this.Begin.ToStringInvariant();
            }
            return this.Begin + "-" + this.End;
        }
    }
}
