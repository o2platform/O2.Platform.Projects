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

namespace Microsoft.ServiceModel.Web.SpecializedServices
{
    #region Base class for AtomPub service - implements REST interface and has utility methods
    public abstract class AtomPubServiceBase
    {
        // URI template definitions for how clients can access the items.
        /// <summary>
        /// The URI template to get the Atom feed representing the collection. The URL is of the form http://<url-for-svc-file>/collection1
        /// </summary>
        public const string AllEntriesTemplate = "{collection}";
        /// <summary>
        /// The URI template to get the Atom feed representing the partial collection. The URL is of the form http://<url-for-svc-file>/collection1?startIndex=0&maxItems=100
        /// </summary>
        public const string PagedEntriesTemplate = "{collection}?page={pageNumber}";
        /// <summary>
        /// The URI template to manipulate a particular Atom Entry. The URL is of the form http://<url-for-svc-file>/collection1/item1
        /// </summary>
        public const string EntryTemplate = "{collection}/{id}";
        /// <summary>
        /// The URI template to manipulate a media item. The URL is of the form http://<url-for-svc-file>/collection1/media/item1
        /// </summary>
        public const string MediaItemTemplate = "{collection}/media/{id}";
        /// <summary>
        /// the URI template for the service document describing the collection. The URL is of the form http://<url-for-svc-file>/
        /// </summary>
        public const string DocumentTemplate = "";

        /// <summary>
        /// Specifies the maximum number of entries that are returned in the feed at a single time. If this is less than int.MaxValue, then the feed will
        /// contain a link to the next set of entries.
        /// </summary>
        protected virtual int GetMaximumEntriesInFeed(string collection)
        {
            return int.MaxValue;
        }

        /// <summary>
        /// Returns the items in the collection in the specified range. 
        /// </summary>
        /// <param name="collection">collection name</param>
        /// <param name="id">id of the entry</param>
        /// <returns></returns>
        protected abstract IEnumerable<SyndicationItem> GetEntries(string collection, int startIndex, int maxEntries, out bool hasMoreEntries);

        /// <summary>
        /// Gets the SyndicationItem corresponding to the id. Return null if it does not exist
        /// </summary>
        /// <param name="collection">collection name</param>
        /// <param name="id">id of the entry</param>
        /// <returns></returns>
        protected abstract SyndicationItem GetEntry(string collection, string id);

        /// <summary>
        /// Gets the SyndicationItem corresponding to the id. Return null if it does not exist.
        /// Set the contentType of the media item.
        /// </summary>
        /// <param name="collection">collection name</param> 
        /// <param name="id">id of the entry</param>
        /// <param name="contentType">content type of the item</param>
        /// <returns></returns>
        protected abstract Stream GetMedia(string collection, string id, out string contentType);

        /// <summary>
        /// Add the media item (represented by the stream, contentType and description) to the collection
        /// Return the id of the media item and the Atom entry representing it. If the item could not be added return null.
        /// </summary>
        /// <param name="collection">collection name</param>
        /// <param name="stream">request entity body</param>
        /// <param name="contentType">content type of request</param>
        /// <param name="description">description, as provided in the Slug header</param>
        /// <param name="location">Uri for the media entry</param>
        /// <returns></returns>
        protected abstract SyndicationItem AddMedia(string collection, Stream stream, string contentType, string description, out Uri location);

        /// <summary>
        /// Add the Atom entry to the collection. Return its id and the actual entry that was added to the collection. 
        /// If the item could not be added return null.
        /// </summary>
        /// <param name="collection">collection name</param>
        /// <param name="entry">entry to be added</param>
        /// <param name="location">URI for the added entry</param>
        /// <returns></returns>
        protected abstract SyndicationItem AddEntry(string collection, SyndicationItem entry, out Uri location);

