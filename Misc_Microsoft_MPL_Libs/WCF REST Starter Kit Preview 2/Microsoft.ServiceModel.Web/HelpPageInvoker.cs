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
using System.Xml.Serialization;
using System.Collections.Specialized;

namespace Microsoft.ServiceModel.Web
{
    class HelpPageInvoker : IOperationInvoker
    {
        public const string OperationName = "HelpPageInvoke";

        public const string AllOperationsTemplate = "help";
        public const string OperationRequestSchemaTemplate = "help/{operation}/request/schema";
        public const string OperationRequestExampleTemplate = "help/{operation}/request/example";
        public const string OperationResponseSchemaTemplate = "help/{operation}/response/schema";
        public const string OperationResponseExampleTemplate = "help/{operation}/response/example";

        string feedId = "urn:uuid:" + Guid.NewGuid().ToString();
        DateTime startupTime = DateTime.UtcNow;

        public ContractDescription Description { get; set; }

        public Uri BaseUri { get; set; }

        public WebHttpBehavior Behavior { get; set; }

        public IDispatchOperationSelector GetHelpPageOperationSelector()
        {
            return new HelpPageOperationSelector(this.BaseUri);
        }

        void GetWebGetAndInvoke(OperationDescription od, out WebGetAttribute get, out WebInvokeAttribute invoke)
        {
            get = od.Behaviors.Find<WebGetAttribute>();
            invoke = od.Behaviors.Find<WebInvokeAttribute>();
            if (get == null && invoke == null)
            {
                // default is POST
                invoke = new WebInvokeAttribute();
            }
        }

