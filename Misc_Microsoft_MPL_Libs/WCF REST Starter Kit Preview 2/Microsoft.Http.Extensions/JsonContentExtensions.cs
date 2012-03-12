//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Runtime.Serialization.Json
{
    using System;
    using System.Xml;
    using System.Xml.Linq;
    using Microsoft.Http;

    public static class JsonContentExtensions
    {

        static XmlDictionaryReaderQuotas DefaultQuotas
        {
            get
            {
                return XmlDictionaryReaderQuotas.Max;
            }
        }
        public static T ReadAsJsonDataContract<T>(this HttpContent content)
        {
            return (T) ReadAsJsonDataContract(content, new DataContractJsonSerializer(typeof(T)));
        }
        public static T ReadAsJsonDataContract<T>(this HttpContent content, params Type[] extraTypes)
        {
            return (T) ReadAsJsonDataContract(content, new DataContractJsonSerializer(typeof(T), extraTypes));
        }

        public static object ReadAsJsonDataContract(this HttpContent content, DataContractJsonSerializer serializer)
        {
            using (var r = content.ReadAsStream())
            {
                return serializer.ReadObject(r);
            }
        }

        public static T ReadAsJsonDataContract<T>(this HttpContent content, DataContractJsonSerializer serializer)
        {
            using (var r = content.ReadAsStream())
            {
                return (T) serializer.ReadObject(r);
            }
        }
    }
}
