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
    using System.Web;
    
    class WebErrorHandler : IErrorHandler
    {
        public bool EnableAspNetCustomErrors { get; set; }

        public bool IncludeExceptionDetailInFaults { get; set; }

        public bool HandleError(Exception error)
        {
            return true;
        }

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            WebProtocolException webError = error as WebProtocolException;
            string errorMessage = (this.IncludeExceptionDetailInFaults) ? error.Message : "The server encountered an error processing the request. Please see the server logs for more details.";
            if (webError == null)
            {
                if (error is SecurityAccessDeniedException) webError = new WebProtocolException(HttpStatusCode.Unauthorized, errorMessage, error);
                else if (error is ServerTooBusyException) webError = new WebProtocolException(HttpStatusCode.ServiceUnavailable, errorMessage, error);
                else if (error is FaultException)
                {
                    FaultException fe = error as FaultException;
                    if (fe.Code.IsSenderFault)
                    {
                        if (fe.Code.SubCode.Name == "FailedAuthentication")
                        {
                            webError = new WebProtocolException(HttpStatusCode.Unauthorized, fe.Reason.Translations[0].Text, fe);
                        }
                        else
                        {
                            webError = new WebProtocolException(HttpStatusCode.BadRequest, fe.Reason.Translations[0].Text, fe);
                        }
                    }
                    else
                    {
                        webError = new WebProtocolException(HttpStatusCode.InternalServerError, fe.Reason.Translations[0].Text, fe);
                    }
                }
                else
                {
                    webError = new WebProtocolException(HttpStatusCode.InternalServerError, errorMessage, error);
                }
            }
            if (version == MessageVersion.None)
            {
                WebMessageFormat format = WebMessageFormat.Xml;
                object dummy;
                if (OperationContext.Current.IncomingMessageProperties.TryGetValue(ResponseWebFormatPropertyAttacher.PropertyName, out dummy))
                {
                    format = (WebMessageFormat) dummy;
                }
                fault = Message.CreateMessage(MessageVersion.None, null, new ErrorBodyWriter() { Error = webError, Format = format });
                HttpResponseMessageProperty prop = new HttpResponseMessageProperty();
                prop.StatusCode = webError.StatusCode;
                prop.StatusDescription = webError.StatusDescription;
                if (format == WebMessageFormat.Json)
                {
                    prop.Headers[HttpResponseHeader.ContentType] = "application/json";
                }
                else if (webError.IsDetailXhtml)
                {
                    prop.Headers[HttpResponseHeader.ContentType] = "text/html";
                }
                fault.Properties[HttpResponseMessageProperty.Name] = prop;
                WebBodyFormatMessageProperty formatProp = new WebBodyFormatMessageProperty((format == WebMessageFormat.Json) ? WebContentFormat.Json : WebContentFormat.Xml);
                fault.Properties[WebBodyFormatMessageProperty.Name] = formatProp;
            }
            if (this.EnableAspNetCustomErrors && HttpContext.Current != null)
            {
                HttpContext.Current.AddError(error);
            }
        }

        class ErrorBodyWriter : BodyWriter
        {
            public ErrorBodyWriter()
                : base(true)
            {
            }

            public WebProtocolException Error { get; set; }

            public WebMessageFormat Format { get; set; }

            protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
            {
                Error.WriteDetail(writer, this.Format);
            }
        }
    }
}
