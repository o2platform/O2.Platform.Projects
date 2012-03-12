//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace Microsoft.ServiceModel.Web
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.Net;
    using System.Runtime.Serialization;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Channels;
    using System.Xml;
    using System.ServiceModel.Security;
    using System.ServiceModel.Web;
    
    class ResponseWebFormatPropertyAttacher : IParameterInspector
    {
        public const string PropertyName = "ResponseWebFormat";

        public WebMessageFormat Format { get; set; }

        public void AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
        {
        }

        public object BeforeCall(string operationName, object[] inputs)
        {
            OperationContext.Current.IncomingMessageProperties[PropertyName] = this.Format;
            return null;
        }
    }
}
