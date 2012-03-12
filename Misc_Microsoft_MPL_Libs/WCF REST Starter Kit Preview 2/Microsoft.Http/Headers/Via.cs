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


    public class Via
    {
        string protocol;

        public string Comment
        {
            get;
            set;
        }
        public string ProtocolName
        {
            get
            {
                if (protocol == null)
                {
                    return "HTTP";
                }
                return protocol;
            }
            set
            {
                protocol = value;
            }
        }
        public string ProtocolVersion
        {
            get;
            set;
        }

        // no pseudonym / hostname distinction
        public Host ReceivedBy
        {
            get;
            set;
        }

        public static Via Parse(string value)
        {
            var via = new Via();
            var open = value.IndexOf('(');
            if (open != -1)
            {
                var comment = value.Substring(open + 1);
                comment = comment.Substring(0, comment.LastIndexOf(')')).Trim();
                via.Comment = comment;
                value = value.Substring(0, open);
            }
            value = value.Trim();
            var parts = value.Split();

            var slash = parts[0].IndexOf('/');
            if (slash != -1)
            {
                via.ProtocolName = parts[0].Substring(0, slash);
                via.ProtocolVersion = parts[0].Substring(slash + 1);
            }
            else
            {
                via.ProtocolVersion = parts[0];
            }

            if (parts.Length == 1 || parts.Length > 2)
            {
                throw new FormatException(value);
            }

            via.ReceivedBy = Host.Parse(parts[1]);

            return via;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (protocol != null)
            {
                sb.Append(protocol);
                sb.Append('/');
            }
            sb.Append(this.ProtocolVersion);
            if (sb.Length != 0)
            {
                sb.Append(' ');
            }
            sb.Append(this.ReceivedBy);
            if (!string.IsNullOrEmpty(this.Comment))
            {
                if (sb.Length != 0)
                {
                    sb.Append(' ');
                }
                sb.Append('(');
                sb.Append(this.Comment);
                sb.Append(')');
            }
            return sb.ToString();
        }
    }
}