        //[WebGet(UriTemplate="help")]
        public Atom10FeedFormatter GetFeed()
        {
            List<SyndicationItem> items = new List<SyndicationItem>();
            foreach (OperationDescription od in this.Description.Operations)
            {
                WebGetAttribute get;
                WebInvokeAttribute invoke;
                GetWebGetAndInvoke(od, out get, out invoke);    
                string method = GetMethod(get, invoke);
                string requestFormat = null;
                if (invoke != null)
                {
                    requestFormat = GetRequestFormat(invoke, od);
                }
                string responseFormat = GetResponseFormat(get, invoke, od);
                string uriTemplate = GetUriTemplate(get, invoke, od);
                WebMessageBodyStyle bodyStyle = GetBodyStyle(get, invoke);

                string requestSchemaLink = null;
                string responseSchemaLink = null;
                string requestExampleLink = null;
                string responseExampleLink = null;

                if (bodyStyle == WebMessageBodyStyle.Bare)
                {
                    UriTemplate responseSchemaTemplate = new UriTemplate(OperationResponseSchemaTemplate);
                    responseSchemaLink = responseSchemaTemplate.BindByPosition(this.BaseUri, od.Name).AbsoluteUri;

                    UriTemplate responseExampleTemplate = new UriTemplate(OperationResponseExampleTemplate);
                    responseExampleLink = responseExampleTemplate.BindByPosition(this.BaseUri, od.Name).AbsoluteUri;
                    if (invoke != null)
                    {
                        UriTemplate requestSchemaTemplate = new UriTemplate(OperationRequestSchemaTemplate);
                        requestSchemaLink = requestSchemaTemplate.BindByPosition(this.BaseUri, od.Name).AbsoluteUri;

                        UriTemplate requestExampleTemplate = new UriTemplate(OperationRequestExampleTemplate);
                        requestExampleLink = requestExampleTemplate.BindByPosition(this.BaseUri, od.Name).AbsoluteUri;
                    }
                }

                uriTemplate = String.Format("{0}/{1}", this.BaseUri.AbsoluteUri, uriTemplate);
                uriTemplate = HttpUtility.HtmlEncode(uriTemplate);

                string xhtmlDescription = String.Format("<div xmlns=\"http://www.w3.org/1999/xhtml\"><table border=\"5\"><tr><td>UriTemplate</td><td>{0}</td></tr><tr><td>Method</td><td>{1}</td></tr>", uriTemplate, method);
                if (!string.IsNullOrEmpty(requestFormat))
                {
                    xhtmlDescription += String.Format("<tr><td>Request Format</td><td>{0}</td></tr>", requestFormat);
                }
                if (requestSchemaLink != null)
                {
                    xhtmlDescription += String.Format("<tr><td>Request Schema</td><td><a href=\"{0}\">{0}</a></td></tr>", HttpUtility.HtmlEncode(requestSchemaLink));
                }
                if (requestExampleLink != null)
                {
                    xhtmlDescription += String.Format("<tr><td>Request Example</td><td><a href=\"{0}\">{0}</a></td></tr>", HttpUtility.HtmlEncode(requestExampleLink));
                }
                xhtmlDescription += String.Format("<tr><td>Response Format</td><td>{0}</td></tr>", responseFormat);
                if (responseSchemaLink != null)
                {
                    xhtmlDescription += String.Format("<tr><td>Response Schema</td><td><a href=\"{0}\">{0}</a></td></tr>", HttpUtility.HtmlEncode(responseSchemaLink));
                }
                if (responseExampleLink != null)
                {
                    xhtmlDescription += String.Format("<tr><td>Response Example</td><td><a href=\"{0}\">{0}</a></td></tr>", HttpUtility.HtmlEncode(responseExampleLink));
                }
                WebHelpAttribute help = od.Behaviors.Find<WebHelpAttribute>();
                if (help != null && !string.IsNullOrEmpty(help.Comment))
                {
                    xhtmlDescription += String.Format("<tr><td>Description</td><td>{0}</td></tr>", help.Comment);
                }
                xhtmlDescription += "</table></div>";
                SyndicationItem item = new SyndicationItem()
                {
                    Id = "http://tmpuri.org/" + od.Name,
                    Content = new TextSyndicationContent(xhtmlDescription, TextSyndicationContentKind.XHtml),
                    LastUpdatedTime = DateTime.UtcNow,
                    Title = new TextSyndicationContent(String.Format("{0}: {1}", Description.Name, od.Name)),
                };
                items.Add(item);
            }

            SyndicationFeed feed = new SyndicationFeed()
            {
                Title = new TextSyndicationContent("Service help page"),
                Id = feedId,
                LastUpdatedTime = DateTime.UtcNow,
                Items = items
            };
            WebOperationContext.Current.OutgoingResponse.ContentType = "application/atom+xml";
            return feed.GetAtom10Formatter();
        }

        static bool IsUntypedMessage(MessageDescription message)
        {
            if (message == null)
            {
                return false;
            }
            return (message.Body.ReturnValue != null && message.Body.Parts.Count == 0 && message.Body.ReturnValue.Type == typeof(Message)) ||
                (message.Body.ReturnValue == null && message.Body.Parts.Count == 1 && message.Body.Parts[0].Type == typeof(Message));
        }

        private string GetUriTemplate(WebGetAttribute get, WebInvokeAttribute invoke, OperationDescription od)
        {
            if (get != null)
            {
                if (get.UriTemplate != null)
                {
                    return get.UriTemplate;
                }
                else
                {
                    StringBuilder sb = new StringBuilder(od.Name);
                    if (!IsUntypedMessage(od.Messages[0]))
                    {
                        sb.Append("?");
                        foreach (MessagePartDescription mpd in od.Messages[0].Body.Parts)
                        {
                            string parameterName = mpd.Name;
                            sb.Append(parameterName);
                            sb.Append("={");
                            sb.Append(parameterName);
                            sb.Append("}&");
                        }
                        sb.Remove(sb.Length - 1, 1);
                    }
                    return sb.ToString();
                }
            }
            if (invoke.UriTemplate != null)
            {
                return invoke.UriTemplate;
            }
            else
            {
                return od.Name;
            }
        }

