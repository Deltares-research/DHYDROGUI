using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using DeltaShell.Dimr.xsd;

public static class DelftXsdValidator
{
    public static XmlSerializer ValidateDataObjectModel(XmlSerializer serializer, List<string> errorMessages)
    {
        if (serializer == null) { throw new ArgumentException("Serializer cannot be null"); }
        if (errorMessages == null) { throw new ArgumentException("Error Report cannot be null"); }

        ValidateAttributes(serializer, errorMessages);
        ValidateElements(serializer, errorMessages);
        //errorList = errorReport;
        
        //errorList?.Invoke(($"The following items do not match the xsd: "), errorMessages);

        return serializer;
    }

    private static void ValidateElements(XmlSerializer serializer, List<string> errorMessages)
    {
        serializer.UnknownElement += (sender, args) =>
        {
            errorMessages.Add($" Element: {args.Element.Name} is missing");

            var xmlParsedObject = args.ObjectBeingDeserialized as IXmlParsedObject;

            if (xmlParsedObject?.UnKnownElements == null)
            {
                xmlParsedObject.UnKnownElements = new List<XmlElement>();
            }

            xmlParsedObject.UnKnownElements.Add(args.Element);
        };
    }

    private static void ValidateAttributes(XmlSerializer serializer, List<string> errorMessages)
    {
        serializer.UnknownAttribute += (sender, args) =>
        {
            errorMessages.Add($" Attribute: {args.Attr.Name} is missing");

            var xmlParsedObject = args.ObjectBeingDeserialized as IXmlParsedObject;

            if (xmlParsedObject?.UnKnownAttributes == null)
            {
                xmlParsedObject.UnKnownAttributes = new List<XmlAttribute>();
            }

            xmlParsedObject.UnKnownAttributes.Add(args.Attr);
        };
    }
}