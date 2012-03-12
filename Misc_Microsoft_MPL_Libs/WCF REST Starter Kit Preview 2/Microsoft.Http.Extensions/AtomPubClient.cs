//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Http
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Http.Headers;
    using System.Net;
    using System.ServiceModel.Syndication;
    using System.Runtime.Serialization;

    public class AtomPubClient : HttpClient
    {
        const string AtomEntryContentType = "application/atom+xml;type=entry";

        public SyndicationItem AddEntry(SyndicationFeed feed, SyndicationItem newEntry)
        {
            if (feed == null)
            {
                throw new ArgumentNullException("feed");
            }
            if (newEntry == null)
            {
                throw new ArgumentNullException("newEntry");
            }
            return this.Post(feed.GetSelfLink(true).Uri, AtomEntryContentType, HttpContentExtensions.CreateDataContract(newEntry.GetAtom10Formatter())).EnsureStatusIs(HttpStatusCode.Created).Content.ReadAsDataContract<Atom10ItemFormatter>().Item;
        }

        public SyndicationItem AddEntry(Uri feedUri, SyndicationItem newEntry)
        {
            if (feedUri == null)
            {
                throw new ArgumentNullException("feedUri");
            }
            if (newEntry == null)
            {
                throw new ArgumentNullException("newEntry");
            }
            return this.Post(feedUri, AtomEntryContentType, HttpContentExtensions.CreateDataContract(newEntry.GetAtom10Formatter())).EnsureStatusIs(HttpStatusCode.Created).Content.ReadAsDataContract<Atom10ItemFormatter>().Item;
        }

        public SyndicationItem AddMediaResource(Uri mediaCollectionUri, string contentType, string description, HttpContent mediaContent)
        {
            if (mediaCollectionUri == null)
            {
                throw new ArgumentNullException("mediaCollectionUri");
            }
            if (mediaContent == null)
            {
                throw new ArgumentNullException("mediaContent");
            }
            HttpRequestMessage request = new HttpRequestMessage()
                {
                    Content = mediaContent,
                    Method = "POST",
                    Uri = mediaCollectionUri
                };
            if (!string.IsNullOrEmpty(description))
            {
                request.Headers["Slug"] = description;
            }
            if (!string.IsNullOrEmpty(contentType))
            {
                request.Headers.ContentType = contentType;
            }
            return this.Send(request).EnsureStatusIs(HttpStatusCode.Created).Content.ReadAsDataContract<Atom10ItemFormatter>().Item;

        }

        public SyndicationItem AddMediaResource(SyndicationFeed feed, string contentType, string description, HttpContent mediaContent)
        {
            if (feed == null)
            {
                throw new ArgumentNullException("feed");
            }
            if (mediaContent == null)
            {
                throw new ArgumentNullException("mediaContent");
            }
            return AddMediaResource(feed.GetSelfLink(true).Uri, contentType, description, mediaContent);
        }

        public void DeleteEntry(SyndicationItem entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }
            this.Delete(entry.GetEditLink(true).Uri).EnsureStatusIs(HttpStatusCode.OK).Dispose();
        }

        public void DeleteEntry(Uri itemUri)
        {
            if (itemUri == null)
            {
                throw new ArgumentNullException("itemUri");
            }
            this.Delete(itemUri).EnsureStatusIs(HttpStatusCode.OK).Dispose();
        }

        public SyndicationItem GetEntry(Uri itemUri)
        {
            if (itemUri == null)
            {
                throw new ArgumentNullException("itemUri");
            }
            return this.Get(itemUri).EnsureStatusIs(HttpStatusCode.OK).Content.ReadAsDataContract<Atom10ItemFormatter>().Item;
        }

        public SyndicationFeed GetFeed(Uri feedUri)
        {
            if (feedUri == null)
            {
                throw new ArgumentNullException("feedUri");
            }
            return this.Get(feedUri).EnsureStatusIs(HttpStatusCode.OK).Content.ReadAsDataContract<Atom10FeedFormatter>().Feed;
        }

        public ServiceDocument GetServiceDocument(Uri serviceDocumentUri)
        {
            if (serviceDocumentUri == null)
            {
                throw new ArgumentNullException("serviceDocumentUri");
            }
            return this.Get(serviceDocumentUri).EnsureStatusIs(HttpStatusCode.OK).Content.ReadAsDataContract<AtomPub10ServiceDocumentFormatter>().Document;
        }

        public SyndicationItem UpdateEntry(SyndicationItem oldValue, SyndicationItem newValue)
        {
            if (oldValue == null)
            {
                throw new ArgumentNullException("oldValue");
            }
            if (newValue == null)
            {
                throw new ArgumentNullException("newValue");
            }
            return this.Put(oldValue.GetEditLink(true).Uri, AtomEntryContentType, HttpContentExtensions.CreateDataContract(newValue.GetAtom10Formatter())).EnsureStatusIs(HttpStatusCode.OK).Content.ReadAsDataContract<Atom10ItemFormatter>().Item;
        }

        public SyndicationItem UpdateEntry(Uri editUri, SyndicationItem newValue)
        {
            if (editUri == null)
            {
                throw new ArgumentNullException("editUri");
            }
            if (newValue == null)
            {
                throw new ArgumentNullException("newValue");
            }
            return this.Put(editUri, AtomEntryContentType, HttpContentExtensions.CreateDataContract(newValue.GetAtom10Formatter())).EnsureStatusIs(HttpStatusCode.OK).Content.ReadAsDataContract<Atom10ItemFormatter>().Item;
        }
    }
}