        /// <summary>
        /// Update the Atom entry specified by the id. If none exists, return null. Return the updated Atom entry. Return null if the entry does not exist.
        /// This method must be idempotent.
        /// </summary>
        /// <param name="collection">collection name</param>
        /// <param name="id">id of the entry</param>
        /// <param name="entry">Entry to put</param>
        /// <returns></returns>
        protected abstract SyndicationItem PutEntry(string collection, string id, SyndicationItem entry);

        /// <summary>
        /// Update the media item specified by the id. Return false if no such item exists.
        /// This method must be idempotent.
        /// </summary>
        /// <param name="collection">collection name</param>
        /// <param name="id">id of the item</param>
        /// <param name="stream">new value for the media item</param>
        /// <param name="contentType">content type of the new value</param>
        /// <param name="description">description, as specifued in the Slug header</param>
        /// <returns></returns>
        protected abstract bool PutMedia(string collection, string id, Stream stream, string contentType, string description);

        /// <summary>
        /// Delete the Atom entry with the specified id. Return false if no such entry exists.
        /// This method should be idempotent.
        /// </summary>
        /// <param name="collection">collection name</param>
        /// <param name="id">id of the entry</param>
        /// <returns></returns>
        protected abstract bool DeleteEntry(string collection, string id);

        /// <summary>
        /// Delete the media item with the specified id. Return false if no such item exists.
        /// This method should be idempotent.
        /// </summary>
        /// <param name="collection">collection name</param>
        /// <param name="id">id of the entry</param>
        /// <returns></returns>
        protected abstract bool DeleteMedia(string collection, string id);

        /// <summary>
        /// Create a feed container object (containing no entries) for the input collection
        /// </summary>
        /// <param name="collection">collection name</param>
        /// <returns></returns>
        protected abstract SyndicationFeed CreateFeed(string collection);

        /// <summary>
        /// Return true if the collection name is a valid collection, false otherwise
        /// </summary>
        /// <param name="collection">collection name</param>
        /// <returns></returns>
        protected abstract bool IsValidCollection(string collection);

        /// <summary>
        /// Return the service document describing the collections hosted by the service
        /// </summary>
        /// <returns></returns>
        protected abstract ServiceDocument GetServiceDocument();

        /// <summary>
        /// Return the content types of items that can be added to the collection
        /// </summary>
        /// <param name="collection">collection name</param>
        /// <returns></returns>
        protected abstract IEnumerable<string> GetAllowedContentTypes(string collection);

        /// <summary>
        /// Return true if an item of the specified content type can be added to the collection
        /// </summary>
        protected virtual bool IsContentTypeAllowed(string collection, string contentType)
        {
            // Default content type matching logic does basic string equality. Modify the logic if needed
            foreach (string allowedContentType in this.GetAllowedContentTypes(collection))
            {
                if (allowedContentType == "*/*") return true;
                if (contentType.StartsWith(allowedContentType, StringComparison.Ordinal)) return true;
                if (allowedContentType.EndsWith("/*", StringComparison.Ordinal))
                {
                    // this case handles allowed content types like image/*
                    string prefix = allowedContentType.Substring(0, allowedContentType.Length - 1);
                    if (contentType.StartsWith(prefix, StringComparison.Ordinal)) return true;
                }
            }
            return false;
        }

        #region helper methods that set status codes, create the correct links and call the application methods
        static readonly UriTemplate allEntriesTemplate = new UriTemplate(AllEntriesTemplate);
        static readonly UriTemplate entryTemplate = new UriTemplate(EntryTemplate);
        static readonly UriTemplate mediaItemTemplate = new UriTemplate(MediaItemTemplate);
        static readonly UriTemplate pagedEntriesTemplate = new UriTemplate(PagedEntriesTemplate);

        static Uri GetEntryUri(string collection, string id)
        {
            return entryTemplate.BindByPosition(WebOperationContext.Current.IncomingRequest.UriTemplateMatch.BaseUri, collection, id);
        }

        static Uri GetMediaItemUri(string collection, string id)
        {
            return mediaItemTemplate.BindByPosition(WebOperationContext.Current.IncomingRequest.UriTemplateMatch.BaseUri, collection, id);
        }

