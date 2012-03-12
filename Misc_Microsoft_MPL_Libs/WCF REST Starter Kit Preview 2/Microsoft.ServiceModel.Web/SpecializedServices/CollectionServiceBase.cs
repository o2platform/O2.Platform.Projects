/// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using Microsoft.ServiceModel.Web;
using System.ServiceModel.Activation;
using System.Linq;
using System.Net;


namespace Microsoft.ServiceModel.Web.SpecializedServices
{
    #region Base class for collection service - implements REST interface and contains helper methods
    public abstract class CollectionServiceBase<TItem> where TItem : class
    {
        // URI template definitions for how clients can access the items using XML or JSON. Modify if needed
        // The URI template to get all the items in XML format or add an item in XML format. The URL is of the form http://<url-for-svc-file>/
        public const string XmlItemsTemplate = "";
        // The URI template to manipulate a particular item in XML format. The URL is of the form http://<url-for-svc-file>/item1
        public const string XmlItemTemplate = "{id}";
        // The URI template to get all the items in JSON format or add an item in JSON format. The URL is of the form http://<url-for-svc-file>/?format=json
        public const string JsonItemsTemplate = "?format=json";
        // The URI template to manipulate a particular item in JSON format. The URL is of the form http://<url-for-svc-file>/item1?format=json
        public const string JsonItemTemplate = "{id}?format=json";

        // Return an enumeration of the (id, item) pairs. Return null if no items are present
        protected abstract IEnumerable<KeyValuePair<string, TItem>> OnGetItems();

        // Get the item with the specified id. 
        // A null return value will result in a NotFound
        protected abstract TItem OnGetItem(string id);

        // Add the item to the enumeration and return its id. Return null if adding the item failed
        // A null return value will result in a response status code of InternalServerError (500), unless the method explicitly sets the status code to a different error
        protected abstract TItem OnAddItem(TItem initialValue, out string id);

        // Update the item with the id specified. 
        // a null return value will result in a response status code of NotFound (404) if the item does not exist; 
        protected abstract TItem OnUpdateItem(string id, TItem newValue);

        // Delete the item with the specified id, if it exists. Return false if the item does not exist.
        // A return value of false will result in a response status code of NotFound (404) unless the method explicitly sets the status code to a different error.
        protected abstract bool OnDeleteItem(string id);

        #region helper methods that set status codes, create the correct links and call the application methods
        static readonly UriTemplate xmlItems = new UriTemplate(XmlItemsTemplate);
        static readonly UriTemplate xmlItem = new UriTemplate(XmlItemTemplate);
        static readonly UriTemplate jsonItems = new UriTemplate(JsonItemsTemplate);
        static readonly UriTemplate jsonItem = new UriTemplate(JsonItemTemplate);

        static Uri GetItemLink(string id, UriTemplate template)
        {
            return template.BindByPosition(WebOperationContext.Current.IncomingRequest.GetBaseUri(), id);
        }

        ItemInfoList<TItem> HandleGetItems(UriTemplate template)
        {
            IEnumerable<KeyValuePair<string, TItem>> items = OnGetItems();
            if (items == null) items = new List<KeyValuePair<string, TItem>>();
            return new ItemInfoList<TItem>(items.Select<KeyValuePair<string, TItem>, ItemInfo<TItem>>((pair) => new ItemInfo<TItem>()
            {
                Item = pair.Value,
                EditLink = GetItemLink(pair.Key, template)
            }));
        }

        TItem HandleGetItem(string id)
        {
            TItem item = OnGetItem(id);
            if (item == null)
            {
                throw new WebProtocolException(HttpStatusCode.NotFound);
            }
            return item;
        }

        ItemInfo<TItem> HandleAddItem(TItem initialValue, UriTemplate template)
        {
            string id;
            TItem createdItem = OnAddItem(initialValue, out id);
            if (createdItem == null)
            {
                throw new WebProtocolException(HttpStatusCode.InternalServerError);
            }
            WebOperationContext.Current.OutgoingResponse.SetStatusAsCreated(GetItemLink(id, template));
            return new ItemInfo<TItem>() { Item = createdItem, EditLink = GetItemLink(id, template) };
        }

        TItem HandleUpdateItem(string id, TItem newValue)
        {
            TItem updatedItem = OnUpdateItem(id, newValue);
            if (updatedItem == null)
            {
                throw new WebProtocolException(HttpStatusCode.NotFound);
            }
            return updatedItem;
        }

        void HandleDeleteItem(string id)
        {
            bool wasDeleted = OnDeleteItem(id);
            if (!wasDeleted)
            {
                throw new WebProtocolException(HttpStatusCode.NotFound);
            }
        }
        #endregion

        // The service interface allows GET, POST, PUT and DELETE HTTP methods on the the collection resource in both XML and JSON formats. 
        // Modify the interface, if needed, to 
        //    1. Restrict support to just XML or just JSON
        //    2. Restrict the HTTP methods allowed on the resource
        #region JSON and XML interfaces for exposing the resource over HTTP.

