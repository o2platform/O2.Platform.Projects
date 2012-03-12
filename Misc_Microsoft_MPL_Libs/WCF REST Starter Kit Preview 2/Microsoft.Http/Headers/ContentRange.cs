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


    public class ContentRange
    {
        public int? FirstBytePosition
        {
            get;
            set;
        }

        public int? LastBytePosition
        {
            get;
            set;
        }

        public int? Length
        {
            get;
            set;
        }

        public bool LengthIsStar
        {
            get;
            set;
        }

        public bool RangeIsStar
        {
            get;
            set;
        }

        public static ContentRange Parse(string value)
        {
            if (!value.StartsWithInvariant("bytes "))
            {
                throw new FormatException();
            }
            value = value.Substring(6);
            var parts = value.Split('-', '/');
            if (parts.Length <= 1 || parts.Length > 3)
            {
                throw new FormatException(value);
            }
            var r = new ContentRange();
            if (parts.Length == 2)
            {
                if (parts[0] != "*")
                {
                    throw new FormatException(parts[0]);
                }
                r.RangeIsStar = true;
            }
            else
            {
                r.FirstBytePosition = int.Parse(parts[0], CultureInfo.InvariantCulture);
                r.LastBytePosition = int.Parse(parts[1], CultureInfo.InvariantCulture);
            }

            if (parts.Last() == "*")
            {
                r.LengthIsStar = true;
            }
            else
            {
                r.Length = int.Parse(parts.Last(), CultureInfo.InvariantCulture);
            }
            return r;
        }

        public override string ToString()
        {
            bool hasValues = this.RangeIsStar || this.LengthIsStar || this.FirstBytePosition.HasValue || this.LastBytePosition.HasValue || this.Length.HasValue;

            if (!hasValues)
            {
                return "";
            }

            StringBuilder b = new StringBuilder();
            b.Append("bytes ");
            if (this.RangeIsStar)
            {
                b.Append('*');
            }
            else
            {
                b.Append(this.FirstBytePosition);
                b.Append("-");
                b.Append(this.LastBytePosition);
            }
            b.Append('/');
            if (this.LengthIsStar)
            {
                b.Append('*');
            }
            else
            {
                b.Append(this.Length);
            }
            return b.ToString();
        }
    }
}