        protected static Uri GetAllEntriesUri(string collection)
        {
            return allEntriesTemplate.BindByPosition(WebOperationContext.Current.IncomingRequest.UriTemplateMatch.BaseUri, collection);
        }

        static void SetAtomServiceDocumentContentType()
        {
            // NOTE: the official content type of a service document is application/atomsvc+xml but text/xml is set in the template to render in the browsers
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/xml";
        }

        static void SetAtomContentType()
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = ContentTypes.Atom;
        }

        static void SetAtomEntryContentType()
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = ContentTypes.AtomEntry;
        }

        static string GetDescriptionFromSlugHeader()
        {
            return WebOperationContext.Current.IncomingRequest.Headers["Slug"];
        }

        static SyndicationLink CreateEditLink(string collection, string id)
        {
            return new SyndicationLink(GetEntryUri(collection, id), "edit", "Edit Atom entry", ContentTypes.AtomEntry, 0);
        }

        static SyndicationLink CreateEditMediaLink(string collection, string id, string contentType)
        {
            return new SyndicationLink(GetMediaItemUri(collection, id), "edit-media", "Edit media item", contentType, 0);
        }

        static SyndicationLink GetPreviousPageLink(string collection, int pageNumber)
        {
            Uri feedUri;
            if (pageNumber == 0)
            {
                feedUri = allEntriesTemplate.BindByPosition(WebOperationContext.Current.IncomingRequest.UriTemplateMatch.BaseUri, collection);
            }
            else
            {
                feedUri = pagedEntriesTemplate.BindByPosition(WebOperationContext.Current.IncomingRequest.UriTemplateMatch.BaseUri, collection, pageNumber.ToString(CultureInfo.InvariantCulture));
            }
            return new SyndicationLink(feedUri, "previous", "Previous entries", ContentTypes.Atom, 0);
        }

        static SyndicationLink GetNextPageLink(string collection, int pageNumber)
        {
            Uri feedUri = pagedEntriesTemplate.BindByPosition(WebOperationContext.Current.IncomingRequest.UriTemplateMatch.BaseUri, collection, pageNumber.ToString(CultureInfo.InvariantCulture));
            return new SyndicationLink(feedUri, "next", "Next entries", ContentTypes.Atom, 0);
        }

        static SyndicationLink GetSelfLink(string collection, int pageNumber)
        {
            Uri feedUri;
            if (pageNumber == 0)
            {
                feedUri = allEntriesTemplate.BindByPosition(WebOperationContext.Current.IncomingRequest.UriTemplateMatch.BaseUri, collection);
            }
            else
            {
                feedUri = pagedEntriesTemplate.BindByPosition(WebOperationContext.Current.IncomingRequest.UriTemplateMatch.BaseUri, collection, pageNumber.ToString(CultureInfo.InvariantCulture));
            }
            return SyndicationLink.CreateSelfLink(feedUri, ContentTypes.Atom);
        }

        protected static void ConfigureMediaEntry(string collection, SyndicationItem entry, string id, string contentType, out Uri location)
        {
            location = GetEntryUri(collection, id);
            Uri mediaUri = GetMediaItemUri(collection, id);
            entry.Content = new UrlSyndicationContent(mediaUri, contentType);
            entry.Links.Add(CreateEditMediaLink(collection, id, contentType));
            entry.Links.Add(CreateEditLink(collection, id));
            entry.Links.Add(SyndicationLink.CreateMediaEnclosureLink(mediaUri, contentType, 0));
        }

        protected static void ConfigureAtomEntry(string collection, SyndicationItem entry, string id, out Uri location)
        {
            location = GetEntryUri(collection, id);
            entry.Links.Add(CreateEditLink(collection, id));
        }

        #endregion

        // The service interface allows GET, POST, PUT and DELETE HTTP methods using the Atom Publishing Protocol. 
        #region interfaces for exposing the resource over Atom Publishing Protocol.

        public AtomPub10ServiceDocumentFormatter GetDocument()
        {
            ServiceDocument serviceDocument = GetServiceDocument();
            SetAtomServiceDocumentContentType();
            return (AtomPub10ServiceDocumentFormatter)serviceDocument.GetFormatter();
        }

        public Atom10FeedFormatter GetFeed(string collection)
        {
            if (!IsValidCollection(collection))
            {
                WebOperationContext.Current.OutgoingResponse.SetStatusAsNotFound();
                return null;
            }

            UriTemplateMatch match = WebOperationContext.Current.IncomingRequest.UriTemplateMatch;
            int pageNumber = 0;

            // check if the query string has a pageNumber
            string pageQueryParam = match.QueryParameters["page"];
            if (!string.IsNullOrEmpty(pageQueryParam))
            {
                if (!int.TryParse(pageQueryParam, out pageNumber))
                {
                    throw new WebProtocolException(HttpStatusCode.BadRequest);
                }
            }
            int startIndex = pageNumber * this.GetMaximumEntriesInFeed(collection);
            bool hasMoreEntries;
            IEnumerable<SyndicationItem> entries = GetEntries(collection, startIndex, this.GetMaximumEntriesInFeed(collection), out hasMoreEntries);
            if (entries == null) entries = new List<SyndicationItem>();
            SyndicationFeed feed = CreateFeed(collection);
            feed.Items = entries;
            feed.Links.Add(GetSelfLink(collection, pageNumber));
            // add links to the previous page and next page if paging is enabled.
            if (pageNumber > 0)
            {
                feed.Links.Add(GetPreviousPageLink(collection, pageNumber - 1));
            }
            if (hasMoreEntries)
            {
                feed.Links.Add(GetNextPageLink(collection, pageNumber + 1));
            }
            SetAtomContentType();
            return feed.GetAtom10Formatter();
        }

        public Atom10ItemFormatter AddEntry(string collection, Stream stream)
        {
            if (!IsValidCollection(collection))
            {
                throw new WebProtocolException(HttpStatusCode.NotFound);
            }
            string description = GetDescriptionFromSlugHeader();
            string contentType = WebOperationContext.Current.IncomingRequest.ContentType;
            if (!IsContentTypeAllowed(collection, contentType))
            {
                throw new WebProtocolException(HttpStatusCode.UnsupportedMediaType);
            }
            SyndicationItem newEntry;
            Uri location;
            if (contentType.StartsWith("application/atom+xml", StringComparison.Ordinal))
            {
                // an Atom entry is being posted
                using (XmlReader reader = XmlReader.Create(stream, new XmlReaderSettings() { IgnoreWhitespace = true, IgnoreComments = true }))
                {
                    SyndicationItem entry = SyndicationItem.Load(reader);
                    newEntry = this.AddEntry(collection, entry, out location);
                }
            }
            else
            {
                newEntry = this.AddMedia(collection, stream, contentType, description, out location);
            }
            if (newEntry == null)
            {
                throw new WebProtocolException(HttpStatusCode.InternalServerError);
            }
            WebOperationContext.Current.OutgoingResponse.SetStatusAsCreated(location);
            SetAtomEntryContentType();
            return newEntry.GetAtom10Formatter();
        }

        public Atom10ItemFormatter GetAtomEntry(string collection, string id)
        {
            if (!IsValidCollection(collection))
            {
                throw new WebProtocolException(HttpStatusCode.NotFound);
            }
            SyndicationItem entry = GetEntry(collection, id);
            if (entry == null)
            {
                throw new WebProtocolException(HttpStatusCode.NotFound);
            }
            SetAtomEntryContentType();
            return entry.GetAtom10Formatter();
        }

        public Atom10ItemFormatter PutEntry(string collection, string id, Stream body)
        {
            if (!IsValidCollection(collection))
            {
                throw new WebProtocolException(HttpStatusCode.NotFound);
            }
            SyndicationItem newEntry;
            using (XmlReader reader = XmlReader.Create(body, new XmlReaderSettings() { IgnoreWhitespace = true, IgnoreComments = true }))
            {
                newEntry = SyndicationItem.Load(reader);
            }
            SyndicationItem updatedEntry = PutEntry(collection, id, newEntry);
            if (updatedEntry == null)
            {
                throw new WebProtocolException(HttpStatusCode.NotFound);    
            }
            SetAtomEntryContentType();
            return updatedEntry.GetAtom10Formatter();
        }

        public void DeleteAtomEntry(string collection, string id)
        {
            if (!IsValidCollection(collection))
            {
                throw new WebProtocolException(HttpStatusCode.NotFound);
            }
            bool wasDeleted = DeleteEntry(collection, id);
            if (!wasDeleted)
            {
                throw new WebProtocolException(HttpStatusCode.NotFound);
            }
        }

        public Stream GetMediaItem(string collection, string id)
        {
            if (!IsValidCollection(collection))
            {
                throw new WebProtocolException(HttpStatusCode.NotFound);
            }
            string contentType;
            Stream stream = GetMedia(collection, id, out contentType);
            if (stream == null)
            {
                throw new WebProtocolException(HttpStatusCode.NotFound);
            }
            WebOperationContext.Current.OutgoingResponse.ContentType = contentType;
            return stream;
        }

        public void PutMediaItem(string collection, string id, Stream s)
        {
            if (!IsValidCollection(collection))
            {
                throw new WebProtocolException(HttpStatusCode.NotFound);
            }
            bool updatedItem = PutMedia(collection, id, s, WebOperationContext.Current.IncomingRequest.ContentType, GetDescriptionFromSlugHeader());
            if (!updatedItem)
            {
                throw new WebProtocolException(HttpStatusCode.NotFound);
            }
        }

        public void DeleteMediaItem(string collection, string id)
        {
            if (!IsValidCollection(collection))
            {
                throw new WebProtocolException(HttpStatusCode.NotFound);
            }
            bool deletedItem = DeleteMedia(collection, id);
            if (!deletedItem)
            {
                throw new WebProtocolException(HttpStatusCode.NotFound);
            }
        }
        #endregion
    }

    #endregion

    #region HTTP REST interface for AtomPub service
    [ServiceContract]
    public interface IAtomPubService
    {
        [WebGet(UriTemplate = AtomPubServiceBase.DocumentTemplate)]
        [OperationContract]
        AtomPub10ServiceDocumentFormatter GetDocument();

        [WebGet(UriTemplate = AtomPubServiceBase.AllEntriesTemplate)]
        [OperationContract]
        Atom10FeedFormatter GetFeed(string collection);

        [WebInvoke(UriTemplate = AtomPubServiceBase.AllEntriesTemplate)]
        [OperationContract]
        Atom10ItemFormatter AddEntry(string collection, Stream stream);

        [WebGet(UriTemplate = AtomPubServiceBase.EntryTemplate)]
        [OperationContract]
        Atom10ItemFormatter GetAtomEntry(string collection, string id);

        [WebInvoke(Method = "PUT", UriTemplate = AtomPubServiceBase.EntryTemplate)]
        [OperationContract]
        Atom10ItemFormatter PutEntry(string collection, string id, Stream body);

        [WebInvoke(Method = "DELETE", UriTemplate = AtomPubServiceBase.EntryTemplate)]
        [OperationContract]
        void DeleteAtomEntry(string collection, string id);

        [WebGet(UriTemplate = AtomPubServiceBase.MediaItemTemplate)]
        [OperationContract]
        Stream GetMediaItem(string collection, string id);

        [WebInvoke(Method = "PUT", UriTemplate = AtomPubServiceBase.MediaItemTemplate)]
        [OperationContract]
        void PutMediaItem(string collection, string id, Stream s);

        [WebInvoke(Method = "DELETE", UriTemplate = AtomPubServiceBase.MediaItemTemplate)]
        [OperationContract]
        void DeleteMediaItem(string collection, string id);
    }
    #endregion
}