        #region HTTP methods using XML format
        public ItemInfoList<TItem> GetItemsInXml()
        {
            return HandleGetItems(xmlItem);
        }

        public TItem GetItemInXml(string id)
        {
            return HandleGetItem(id);
        }

        public ItemInfo<TItem> AddItemInXml(TItem initialValue)
        {
            return HandleAddItem(initialValue, xmlItem);
        }

        public TItem UpdateItemInXml(string id, TItem newValue)
        {
            return HandleUpdateItem(id, newValue);
        }

        #endregion

        #region HTTP methods using JSON format

        public ItemInfoList<TItem> GetItemsInJson()
        {
            return HandleGetItems(jsonItem);
        }

        public TItem GetItemInJson(string id)
        {
            return HandleGetItem(id);
        }

        public ItemInfo<TItem> AddItemInJson(TItem initialValue)
        {
            return HandleAddItem(initialValue, jsonItem);
        }

        public TItem UpdateItemInJson(string id, TItem newValue)
        {
            return HandleUpdateItem(id, newValue);
        }

        #endregion

        public void DeleteItem(string id)
        {
            HandleDeleteItem(id);
        }

        #endregion
    }
    #endregion

    #region Types used in the Collection REST interface for encapsulating the item and its link
    [DataContract(Name = "ItemInfo", Namespace="")]
    public class ItemInfo<TItem> where TItem : class
    {
        [DataMember]
        public TItem Item { get; set; }
        [DataMember]
        public Uri EditLink { get; set; }
    }

    [CollectionDataContract(Name = "ItemInfoList", Namespace="")]
    public class ItemInfoList<TItem> : List<ItemInfo<TItem>> where TItem : class
    {
        public ItemInfoList()
            : base()
        {
        }

        public ItemInfoList(IEnumerable<ItemInfo<TItem>> items)
            : base(items)
        {
        }
    }
    #endregion 

    #region HTTP REST interface for the collection service
    [ServiceContract]
    public interface ICollectionService<TItem> where TItem : class
    {
        #region XML format APIs
        [WebHelp(Comment = "Returns the items in the collection in XML format, along with URI links to each item.")]
        [WebGet(UriTemplate = CollectionServiceBase<TItem>.XmlItemsTemplate)]
        [OperationContract]
        ItemInfoList<TItem> GetItemsInXml();

        [WebHelp(Comment = "Returns the item with the specified id in XML format.")]
        [WebGet(UriTemplate = CollectionServiceBase<TItem>.XmlItemTemplate)]
        [OperationContract]
        TItem GetItemInXml(string id);

        [WebHelp(Comment = "Adds the incoming item, in XML format, to the collection and returns the item along with a link to edit it.")]
        [WebInvoke(Method = "POST", UriTemplate = CollectionServiceBase<TItem>.XmlItemsTemplate)]
        [OperationContract]
        ItemInfo<TItem> AddItemInXml(TItem initialValue);

        [WebHelp(Comment = "Edits the item specified by its id, based on the incoming XML and returns the updated item in XML format.")]
        [WebInvoke(Method = "PUT", UriTemplate = CollectionServiceBase<TItem>.XmlItemTemplate)]
        [OperationContract]
        TItem UpdateItemInXml(string id, TItem newValue);
        #endregion

        #region JSON format APIs
        [WebHelp(Comment = "Returns the items in the collection in JSON format, along with URI links to each item.")]
        [WebGet(UriTemplate = CollectionServiceBase<TItem>.JsonItemsTemplate, ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        ItemInfoList<TItem> GetItemsInJson();

        [WebHelp(Comment = "Returns the item with the specified id in JSON format.")]
        [WebGet(UriTemplate = CollectionServiceBase<TItem>.JsonItemTemplate, ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        TItem GetItemInJson(string id);

        [WebHelp(Comment = "Adds the incoming item, in JSON format, to the collection and returns the item along with a link to edit it.")]
        [WebInvoke(Method = "POST", UriTemplate = CollectionServiceBase<TItem>.JsonItemsTemplate, ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        ItemInfo<TItem> AddItemInJson(TItem initialValue);

        [WebHelp(Comment = "Edits the item specified by its id, based on the incoming JSON and returns the updated item in JSON format.")]
        [WebInvoke(Method = "PUT", UriTemplate = CollectionServiceBase<TItem>.JsonItemTemplate, ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        TItem UpdateItemInJson(string id, TItem newValue);
        #endregion

        [WebHelp(Comment="Deletes the item with the specified id.")]
        [WebInvoke(Method = "DELETE", UriTemplate = CollectionServiceBase<TItem>.XmlItemTemplate)]
        [OperationContract]
        void DeleteItem(string id);
    }
    #endregion
}