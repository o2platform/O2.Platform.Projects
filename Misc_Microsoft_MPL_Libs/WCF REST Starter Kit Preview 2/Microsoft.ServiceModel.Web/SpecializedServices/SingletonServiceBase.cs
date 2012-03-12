/// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Activation;
using System.Linq;
using System.Net;
using Microsoft.ServiceModel.Web;


namespace Microsoft.ServiceModel.Web.SpecializedServices
{
    #region Singleton service base class - implements the REST interface and has helper functions
    public abstract class SingletonServiceBase<TItem> where TItem : class
    {
        // URI template definitions for how clients can access the resource using XML or JSON.
        // The URI template to manipulate the resource in its XML format. The URL is of the form http://<url-for-svc-file>/
        public const string XmlItemTemplate = "";
        // The URI template to manipulate the resource in its JSON format. The URL is of the form http://<url-for-svc-file>/?json
        public const string JsonItemTemplate = "?format=json";

        // Get the item
        // A null return value will result in a NotFound
        protected abstract TItem OnGetItem();

        // Add the item. Return null if adding the item failed
        // A null return value will result in a response status code of InternalServerError (500)
        protected abstract TItem OnAddItem(TItem initialValue, out bool wasItemCreated);

        // Update the item. 
        // a null return value will result in a response status code of NotFound (404) if the item does not exist; 
        protected abstract TItem OnUpdateItem(TItem newValue, out bool wasItemCreated);

        // Delete the item, if it exists. Return false if the item does not exist.
        // A return value of false will result in a response status code of NotFound (404) unless the method explicitly sets the status code to a different error.
        protected abstract bool OnDeleteItem();

        #region helper methods that call the application methods and set the status codes accordingly
        
        static readonly UriTemplate xmlTemplate = new UriTemplate(XmlItemTemplate);
        static readonly UriTemplate jsonTemplate = new UriTemplate(JsonItemTemplate);

        TItem HandleGet(UriTemplate template)
        {
            TItem result = OnGetItem();
            if (result == null)
            {
                throw new WebProtocolException(HttpStatusCode.NotFound);
            }
            return result;
        }

        TItem HandleAdd(TItem initialValue, UriTemplate template)
        {
            bool wasResourceCreated;
            TItem result = OnAddItem(initialValue, out wasResourceCreated);
            if (result == null)
            {
                throw new WebProtocolException(HttpStatusCode.InternalServerError);
            }
            if (wasResourceCreated)
            {
                Uri location = WebOperationContext.Current.BindTemplateToRequestUri(template);
                WebOperationContext.Current.OutgoingResponse.SetStatusAsCreated(location);
            }
            return result;
        }

        TItem HandleUpdate(TItem newValue, UriTemplate template)
        {
            bool wasResourceCreated;
            TItem result = OnUpdateItem(newValue, out wasResourceCreated);
            if (result == null)
            {
                throw new WebProtocolException(HttpStatusCode.InternalServerError);
            }
            if (wasResourceCreated)
            {
                Uri location = WebOperationContext.Current.BindTemplateToRequestUri(template);
                WebOperationContext.Current.OutgoingResponse.SetStatusAsCreated(location);
            }
            return result;
        }

        void HandleDelete()
        {
            if (!OnDeleteItem())
            {
                throw new WebProtocolException(HttpStatusCode.NotFound);
            }
        }

        #endregion

        // The service interface allows GET, POST, PUT and DELETE HTTP methods on the resource in both XML and JSON formats. 
        // Modify the interface, if needed, to 
        //    1. Restrict support to just XML or just JSON
        //    2. Restrict the HTTP methods allowed on the resource
        #region JSON and XML interfaces for exposing the resource over HTTP.

        #region HTTP methods using XML format
        public TItem GetItemInXml()
        {
            return HandleGet(xmlTemplate);
        }

        public TItem AddItemInXml(TItem initialValue)
        {
            return HandleAdd(initialValue, xmlTemplate);
        }

        public TItem UpdateItemInXml(TItem newValue)
        {
            return HandleUpdate(newValue, xmlTemplate);
        }
        #endregion

        #region HTTP methods using JSON format

        public TItem GetItemInJson()
        {
            return HandleGet(jsonTemplate);
        }

        public TItem AddItemInJson(TItem initialValue)
        {
            return HandleAdd(initialValue, jsonTemplate);
        }

        public TItem UpdateItemInJson(TItem newValue)
        {
            return HandleUpdate(newValue, jsonTemplate);
        }
        #endregion

        public void DeleteItem()
        {
            HandleDelete();
        }

        #endregion
    }
    #endregion

    #region HTTP REST Interface for singleton service
    [ServiceContract]
    public interface ISingletonService<TItem> where TItem : class
    {
        #region XML format APIs
        [WebHelp(Comment = "Returns the item in XML format.")]
        [WebGet(UriTemplate = SingletonServiceBase<TItem>.XmlItemTemplate)]
        [OperationContract]
        TItem GetItemInXml();

        [WebHelp(Comment = "Initializes the item based on the incoming XML.")]
        [WebInvoke(Method = "POST", UriTemplate = SingletonServiceBase<TItem>.XmlItemTemplate)]
        [OperationContract]
        TItem AddItemInXml(TItem initialValue);

        [WebHelp(Comment = "Edits the item based on the incoming XML and returns the updated item in XML format.")]
        [WebInvoke(Method = "PUT", UriTemplate = SingletonServiceBase<TItem>.XmlItemTemplate)]
        [OperationContract]
        TItem UpdateItemInXml(TItem newValue);
        #endregion

        #region JSON format APIs
        [WebHelp(Comment = "Returns the item in JSON format.")]
        [WebGet(UriTemplate = SingletonServiceBase<TItem>.JsonItemTemplate, ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        TItem GetItemInJson();

        [WebHelp(Comment = "Initializes the item based on the incoming JSON.")]
        [WebInvoke(Method = "POST", UriTemplate = SingletonServiceBase<TItem>.JsonItemTemplate, ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        TItem AddItemInJson(TItem initialValue);

        [WebHelp(Comment = "Edits the item based on the incoming JSON and returns the updated item in JSON format.")]
        [WebInvoke(Method = "PUT", UriTemplate = SingletonServiceBase<TItem>.JsonItemTemplate, ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        TItem UpdateItemInJson(TItem newValue);
        #endregion

        [WebHelp(Comment="Deletes the item.")]
        [WebInvoke(Method = "DELETE", UriTemplate = SingletonServiceBase<TItem>.XmlItemTemplate)]
        [OperationContract]
        void DeleteItem();
    }
    #endregion
}