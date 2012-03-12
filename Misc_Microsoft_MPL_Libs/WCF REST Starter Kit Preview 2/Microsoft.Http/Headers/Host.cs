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


    public class Host
    {
        public Host()
        {
        }
        public Host(string host, int? port)
        {
            this.HostName = host;
            this.Port = port;
        }
        public string HostName
        {
            get;
            set;
        }

        public int? Port
        {
            get;
            set;
        }

        public static Host Parse(string value)
        {
            var parts = value.Split(':');
            if (parts.Length == 1)
            {
                return new Host(parts[0], null);
            }
            else if (parts.Length == 2)
            {
                return new Host(parts[0], int.Parse(parts[1], CultureInfo.InvariantCulture));
            }
            else
            {
                throw new FormatException(value);
            }
        }

        public override string ToString()
        {
            return HostName + (Port == null ? "" : ":" + Port.Value);
        }
    }
}
