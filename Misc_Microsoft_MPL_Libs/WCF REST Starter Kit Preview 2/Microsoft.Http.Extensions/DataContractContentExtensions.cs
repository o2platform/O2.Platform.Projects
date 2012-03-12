//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Runtime.Serialization
{
    using System;
    using Microsoft.Http;

    public static class DataContractContentExtensions
    {
        public static T ReadAsDataContract<T>(this HttpContent content)
        {
            return ReadAsDataContract<T>(content, new DataContractSerializer(typeof(T)));
        }

        public static T ReadAsDataContract<T>(this HttpContent content, params Type[] extraTypes)
        {
            return ReadAsDataContract<T>(content, new DataContractSerializer(typeof(T), extraTypes));
        }

        public static T ReadAsDataContract<T>(this HttpContent content, DataContractSerializer serializer)
        {
            using (var stream = content.ReadAsStream())
            {
                return (T)serializer.ReadObject(stream);
            }
        }
    }
}
