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


    static class AuthenticationHelper
    {
        public static void Parse(string value, out string scheme, out Collection<string> parameters)
        {
            var space = value.IndexOf(' ');
            scheme = value.Substring(0, space).Trim();
            var remaining = value.Substring(space + 1).Trim();
            parameters = HeaderStore.ParseMultiValue(remaining, ',');
        }


        public static string ToString(string scheme, Collection<string> parameters)
        {
            if (string.IsNullOrEmpty(scheme) && (parameters == null || parameters.Count == 0))
            {
                return "";
            }
            if (parameters == null || parameters.Count == 0)
            {
                return scheme;
            }
            return scheme + " " + string.Join(", ", parameters.ToArray());
        }
        public static bool TryGetParameter(Collection<string> parameters, string parameter, out string value)
        {
            var lookFor = parameter + "=";
            foreach (var x in parameters)
            {
                if (x.StartsWithInvariant(lookFor))
                {
                    value = x.Substring(lookFor.Length).Trim();
                    if (value.StartsWithInvariant('"') && value.EndsWithInvariant('"'))
                    {
                        value = value.Substring(1, value.Length - 2);
                    }
                    return true;
                }
            }
            value = null;
            return false;
        }
    }
}
