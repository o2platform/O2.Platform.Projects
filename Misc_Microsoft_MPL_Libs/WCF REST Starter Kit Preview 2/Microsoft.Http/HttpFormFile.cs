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


    public sealed class HttpFormFile
    {
        public HttpContent Content
        {
            get;
            set;
        }
        public string FileName
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
    }
}
