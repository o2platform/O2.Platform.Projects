using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Xml.Schema.Linq;
using System.Xml;
using O2.Kernel.ExtensionMethods;
using System.IO;
using System.Windows.Forms;

namespace O2.DotNetWrappers.ExtensionMethods
{
    public static class Linq_ExtensionMethods
    {

        #region Linq XML

        public static XDocument xDocument(this string xml)
        {
            var xmlToLoad = xml.fileExists() ? xml.fileContents() : xml;
            if (xmlToLoad.valid())
            {
                if (xmlToLoad.starts("\n"))       // checks for the cases where there the text starts with \n (which will prevent the document to be loaded
                    xmlToLoad = xmlToLoad.trim();
                XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
                xmlReaderSettings.XmlResolver = null;
                xmlReaderSettings.ProhibitDtd = false;
                using (StringReader stringReader = new StringReader(xmlToLoad))
                using (XmlReader xmlReader = XmlReader.Create(stringReader, xmlReaderSettings))
                    return XDocument.Load(xmlReader);
            }
            return null;

        }

        public static XElement xRoot(this string xml)
        {
            if (xml.valid())    // checks if the string is not empty
            {                
                var xDocument = xml.xDocument();
                if (xDocument != null)
                    return xDocument.Root;
            }
            return null;
        }

        public static IEnumerable<XElement> allNodes(this XElement xElement)
        {
            return xElement.DescendantsAndSelf();
        }

        public static string name(this XElement xElement)
        {
            return xElement.Name.str();
        }

        public static bool hasDataForChildTreeNodes(this XElement xElement)
        {
            return xElement.Nodes().Any() ||
                   xElement.Attributes().Any() ||
                   (xElement.Nodes().Any() && xElement.Value.valid());
        }

        public static XElement xElement(this XTypedElement xTypedElement)
        {
            return (XElement)xTypedElement.prop("Untyped");
        }

        public static string xElementName(this XTypedElement xTypedElement)
        {
            return xTypedElement.xElement().Name.str();
        }

        public static XName xName(this string name)
        {
            return XName.Get(name);
        }

        // Descendants returns all child xElements
        public static List<XElement> elementsAll(this XElement xElement)
        {
            return xElement.Descendants().ToList();
        }

        public static List<XElement> elementsAll(this XElement xElement, string name)
        {
            return xElement.Descendants(name.xName()).ToList();
        }

        public static List<XElement> elements(this XElement xElement)
        {
            return xElement.elements(false);
        }


        public static List<XElement> elements(this XElement xElement, string elementName)
        {
            return xElement.elements(elementName, false);
        }

        public static List<XElement> elements(this XElement xElement, bool includeSelf)
        {
            return xElement.elements("", includeSelf);
        }

        // Elements returns just the direct childs    	    	
        public static List<XElement> elements(this XElement xElement, string elementName, bool includeSelf)
        {
            var xElements = (elementName.valid())
                                ? xElement.Elements(elementName).ToList()
                                : xElement.Elements().ToList();
            if (includeSelf)
                xElements.Add(xElement);
            return xElements;
        }

        public static List<XElement> elements(this IEnumerable<XElement> xElements)
        {
            return xElements.elements(false);
        }

        public static List<XElement> elements(this IEnumerable<XElement> xElements, string elementName)
        {
            return xElements.elements(elementName, false);
        }
        public static List<XElement> elements(this IEnumerable<XElement> xElements, bool includeSelf)
        {
            return xElements.elements("", includeSelf);
        }

        public static List<XElement> elements(this IEnumerable<XElement> xElements, string elementName, bool includeSelf)
        {
            var childXElements = new List<XElement>();
            xElements.forEach<XElement>((xElement) => childXElements.AddRange(xElement.elements(elementName, includeSelf)));
            return childXElements;
        }

        public static XAttribute attribute(this XElement xElement, string name)
        {
            if (xElement != null)
                return xElement.Attribute(name);
            "in XElement.attribute(...), xElement was null (name = {0})".error(name);
            return null;
        }

        public static string attributeValue(this XElement xElement, string name)
        {
            return xElement.attribute(name).value();
        }

        public static string value(this XAttribute xAttribute)
        {
            if (xAttribute != null)
                return xAttribute.Value;
            "in XAttribute.value(...), xAttribute was null".error();
            return null;
        }

        public static List<XAttribute> attributes(this XElement xElement)
        {
            return xElement.attributes("");
        }
        public static List<XAttribute> attributes(this XElement xElement, string attributeName)
        {
            return attributeName.valid()
                ? xElement.Attributes(attributeName.xName()).ToList()
                : xElement.Attributes().ToList();
        }

        public static List<XAttribute> attributes(this IEnumerable<XElement> xElements)
        {
            return xElements.attributes("");
        }

        public static List<XAttribute> attributes(this IEnumerable<XElement> xElements, string attributeName)
        {
            var attributes = new List<XAttribute>();
            xElements.forEach<XElement>((xElement) => attributes.AddRange(xElement.attributes(attributeName)));
            return attributes;
        }

        public static List<string> values(this IEnumerable<XAttribute> xAttributes)
        {
            return xAttributes.stringList();
        }
    
