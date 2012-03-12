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
    class WrappedOperationSelector : IDispatchOperationSelector
    {
        IDispatchOperationSelector[] selectors;

        public WrappedOperationSelector(params IDispatchOperationSelector[] selectors)
        {
            if (selectors != null)
            {
                this.selectors = new IDispatchOperationSelector[selectors.Length];
                for (int i = 0; i < selectors.Length; ++i)
                {
                    this.selectors[i] = selectors[i];
                }
            }
            else
            {
                this.selectors = new IDispatchOperationSelector[] { };
            }
        }
     
        public string SelectOperation(ref Message message)
        {
            for (int i = 0; i < this.selectors.Length; ++i)
            {
                string name = this.selectors[i].SelectOperation(ref message);
                if (!string.IsNullOrEmpty(name))
                {
                    return name;
                }
            }
            return string.Empty;
        }
    }
}
