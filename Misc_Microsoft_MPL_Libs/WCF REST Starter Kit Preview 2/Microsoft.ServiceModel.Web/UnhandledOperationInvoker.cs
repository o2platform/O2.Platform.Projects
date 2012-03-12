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
using System.ServiceModel.Syndication;
using System.Threading;
using System.ServiceModel.Web;
using System.Xml;
using System.Runtime.Serialization;
using System.Xml.Schema;
using System.Xml.Linq;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Globalization;

namespace Microsoft.ServiceModel.Web
{
    class UnhandledOperationInvoker : IOperationInvoker
    {
        public Uri BaseUri { get; set; }

        public string HelpPageLink { get; set; }

        public object[] AllocateInputs()
        {
            return new object[] { null };
        }

        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {
            Message message = inputs[0] as Message;
            // We might be here because we desire a redirect...
            Uri newLocation = null;
            Uri to = message.Headers.To;
            if (message.Properties.ContainsKey("WebHttpRedirect"))
            {
                newLocation = message.Properties["WebHttpRedirect"] as Uri;
            }
            if (newLocation != null && to != null)
            {
                // ...redirect
                Message redirectResult = Message.CreateMessage(MessageVersion.None, null);
                HttpResponseMessageProperty redirectResp = new HttpResponseMessageProperty();
                redirectResp.StatusCode = HttpStatusCode.TemporaryRedirect;
                redirectResp.Headers.Add(HttpResponseHeader.Location, newLocation.AbsoluteUri);
                redirectResp.Headers.Add(HttpResponseHeader.ContentType, "text/html");
                redirectResult.Properties.Add(HttpResponseMessageProperty.Name, redirectResp);
                outputs = null;
                return redirectResult;
            }
            // otherwise we are here to issue either a 404 or a 405
            bool uriMatched = false;
            if (message.Properties.ContainsKey("UriMatched"))
            {
                uriMatched = (bool)message.Properties["UriMatched"];
            }
            Uri helpPageUri;
            if (!Uri.TryCreate(this.HelpPageLink, UriKind.Absolute, out helpPageUri))
            {
                helpPageUri = new Uri(string.Format(CultureInfo.InvariantCulture, "{0}/{1}",  this.BaseUri.AbsoluteUri, this.HelpPageLink));
            }
            Message result = Message.CreateMessage(MessageVersion.None, null, new ErrorPageBodyWriter() { UriMatched = uriMatched, HelpPageUri = helpPageUri });
            HttpResponseMessageProperty resp = new HttpResponseMessageProperty();
            if (uriMatched)
            {
                resp.StatusCode = HttpStatusCode.MethodNotAllowed;
            }
            else
            {
                resp.StatusCode = HttpStatusCode.NotFound;
            }
            resp.Headers.Add(HttpResponseHeader.ContentType, "text/html");
            result.Properties.Add(HttpResponseMessageProperty.Name, resp);
            outputs = null;
            return result;
        }

        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
        {
            throw new NotSupportedException();
        }

        public bool IsSynchronous
        {
            get { return true; }
        }

        class ErrorPageBodyWriter : BodyWriter
        {
            const string xhtmlFormat = "<?xml version=\"1.0\" encoding=\"utf-8\"?><html xmlns=\"http://www.w3.org/1999/xhtml\" version=\"-//W3C//DTD XHTML 2.0//EN\" xml:lang=\"en\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.w3.org/1999/xhtml http://www.w3.org/MarkUp/SCHEMA/xhtml2.xsd\"><HEAD><TITLE>Request Error</TITLE></HEAD><BODY><DIV id=\"content\"><P class=\"heading1\"><B>Error Description:</B> '{0}'</P><P><B>This may be because an invalid URI or HTTP method was specified. Please see the <A HREF=\"{1}\">service help page</A> for constructing valid requests to the service.</B></P><!-- Padding xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx--></DIV></BODY></html>";

            public ErrorPageBodyWriter()
                : base(true)
            {
            }

            public bool UriMatched { get; set; }

            public Uri HelpPageUri { get; set; }

            protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
            {
                string helpPage;
                if (this.UriMatched)
                {
                    helpPage = String.Format(xhtmlFormat, "HTTP Method not allowed", this.HelpPageUri.AbsoluteUri);
                }
                else
                {
                    helpPage = String.Format(xhtmlFormat, "Resource does not exist", this.HelpPageUri.AbsoluteUri);
                }
                XElement xml = XElement.Load(new StringReader(helpPage));
                xml.WriteTo(writer);
            }
        }
    }
}