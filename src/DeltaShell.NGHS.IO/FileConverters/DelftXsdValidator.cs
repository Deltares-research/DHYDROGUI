using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using DeltaShell.Dimr.xsd;

public static class DelftXsdValidator
{
    public static XmlSerializer ValidateWithSerializer(XmlSerializer serializer)
    {
        if (serializer == null) { throw new ArgumentException("Serializer cannot be null"); }

        ValidateAttributes(serializer);
        ValidateElements(serializer);

        return serializer;
    }

    private static void ValidateElements(XmlSerializer serializer)
    {
        serializer.UnknownElement += (sender, args) =>
        {
            Console.WriteLine($@"Element : {args.Element.Name}");
            IXmlParsedObject xmlParsedObject = args.ObjectBeingDeserialized as IXmlParsedObject;
            if (xmlParsedObject == null)
            {
                Console.WriteLine($@"Element : {args.Element.Name} skipped");
                return;
            }

            if (xmlParsedObject.UnKnownElements == null)
            {
                xmlParsedObject.UnKnownElements = new List<XmlElement>();
            }

            xmlParsedObject.UnKnownElements.Add(args.Element);
        };
    }

    private static void ValidateAttributes(XmlSerializer serializer)
    {
        serializer.UnknownAttribute += (sender, args) =>
        {
            Console.WriteLine($@"Attribute : {args.Attr.Name}");

            IXmlParsedObject xmlParsedObject = args.ObjectBeingDeserialized as IXmlParsedObject;

            if (xmlParsedObject == null)
            {
                Console.WriteLine($@"Attribute : {args.Attr.Name} skipped");
                return;
            }

            if (xmlParsedObject.UnKnownAttributes == null)
            {
                xmlParsedObject.UnKnownAttributes = new List<XmlAttribute>();
            }

            xmlParsedObject.UnKnownAttributes.Add(args.Attr);
        };
    }
}