        private string GetResponseFormat(WebGetAttribute get, WebInvokeAttribute invoke, OperationDescription od)
        {
            if (IsResponseStream(od))
            {
                return "binary";
            }
            if (get != null && get.IsResponseFormatSetExplicitly) return get.ResponseFormat.ToString();
            if (invoke != null && invoke.IsResponseFormatSetExplicitly) return invoke.ResponseFormat.ToString();
            return this.Behavior.DefaultOutgoingResponseFormat.ToString();
        }

        private string GetRequestFormat(WebInvokeAttribute invoke, OperationDescription od)
        {
            if (IsRequestStream(od))
            {
                return "binary";
            }
            return "xml or json";
        }

        private bool IsResponseStream(OperationDescription od)
        {
            foreach (MessageDescription message in od.Messages)
            {
                if (message.Direction == MessageDirection.Output)
                {
                    if (message.Body.ReturnValue != null && message.Body.Parts.Count == 0)
                    {
                        return (message.Body.ReturnValue != null && message.Body.ReturnValue.Type == typeof(Stream));
                    }
                }
            }
            return false;
        }

        private bool IsRequestStream(OperationDescription od)
        {
            foreach (MessageDescription message in od.Messages)
            {
                if (message.Direction == MessageDirection.Input)
                {
                    if (message.Body.Parts.Count == 1)
                    {
                        return (message.Body.Parts[0].Type == typeof(Stream));
                    }
                }
            }
            return false;
        }

        private string GetMethod(WebGetAttribute get, WebInvokeAttribute invoke)
        {
            if (get != null) return "GET";
            if (invoke != null && !string.IsNullOrEmpty(invoke.Method)) return invoke.Method;
            return "POST";

        }

