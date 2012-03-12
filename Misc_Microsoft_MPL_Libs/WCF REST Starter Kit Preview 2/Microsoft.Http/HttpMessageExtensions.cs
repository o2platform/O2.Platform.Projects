//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Http
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Net;

    public static partial class HttpMessageExtensions
    {
        public static HttpResponseMessage CreateResponse(this HttpRequestMessage request, HttpStatusCode code)
        {
            var resp = new HttpResponseMessage()
                {
                    Uri = request.Uri,
                    Method = request.Method,
                    StatusCode = code,
                    Request = request,
                };
            return resp;
        }

        public static T GetPropertyOrDefault<T>(this HttpRequestMessage message)
        {
            if (!message.HasProperties)
            {
                return default(T);
            }
            return message.Properties.OfType<T>().SingleOrDefault();
        }
        public static T GetPropertyOrDefault<T>(this HttpResponseMessage message)
        {
            if (!message.HasProperties)
            {
                return default(T);
            }
            return message.Properties.OfType<T>().SingleOrDefault();
        }

        static string ToString(HttpStatusCode code)
        {
            return code + " (" + (int)code + ")";
        }

        static Exception CreateException(HttpResponseMessage response, HttpStatusCode acceptable, Array other)
        {
            if (other.Length == 0)
            {
                return new ArgumentOutOfRangeException(null, ToString(response.StatusCode) + " is not " + ToString(acceptable));
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(ToString(acceptable));
                foreach (var x in other)
                {
                    sb.Append(", ");
                    sb.Append(ToString((HttpStatusCode)x));
                }
                return new ArgumentOutOfRangeException(null, ToString(response.StatusCode) + " is not one of the following: " + sb);
            }
        }

        public static HttpResponseMessage EnsureStatusIs(this HttpResponseMessage response, HttpStatusCode acceptable, params HttpStatusCode[] otherAcceptable)
        {
            if (response.StatusCode != acceptable && !otherAcceptable.Contains(response.StatusCode))
            {
                response.Dispose();
                var e = CreateException(response, acceptable, otherAcceptable);
                throw e;
            }
            return response;
        }

        public static HttpResponseMessage EnsureStatusIs(this HttpResponseMessage response, int acceptable, params int[] otherAcceptable)
        {
            if ((int)response.StatusCode != acceptable && !otherAcceptable.Contains((int)response.StatusCode))
            {
                response.Dispose();
                var e = CreateException(response, (HttpStatusCode)acceptable, otherAcceptable);
                throw e;
            }
            return response;
        }

        static readonly HttpStatusCode[] additionalSuccessCodes = { 
                                                                      HttpStatusCode.Created,
                                                                      HttpStatusCode.Accepted,
                                                                      HttpStatusCode.NonAuthoritativeInformation,
                                                                      HttpStatusCode.NoContent,
                                                                      HttpStatusCode.ResetContent, 
                                                                      HttpStatusCode.PartialContent
                                                                  };
        public static HttpResponseMessage EnsureStatusIsSuccessful(this HttpResponseMessage response)
        {
            return EnsureStatusIs(response, HttpStatusCode.OK, additionalSuccessCodes);
        }
    }
}