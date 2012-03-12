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


    public enum TransferCoding
    {
        Chunked,
        Identity,
        GZip,
        Compress,
        Deflate
    }

    // bytes n-m/length
}
