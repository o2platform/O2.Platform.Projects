//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Syndication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class SyndicationFeedExtensions
    {

        public static SyndicationLink GetEditLink(this SyndicationItem item, bool throwIfNotPresent)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            SyndicationLink editLink = item.Links.Where((link) =>(link.RelationshipType == "edit")).SingleOrDefault();
            if (editLink == null && throwIfNotPresent)
            {
                throw new ArgumentException("The item does not have an edit link");
            }
            return editLink;
        }

        public static SyndicationLink GetEditMediaLink(this SyndicationItem item, bool throwIfNotPresent)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            SyndicationLink editLink = item.Links.Where((link) =>(link.RelationshipType == "edit-media")).SingleOrDefault();
            if (editLink == null && throwIfNotPresent)
            {
                throw new ArgumentException("The item does not have an edit-media link");
            }
            return editLink;
        }

        public static SyndicationLink GetFirstPageLink(this SyndicationFeed feed, bool throwIfNotPresent)
        {
            if (feed == null)
            {
                throw new ArgumentNullException("feed");
            }
            SyndicationLink firstPageLink = feed.Links.Where((link) =>(link.RelationshipType == "first")).SingleOrDefault();
            return firstPageLink;
        }

        public static SyndicationLink GetLastPageLink(this SyndicationFeed feed, bool throwIfNotPresent)
        {
            if (feed == null)
            {
                throw new ArgumentNullException("feed");
            }
            SyndicationLink lastPageLink = feed.Links.Where((link) =>(link.RelationshipType == "last")).SingleOrDefault();
            return lastPageLink;
        }

        public static Uri GetMediaResourceUri(this SyndicationItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            if (!item.IsMediaEntry())
            {
                throw new ArgumentException("The item is not a media entry");
            }
            return ((UrlSyndicationContent) item.Content).Url;
        }

        public static SyndicationLink GetNextPageLink(this SyndicationFeed feed)
        {
            if (feed == null)
            {
                throw new ArgumentNullException("feed");
            }
            SyndicationLink nextPageLink = feed.Links.Where((link) =>(link.RelationshipType == "next")).SingleOrDefault();
            return nextPageLink;
        }

        public static SyndicationLink GetPreviousPageLink(this SyndicationFeed feed, bool throwIfNotPresent)
        {
            if (feed == null)
            {
                throw new ArgumentNullException("feed");
            }
            SyndicationLink prevPageLink = feed.Links.Where((link) =>(link.RelationshipType == "previous")).SingleOrDefault();
            return prevPageLink;
        }
        public static SyndicationLink GetSelfLink(this SyndicationFeed feed, bool throwIfNotPresent)
        {
            if (feed == null)
            {
                throw new ArgumentNullException("feed");
            }
            SyndicationLink selfLink = feed.Links.Where((link) =>(link.RelationshipType == "self")).SingleOrDefault();
            if (selfLink == null && throwIfNotPresent)
            {
                throw new ArgumentException("The feed does not have a self link");
            }
            return selfLink;
        }

        public static SyndicationLink GetSelfLink(this SyndicationItem item, bool throwIfNotPresent)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            SyndicationLink selfLink = item.Links.Where((link) =>(link.RelationshipType == "self")).SingleOrDefault();
            if (selfLink == null && throwIfNotPresent)
            {
                throw new ArgumentException("The item does not have a self link");
            }
            return selfLink;
        }

        public static bool IsMediaEntry(this SyndicationItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            return (item.Content is UrlSyndicationContent);
        }
    }

}
