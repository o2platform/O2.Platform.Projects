/// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Activation;
using System.ServiceModel.Syndication;
using Microsoft.ServiceModel.Web;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Globalization;
using System.ComponentModel;
using System.Reflection;
using System.ServiceModel.Description;

namespace Microsoft.ServiceModel.Web.SpecializedServices
{
    public class SingletonServiceHost : WebServiceHost2
    {
        public SingletonServiceHost(object singletonInstance, params Uri[] baseAddresses)
            : base(singletonInstance, baseAddresses)
        {
        }

        public SingletonServiceHost(Type serviceType, params Uri[] baseAddresses)
            : base(serviceType, false, baseAddresses)
        {
        }

        protected override void OnOpening()
        {
            base.OnOpening();
            // Add any WebCache and WebHelp attributes specified on the derived class to the service description
            AtomPubServiceHost.CopyAttributeBehaviors(this.Description, new Type[] { typeof(WebCacheAttribute), typeof(WebHelpAttribute) }, "OnGetItem", new string[] { "GetItemInXml", "GetItemInJson" });
            AtomPubServiceHost.CopyAttributeBehaviors(this.Description, new Type[] { typeof(WebHelpAttribute) }, "OnAddItem", new string[] { "AddItemInXml", "AddItemInJson" });
            AtomPubServiceHost.CopyAttributeBehaviors(this.Description, new Type[] { typeof(WebHelpAttribute) }, "OnUpdateItem", new string[] { "UpdateItemInXml", "UpdateItemInJson" });
            AtomPubServiceHost.CopyAttributeBehaviors(this.Description, new Type[] { typeof(WebHelpAttribute) }, "OnDeleteItem", new string[] { "DeleteItem" });
        }
    }
}