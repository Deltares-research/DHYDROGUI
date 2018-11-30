using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using DeltaShell.Dimr.xsd;

namespace DeltaShell.NGHS.IO.FileConverters
{
    /// <summary>
    /// Validator that validates xml input based on it's xsd file definition.
    /// </summary>
    public static class DelftXsdValidator
    {
        public static void CollectUnsupportedFeatures(XmlSerializer serializer, List<string> unsupportedFeatures)
        {
            if (serializer == null) { throw new ArgumentException("Serializer cannot be null"); }

            ValidateAttributes(serializer, unsupportedFeatures);
            ValidateElements(serializer,   
                
                unsupportedFeatures);
        }

        private static void ValidateElements(XmlSerializer serializer, List<string> errorMessages)
        {
            serializer.UnknownElement += (sender, args) =>
            {
                errorMessages.Add($"Element: {args.Element.Name}");

                var xmlParsedObject = args.ObjectBeingDeserialized as IXmlParsedObject;

                if (xmlParsedObject != null && xmlParsedObject?.UnKnownElements == null)
                {
                    xmlParsedObject.UnKnownElements = new List<XmlElement>();
                }

                xmlParsedObject?.UnKnownElements.Add(args.Element);
            };
        }

        private static void ValidateAttributes(XmlSerializer serializer, List<string> errorMessages)
        {
            serializer.UnknownAttribute += (sender, args) =>
            {
                errorMessages.Add($"Attribute: {args.Attr.Name}");

                var xmlParsedObject = args.ObjectBeingDeserialized as IXmlParsedObject;

                if (xmlParsedObject != null && xmlParsedObject?.UnKnownAttributes == null)
                {
                    xmlParsedObject.UnKnownAttributes = new List<XmlAttribute>();
                }

                xmlParsedObject?.UnKnownAttributes.Add(args.Attr);
            };
        }
    }
}