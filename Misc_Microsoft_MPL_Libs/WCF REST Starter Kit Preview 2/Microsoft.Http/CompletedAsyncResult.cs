//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Reflection;
    using System.Diagnostics;


    class CompletedAsyncResult : AsyncResult
    {
        public CompletedAsyncResult(AsyncCallback callback, object state)
            : base(callback, state)
        {
            base.Complete(true);
        }

        public static void End(IAsyncResult result)
        {
            Debug.Assert(result.IsCompleted);
            AsyncResult.End<CompletedAsyncResult>(result);
        }
    }
}
