//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Http
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.ServiceModel.Syndication;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Serialization;

    public static class HttpContentExtensions
    {
        static readonly string DefaultXmlContentType = "application/xml";
        static readonly Encoding DefaultXmlEncoding = Encoding.UTF8;

        public static HttpContent Create(Action<XmlWriter> writeTo, Encoding encoding, string contentType)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            Action<Stream> streamWriter = (stream) =>
            {
                using (var w = new XmlTextWriter(stream, encoding))
                {
                    writeTo(w);
                    w.Flush();
                }
            };

            return HttpContent.Create(streamWriter, contentType);
        }

        public static HttpContent Create(XElement element)
        {
            return Create(element, DefaultXmlEncoding, DefaultXmlContentType);
        }
        public static HttpContent Create(XElement element, Encoding encoding, string contentType)
        {
            return Create(element.WriteTo, encoding, contentType);
        }

        public static HttpContent CreateAtom10SyndicationFeed(SyndicationFeed feed)
        {
            return CreateAtom10SyndicationFeed(feed, DefaultXmlEncoding, "application/atom+xml");
        }

        public static HttpContent CreateAtom10SyndicationFeed(SyndicationFeed feed, Encoding encoding, string contentType)
        {
            return HttpContentExtensions.Create((writer) => feed.SaveAsAtom10(writer), encoding, contentType);
        }
        public static HttpContent CreateDataContract<T>(T value, params Type[] extraTypes)
        {
            return CreateDataContract(value, DefaultXmlEncoding, DefaultXmlContentType, extraTypes);
        }

        public static HttpContent CreateDataContract<T>(T value, Encoding encoding, string contentType, params Type[] extraTypes)
        {
            return CreateDataContract(value, new DataContractSerializer(typeof(T), extraTypes), encoding, contentType);
        }

        public static HttpContent CreateDataContract<T>(T value, DataContractSerializer serializer, Encoding encoding, string contentType)
        {
            Action<XmlWriter> action = (writer) => serializer.WriteObject(writer, value);
            return HttpContentExtensions.Create(action, encoding, contentType);
        }
        public static HttpContent CreateJsonDataContract<T>(T value, DataContractJsonSerializer serializer, Encoding encoding, string contentType)
        {
            Action<Stream> action = (stream) =>
            {
                using (var w = JsonReaderWriterFactory.CreateJsonWriter(stream, encoding, false))
                {
                    serializer.WriteObject(w, value);
                    w.Flush();
                }
                stream.Flush();
            };
            return HttpContent.Create(action, contentType);
        }

        public static HttpContent CreateJsonDataContract<T>(T value, params Type[] extraTypes)
        {
            return CreateJsonDataContract(value, Encoding.UTF8, "application/json", extraTypes);
        }

        public static HttpContent CreateJsonDataContract<T>(T value, Encoding encoding, string contentType, params Type[] extraTypes)
        {
            return CreateJsonDataContract(value, new DataContractJsonSerializer(typeof(T), extraTypes), encoding, contentType);
        }

        public static HttpContent CreateRss20SyndicationFeed(SyndicationFeed feed)
        {
            return CreateRss20SyndicationFeed(feed, DefaultXmlEncoding, "application/rss+xml");
        }

        public static HttpContent CreateRss20SyndicationFeed(SyndicationFeed feed, Encoding encoding, string contentType)
        {
            return HttpContentExtensions.Create((writer) => feed.SaveAsRss20(writer), encoding, contentType);
        }

        public static HttpContent CreateXmlSerializable<T>(T value, params Type[] extraTypes)
        {
            return CreateXmlSerializable(value, DefaultXmlEncoding, DefaultXmlContentType, extraTypes);
        }

        public static HttpContent CreateXmlSerializable<T>(T value, Encoding encoding, string contentType, params Type[] extraTypes)
        {
            return CreateXmlSerializable(value, new XmlSerializer(typeof(T), extraTypes), encoding, contentType);
        }

        public static HttpContent CreateXmlSerializable<T>(T value, XmlSerializer serializer, Encoding encoding, string contentType)
        {
            Action<XmlWriter> action = (writer) => serializer.Serialize(writer, value);
            return HttpContentExtensions.Create(action, encoding, contentType);
        }
    }

}
