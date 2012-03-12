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
using System.ServiceModel.Description;

namespace Microsoft.ServiceModel.Web
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class WebHelpAttribute : Attribute, IOperationBehavior
    {
        public string Comment { get; set; }

        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
        }

        public void Validate(OperationDescription operationDescription)
        {
        }
    }
}