        Type GetRequestBodyType(OperationDescription od, out bool isXmlSerializerType)
        {
            isXmlSerializerType = (od.Behaviors.Find<XmlSerializerOperationBehavior>() != null);
            if (od.Behaviors.Find<WebGetAttribute>() != null)
            {
                return null;
            }
            WebInvokeAttribute invoke = od.Behaviors.Find<WebInvokeAttribute>();
            if (invoke == null) invoke = new WebInvokeAttribute();
            List<string> uriParameters = new List<string>();
            if (invoke.UriTemplate != null)
            {
                UriTemplate template = new UriTemplate(invoke.UriTemplate);
                foreach (string pathVariable in template.PathSegmentVariableNames)
                {
                    uriParameters.Add(pathVariable);
                }
                foreach (string queryVariable in template.QueryValueVariableNames)
                {
                    uriParameters.Add(queryVariable);
                }
            }
            if (od.Messages[0].MessageType != null) return null;
            List<Type> bodyParts = new List<Type>();
            foreach (MessagePartDescription messagePart in od.Messages[0].Body.Parts)
            {
                bool isUriPart = false;
                foreach (string var in uriParameters)
                {
                    if (String.Equals(var, messagePart.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        isUriPart = true;
                        break;
                    }
                }
                if (isUriPart)
                {
                    continue;
                }
                bodyParts.Add(messagePart.Type);
            }
            if ((bodyParts.Count == 0) || (bodyParts.Count > 1))
            {
                return null;
            }
            return bodyParts[0];
        }

        Type GetResponseBodyType(OperationDescription od, out bool isXmlSerializerType)
        {
            isXmlSerializerType = (od.Behaviors.Find<XmlSerializerOperationBehavior>() != null);
            if (od.Messages[1].MessageType != null) return null;
            if (od.Messages[1].Body.Parts.Count > 0) return null;
            return (od.Messages[1].Body.ReturnValue.Type);
        }

        Message CreateTextMessage(string message)
        {
            Message result = Message.CreateMessage(MessageVersion.None, null, new TextBodyWriter(message));
            result.Properties[WebBodyFormatMessageProperty.Name] = new WebBodyFormatMessageProperty(WebContentFormat.Raw);
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/plain";
            return result;
        }

        bool IsBodySpecial(Type body, string direction, out Message message)
        {
            message = null;
            if (body == null || body == typeof(void))
            {
                message = CreateTextMessage(String.Format("The {0} body is empty.", direction));
            }
            else if (body == typeof(Stream))
            {
                message = CreateTextMessage(String.Format("The {0} body is a byte stream. See the service documentation for allowed content types.", direction));
            }
            else if (typeof(Atom10FeedFormatter).IsAssignableFrom(body))
            {
                message = CreateTextMessage(String.Format("The {0} body is an Atom 1.0 syndication feed. See http://tools.ietf.org/html/rfc4287 for more details.", direction));
            }
            else if (typeof(Atom10ItemFormatter).IsAssignableFrom(body))
            {
                message = CreateTextMessage(String.Format("The {0} body is an Atom 1.0 syndication entry. See http://tools.ietf.org/html/rfc4287 for more details.", direction));
            }
            else if (typeof(AtomPub10ServiceDocumentFormatter).IsAssignableFrom(body))
            {
                message = CreateTextMessage(String.Format("The {0} body is an Atom Pub service document. See http://www.rfc-editor.org/rfc/rfc5023.txt for more details.", direction));
            }
            else if (typeof(AtomPub10CategoriesDocumentFormatter).IsAssignableFrom(body))
            {
                message = CreateTextMessage(String.Format("The {0} body is an Atom Pub categories document. See http://www.rfc-editor.org/rfc/rfc5023.txt for more details.", direction));
            }
            else if (typeof(Rss20FeedFormatter).IsAssignableFrom(body))
            {
                message = CreateTextMessage(String.Format("The {0} body is an RSS 2.0 syndication feed. See http://validator.w3.org/feed/docs/rss2.html for more details.", direction));
            }
            else if (typeof(NameValueCollection).IsAssignableFrom(body))
            {
                message = CreateTextMessage(String.Format("The {0} body is a HTML Forms data.", direction));
            }
            else if (typeof(XElement).IsAssignableFrom(body) || typeof(XmlElement).IsAssignableFrom(body))
            {
                message = CreateTextMessage(String.Format("The {0} body is arbitrary XML. See the service documentation for conformant XML documents.", direction));
            }
            return (message != null);
        }

        Message CreateSchema(Type body, bool isXmlSerializerType)
        {
            System.Collections.IEnumerable schemas;
            if (isXmlSerializerType)
            {
                XmlReflectionImporter importer = new XmlReflectionImporter();
                XmlTypeMapping typeMapping = importer.ImportTypeMapping(body);
                XmlSchemas s = new XmlSchemas();
                XmlSchemaExporter exporter = new XmlSchemaExporter(s);
                exporter.ExportTypeMapping(typeMapping);
                schemas = s.GetSchemas(null);
            }
            else
            {
                XsdDataContractExporter exporter = new XsdDataContractExporter();
                exporter.Export(body);
                schemas = exporter.Schemas.Schemas();
            }
            using (MemoryStream stream = new MemoryStream())
            {
                XmlWriterSettings xws = new XmlWriterSettings() { Indent = true };
                using (XmlWriter w = XmlWriter.Create(stream, xws))
                {
                    w.WriteStartElement("Schemas");
                    foreach (XmlSchema schema in schemas)
                    {
                        if (schema.TargetNamespace != "http://www.w3.org/2001/XMLSchema")
                        {
                            schema.Write(w);
                        }
                    }
                }
                stream.Seek(0, SeekOrigin.Begin);
                using (XmlReader reader = XmlReader.Create(stream))
                {
                    return Message.CreateMessage(MessageVersion.None, null, XElement.Load(reader, LoadOptions.PreserveWhitespace));
                }
            }
        }

        WebMessageBodyStyle GetBodyStyle(WebGetAttribute get, WebInvokeAttribute invoke)
        {
            if (get != null)
            {
                return get.BodyStyle;
            }
            return invoke.BodyStyle;
        }

        //[WebGet(UriTemplate = "help/{operation}/request/schema")]
        public Message GetRequestXmlSchema(string operation)
        {
            foreach (OperationDescription od in this.Description.Operations)
            {
                if (od.Name == operation)
                {
                    
                    bool isXmlSerializerType;
                    Type body = GetRequestBodyType(od, out isXmlSerializerType);
                    Message result;
                    if (IsBodySpecial(body, "request", out result))
                    {
                        return result;
                    }
                    try
                    {
                        return CreateSchema(body, isXmlSerializerType);
                    }
                    catch (Exception e)
                    {
                        return CreateTextMessage(String.Format("Could not generate schema for request. Failed with error: {0}", e.Message));
                    }
                }
            }
            return null;
        }

        Message CreateExample(Type type, OperationDescription od, bool generateJson)
        {
            bool usesXmlSerializer = od.Behaviors.Contains(typeof(XmlSerializerOperationBehavior));
            XmlQualifiedName name;
            XmlSchemaSet schemaSet = new XmlSchemaSet();
            IDictionary<XmlQualifiedName, Type> knownTypes = new Dictionary<XmlQualifiedName, Type>();
            if (usesXmlSerializer)
            {
                XmlReflectionImporter importer = new XmlReflectionImporter();
                XmlTypeMapping typeMapping = importer.ImportTypeMapping(type);
                name = new XmlQualifiedName(typeMapping.ElementName, typeMapping.Namespace);
                XmlSchemas schemas = new XmlSchemas();
                XmlSchemaExporter exporter = new XmlSchemaExporter(schemas);
                exporter.ExportTypeMapping(typeMapping);
                foreach (XmlSchema schema in schemas)
                {
                    schemaSet.Add(schema);
                }
            }
            else
            {
                XsdDataContractExporter exporter = new XsdDataContractExporter();
                List<Type> listTypes = new List<Type>(od.KnownTypes);
                listTypes.Add(type);
                exporter.Export(listTypes);
                if (!exporter.CanExport(type))
                {
                    throw new NotSupportedException(String.Format("Example generation is not supported for type '{0}'", type));
                }
                name = exporter.GetRootElementName(type);
                foreach (Type knownType in od.KnownTypes)
                {
                    XmlQualifiedName knownTypeName = exporter.GetSchemaTypeName(knownType);
                    if (!knownTypes.ContainsKey(knownTypeName))
                    {
                        knownTypes.Add(knownTypeName, knownType);
                    }
                }

                foreach (XmlSchema schema in exporter.Schemas.Schemas())
                {
                    schemaSet.Add(schema);
                }
            }
            schemaSet.Compile();

            XmlWriterSettings settings = new XmlWriterSettings
            {
                CloseOutput = false,
                Indent = true,
            };

            if (generateJson)
            {
                var jsonExample = new XDocument();
                using (XmlWriter writer = XmlWriter.Create(jsonExample.CreateWriter(), settings))
                {
                    HelpExampleGenerator.GenerateJsonSample(schemaSet, name, writer, knownTypes);
                }
                var reader = jsonExample.CreateReader();
                reader.MoveToContent();
                var message = Message.CreateMessage(MessageVersion.None, (string)null, reader);
                WebOperationContext.Current.OutgoingResponse.ContentType = "text/plain";
                message.Properties[WebBodyFormatMessageProperty.Name] = new WebBodyFormatMessageProperty(WebContentFormat.Json);
                return message;
            }
            else
            {
                var xmlExample = new XDocument();
                using (XmlWriter writer = XmlWriter.Create(xmlExample.CreateWriter(), settings))
                {
                    HelpExampleGenerator.GenerateXmlSample(schemaSet, name, writer);
                }
                var reader = xmlExample.CreateReader();
                reader.MoveToContent();
                var message = Message.CreateMessage(MessageVersion.None, (string)null, reader);
                message.Properties[WebBodyFormatMessageProperty.Name] = new WebBodyFormatMessageProperty(WebContentFormat.Xml);
                WebOperationContext.Current.OutgoingResponse.ContentType = "text/xml";
                return message;
            }
        }

        //[WebGet(UriTemplate = "help{operation}/request/example")]
        public Message GetRequestExample(string operation)
        {
            foreach (OperationDescription od in this.Description.Operations)
            {
                if (od.Name == operation)
                {
                    bool isXmlSerializerType;
                    Type body = GetRequestBodyType(od, out isXmlSerializerType);
                    Message result;
                    if (IsBodySpecial(body, "request", out result))
                    {
                        return result;
                    }

                    WebInvokeAttribute invoke = od.Behaviors.Find<WebInvokeAttribute>();
                    bool generateJson = false;
                    if (GetResponseFormat(null, invoke, od) == "Json")
                    {
                        generateJson = true;
                    }
                    try
                    {
                        return CreateExample(body, od, generateJson);
                    }
                    catch (Exception e)
                    {
                        return CreateTextMessage(String.Format("Could not generate example for request. Failed with error: {0}", e.Message));
                    }
                }
            }
            return null;
        }

        //[WebGet(UriTemplate = "help/{operation}/response/schema")]
        public Message GetResponseXmlSchema(string operation)
        {
            foreach (OperationDescription od in this.Description.Operations)
            {
                if (od.Name == operation)
                {
                    bool isXmlSerializerType;
                    Type body = GetResponseBodyType(od, out isXmlSerializerType);
                    Message result;
                    if (IsBodySpecial(body, "response", out result))
                    {
                        return result;
                    }
                    try
                    {
                        return CreateSchema(body, isXmlSerializerType);
                    }
                    catch (Exception e)
                    {
                        return CreateTextMessage(String.Format("Could not generate schema for response. Failed with error: {0}", e.Message));
                    }
                }
            }
            return null;
        }

        //[WebGet(UriTemplate = "help{operation}/response/example")]
        public Message GetResponseExample(string operation)
        {
            foreach (OperationDescription od in this.Description.Operations)
            {
                if (od.Name == operation)
                {
                    bool isXmlSerializerType;
                    Type body = GetResponseBodyType(od, out isXmlSerializerType);
                    Message result;
                    if (IsBodySpecial(body, "response", out result))
                    {
                        return result;
                    }
                    bool generateJson = false;
                    if (GetResponseFormat(od.Behaviors.Find<WebGetAttribute>(), od.Behaviors.Find<WebInvokeAttribute>(), od) == "Json")
                    {
                        generateJson = true;
                    }
                    try
                    {
                        return CreateExample(body, od, generateJson);
                    }
                    catch (Exception e)
                    {
                        return CreateTextMessage(String.Format("Could not generate example for response. Failed with error: {0}", e.Message));
                    }
                }
            }
            return null;
        }

        string GetDefaultInvokeMethod()
        {
            return "POST";
        }

        WebMessageFormat GetDefaultResponseFormat()
        {
            return WebMessageFormat.Xml;
        }

        string GetDefaultGetTemplate(OperationDescription od)
        {
            return null;
        }

        void CacheValidationCallback(HttpContext context, object state, ref HttpValidationStatus result)
        {
            if (((DateTime)state) == this.startupTime)
            {
                result = HttpValidationStatus.Valid;
            }
            else
            {
                result = HttpValidationStatus.Invalid;
            }
        }

        void CacheResult()
        {
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.Public);
                HttpContext.Current.Response.Cache.SetMaxAge(TimeSpan.MaxValue);
                HttpContext.Current.Response.Cache.AddValidationCallback(new HttpCacheValidateHandler(this.CacheValidationCallback), this.startupTime);
                HttpContext.Current.Response.Cache.SetValidUntilExpires(true);
            }
        }

