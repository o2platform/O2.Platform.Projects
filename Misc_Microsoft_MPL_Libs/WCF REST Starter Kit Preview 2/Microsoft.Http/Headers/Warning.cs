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


    public class Warning
    {

        int? code;

        public Host Agent
        {
            get;
            set;
        }

        public int Code
        {
            get
            {
                return code.Value;
            }
            set
            {
                code = value;
            }
        }

        public string Text
        {
            get;
            set;
        }

        public static Warning Parse(string value)
        {
            value = value.Trim();
            switch (value)
            {
                case "110 Response is stale":
                    return new Warning()
                        {
                            Code = 110,
                            Text = "Response is stale"
                        };
                case "111 Revalidation failed":
                    return new Warning()
                        {
                            Code = 111,
                            Text = "Revalidation failed"
                        };

                case "112 Disconnected operation":
                    return new Warning()
                        {
                            Code = 112,
                            Text = "Disconnected operation"
                        };
                case "113 Heuristic expiration":
                    return new Warning()
                        {
                            Code = 113,
                            Text = "Heuristic expiration"
                        };
            }
            var w = new Warning();
            int space = value.IndexOf(' ');
            w.Code = int.Parse(value.Substring(0, space),CultureInfo.InvariantCulture);
            value = value.Substring(space + 1).Trim();
            space = value.IndexOf(' ');
            w.Agent = Host.Parse(value.Substring(0, space));
            value = value.Substring(space + 1).Trim();
            w.Text = value;
            return w;
        }
        public override string ToString()
        {
            if (!this.code.HasValue && this.Agent == null && string.IsNullOrEmpty(this.Text))
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(this.Code);
            sb.Append(' ');
            if (Agent != null)
            {
                sb.Append(this.Agent.ToString());
                sb.Append(' ');
            }
            sb.Append(this.Text);
            return sb.ToString();
        }
    }
}
