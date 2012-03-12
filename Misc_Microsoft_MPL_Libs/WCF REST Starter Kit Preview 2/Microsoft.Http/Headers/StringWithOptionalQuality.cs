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


    public class StringWithOptionalQuality
    {
        // A weight is normalized to a real number in the range 0 through 1, where 0 is the minimum and 1 the maximum value.
        // If a parameter has a quality value of 0, then content with this parameter is `not acceptable' for the client.
        // HTTP/1.1 applications MUST NOT generate more than three digits after the decimal point. 
        double? q;
        string v;

        public StringWithOptionalQuality()
        {
        }

        public StringWithOptionalQuality(string value)
            : this(value, null)
        {
        }

        public StringWithOptionalQuality(string value, double? quality)
        {
            this.Value = value;
            this.Quality = quality;
        }
        public double? Quality
        {
            get
            {
                return q;
            }
            set
            {
                if (value == null)
                {
                    q = null;
                    return;
                }

                if (value > 1 || value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                q = value;
            }
        }
        public string Value
        {
            get
            {
                return v;
            }
            set
            {
                if (value != null && value.Contains(";") && value.Contains("q="))
                {
                    throw new ArgumentOutOfRangeException(value);
                }
                v = value;
            }
        }

        public static implicit operator StringWithOptionalQuality(string value)
        {
            return StringWithOptionalQuality.Parse(value);
        }

        public static StringWithOptionalQuality Parse(string value)
        {
            if (!value.Contains(";") || !value.Contains("q="))
            {
                return new StringWithOptionalQuality(value);
            }
            var parts = value.Split(';');
            StringBuilder sb = new StringBuilder(value.Length);
            double? q = null;
            foreach (var p in parts)
            {
                var pt = p.Trim();
                if (pt.StartsWithInvariant("q="))
                {
                    double quality;
                    if (!double.TryParse(pt.Substring(2), out quality))
                    {
                        throw new FormatException(value);
                    }
                    q = quality;
                }
                else
                {
                    if (sb.Length != 0)
                    {
                        sb.Append(';');
                    }
                    sb.Append(pt);
                }
            }
            return new StringWithOptionalQuality(sb.ToString(), q);
        }

        public override bool Equals(object obj)
        {
            var other = obj as StringWithOptionalQuality;
            return other != null && this.ToString() == other.ToString();
        }
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.Value))
            {
                return "";
            }
            if (q == null)
            {
                return this.Value;
            }
            var x = Math.Round(q.Value, 3);
            var s = x.ToString(CultureInfo.InvariantCulture);
            if (s == "0")
            {
                s = "0.0";
            }
            else if (s == "1")
            {
                s = "1.0";
            }
            return this.Value + ";q=" + s;
        }
    }
}
