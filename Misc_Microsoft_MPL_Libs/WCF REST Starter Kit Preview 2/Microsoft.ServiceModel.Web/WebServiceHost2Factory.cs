//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace Microsoft.ServiceModel.Web
{
    using System.Web;
    using System.Web.Hosting;
    using System.IO;
    using System.ServiceModel.Web;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Activation;
    using System.Web.Compilation;
    using System.Reflection;
    using System;
    using System.ServiceModel;
    using System.Collections.ObjectModel;

    /*public class WebServiceHost2Factory : ServiceHostFactory
    {
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            return new WebServiceHost2(serviceType, true, baseAddresses);
        }
    }*/
}