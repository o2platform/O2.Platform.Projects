//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Xml.Linq
{
    using System.Xml;
    using Microsoft.Http;

    public static partial class XElementContentExtensions
    {
        public static XElement ReadAsXElement(this HttpContent content)
        {
            using (var reader = XmlContentExtensions.ReadAsXmlReader(content))
            {
                var e = XElement.Load(reader);
                return e;
            }
        }
    }
}
