using System;
using System.Collections.Generic;
using System.Xml;
using DeltaShell.NGHS.IO.Factories;

namespace DeltaShell.NGHS.IO.FileConverters
{
    /// <summary>
    /// Xml file converter which evaluates and deserializes an xml reader object.
    /// </summary>
    public static class DelftXmlFileConverter
    {
        public static object Convert(XmlReader file, string rootName, List<string> unsupportedFeatures)
        {
            if (file == null)
            {
                throw new ArgumentException("Reader cannot be null");
            }

            if (rootName == null)
            {
                throw new ArgumentException("Rootname cannot be empty");
            }

            if (unsupportedFeatures == null)
            {
                throw new ArgumentException("Unsupported Features cannot be null");
            }

            var selector = new DelftConfigXmlSerializerSelector();
            var serializer = selector.ReturnSerializer(rootName);
            DelftXsdValidator.CollectUnsupportedFeatures(serializer, unsupportedFeatures);

            var dataAccessObject = serializer.Deserialize(file);

            return dataAccessObject;
        }
    }
}