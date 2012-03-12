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

    public class Credential
    {
        public static Credential CreateBasic(string user, string password)
        {
            var enc = Encoding.Default;
            return new Credential("Basic", Convert.ToBase64String(enc.GetBytes(user + ":" + password)));
        }
        public Credential()
        {
        }

        public Credential(string scheme, params string[] parameters)
        {
            this.Scheme = scheme;
            foreach (var p in parameters)
            {
                this.Parameters.Add(p);
            }
        }

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
                parameters = value;
            }
        }
        public string Scheme
        {
            get;
            set;
        }

        public static Credential Parse(string value)
        {
            string scheme;
            Collection<string> parameters;
            AuthenticationHelper.Parse(value, out scheme, out parameters);

            return new Credential()
                {
                    Scheme = scheme,
                    Parameters = parameters,
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
