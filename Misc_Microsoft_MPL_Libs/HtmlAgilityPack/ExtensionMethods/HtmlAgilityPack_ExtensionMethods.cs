using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using FluentSharp.CoreLib;
using O2.DotNetWrappers.ExtensionMethods;

//O2Ref:System.Xml.Linq.dll
//O2Ref:System.Xml.dll

//O2File:_Extra_methods_WinForms_TreeView.cs

namespace O2.XRules.Database.Utils
{
    public static class HtmlAgilityPack_ExtensionMethods
    {
        #region HtmlAgilityPack.HtmlDocument

        public static HtmlAgilityPack.HtmlDocument htmlDocument(this string htmlCode)
        {
            var htmlDocument = new HtmlAgilityPack.HtmlDocument();
            htmlDocument.LoadHtml(htmlCode);
            return htmlDocument;
        }

        public static string html(this HtmlAgilityPack.HtmlDocument htmlDocument)
        {
            return htmlDocument.DocumentNode.OuterHtml;
        }

        public static List<HtmlAgilityPack.HtmlNode> filter(this HtmlAgilityPack.HtmlDocument htmlDocument, string query)
        {
            return htmlDocument.select(query);
        }

        public static List<HtmlAgilityPack.HtmlNode> select(this HtmlAgilityPack.HtmlDocument htmlDocument, string query)
        {
            return htmlDocument.DocumentNode.SelectNodes(query).toList<HtmlAgilityPack.HtmlNode>();
        }

        public static List<HtmlAgilityPack.HtmlNode> links(this HtmlAgilityPack.HtmlDocument htmlDocument)
        {
            return htmlDocument.select("//a");
        }

        #endregion

        #region HtmlAgilityPack.HtmlNode

        public static List<string> html(this List<HtmlAgilityPack.HtmlNode> htmlNodes)
        {
            return htmlNodes.outerHtml();
        }

        public static List<string> outerHtml(this List<HtmlAgilityPack.HtmlNode> htmlNodes)
        {
            var outerHtml = new List<string>();
            foreach (var htmlNode in htmlNodes)
                outerHtml.add(htmlNode.outerHtml());
            return outerHtml;
        }

        public static string html(this HtmlAgilityPack.HtmlNode htmlNode)
        {
            return htmlNode.outerHtml();
        }

        public static string outerHtml(this HtmlAgilityPack.HtmlNode htmlNode)
        {
            return htmlNode.OuterHtml;
        }

        public static string innerHtml(this HtmlAgilityPack.HtmlNode htmlNode)
        {
            return htmlNode.InnerHtml;
        }

        public static List<string> innerHtml(this List<HtmlAgilityPack.HtmlNode> htmlNodes)
        {
            var outerHtml = new List<string>();
            foreach (var htmlNode in htmlNodes)
                outerHtml.add(htmlNode.innerHtml());
            return outerHtml;
        }

        public static string value(this HtmlAgilityPack.HtmlNode htmlNode)
        {
            return htmlNode.innerHtml();
        }

        public static List<string> values(this List<HtmlAgilityPack.HtmlNode> htmlNodes)
        {
            return htmlNodes.innerHtml();
        }

        public static string value(this HtmlAgilityPack.HtmlAttribute attribute)
        {
            return attribute.Value;
        }
        #endregion
    }

    public static class HtmlAgilityPack_ExtensionMethods_Elements
    {

        public static List<HtmlAgilityPack.HtmlNode> nodes(this List<HtmlAgilityPack.HtmlNode> htmlNodes)
        {
            return htmlNodes.nodes("");
        }

        public static List<HtmlAgilityPack.HtmlNode> nodes(this List<HtmlAgilityPack.HtmlNode> htmlNodes, string nodeName)
        {
            var allNodes = new List<HtmlAgilityPack.HtmlNode>();
            foreach (var htmlNode in htmlNodes)
                allNodes.add(htmlNode.nodes(nodeName));
            return allNodes;
        }

        public static List<HtmlAgilityPack.HtmlNode> nodes(this HtmlAgilityPack.HtmlNode htmlNode)
        {
            return htmlNode.nodes("");
        }
        public static List<HtmlAgilityPack.HtmlNode> nodes(this HtmlAgilityPack.HtmlNode htmlNode, string nodeName)
        {
            var htmlNodes = new List<HtmlAgilityPack.HtmlNode>();
            foreach (var node in htmlNode.ChildNodes)
                if (nodeName.valid().isFalse() || node.Name == nodeName)
                    htmlNodes.add(node);
            return htmlNodes;
        }

        public static HtmlAgilityPack.HtmlNode node(this HtmlAgilityPack.HtmlNode htmlNode, string nodeName)
        {
            foreach (var node in htmlNode.ChildNodes)
                if (nodeName.valid().isFalse() || node.Name == nodeName)
                    return htmlNode;
            return null;
        }
    }

    public static class HtmlAgilityPack_ExtensionMethods_Attributes
    {
        public static List<HtmlAgilityPack.HtmlAttribute> attributes(this List<HtmlAgilityPack.HtmlNode> htmlNodes)
        {
            return htmlNodes.attributes("");
        }

        public static List<HtmlAgilityPack.HtmlAttribute> attributes(this List<HtmlAgilityPack.HtmlNode> htmlNodes, string attributeName)
        {
            var allAttributes = new List<HtmlAgilityPack.HtmlAttribute>();
            foreach (var htmlNode in htmlNodes)
                allAttributes.add(htmlNode.attributes(attributeName));
            return allAttributes;
        }

        public static List<HtmlAgilityPack.HtmlAttribute> attributes(this HtmlAgilityPack.HtmlNode htmlNode)
        {
            return htmlNode.attributes("");
        }
        public static List<HtmlAgilityPack.HtmlAttribute> attributes(this HtmlAgilityPack.HtmlNode htmlNode, string attributeName)
        {
            var attributes = new List<HtmlAgilityPack.HtmlAttribute>();
            foreach (var htmlAttribute in htmlNode.Attributes)
                if (attributeName.valid().isFalse() || htmlAttribute.Name == attributeName)
                    attributes.add(htmlAttribute);
            return attributes;
        }

        public static HtmlAgilityPack.HtmlAttribute attribute(this HtmlAgilityPack.HtmlNode htmlNode, string attributeName)
        {
            foreach (var htmlAttribute in htmlNode.Attributes)
                if (attributeName.valid().isFalse() || htmlAttribute.Name == attributeName)
                    return htmlAttribute;
            return null;
        }

        public static List<string> names(this List<HtmlAgilityPack.HtmlAttribute> htmlAttributes)
        {
            var names = new List<string>();
            foreach (var htmlAttribute in htmlAttributes)
                if (names.Contains(htmlAttribute.Name).isFalse())
                    names.add(htmlAttribute.Name);
            return names;
        }

        public static List<string> values(this List<HtmlAgilityPack.HtmlAttribute> htmlAttributes)
        {
            var values = new List<string>();
            foreach (var htmlAttribute in htmlAttributes)
                values.add(htmlAttribute.Value);
            return values;
        }

    }
}
