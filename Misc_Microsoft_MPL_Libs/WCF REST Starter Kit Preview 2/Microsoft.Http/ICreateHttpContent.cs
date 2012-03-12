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
    using System.Globalization;
    using System.IO;


    public interface ICreateHttpContent
    {
        HttpContent CreateHttpContent();
    }
}