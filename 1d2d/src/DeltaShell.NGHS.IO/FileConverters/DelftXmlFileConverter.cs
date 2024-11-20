using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace DeltaShell.NGHS.IO.FileConverters
{
    /// <summary>
    /// Xml file converter which evaluates and deserializes an xml reader object.
    /// </summary>
    public static class DelftXmlFileConverter
    {
        /// <summary>
        /// De-serializes a <typeparamref name="T"/> object from the supplied <see cref="file"/> and
        /// lists all unsupported elements and attributes
        /// </summary>
        /// <param name="file"><see cref="StreamReader"/> to the xml file</param>
        /// <param name="unsupportedFeatures">List of unsupported item messages</param>
        /// <typeparam name="T">The type to parse from the <paramref name="file"/></typeparam>
        /// <returns>Parsed <typeparamref name="T"/> object</returns>
        /// <exception cref="ArgumentException">When one of the arguments is null</exception>
        /// <exception cref="XmlException">When de-serializing fails</exception>
        public static T Convert<T>(StreamReader file, List<string> unsupportedFeatures) where T : class
        {
            if (file == null)
            {
                throw new ArgumentException("Reader cannot be null");
            }

            if (unsupportedFeatures == null)
            {
                throw new ArgumentException("Unsupported Features cannot be null");
            }

            var serializer = new XmlSerializer(typeof(T));

            serializer.UnknownElement += (sender, args) =>
            {
                unsupportedFeatures.Add($"Element: \"{args.Element.Name}\" at line {args.LineNumber} position {args.LinePosition}");
            };

            serializer.UnknownAttribute += (sender, args) =>
            {
                unsupportedFeatures.Add($"Attribute: \"{args.Attr.Name}\" at line {args.LineNumber} position {args.LinePosition}");
            };

            try
            {
                return (T) serializer.Deserialize(file);
            }
            catch (InvalidOperationException e)
            {
                throw new XmlException($"Error during parsing : {e.InnerException?.Message}");
            }
        }
    }
}