        #region IOperationInvoker Members

        public object[] AllocateInputs()
        {
            return new object[] { null };
        }

        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {
            outputs = null;
            Message result = null;
            try
            {
                UriTemplateMatch match = (UriTemplateMatch)OperationContext.Current.IncomingMessageProperties["HelpPageMatch"];
                string operation = (string)match.Data;
                if (operation == "GetFeed")
                {
                    Atom10FeedFormatter feed = GetFeed();
                    WebOperationContext.Current.OutgoingResponse.ContentType = "application/atom+xml";
                    result = Message.CreateMessage(MessageVersion.None, null, feed);
                }
                else if (operation == "GetRequestSchema")
                {
                    result = GetRequestXmlSchema(match.BoundVariables["operation"]);
                }
                else if (operation == "GetRequestExample")
                {
                    result = GetRequestExample(match.BoundVariables["operation"]);
                }
                else if (operation == "GetResponseSchema")
                {
                    result = GetResponseXmlSchema(match.BoundVariables["operation"]);
                }
                else if (operation == "GetResponseExample")
                {
                    result = GetResponseExample(match.BoundVariables["operation"]);
                }
                else
                {
                    WebOperationContext.Current.OutgoingResponse.SetStatusAsNotFound();
                    result = Message.CreateMessage(MessageVersion.None, null);
                }
                if (result != null)
                {
                    CacheResult();
                }
                else
                {
                    WebOperationContext.Current.OutgoingResponse.SetStatusAsNotFound();
                }
            }
            catch (Exception e)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
                WebOperationContext.Current.OutgoingResponse.StatusDescription = e.Message;
            }
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

