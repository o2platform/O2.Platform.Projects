//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Web;
using System.IdentityModel.Claims;
using System.ServiceModel.Security;
using System.Collections.ObjectModel;
using System.ServiceModel.Dispatcher;

namespace Microsoft.ServiceModel.Web
{
    delegate void Process(ref RequestContext context);

    public abstract class RequestInterceptor
    {
        Process process;

        protected RequestInterceptor(bool isSynchronous)
        {
            this.IsSynchronous = isSynchronous;
            process = this.ProcessRequest;
        }

        public bool IsSynchronous { get; private set; }

        public abstract void ProcessRequest(ref RequestContext requestContext);

        public virtual IAsyncResult BeginProcessRequest(RequestContext context, AsyncCallback callback, object state)
        {
            return this.process.BeginInvoke(ref context, callback, state);
        }

        public virtual RequestContext EndProcessRequest(IAsyncResult result)
        {
            RequestContext context = null;
            this.process.EndInvoke(ref context, result);
            return context;
        }
    }
}
