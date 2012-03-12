//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.IO;
using System.Net;
using System.Runtime;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Syndication;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Microsoft.ServiceModel.Web
{
    class HelpExampleGenerator
    {
        const int MaxDepthLevel = 256;
        public const string XmlSchemaNamespace = "http://www.w3.org/2001/XMLSchema";
        public const string XmlNamespacePrefix = "xmlns";
        public const string XmlSchemaInstanceNamespace = "http://www.w3.org/2001/XMLSchema-instance";
        public const string XmlSchemaInstanceNil = "nil";
        public const string XmlSchemaInstanceType = "type";

        static Dictionary<Type, Action<XmlSchemaObject, HelpExampleGeneratorContext>> XmlObjectHandler = new Dictionary<Type, Action<XmlSchemaObject, HelpExampleGeneratorContext>>
        {                
            { typeof(XmlSchemaComplexContent), ContentHandler },
            { typeof(XmlSchemaSimpleContent), ContentHandler },
            { typeof(XmlSchemaSimpleTypeRestriction), SimpleTypeRestrictionHandler },                
            { typeof(XmlSchemaChoice), ChoiceHandler },
            // Nothing to do, inheritance is resolved by Schema compilation process                
            { typeof(XmlSchemaComplexContentExtension), EmptyHandler },
            { typeof(XmlSchemaSimpleContentExtension), EmptyHandler },
            // No need to generate XML for these objects
            { typeof(XmlSchemaAny), EmptyHandler },
            { typeof(XmlSchemaAnyAttribute), EmptyHandler },
            { typeof(XmlSchemaAnnotated), EmptyHandler },
            { typeof(XmlSchema), EmptyHandler },
            // The following schema objects are not handled            
            { typeof(XmlSchemaAttributeGroup), ErrorHandler },
            { typeof(XmlSchemaAttributeGroupRef), ErrorHandler },
            { typeof(XmlSchemaComplexContentRestriction), ErrorHandler },
            { typeof(XmlSchemaSimpleContentRestriction), ErrorHandler },
            // Enumerations are supported by the GenerateContentForSimpleType
            { typeof(XmlSchemaEnumerationFacet), EmptyHandler },
            { typeof(XmlSchemaMaxExclusiveFacet), ErrorHandler },
            { typeof(XmlSchemaMaxInclusiveFacet), ErrorHandler },
            { typeof(XmlSchemaMinExclusiveFacet), ErrorHandler },
            { typeof(XmlSchemaMinInclusiveFacet), ErrorHandler },
            { typeof(XmlSchemaNumericFacet), ErrorHandler },
            { typeof(XmlSchemaFractionDigitsFacet), ErrorHandler },
            { typeof(XmlSchemaLengthFacet), ErrorHandler },
            { typeof(XmlSchemaMaxLengthFacet), ErrorHandler },
            { typeof(XmlSchemaMinLengthFacet), ErrorHandler },
            { typeof(XmlSchemaTotalDigitsFacet), ErrorHandler },
            { typeof(XmlSchemaPatternFacet), ErrorHandler },
            { typeof(XmlSchemaWhiteSpaceFacet), ErrorHandler },
            { typeof(XmlSchemaGroup), ErrorHandler },
            { typeof(XmlSchemaIdentityConstraint), ErrorHandler },
            { typeof(XmlSchemaKey), ErrorHandler },
            { typeof(XmlSchemaKeyref), ErrorHandler },
            { typeof(XmlSchemaUnique), ErrorHandler },
            { typeof(XmlSchemaNotation), ErrorHandler },
            { typeof(XmlSchemaAll), ErrorHandler },
            { typeof(XmlSchemaGroupRef), ErrorHandler },
            { typeof(XmlSchemaSimpleTypeUnion), ErrorHandler },
            { typeof(XmlSchemaSimpleTypeList), ErrorHandler },
            { typeof(XmlSchemaXPath), ErrorHandler },
            { typeof(XmlSchemaAttribute), XmlAttributeHandler },
            { typeof(XmlSchemaElement), XmlElementHandler },
            { typeof(XmlSchemaComplexType), XmlComplexTypeHandler },
            { typeof(XmlSchemaSequence), XmlSequenceHandler },
            { typeof(XmlSchemaSimpleType), XmlSimpleTypeHandler },
        };
        static Dictionary<Type, Action<XmlSchemaObject, HelpExampleGeneratorContext>> JsonObjectHandler = new Dictionary<Type, Action<XmlSchemaObject, HelpExampleGeneratorContext>>
        {                
            { typeof(XmlSchemaComplexContent), ContentHandler },
            { typeof(XmlSchemaSimpleContent), ContentHandler },
            { typeof(XmlSchemaSimpleTypeRestriction), SimpleTypeRestrictionHandler },                
            { typeof(XmlSchemaChoice), ChoiceHandler },
            // Nothing to do, inheritance is resolved by Schema compilation process                
            { typeof(XmlSchemaComplexContentExtension), EmptyHandler },
            { typeof(XmlSchemaSimpleContentExtension), EmptyHandler },
            // No need to generate XML for these objects
            { typeof(XmlSchemaAny), EmptyHandler },
            { typeof(XmlSchemaAnyAttribute), EmptyHandler },
            { typeof(XmlSchemaAnnotated), EmptyHandler },
            { typeof(XmlSchema), EmptyHandler },
            // The following schema objects are not handled            
            { typeof(XmlSchemaAttributeGroup), ErrorHandler },
            { typeof(XmlSchemaAttributeGroupRef), ErrorHandler },
            { typeof(XmlSchemaComplexContentRestriction), ErrorHandler },
            { typeof(XmlSchemaSimpleContentRestriction), ErrorHandler },
            // Enumerations are supported by the GenerateContentForSimpleType
            { typeof(XmlSchemaEnumerationFacet), EmptyHandler },
            { typeof(XmlSchemaMaxExclusiveFacet), ErrorHandler },
            { typeof(XmlSchemaMaxInclusiveFacet), ErrorHandler },
            { typeof(XmlSchemaMinExclusiveFacet), ErrorHandler },
            { typeof(XmlSchemaMinInclusiveFacet), ErrorHandler },
            { typeof(XmlSchemaNumericFacet), ErrorHandler },
            { typeof(XmlSchemaFractionDigitsFacet), ErrorHandler },
            { typeof(XmlSchemaLengthFacet), ErrorHandler },
            { typeof(XmlSchemaMaxLengthFacet), ErrorHandler },
            { typeof(XmlSchemaMinLengthFacet), ErrorHandler },
            { typeof(XmlSchemaTotalDigitsFacet), ErrorHandler },
            { typeof(XmlSchemaPatternFacet), ErrorHandler },
            { typeof(XmlSchemaWhiteSpaceFacet), ErrorHandler },
            { typeof(XmlSchemaGroup), ErrorHandler },
            { typeof(XmlSchemaIdentityConstraint), ErrorHandler },
            { typeof(XmlSchemaKey), ErrorHandler },
            { typeof(XmlSchemaKeyref), ErrorHandler },
            { typeof(XmlSchemaUnique), ErrorHandler },
            { typeof(XmlSchemaNotation), ErrorHandler },
            { typeof(XmlSchemaAll), ErrorHandler },
            { typeof(XmlSchemaGroupRef), ErrorHandler },
            { typeof(XmlSchemaSimpleTypeUnion), ErrorHandler },
            { typeof(XmlSchemaSimpleTypeList), ErrorHandler },
            { typeof(XmlSchemaXPath), ErrorHandler },
            { typeof(XmlSchemaElement), JsonElementHandler },
            { typeof(XmlSchemaComplexType), JsonComplexTypeHandler },
            { typeof(XmlSchemaSequence), JsonSequenceHandler },
            { typeof(XmlSchemaSimpleType), JsonSimpleTypeHandler },
        };

        public static void GenerateJsonSample(XmlSchemaSet schemaSet, XmlQualifiedName name, XmlWriter writer, IDictionary<XmlQualifiedName, Type> knownTypes)
        {
            HelpExampleGeneratorContext context = new HelpExampleGeneratorContext
            {
                currentDepthLevel = 0,
                elementDepth = new Dictionary<XmlSchemaElement, int>(),
                knownTypes = knownTypes,
                objectHandler = JsonObjectHandler,
                schemaSet = schemaSet,
                overrideElementName = "root",
                writer = writer,
            };

            if (!schemaSet.IsCompiled)
            {
                schemaSet.Compile();
            }
            InvokeHandler(schemaSet.GlobalElements[name], context);
        }

        public static void GenerateXmlSample(XmlSchemaSet schemaSet, XmlQualifiedName name, XmlWriter writer)
        {
            HelpExampleGeneratorContext context = new HelpExampleGeneratorContext
            {
                currentDepthLevel = 0,
                elementDepth = new Dictionary<XmlSchemaElement, int>(),
                knownTypes = null,
                objectHandler = XmlObjectHandler,
                schemaSet = schemaSet,
                overrideElementName = null,
                writer = writer,
            };

            if (!schemaSet.IsCompiled)
            {
                schemaSet.Compile();
            }

            InvokeHandler(schemaSet.GlobalElements[name], context);
        }

        [System.Diagnostics.DebuggerStepThrough]
        static void InvokeHandler(XmlSchemaObject schemaObject, HelpExampleGeneratorContext context)
        {
            if (++context.currentDepthLevel < MaxDepthLevel)
            {
                Action<XmlSchemaObject, HelpExampleGeneratorContext> action;
                Type objectType = schemaObject.GetType();
                if (context.objectHandler.TryGetValue(objectType, out action))
                {
                    action(schemaObject, context);
                }
                else if (objectType.Name != "EmptyParticle")
                {
                    throw new InvalidOperationException(String.Format("Handler for type {0} not found.", schemaObject.GetType().Name));
                }
                --context.currentDepthLevel;
            }
            else
            {
                throw new InvalidOperationException(String.Format("Max depth level reached at {0}.", schemaObject.GetType().Name));
            }
        }

        static void XmlAttributeHandler(XmlSchemaObject schemaObject, HelpExampleGeneratorContext context)
        {
            XmlSchemaAttribute attribute = (XmlSchemaAttribute)schemaObject;
            string content = GenerateContentForXmlSimpleType(attribute.AttributeSchemaType);
            if (String.IsNullOrEmpty(content))
            {
                context.writer.WriteAttributeString("i", XmlSchemaInstanceNil, XmlSchemaInstanceNamespace, "true");
            }
            else
            {
                context.writer.WriteAttributeString(attribute.QualifiedName.Name, attribute.QualifiedName.Namespace, content);
            }
        }

        static void ChoiceHandler(XmlSchemaObject schemaObject, HelpExampleGeneratorContext context)
        {
            XmlSchemaChoice choice = (XmlSchemaChoice)schemaObject;
            InvokeHandler(choice.Items[0], context);
        }

        static void ContentHandler(XmlSchemaObject schemaObject, HelpExampleGeneratorContext context)
        {
            XmlSchemaContentModel model = (XmlSchemaContentModel)schemaObject;
            InvokeHandler(model.Content, context);
        }

        static void SimpleTypeRestrictionHandler(XmlSchemaObject schemaObject, HelpExampleGeneratorContext context)
        {
            XmlSchemaSimpleTypeRestriction restriction = (XmlSchemaSimpleTypeRestriction)schemaObject;
            foreach (XmlSchemaObject facet in restriction.Facets)
            {
                InvokeHandler(facet, context);
            }
        }

        static void ErrorHandler(XmlSchemaObject schemaObject, HelpExampleGeneratorContext context)
        {
            throw new InvalidOperationException(String.Format("Schema object {0} not supported.", schemaObject.GetType().Name));
        }

        static void EmptyHandler(XmlSchemaObject schemaObject, HelpExampleGeneratorContext context)
        {
        }

        static void XmlElementHandler(XmlSchemaObject schemaObject, HelpExampleGeneratorContext context)
        {
            XmlSchemaElement element = (XmlSchemaElement)schemaObject;
            XmlSchemaElement contentElement = GenerateValidElementsComment(element, context);
            context.writer.WriteStartElement(element.QualifiedName.Name, element.QualifiedName.Namespace);
            if (contentElement != element)
            {
                string value = contentElement.QualifiedName.Name;
                if (contentElement.QualifiedName.Namespace != element.QualifiedName.Namespace && !String.IsNullOrEmpty(contentElement.QualifiedName.Namespace))
                {
                    string prefix = context.writer.LookupPrefix(contentElement.QualifiedName.Namespace);
                    if (prefix == null)
                    {
                        prefix = string.Concat("d", context.currentDepthLevel.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
                        context.writer.WriteAttributeString(XmlNamespacePrefix, prefix, null, contentElement.QualifiedName.Namespace);
                    }
                    value = String.Format(CultureInfo.InvariantCulture, "{0}:{1}", prefix, contentElement.QualifiedName.Name);
                }
                context.writer.WriteAttributeString("i", XmlSchemaInstanceType, XmlSchemaInstanceNamespace, value);
            }
            foreach (XmlSchemaObject constraint in contentElement.Constraints)
            {
                InvokeHandler(constraint, context);
            }
            InvokeHandler(contentElement.ElementSchemaType, context);
            context.writer.WriteEndElement();
        }

        static void XmlComplexTypeHandler(XmlSchemaObject schemaObject, HelpExampleGeneratorContext context)
        {
            XmlSchemaComplexType complexType = (XmlSchemaComplexType)schemaObject;
            foreach (XmlSchemaObject attribute in complexType.AttributeUses.Values)
            {
                InvokeHandler(attribute, context);
            }
            if (complexType.ContentModel != null)
            {
                InvokeHandler(complexType.ContentModel, context);
            }
            InvokeHandler(complexType.ContentTypeParticle, context);
            if (complexType.IsMixed)
            {
                context.writer.WriteString("This element contains text.");
            }
        }

        static void XmlSequenceHandler(XmlSchemaObject schemaObject, HelpExampleGeneratorContext context)
        {
            XmlSchemaSequence sequence = (XmlSchemaSequence)schemaObject;
            foreach (XmlSchemaObject innerObject in sequence.Items)
            {
                XmlSchemaElement element = innerObject as XmlSchemaElement;
                for (int count = 0; count < 2 && element.MaxOccurs > count; ++count)
                {
                    if (element != null && IsObject(element))
                    {

                        int instances = 0;
                        context.elementDepth.TryGetValue(element, out instances);
                        context.elementDepth[element] = ++instances;
                        if (instances < 3)
                        {
                            InvokeHandler(innerObject, context);
                        }
                        else
                        {
                            context.writer.WriteStartElement(element.QualifiedName.Name, element.QualifiedName.Namespace);
                            context.writer.WriteAttributeString("i", XmlSchemaInstanceNil, XmlSchemaInstanceNamespace, "true");
                            context.writer.WriteEndElement();
                        }
                        --context.elementDepth[element];
                    }
                    else
                    {
                        InvokeHandler(innerObject, context);
                    }
                }
            }
        }

        static void XmlSimpleTypeHandler(XmlSchemaObject schemaObject, HelpExampleGeneratorContext context)
        {
            XmlSchemaSimpleType simpleType = (XmlSchemaSimpleType)schemaObject;
            if (simpleType.QualifiedName.Namespace != "http://schemas.microsoft.com/2003/10/Serialization/"
                && simpleType.QualifiedName.Namespace != XmlSchemaNamespace
                && simpleType.QualifiedName.Name != "guid")
            {
                InvokeHandler(simpleType.Content, context);
            }
            string content = GenerateContentForXmlSimpleType(simpleType);
            if (String.IsNullOrEmpty(content))
            {
                context.writer.WriteAttributeString("i", XmlSchemaInstanceNil, XmlSchemaInstanceNamespace, "true");
            }
            else
            {
                context.writer.WriteString(content);
            }
        }

        static string GenerateContentForXmlSimpleType(XmlSchemaSimpleType simpleType)
        {
            if (simpleType.Content != null && simpleType.Content is XmlSchemaSimpleTypeRestriction)
            {
                XmlSchemaSimpleTypeRestriction restriction = (XmlSchemaSimpleTypeRestriction)simpleType.Content;
                foreach (XmlSchemaObject facet in restriction.Facets)
                {
                    if (facet is XmlSchemaEnumerationFacet)
                    {
                        XmlSchemaEnumerationFacet enumeration = (XmlSchemaEnumerationFacet)facet;
                        return enumeration.Value;
                    }
                }
            }

            if (simpleType.QualifiedName.Name == "dateTime")
            {
                DateTime dateTime = DateTime.Parse("1999-05-31T11:20:00", CultureInfo.InvariantCulture);
                return dateTime.ToString("s", CultureInfo.InvariantCulture);
            }
            else if (simpleType.QualifiedName.Name == "char")
            {
                return "97";
            }

            return GetConstantValue(simpleType.QualifiedName.Name);
        }

        static void JsonElementHandler(XmlSchemaObject schemaObject, HelpExampleGeneratorContext context)
        {
            XmlSchemaElement element = (XmlSchemaElement)schemaObject;
            XmlSchemaElement contentElement = GetDerivedTypes(element, context).FirstOrDefault();
            if (contentElement == null)
            {
                contentElement = element;
            }

            if (context.overrideElementName != null)
            {
                context.writer.WriteStartElement(null, context.overrideElementName, null);
                context.overrideElementName = null;
            }
            else
            {
                context.writer.WriteStartElement(null, element.Name, null);
            }

            if (IsArrayElementType(element))
            {
                context.writer.WriteAttributeString("type", "array");
                context.overrideElementName = "item";
            }
            else if (IsObject(element))
            {
                if (contentElement != element)
                {
                    Type derivedType = null;
                    context.knownTypes.TryGetValue(contentElement.QualifiedName, out derivedType);
                    if (derivedType != null)
                    {
                        context.writer.WriteStartAttribute(null, "__type", null);
                        context.writer.WriteString(String.Format(CultureInfo.InvariantCulture, "{0}:#{1}", derivedType.Name, derivedType.Namespace));
                        context.writer.WriteEndAttribute();
                    }
                }
                context.writer.WriteAttributeString("type", "object");
            }
            InvokeHandler(contentElement.ElementSchemaType, context);
            context.overrideElementName = null;
            context.writer.WriteEndElement();
        }

        static void JsonComplexTypeHandler(XmlSchemaObject schemaObject, HelpExampleGeneratorContext context)
        {
            XmlSchemaComplexType complexType = (XmlSchemaComplexType)schemaObject;
            if (complexType.ContentModel != null)
            {
                InvokeHandler(complexType.ContentModel, context);
            }
            InvokeHandler(complexType.ContentTypeParticle, context);
        }

        static void JsonSequenceHandler(XmlSchemaObject schemaObject, HelpExampleGeneratorContext context)
        {
            XmlSchemaSequence sequence = (XmlSchemaSequence)schemaObject;
            foreach (XmlSchemaObject innerObject in sequence.Items)
            {
                XmlSchemaElement element = innerObject as XmlSchemaElement;
                if (element != null && IsObject(element))
                {
                    int instances = 0;
                    context.elementDepth.TryGetValue(element, out instances);
                    context.elementDepth[element] = ++instances;
                    if (instances < 3)
                    {
                        InvokeHandler(innerObject, context);
                    }
                    else
                    {
                        if (context.overrideElementName != null)
                        {
                            context.writer.WriteStartElement(context.overrideElementName);
                            context.overrideElementName = null;
                        }
                        else
                        {
                            context.writer.WriteStartElement(element.QualifiedName.Name);
                        }
                        context.writer.WriteAttributeString("type", "null");
                        context.writer.WriteEndElement();
                    }
                    --context.elementDepth[element];
                }
                else
                {
                    InvokeHandler(innerObject, context);
                }
            }
        }

        static void JsonSimpleTypeHandler(XmlSchemaObject schemaObject, HelpExampleGeneratorContext context)
        {
            XmlSchemaSimpleType simpleType = (XmlSchemaSimpleType)schemaObject;
            // Enumerations return 0
            if (simpleType.Content != null && simpleType.Content is XmlSchemaSimpleTypeRestriction)
            {
                XmlSchemaSimpleTypeRestriction restriction = (XmlSchemaSimpleTypeRestriction)simpleType.Content;
                foreach (XmlSchemaObject facet in restriction.Facets)
                {
                    if (facet is XmlSchemaEnumerationFacet)
                    {
                        context.writer.WriteAttributeString(string.Empty, "type", string.Empty, "number");
                        context.writer.WriteString("0");
                        return;
                    }
                }
            }

            string value = GetConstantValue(simpleType.QualifiedName.Name);

            if (simpleType.QualifiedName.Name == "base64Binary")
            {
                char[] base64stream = value.ToCharArray();
                context.writer.WriteAttributeString(string.Empty, "type", string.Empty, "array");
                for (int i = 0; i < base64stream.Length; i++)
                {
                    context.writer.WriteStartElement("item", string.Empty);
                    context.writer.WriteAttributeString(string.Empty, "type", string.Empty, "number");
                    context.writer.WriteValue((int)base64stream[i]);
                    context.writer.WriteEndElement();
                }
            }
            else if (simpleType.QualifiedName.Name == "dateTime")
            {
                DateTime dateTime = DateTime.Parse("1999-05-31T11:20:00", CultureInfo.InvariantCulture);
                context.writer.WriteString("/Date(");
                context.writer.WriteValue((dateTime.ToUniversalTime().Ticks - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks) / 10000);

                switch (dateTime.Kind)
                {
                    case DateTimeKind.Unspecified:
                    case DateTimeKind.Local:
                        TimeSpan ts = TimeZone.CurrentTimeZone.GetUtcOffset(dateTime.ToLocalTime());
                        if (ts.Ticks < 0)
                        {
                            context.writer.WriteString("-");
                        }
                        else
                        {
                            context.writer.WriteString("+");
                        }
                        int hours = Math.Abs(ts.Hours);
                        context.writer.WriteString((hours < 10) ? "0" + hours : hours.ToString(CultureInfo.InvariantCulture));
                        int minutes = Math.Abs(ts.Minutes);
                        context.writer.WriteString((minutes < 10) ? "0" + minutes : minutes.ToString(CultureInfo.InvariantCulture));
                        break;
                    case DateTimeKind.Utc:
                        break;
                }
                context.writer.WriteString(")/");
            }
            else if (simpleType.QualifiedName.Name == "char")
            {
                context.writer.WriteString(XmlConvert.ToString('a'));
            }
            else if (!String.IsNullOrEmpty(value))
            {
                if (simpleType.QualifiedName.Name == "integer" ||
                    simpleType.QualifiedName.Name == "int" ||
                    simpleType.QualifiedName.Name == "long" ||
                    simpleType.QualifiedName.Name == "unsignedLong" ||
                    simpleType.QualifiedName.Name == "unsignedInt" ||
                    simpleType.QualifiedName.Name == "short" ||
                    simpleType.QualifiedName.Name == "unsignedShort" ||
                    simpleType.QualifiedName.Name == "byte" ||
                    simpleType.QualifiedName.Name == "unsignedByte" ||
                    simpleType.QualifiedName.Name == "decimal" ||
                    simpleType.QualifiedName.Name == "float" ||
                    simpleType.QualifiedName.Name == "double" ||
                    simpleType.QualifiedName.Name == "negativeInteger" ||
                    simpleType.QualifiedName.Name == "nonPositiveInteger" ||
                    simpleType.QualifiedName.Name == "positiveInteger" ||
                    simpleType.QualifiedName.Name == "nonNegativeInteger")
                {
                    context.writer.WriteAttributeString("type", "number");
                }
                else if (simpleType.QualifiedName.Name == "boolean")
                {
                    context.writer.WriteAttributeString("type", "boolean");
                }
                context.writer.WriteString(value);
            }
            else
            {
                context.writer.WriteAttributeString("type", "null");
            }
        }

        static string GetConstantValue(string typeName)
        {
            if (typeName == "base64Binary")
            {
                return "QmFzZSA2NCBTdHJlYW0=";
            }
            else if (typeName == "string" ||
                typeName == "normalizedString" ||
                typeName == "token" ||
                typeName == "NMTOKEN" ||
                typeName == "NMTOKENS")
            {
                return "String content";
            }
            else if (typeName == "hexBinary")
            {
                return "GpM7";
            }
            else if (typeName == "integer" || typeName == "int")
            {
                return "2147483647";
            }
            else if (typeName == "positiveInteger" || typeName == "nonNegativeInteger")
            {
                return "+2147483647";
            }
            else if (typeName == "long")
            {
                return "9223372036854775807";
            }
            else if (typeName == "unsignedLong")
            {
                return "18446744073709551615";
            }
            else if (typeName == "unsignedInt")
            {
                return "4294967295";
            }
            else if (typeName == "short")
            {
                return "32767";
            }
            else if (typeName == "unsignedShort")
            {
                return "65535";
            }
            else if (typeName == "byte")
            {
                return "127";
            }
            else if (typeName == "unsignedByte")
            {
                return "255";
            }
            else if (typeName == "decimal")
            {
                return "12678967.543233";
            }
            else if (typeName == "float")
            {
                return "1.26743237E+15";
            }
            else if (typeName == "double")
            {
                return "1.26743233E+15";
            }
            else if (typeName == "negativeInteger" || typeName == "nonPositiveInteger")
            {
                return "-12678967543233";
            }
            else if (typeName == "boolean")
            {
                return "true";
            }
            else if (typeName == "duration")
            {
                return "P428DT10H30M12.3S";
            }
            else if (typeName == "date")
            {
                return "1999-05-31";
            }
            else if (typeName == "time")
            {
                return "13:20:00.000, 13:20:00.000-05:00";
            }
            else if (typeName == "gYear")
            {
                return "1999";
            }
            else if (typeName == "gYearMonth")
            {
                return "1999-02";
            }
            else if (typeName == "gMonth")
            {
                return "--05";
            }
            else if (typeName == "gMonthDay")
            {
                return "--05-31";
            }
            else if (typeName == "gDay")
            {
                return "---31";
            }
            else if (typeName == "Name")
            {
                return "Name";
            }
            else if (typeName == "QName" || typeName == "NOTATION")
            {
                return "namespace:Name";
            }
            else if (typeName == "NCName" ||
                typeName == "ID" ||
                typeName == "IDREF" ||
                typeName == "IDREFS" ||
                typeName == "ENTITY" ||
                typeName == "ENTITY" ||
                typeName == "ID")
            {
                return "NCNameString";
            }
            else if (typeName == "anyURI")
            {
                return "http://www.example.com/";
            }
            else if (typeName == "language")
            {
                return "en-US";
            }
            else if (typeName == "guid")
            {
                return "1627aea5-8e0a-4371-9022-9b504344e724";
            }
            return null;
        }

        static XmlSchemaElement GenerateValidElementsComment(XmlSchemaElement element, HelpExampleGeneratorContext context)
        {
            XmlSchemaElement firstNonAbstractElement = element;
            StringBuilder validTypes = new StringBuilder();
            foreach (XmlSchemaElement derivedElement in GetDerivedTypes(element, context))
            {
                if (firstNonAbstractElement == element)
                {
                    firstNonAbstractElement = derivedElement;
                }
                if (validTypes.Length > 0)
                {
                    validTypes.AppendFormat(", {0}", derivedElement.Name);
                }
                else
                {
                    validTypes.AppendFormat(String.Format("Valid elements of type: {0}", derivedElement.Name));
                }
            }
            if (validTypes.Length > 0)
            {
                context.writer.WriteComment(validTypes.ToString());
            }
            return firstNonAbstractElement;
        }

        static IEnumerable<XmlSchemaElement> GetDerivedTypes(XmlSchemaElement element, HelpExampleGeneratorContext context)
        {
            if (element.ElementSchemaType is XmlSchemaComplexType)
            {
                foreach (XmlSchemaElement derivedElement in context.schemaSet.GlobalElements.Values.OfType<XmlSchemaElement>().Where(e =>
                    e.IsAbstract == false &&
                    e.ElementSchemaType != element.ElementSchemaType &&
                    e.ElementSchemaType is XmlSchemaComplexType &&
                    DerivesFrom((XmlSchemaComplexType)element.ElementSchemaType, (XmlSchemaComplexType)e.ElementSchemaType)))
                {
                    yield return derivedElement;
                }
            }
        }

        static bool DerivesFrom(XmlSchemaComplexType parent, XmlSchemaComplexType child)
        {
            if (parent == child)
            {
                return true;
            }
            else if (child.BaseXmlSchemaType is XmlSchemaComplexType)
            {
                return DerivesFrom(parent, (XmlSchemaComplexType)child.BaseXmlSchemaType);
            }
            else
            {
                return false;
            }
        }

        static bool IsArrayElementType(XmlSchemaElement element)
        {
            if (element.ElementSchemaType is XmlSchemaComplexType)
            {
                XmlSchemaComplexType complexType = element.ElementSchemaType as XmlSchemaComplexType;
                if (complexType.ContentTypeParticle != null && complexType.ContentTypeParticle is XmlSchemaSequence)
                {
                    XmlSchemaSequence sequence = complexType.ContentTypeParticle as XmlSchemaSequence;
                    if (sequence.Items.Count > 0)
                    {
                        XmlSchemaElement firstElement = sequence.Items[0] as XmlSchemaElement;
                        if (firstElement != null && firstElement.MaxOccurs > 1)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        static bool IsObject(XmlSchemaElement element)
        {
            return element.ElementSchemaType is XmlSchemaComplexType;
        }

        class HelpExampleGeneratorContext
        {
            public string overrideElementName;
            public int currentDepthLevel;
            public IDictionary<XmlQualifiedName, Type> knownTypes;
            public XmlSchemaSet schemaSet;
            public IDictionary<XmlSchemaElement, int> elementDepth;
            public XmlWriter writer;
            public Dictionary<Type, Action<XmlSchemaObject, HelpExampleGeneratorContext>> objectHandler;
        }
    }
}
