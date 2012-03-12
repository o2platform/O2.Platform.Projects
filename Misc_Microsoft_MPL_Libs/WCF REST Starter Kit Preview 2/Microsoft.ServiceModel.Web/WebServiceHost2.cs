//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace Microsoft.ServiceModel.Web
{
    using System.Threading;
    using System.ServiceModel.Diagnostics;
    using System.Diagnostics;
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Web;
    using System.Xml;
    using System.Text;
    using System.Web;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Web.Security;

    public class WebServiceHost2 : WebServiceHost
    {
        Collection<RequestInterceptor> interceptors = new Collection<RequestInterceptor>();

        public WebServiceHost2(object singletonInstance, params Uri[] baseAddresses)
            : base(singletonInstance, baseAddresses)
        {
            this.EnableAutomaticHelpPage = true;
            this.HelpPageLink = HelpPageInvoker.AllOperationsTemplate;
            this.PrincipalPermissionMode = PrincipalPermissionMode.UseAspNetRoles;
            this.TransferMode = TransferMode.Buffered;
        }

        public WebServiceHost2(Type serviceType, bool dummy, params Uri[] baseAddresses)
            : base(serviceType, baseAddresses)
        {
            this.EnableAutomaticHelpPage = true;
            this.HelpPageLink = HelpPageInvoker.AllOperationsTemplate;
            this.PrincipalPermissionMode = PrincipalPermissionMode.UseAspNetRoles;
            this.TransferMode = TransferMode.Buffered;
        }

        #region WebServiceHost settings

        public TransferMode TransferMode { get; set; }

        public Collection<RequestInterceptor> Interceptors { get { return this.interceptors; } }

        public long MaxMessageSize { get; set; }

        public XmlDictionaryReaderQuotas ReaderQuotas { get; set; }

        public int MaxConcurrentCalls { get; set; }

        public bool EnableAutomaticHelpPage { get; set; }

        public string HelpPageLink { get; set; }

        public bool EnableAspNetCustomErrors { get; set; }

        public PrincipalPermissionMode PrincipalPermissionMode { get; set; }

        #endregion

        protected override void OnOpening()
        {
            base.OnOpening();
            foreach (var ep in this.Description.Endpoints)
            {
                if (ep.Behaviors.Find<WebHttpBehavior>() != null)
                {
                    ep.Behaviors.Remove<WebHttpBehavior>();
                    ep.Behaviors.Add(new WebHttpBehavior2() { EnableAspNetCustomErrors = this.EnableAspNetCustomErrors, EnableAutomaticHelpPage = this.EnableAutomaticHelpPage, HelpPageLink = this.HelpPageLink });
                }

                CustomBinding binding = new CustomBinding(ep.Binding);
                if (this.MaxMessageSize != 0)
                {
                    binding.Elements.Find<TransportBindingElement>().MaxReceivedMessageSize = this.MaxMessageSize;
                }
                if (this.TransferMode != TransferMode.Buffered)
                {
                    binding.Elements.Find<HttpTransportBindingElement>().TransferMode = this.TransferMode;
                }
                if (this.ReaderQuotas != null)
                {
                    this.ReaderQuotas.CopyTo(binding.Elements.Find<TextMessageEncodingBindingElement>().ReaderQuotas);
                }
                if (this.Interceptors.Count > 0)
                {
                    binding.Elements.Insert(0, new RequestInterceptorBindingElement(this.Interceptors));
                }
                ep.Binding =  binding;
            }
            if (this.MaxConcurrentCalls != 0)
            {
                ServiceThrottlingBehavior throttlingBehavior = this.Description.Behaviors.Find<ServiceThrottlingBehavior>();
                if (throttlingBehavior == null)
                {
                    throttlingBehavior = new ServiceThrottlingBehavior();
                    this.Description.Behaviors.Add(throttlingBehavior);
                }
                throttlingBehavior.MaxConcurrentCalls = this.MaxConcurrentCalls;
            }
            ServiceAuthorizationBehavior authz = this.Description.Behaviors.Find<ServiceAuthorizationBehavior>();
            authz.PrincipalPermissionMode = this.PrincipalPermissionMode;
            if (authz.PrincipalPermissionMode == PrincipalPermissionMode.UseAspNetRoles && authz.RoleProvider == null && Roles.Enabled)
            {
                authz.RoleProvider = Roles.Provider;
            }
        }
    }
}
