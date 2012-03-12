//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Xml.Serialization
{
    using System;
    using System.Xml;
    using Microsoft.Http;

    public static class XmlSerializerContentExtensions
    {
        public static T ReadAsXmlSerializable<T>(this HttpContent content)
        {
            return ReadAsXmlSerializable<T>(content, new XmlSerializer(typeof(T)));
        }
        public static T ReadAsXmlSerializable<T>(this HttpContent content, params Type[] extraTypes)
        {
            return ReadAsXmlSerializable<T>(content, new XmlSerializer(typeof(T), extraTypes));
        }
        public static T ReadAsXmlSerializable<T>(this HttpContent content, XmlSerializer serializer)
        {
            using (var r = content.ReadAsXmlReader())
            {
                return (T) serializer.Deserialize(r);
            }
        }
        public static object ReadAsXmlSerializable(this HttpContent content, XmlSerializer serializer)
        {
            using (var r = content.ReadAsXmlReader())
            {
                return serializer.Deserialize(r);
            }
        }
    }
}
