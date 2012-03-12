//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace Microsoft.ServiceModel.Web
{
    using System.Threading;
    using System.ServiceModel.Diagnostics;
    using System.Diagnostics;
    using System;
    using System.ServiceModel;
    using System.Net;
    using System.Xml.Linq;
    using System.Xml;
    using System.Runtime.Serialization;
    using System.IO;
    using System.Globalization;
    using System.ServiceModel.Web;
using System.Runtime.Serialization.Json;

    public class WebProtocolException : CommunicationException
    {
        DetailWriter detailWriter;

        public WebProtocolException(HttpStatusCode statusCode) : this(statusCode, GetDefaultStatusDescription(statusCode), null) { }

        public WebProtocolException(HttpStatusCode statusCode, string message, Exception innerException) : this(statusCode, message, message, innerException) { }

        public WebProtocolException(HttpStatusCode statusCode, string statusDescription, XElement detail, bool isDetailXhtml, Exception innerException)
        {
            this.StatusCode = statusCode;
            this.StatusDescription = statusDescription;
            this.IsDetailXhtml = isDetailXhtml;
            this.detailWriter = new XElementDetailWriter() { Element = detail };
        }

        public WebProtocolException(HttpStatusCode statusCode, string statusDescription, string detail, Exception innerException)
            : base(statusDescription, innerException)
        {
            this.StatusCode = statusCode;
            this.StatusDescription = statusDescription;
            this.IsDetailXhtml = true;
            this.detailWriter = new StringDetailWriter() { Detail = detail, StatusCode = statusCode };
        }

        public WebProtocolException(HttpStatusCode statusCode, string statusDescription, object dataContractDetail, Exception innerException) :
            this(statusCode, statusDescription, dataContractDetail, null, innerException)
        {
        }

        public WebProtocolException(HttpStatusCode statusCode, string statusDescription, object dataContractDetail, Func<WebMessageFormat, XmlObjectSerializer> serializerFactory, Exception innerException)
            : base(statusDescription, innerException)
        {
            this.StatusCode = statusCode;
            this.StatusDescription = statusDescription;
            if (dataContractDetail != null)
            {
                if (serializerFactory == null) 
                {
                    serializerFactory = ((format) => (format == WebMessageFormat.Json) ? (XmlObjectSerializer) (new DataContractJsonSerializer(dataContractDetail.GetType())) : (XmlObjectSerializer) (new DataContractSerializer(dataContractDetail.GetType())));
                }
            }
            this.detailWriter = new DataContractDetailWriter() { Detail = dataContractDetail, SerializerFactory = serializerFactory };
        }

        protected WebProtocolException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public HttpStatusCode StatusCode { get; private set; }

        public string StatusDescription { get; private set; }

        public bool IsDetailXhtml { get; private set; }

        internal protected virtual void WriteDetail(XmlWriter writer, WebMessageFormat format)
        {
            this.detailWriter.WriteDetail(writer, format);
        }

        static string GetDefaultStatusDescription(HttpStatusCode statusCode)
        {
            return statusCode.ToString();
        }

        abstract class DetailWriter
        {
            public abstract void WriteDetail(XmlWriter writer, WebMessageFormat format);
        }

        class XElementDetailWriter : DetailWriter
        {
            public XElement Element { get; set; }

            public override void WriteDetail(XmlWriter writer, WebMessageFormat format)
            {
                if (this.Element != null)
                {
                    this.Element.WriteTo(writer);
                }
            }
        }

        class DataContractDetailWriter : DetailWriter
        {
            public object Detail { get; set; }
            public Func<WebMessageFormat, XmlObjectSerializer> SerializerFactory { get; set; }

            public override void WriteDetail(XmlWriter writer, WebMessageFormat format)
            {
                if (this.Detail != null)
                {
                    this.SerializerFactory(format).WriteObject(writer, this.Detail);
                }
            }
        }

        class StringDetailWriter : DetailWriter
        {
            const string xhtmlFormat = "<?xml version=\"1.0\" encoding=\"utf-8\"?><html xmlns=\"http://www.w3.org/1999/xhtml\" version=\"-//W3C//DTD XHTML 2.0//EN\" xml:lang=\"en\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.w3.org/1999/xhtml http://www.w3.org/MarkUp/SCHEMA/xhtml2.xsd\"><HEAD><TITLE>Request Error</TITLE></HEAD><BODY><DIV id=\"content\"><P class=\"heading1\"><B>Error Status Code:</B> '{0}'</P><P><B>Details: </B>{1}</P><!-- Padding xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx--></DIV></BODY></html>";

            public string Detail { get; set; }
            public HttpStatusCode StatusCode { get; set; }

            public override void WriteDetail(XmlWriter writer, WebMessageFormat format)
            {
                if (format == WebMessageFormat.Xml)
                {
                    if (this.Detail != null)
                    {
                        string html = String.Format(CultureInfo.InvariantCulture, xhtmlFormat, this.StatusCode.ToString(), this.Detail);
                        XElement element = XElement.Load(new StringReader(html));
                        element.WriteTo(writer);
                    }
                }
                else
                {
                    new DataContractJsonSerializer(typeof(JsonErrorData)).WriteObject(writer, new JsonErrorData() { Detail = this.Detail });
                }
            }

            [DataContract]
            class JsonErrorData
            {
                [DataMember]
                public string Detail { get; set; }
            }
        }
    }
}
