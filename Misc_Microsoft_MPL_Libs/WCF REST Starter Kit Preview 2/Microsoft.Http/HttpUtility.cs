//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using System.Globalization;
    using System.Diagnostics;
    using System.Net;
    using Microsoft.Http.Headers;

    static class HttpUtility
    {
        public static string ToStringInvariant(this IFormattable formattable)
        {
            return formattable.ToString(null, CultureInfo.InvariantCulture);
        }
        
        public static bool StartsWithInvariant(this string target, string value)
        {
            return target != null && target.StartsWith(value, StringComparison.OrdinalIgnoreCase);
        }
        
        public static bool EndsWithInvariant(this string target, string value)
        {
            return target != null && target.EndsWith(value, StringComparison.OrdinalIgnoreCase);
        }
        
        public static bool StartsWithInvariant(this string target, char value)
        {
            return !string.IsNullOrEmpty(target) && target[0] == value;
        }

        public static bool EndsWithInvariant(this string target, char value)
        {
            return !string.IsNullOrEmpty(target) && target[target.Length - 1] == value;
        }
    }
}