        #endregion

        class TextBodyWriter : BodyWriter
        {
            byte[] messageBytes;

            public TextBodyWriter(string message)
                : base(true)
            {
                this.messageBytes = Encoding.UTF8.GetBytes(message);
            }

            protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
            {
                writer.WriteStartElement("Binary");
                writer.WriteBase64(this.messageBytes, 0, this.messageBytes.Length);
                writer.WriteEndElement();
            }
        }

        class HelpPageOperationSelector : IDispatchOperationSelector
        {
            UriTemplateTable table;

            public HelpPageOperationSelector(Uri baseUri)
            {
                List<KeyValuePair<UriTemplate, object>> templateList = new List<KeyValuePair<UriTemplate, object>>();
                templateList.Add(new KeyValuePair<UriTemplate, object>(new UriTemplate(HelpPageInvoker.AllOperationsTemplate), "GetFeed"));
                templateList.Add(new KeyValuePair<UriTemplate, object>(new UriTemplate(HelpPageInvoker.OperationRequestExampleTemplate), "GetRequestExample"));
                templateList.Add(new KeyValuePair<UriTemplate, object>(new UriTemplate(HelpPageInvoker.OperationRequestSchemaTemplate), "GetRequestSchema"));
                templateList.Add(new KeyValuePair<UriTemplate, object>(new UriTemplate(HelpPageInvoker.OperationResponseExampleTemplate), "GetResponseExample"));
                templateList.Add(new KeyValuePair<UriTemplate, object>(new UriTemplate(HelpPageInvoker.OperationResponseSchemaTemplate), "GetResponseSchema"));
                table = new UriTemplateTable(baseUri, templateList);
                table.MakeReadOnly(false);
            }

            public string SelectOperation(ref Message message)
            {
                if (message == null)
                {
                    return string.Empty;
                }
                object o;
                if (!message.Properties.TryGetValue(HttpRequestMessageProperty.Name, out o))
                {
                    return string.Empty;
                }
                HttpRequestMessageProperty prop = (HttpRequestMessageProperty)o;
                if (prop.Method != "GET")
                {
                    return string.Empty;
                }

                UriTemplateMatch match = table.MatchSingle(message.Properties.Via);
                if (match == null)
                {
                    return string.Empty;
                }
                message.Properties["HelpPageMatch"] = match;
                return HelpPageInvoker.OperationName;
            }
        }
    }
}