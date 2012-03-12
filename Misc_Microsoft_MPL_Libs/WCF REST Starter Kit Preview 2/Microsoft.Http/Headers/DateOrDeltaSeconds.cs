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


    public class DateOrDeltaSeconds
    {
        static readonly HeaderFormatter formatter = HeaderFormatter.Default;

        public DateOrDeltaSeconds(TimeSpan delta)
        {
            this.Delta = delta;
        }

        public DateOrDeltaSeconds(DateTime date)
        {
            this.Date = date;
        }

        public DateOrDeltaSeconds()
        {
        }

        public DateTime? Date
        {
            get;
            set;
        }

        public TimeSpan? Delta
        {
            get;
            set;
        }

        public bool IsDelta
        {
            get
            {
                return this.Delta != null;
            }
        }
        public static DateOrDeltaSeconds Parse(string value)
        {
            int x;
            if (int.TryParse(value, out x))
            {
                return new DateOrDeltaSeconds(formatter.FromString<TimeSpan>(value));
            }

            return new DateOrDeltaSeconds(formatter.FromString<DateTime>(value));
        }

        public override string ToString()
        {
            if (this.Date == null && this.Delta == null)
            {
                return "";
            }

            if (IsDelta)
            {
                return formatter.ToString(this.Delta.Value);
            }
            else
            {
                return formatter.ToString(this.Date.Value);
            }
        }
    }
}
