//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace Microsoft.ServiceModel.Web
{
    using System.Threading;
    using System.ServiceModel.Diagnostics;
    using System.Diagnostics;
    using System;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Web;
    using System.Xml;
    using System.Text;
    using System.Web;
    using System.Collections.Specialized;
    using System.Security;
    using System.Security.Principal;
    using System.Web.Configuration;

    public class WebHttpBehavior2 : WebHttpBehavior
    {
        public WebHttpBehavior2()
            : base()
        {
            this.EnableAutomaticHelpPage = true;
            this.HelpPageLink = HelpPageInvoker.AllOperationsTemplate;
        }

        public bool EnableAutomaticHelpPage { get; set; }

        public string HelpPageLink { get; set; }

        public bool EnableAspNetCustomErrors { get; set; }

        protected override IDispatchMessageFormatter GetRequestDispatchFormatter(OperationDescription operationDescription, ServiceEndpoint endpoint)
        {
            IDispatchMessageFormatter inner = base.GetRequestDispatchFormatter(operationDescription, endpoint);
            return new FormsPostDispatchMessageFormatter(operationDescription, inner, this.GetQueryStringConverter(operationDescription));
        }

        public override void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            base.ApplyDispatchBehavior(endpoint, endpointDispatcher);
            if (this.EnableAutomaticHelpPage)
            {
                HelpPageInvoker invoker = new HelpPageInvoker() { Description = endpoint.Contract, BaseUri = endpoint.ListenUri, Behavior = this };
                endpointDispatcher.DispatchRuntime.OperationSelector = new WrappedOperationSelector(invoker.GetHelpPageOperationSelector(), endpointDispatcher.DispatchRuntime.OperationSelector);
                // add the help page operation
                DispatchOperation helpPageOperation = new DispatchOperation(endpointDispatcher.DispatchRuntime, HelpPageInvoker.OperationName, "help", null);
                helpPageOperation.DeserializeRequest = false;
                helpPageOperation.SerializeReply = false;
                helpPageOperation.Invoker = invoker;
                endpointDispatcher.DispatchRuntime.Operations.Add(helpPageOperation);
            }
            endpointDispatcher.DispatchRuntime.Operations.Remove(endpointDispatcher.DispatchRuntime.UnhandledDispatchOperation);
            endpointDispatcher.DispatchRuntime.UnhandledDispatchOperation = new DispatchOperation(endpointDispatcher.DispatchRuntime, "*", "*", "*");
            endpointDispatcher.DispatchRuntime.UnhandledDispatchOperation.DeserializeRequest = false;
            endpointDispatcher.DispatchRuntime.UnhandledDispatchOperation.SerializeReply = false;
            endpointDispatcher.DispatchRuntime.UnhandledDispatchOperation.Invoker = new UnhandledOperationInvoker() { BaseUri = endpoint.ListenUri, HelpPageLink = this.HelpPageLink };
        }

        protected override void AddServerErrorHandlers(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            foreach (OperationDescription od in endpoint.Contract.Operations)
            {
                WebMessageFormat outgoingFormat = WebMessageFormat.Xml;
                WebGetAttribute getAttr = od.Behaviors.Find<WebGetAttribute>();
                if (getAttr != null)
                {
                    outgoingFormat = (getAttr.IsResponseFormatSetExplicitly) ? getAttr.ResponseFormat : base.DefaultOutgoingResponseFormat;
                }
                else
                {
                    WebInvokeAttribute invokeAttr = od.Behaviors.Find<WebInvokeAttribute>();
                    if (invokeAttr != null)
                    {
                        outgoingFormat = (invokeAttr.IsResponseFormatSetExplicitly) ? invokeAttr.ResponseFormat : base.DefaultOutgoingResponseFormat;
                    }
                }
                endpointDispatcher.DispatchRuntime.Operations[od.Name].ParameterInspectors.Add(new ResponseWebFormatPropertyAttacher() { Format =  outgoingFormat });
            }
            endpointDispatcher.ChannelDispatcher.ErrorHandlers.Add(new WebErrorHandler() { EnableAspNetCustomErrors = this.EnableAspNetCustomErrors, IncludeExceptionDetailInFaults = endpointDispatcher.DispatchRuntime.ChannelDispatcher.IncludeExceptionDetailInFaults });
        }
    }
}
