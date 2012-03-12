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
using System.ServiceModel.Web;
using System.ServiceModel.Syndication;
using System.Security.Cryptography;
using System.IO;
using System.Xml;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Collections.Specialized;
using System.Net;

namespace Microsoft.ServiceModel.Web
{
    public static class WebOperationContextExtensions
    {
        public static Uri GetRequestUri(this IncomingWebRequestContext context)
        {
            return context.UriTemplateMatch.RequestUri;
        }

        public static void ThrowIfEtagMissingOrStale(this IncomingWebRequestContext context, string expectedEtag)
        {
            string incomingEtag = context.Headers[HttpRequestHeader.IfMatch];
            if (string.IsNullOrEmpty(incomingEtag))
            {
                throw new WebProtocolException(HttpStatusCode.BadRequest, "The If-Match header was not specified for the request", null);
            }
            if (!string.Equals(incomingEtag, expectedEtag, StringComparison.Ordinal))
            {
                throw new WebProtocolException(HttpStatusCode.Conflict, String.Format("The resource has an Etag different from '{0}'. Please get the latest copy of the resource.", incomingEtag), null);
            }
        }
        public static Uri GetBaseUri(this IncomingWebRequestContext context)
        {
            return context.UriTemplateMatch.BaseUri;
        }

        public static NameValueCollection GetQueryParameters(this IncomingWebRequestContext context)
        {
            return context.UriTemplateMatch.QueryParameters;
        }

        public static Uri BindTemplateToRequestUri(this WebOperationContext context, UriTemplate template, params string[] values)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (template == null)
            {
                throw new ArgumentNullException("template");
            }
            return template.BindByPosition(context.IncomingRequest.UriTemplateMatch.BaseUri, values);
        }

        public static string SetHashEtag<T>(this OutgoingWebResponseContext context, T entityToHash)
        {
            string etag = null;
            using (MemoryStream stream = new MemoryStream())
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                using (XmlWriter writer = XmlWriter.Create(stream))
                {
                    serializer.WriteObject(writer, entityToHash);
                }
                stream.Seek(0, SeekOrigin.Begin);
                using (SHA1 hasher = new SHA1Managed())
                {
                    byte[] hash = hasher.ComputeHash(stream);
                    etag = Convert.ToBase64String(hash);
                }
            }
            context.ETag = etag;
            return etag;
        }

        public static string SetHashEtag<T>(this OutgoingWebResponseContext context, XmlObjectSerializer serializer, T entityToHash)
        {
            string etag = null;
            using (MemoryStream stream = new MemoryStream())
            {
                using (XmlWriter writer = XmlWriter.Create(stream))
                {
                    serializer.WriteObject(writer, entityToHash);
                }
                stream.Seek(0, SeekOrigin.Begin);
                using (SHA1 hasher = new SHA1Managed())
                {
                    byte[] hash = hasher.ComputeHash(stream);
                    etag = Convert.ToBase64String(hash);
                }
            }
            context.ETag = etag;
            return etag;
        }

        public static string SetHashEtag<T>(this OutgoingWebResponseContext context, XmlSerializer serializer, T entityToHash)
        {
            string etag = null;
            using (MemoryStream stream = new MemoryStream())
            {
                using (XmlWriter writer = XmlWriter.Create(stream))
                {
                    serializer.Serialize(stream, entityToHash);
                }
                stream.Seek(0, SeekOrigin.Begin);
                using (SHA1 hasher = new SHA1Managed())
                {
                    byte[] hash = hasher.ComputeHash(stream);
                    etag = Convert.ToBase64String(hash);
                }
            }
            context.ETag = etag;
            return etag;
        }

        public static string SetHashEtag<T>(this OutgoingWebResponseContext context, BinaryFormatter formatter, T entityToHash)
        {
            string etag = null;
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, entityToHash);
                stream.Seek(0, SeekOrigin.Begin);
                using (SHA1 hasher = new SHA1Managed())
                {
                    byte[] hash = hasher.ComputeHash(stream);
                    etag = Convert.ToBase64String(hash);
                }
            }
            context.ETag = etag;
            return etag;
        }
    }

    public static class SyndicationExtensions
    {
        public static void AddSelfLink(this SyndicationFeed feed, Uri uri)
        {
            if (feed == null)
            {
                throw new ArgumentNullException("feed");
            }
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            feed.Links.Add(SyndicationLink.CreateSelfLink(uri, ContentTypes.Atom));
        }

        public static void AddEditLink(this SyndicationItem entry, Uri uri)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            entry.Links.Add(new SyndicationLink(uri, "edit", "Edit Atom entry", ContentTypes.AtomEntry, 0));
        }

        public static void AddEditMediaLink(this SyndicationItem entry, Uri uri, string contentType, long contentLength)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            entry.Links.Add(new SyndicationLink(uri, "edit-media", "Edit media item", contentType, contentLength));
        }

        public static void AddNextPageLink(this SyndicationFeed feed, Uri uri)
        {
            if (feed == null)
            {
                throw new ArgumentNullException("feed");
            }
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            feed.Links.Add(new SyndicationLink(uri, "next", "Next entries", ContentTypes.Atom, 0));
        }

        public static void AddPreviousPageLink(this SyndicationFeed feed, Uri uri)
        {
            if (feed == null)
            {
                throw new ArgumentNullException("feed");
            }
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            feed.Links.Add(new SyndicationLink(uri, "previous", "Previous entries", ContentTypes.Atom, 0));
        }
    }

    public static class SerializationExtensions
    {
        public static TObject ToObject<TObject>(this XElement xml)
        {
            using (XmlReader reader = xml.CreateReader())
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(TObject));
                return (TObject)serializer.ReadObject(reader);
            }
        }

        public static TObject ToObject<TObject>(this XElement xml, XmlObjectSerializer serializer)
        {
            using (XmlReader reader = xml.CreateReader())
            {
                return (TObject)serializer.ReadObject(reader);
            }
        }

        public static TObject ToObject<TObject>(this XElement xml, XmlSerializer serializer)
        {
            using (XmlReader reader = xml.CreateReader())
            {
                return (TObject)serializer.Deserialize(reader);
            }
        }

        public static XElement ToXml<TObject>(TObject obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(TObject));
                serializer.WriteObject(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                using (XmlReader reader = XmlReader.Create(ms))
                {
                    return XElement.Load(reader);
                }
            }
        }

        public static XElement ToXml<TObject>(TObject obj, XmlObjectSerializer serializer)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                using (XmlReader reader = XmlReader.Create(ms))
                {
                    return XElement.Load(reader);
                }
            }
        }

        public static XElement ToXml<TObject>(TObject obj, XmlSerializer serializer)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                using (XmlReader reader = XmlReader.Create(ms))
                {
                    return XElement.Load(reader);
                }
            }
        }
    }

    public static class ContentTypes
    {
        public const string Atom = "application/atom+xml";
        public const string AtomEntry = "application/atom+xml;type=entry";
        public const string AtomServiceDocument = "application/atomsvc+xml";
    }
}
