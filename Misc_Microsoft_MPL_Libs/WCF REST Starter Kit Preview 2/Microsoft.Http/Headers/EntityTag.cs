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


    public class EntityTag
    {
        public bool IsWeak
        {
            get;
            set;
        }
        public string Tag
        {
            get;
            set;
        }
        public EntityTag()
        {
        }
        public EntityTag(string tag)
        {
            this.Tag = tag;
        }
        public static EntityTag Parse(string original)
        {
            string s = original;
            EntityTag tag = new EntityTag();

            if (s.StartsWithInvariant("W/"))
            {
                tag.IsWeak = true;
                s = s.Substring(2);
            }
            if (s == "*")
            {
                tag.Tag = "*";
            }
            else if (!s.StartsWithInvariant("\"") && !s.EndsWithInvariant("\""))
            {
                tag.Tag = s;
            }
            else if (s.Length < 2 || !s.StartsWithInvariant("\"") || !s.EndsWithInvariant("\""))
            {
                throw new ArgumentOutOfRangeException(s);
            }
            else
            {
                tag.Tag = s.Substring(1, s.Length - 2);
            }
            return tag;
        }

        public override string ToString()
        {
            if (this.Tag == null)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
            if (IsWeak)
            {
                sb.Append("W/");
            }

            if (this.Tag != "*")
            {
                sb.Append('"');
                sb.Append(this.Tag);
                sb.Append('"');
            }
            else
            {
                sb.Append("*");
            }
            return sb.ToString();
        }
    }
}
