//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Http
{
    using System;
    using Microsoft.Http.Headers;
    using System.Collections.Generic;

    public static class HttpMethodExtensions
    {
        public static HttpResponseMessage Delete(this HttpClient client, Uri uri)
        {
            return Method(client, HttpMethod.DELETE, uri);
        }
        public static HttpResponseMessage Delete(this HttpClient client, string uri)
        {
            return Method(client, HttpMethod.DELETE, uri);
        }
        public static HttpResponseMessage Get(this HttpClient client, Uri uri)
        {
            return Method(client, HttpMethod.GET, uri);
        }
        public static HttpResponseMessage Get(this HttpClient client)
        {
            CheckNull(client, "client");
            return Method(client, HttpMethod.GET, client.BaseAddress);
        }
        public static HttpResponseMessage Get(this HttpClient client, Uri uri, HttpQueryString queryString)
        {
            CheckNull(uri, "uri");
            CheckNull(queryString, "queryString");
            uri = HttpQueryString.MakeQueryString(uri, queryString);
            return Method(client, HttpMethod.GET, uri);
        }
        public static HttpResponseMessage Get(this HttpClient client, Uri uri, IEnumerable<KeyValuePair<string, string>> queryString)
        {
            CheckNull(uri, "uri");
            CheckNull(queryString, "queryString");
            uri = HttpQueryString.MakeQueryString(uri, queryString);
            return Method(client, HttpMethod.GET, uri);
        }

        public static HttpResponseMessage Get(this HttpClient client, string uri)
        {
            return Method(client, HttpMethod.GET, uri);
        }

        public static HttpResponseMessage Head(this HttpClient client, Uri uri)
        {
            return Method(client, HttpMethod.HEAD, uri);
        }

        public static HttpResponseMessage Head(this HttpClient client, string uri)
        {
            return Method(client, HttpMethod.HEAD, uri);
        }

        public static HttpResponseMessage Post(this HttpClient client, string uri, HttpContent body)
        {
            return Method(client, HttpMethod.POST, uri, body);
        }
        public static HttpResponseMessage Post(this HttpClient client, Uri uri, HttpContent body)
        {
            return Method(client, HttpMethod.POST, uri, body);
        }
        public static HttpResponseMessage Post(this HttpClient client, string uri, string contentType, HttpContent body)
        {
            CheckNull(contentType, "contentType");
            return Method(client, HttpMethod.POST, new Uri(uri, UriKind.RelativeOrAbsolute), contentType, body);
        }
        public static HttpResponseMessage Post(this HttpClient client, Uri uri, string contentType, HttpContent body)
        {
            CheckNull(contentType, "contentType");

            return Method(client, HttpMethod.POST, uri, contentType, body);
        }

        public static HttpResponseMessage Put(this HttpClient client, string uri, string contentType, HttpContent body)
        {
            CheckNull(contentType, "contentType");

            return Method(client, HttpMethod.PUT, new Uri(uri, UriKind.RelativeOrAbsolute), contentType, body);
        }
        public static HttpResponseMessage Put(this HttpClient client, Uri uri, string contentType, HttpContent body)
        {
            CheckNull(contentType, "contentType");

            return Method(client, HttpMethod.PUT, uri, contentType, body);
        }
        public static HttpResponseMessage Put(this HttpClient client, Uri uri, HttpContent body)
        {
            return Method(client, HttpMethod.PUT, uri, body);
        }
        public static HttpResponseMessage Put(this HttpClient client, string uri, HttpContent body)
        {
            return Method(client, HttpMethod.PUT, uri, body);
        }

        static void CheckNull<T>(T o, string name) where T : class
        {
            if (o == null)
            {
                throw new ArgumentNullException(name);
            }
        }
        static HttpResponseMessage Method(HttpClient client, HttpMethod method, string uri, HttpContent body)
        {
            CheckNull(uri, "uri");
            return Method(client, method, new Uri(uri, UriKind.RelativeOrAbsolute), body);
        }
        static HttpResponseMessage Method(HttpClient client, HttpMethod method, Uri uri, HttpContent body)
        {
            CheckNull(client, "client");
            CheckNull(uri, "uri");
            CheckNull(body, "body");

            return client.Send(method, uri, body);
        }

        static HttpResponseMessage Method(HttpClient client, HttpMethod method, Uri uri, string contentType, HttpContent body)
        {
            CheckNull(client, "client");
            CheckNull(uri, "uri");
            CheckNull(body, "body");
            CheckNull(body, "contentType");

            return client.Send(method, uri, new RequestHeaders()
            {
                ContentType = contentType
            }, body);
        }

        static HttpResponseMessage Method(HttpClient client, HttpMethod method, Uri uri)
        {
            CheckNull(client, "client");
            CheckNull(uri, "uri");
            return client.Send(method, uri);
        }

        static HttpResponseMessage Method(HttpClient client, HttpMethod method, string uri)
        {
            CheckNull(uri, "uri");
            return Method(client, method, new Uri(uri, UriKind.RelativeOrAbsolute));
        }
    }
}
