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


    public class Challenge
    {
        Collection<string> parameters;
        public Collection<string> Parameters
        {
            get
            {
                if (parameters == null)
                {
                    parameters = new Collection<string>();
                }
                return parameters;
            }
            private set
            {
                this.parameters = value;
            }
        }
        public string Scheme
        {
            get;
            set;
        }
        public static Challenge Parse(string value)
        {
            string scheme;
            Collection<string> parameters;
            AuthenticationHelper.Parse(value, out scheme, out parameters);

            return new Challenge()
                {
                    Scheme = scheme,
                    Parameters = parameters
                };
        }

        public string GetParameter(string parameter)
        {
            string value;
            if (!TryGetParameter(parameter, out value))
            {
                throw new KeyNotFoundException(parameter);
            }
            return value;
        }

        public override string ToString()
        {
            return AuthenticationHelper.ToString(this.Scheme, this.parameters);
        }

        public bool TryGetParameter(string parameter, out string value)
        {
            return AuthenticationHelper.TryGetParameter(this.Parameters, parameter, out value);
        }
    }
}