        public static List<string> stringList(this IEnumerable<XAttribute> xAttributes)
        {
            var stringList = new List<String>();
            xAttributes.forEach<XAttribute>((xAttribute) => stringList.Add(xAttribute.Value));
            return stringList;
        }

        public static string value(this XElement xElement)
        {
            return xElement.Value;
        }

        public static List<string> values(this IEnumerable<XElement> xElements)
        {
            var values = new List<string>();
            xElements.forEach<XElement>((xElement) => values.Add(xElement.value()));
            return values;
        }

        public static bool name(this XElement xElement, string name)
        {
            return xElement.name() == name;
        }

        public static XElement element(this XElement xElement, string elementName)
        {
            if (xElement != null)
                foreach (var childElement in xElement.elements())
                    if (childElement.name() == elementName)
                        return childElement;
            return null;
        }

        public static XElement element(this IEnumerable<XElement> xElements, string elementName)
        {
            if (xElements != null)
            {
                // first search in the current list
                foreach (var xElement in xElements)
                    if (xElement.name() == elementName)
                        return xElement;
                // then search in the current list childs
                foreach (var childElement in xElements.elements())
                    if (childElement.name() == elementName)
                        return childElement;
            }
            return null;
        }

        public static string innerXml(this XElement xElement)
        {
            if (xElement == null)
                return "";
            var reader = xElement.CreateReader();
            reader.MoveToContent();
            return reader.ReadInnerXml();
        }

        public static XElement xElement(this XNode xNode)
        {
            if (xNode is XElement)
                return (XElement)xNode;
            return null;
        }        


        #endregion

        #region Controls - TreeView

        public static TreeNode add_Node(this TreeView treeView, XElement xElement)
        {
            return treeView.add_Node(xElement.name(), xElement, xElement.hasDataForChildTreeNodes());
        }

        public static TreeNode add_Node(this TreeNode treeNode, XElement xElement)
        {
            return treeNode.add_Node(xElement.name(), xElement, xElement.hasDataForChildTreeNodes());
        }

        public static TreeNode add_Node(this TreeNode treeNode, XAttribute xAttribute)
        {
            return treeNode.add_Node("{0}: {1}".format(xAttribute.Name, xAttribute.Value));
        }

        public static string getNormalizedValue(this XElement xElement)
        {
            var value = xElement.Value;
            if (value.valid())
                xElement.Nodes().forEach<XElement>(
                    (element) => value = value.replace(element.Value, ""));
            return value.trim();
        }

        public static TreeView autoExpandXElementData(this TreeView treeView)
        {
            //var onBeforeExpand = "onBeforeExpand"
            if (treeView.hasEventHandler("BeforeExpand"))  	// don't add if there is already an onBeforeExpand event already mapped        	        		
                return treeView;
            treeView.beforeExpand<XElement>(
                (xElement) =>
                {
                    treeView.current().clear();
                    xElement.Nodes().forEach<XElement>(
                            (element) => treeView.current().add_Node(element));

                    xElement.Attributes().forEach<XAttribute>(
                            (attribute) => treeView.current().add_Node(attribute));

                    var value = xElement.getNormalizedValue();
                    if (value.valid())
                        treeView.current().add_Node("value: {0}".format(value));
                });
            return treeView;
        }

        public static TreeView xmlShow(this TreeView treeView, string xml)
        {
            return treeView.showXml(xml);
        }

        public static TreeView showXml(this TreeView treeView, object dataToLoad)
        {
            try
            {
                XElement xElement = null;
                if (dataToLoad is string)
                    xElement = ((string)dataToLoad).xRoot();
                else if (dataToLoad is XTypedElement)
                    xElement = ((XTypedElement)dataToLoad).xElement();

                if (xElement != null)
                {
                    treeView.clear();
                    treeView.autoExpandXElementData();
                    treeView.add_Node(xElement);
                    treeView.expand();
                }
            }
            catch (Exception ex)
            {
                ex.log(ex.Message);
            }
            return treeView;
        }

        public static TreeView showXml(this TreeView treeView, XElement xElement)
        {
            treeView.clear();
            treeView.autoExpandXElementData();
            treeView.add_Node(xElement);
            treeView.expand();
            return treeView;
        }

        public static TreeNode showXml(this TreeNode treeNode, List<XElement> xElements)
        {
            foreach (var xElement in xElements)
                treeNode.showXml(xElement);
            return treeNode;
        }

        public static TreeNode showXml(this TreeNode treeNode, XElement xElement)
        {
            if (treeNode.TreeView != null)
                treeNode.TreeView.autoExpandXElementData();
            treeNode.add_Node(xElement);
            treeNode.expand();
            return treeNode;
        }

        public static TreeNode showXml(this TreeNode treeNode, object dataToLoad)
        {
            try
            {
                XElement xElement = null;
                if (dataToLoad is string)
                    xElement = ((string)dataToLoad).xRoot();
                else if (dataToLoad is XTypedElement)
                    xElement = ((XTypedElement)dataToLoad).xElement();

                if (xElement != null)
                {
                    if (treeNode.TreeView != null)
                        treeNode.TreeView.autoExpandXElementData();
                    treeNode.add_Node(xElement);
                    treeNode.expand();
                }
            }
            catch (Exception ex)
            {
                ex.log(ex.Message);
            }
            return treeNode;
        }

        #endregion
    }
}
