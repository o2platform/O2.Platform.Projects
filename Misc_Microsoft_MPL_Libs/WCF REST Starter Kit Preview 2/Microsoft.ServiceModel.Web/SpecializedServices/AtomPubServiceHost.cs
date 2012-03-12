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
    public class AtomPubServiceHost : WebServiceHost2
    {
        public AtomPubServiceHost(object singletonInstance, params Uri[] baseAddresses)
            : base(singletonInstance, baseAddresses)
        {
            if (!(singletonInstance is AtomPubServiceBase))
            {
                throw new ArgumentException(String.Format("singletonInstance must derive from '{0}'", typeof(AtomPubServiceBase)));
            }
        }

        public AtomPubServiceHost(Type serviceType, params Uri[] baseAddresses)
            : base(serviceType, false, baseAddresses)
        {
            if (!typeof(AtomPubServiceBase).IsAssignableFrom(serviceType))
            {
                throw new ArgumentException(String.Format("serviceType must derive from '{0}'", typeof(AtomPubServiceBase)));
            }
        }

        protected override void OnOpening()
        {
            base.OnOpening();
            // Add any WebCache and WebHelp attributes specified on the derived class to the service description
            CopyAttributeBehaviors(this.Description, new Type[] { typeof(WebCacheAttribute), typeof(WebHelpAttribute) }, "GetEntries", new string[] { "GetFeed" });
            CopyAttributeBehaviors(this.Description, new Type[] { typeof(WebCacheAttribute), typeof(WebHelpAttribute) }, "GetServiceDocument", new string[] { "GetDocument" });
            CopyAttributeBehaviors(this.Description, new Type[] { typeof(WebCacheAttribute), typeof(WebHelpAttribute) }, "GetEntry", new string[] { "GetAtomEntry" });
            CopyAttributeBehaviors(this.Description, new Type[] { typeof(WebCacheAttribute), typeof(WebHelpAttribute) }, "GetMedia", new string[] { "GetMediaItem" });
        }

        internal static void CopyAttributeBehaviors(ServiceDescription description, Type[] behaviorTypes, string derivedMethodName, string[] operationNames)
        {
            Type serviceType = description.ServiceType;
            MethodInfo getEntriesMethod = serviceType.GetMethod(derivedMethodName, BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (Type behaviorType in behaviorTypes)
            {
                object[] attrs = getEntriesMethod.GetCustomAttributes(behaviorType, true);
                if (attrs != null && attrs.Length > 0)
                {
                    IOperationBehavior attrAsBehavior = (IOperationBehavior)attrs[0];
                    foreach (ServiceEndpoint endpoint in description.Endpoints)
                    {
                        foreach (OperationDescription od in endpoint.Contract.Operations)
                        {
                            if (operationNames.Contains(od.Name))
                            {
                                od.Behaviors.Remove(behaviorType);
                                od.Behaviors.Add(attrAsBehavior);
                            }
                        }
                    }
                }
            }
        }
    }
}