//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Syndication
{
    using System.Xml;
    using System.Linq;
    using Microsoft.Http;

    public static partial class SyndicationContentExtensions
    {
        static readonly XmlReaderSettings SyndicationFeedReaderSettings = new XmlReaderSettings()
            {
                ProhibitDtd = false,
                IgnoreProcessingInstructions = true,
                CloseInput = true,
                XmlResolver = null,

            };

        public static ServiceDocument ReadAsServiceDocument(this HttpContent content)
        {
            return ServiceDocument.Load(XmlContentExtensions.ReadAsXmlReader(content, SyndicationFeedReaderSettings));
        }

        public static TServiceDocument ReadAsServiceDocument<TServiceDocument>(this HttpContent content) where TServiceDocument : ServiceDocument, new()
        {
            return ServiceDocument.Load<TServiceDocument>(XmlContentExtensions.ReadAsXmlReader(content, SyndicationFeedReaderSettings));
        }

        public static SyndicationFeed ReadAsSyndicationFeed(this HttpContent content)
        {
            return SyndicationFeed.Load(XmlContentExtensions.ReadAsXmlReader(content, SyndicationFeedReaderSettings));
        }

        public static TSyndicationFeed ReadAsSyndicationFeed<TSyndicationFeed>(this HttpContent content) where TSyndicationFeed : SyndicationFeed, new()
        {
            return SyndicationFeed.Load<TSyndicationFeed>(XmlContentExtensions.ReadAsXmlReader(content, SyndicationFeedReaderSettings));
        }
    }

    }
