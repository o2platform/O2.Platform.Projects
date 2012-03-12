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


    public class DateOrEntityTag
    {
        static readonly HeaderFormatter formatter = HeaderFormatter.Default;

        public static implicit operator DateOrEntityTag(DateTime date)
        {
            return new DateOrEntityTag(date);
        }

        public static implicit operator DateOrEntityTag(EntityTag tag)
        {
            return new DateOrEntityTag(tag);
        }

        public DateOrEntityTag()
        {
        }
        public DateOrEntityTag(EntityTag entityTag)
        {
            this.EntityTag = entityTag;
        }

        public DateOrEntityTag(DateTime date)
        {
            this.Date = date;
        }
        public DateTime? Date
        {
            get;
            set;
        }

        public EntityTag EntityTag
        {
            get;
            set;
        }

        public bool IsEntityTag
        {
            get
            {
                return this.EntityTag != null;
            }
        }
        public static DateOrEntityTag Parse(string value)
        {
            if (value.StartsWithInvariant("W/") || value.StartsWithInvariant("\""))
            {
                return new DateOrEntityTag(formatter.FromString<EntityTag>(value));
            }

            return new DateOrEntityTag(formatter.FromString<DateTime>(value));
        }

        public override string ToString()
        {
            if (this.EntityTag == null && this.Date == null)
            {
                return "";
            }

            if (IsEntityTag)
            {
                return formatter.ToString(this.EntityTag);
            }
            else
            {
                return formatter.ToString(this.Date.Value);
            }
        }
    }
}
