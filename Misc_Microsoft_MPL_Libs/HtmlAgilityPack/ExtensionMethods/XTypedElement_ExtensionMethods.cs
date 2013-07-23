using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentSharp.CoreLib;
using Xml.Schema.Linq;
using System.Xml.Linq;
using System.Windows.Forms;

namespace FluentSharp.For_XmlSchemaLinq
{
    public static class XTypedElement_ExtensionMethods
    {
        public static XElement xElement(this XTypedElement xTypedElement)
        {
            return (XElement)xTypedElement.prop("Untyped");
        }
        public static string xElementName(this XTypedElement xTypedElement)
        {
            return xTypedElement.xElement().Name.str();
        }
    }
}
