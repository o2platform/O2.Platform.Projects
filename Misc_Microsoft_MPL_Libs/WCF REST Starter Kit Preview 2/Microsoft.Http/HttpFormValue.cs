//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Collections.ObjectModel;


    public sealed class HttpFormValue
    {
        // using "Name" because of http://www.w3.org/TR/html401/interact/forms.html 
        public string Name
        {
            get;
            set;
        }
        public string Value
        {
            get;
            set;
        }
    }
}
