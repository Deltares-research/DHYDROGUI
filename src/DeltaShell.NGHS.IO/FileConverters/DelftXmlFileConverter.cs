using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using DeltaShell.Dimr.xsd;

namespace DeltaShell.NGHS.IO.FileConverters
{
    /// <summary>
    /// Xml file converter which evaluates and deserializes an xml reader object.
    /// </summary>
    public static class DelftXmlFileConverter
    {
        private static readonly Dictionary<string, Type> LookupSerializer = new Dictionary<string, Type>
        {
            {"dimrConfig".ToLower(),       typeof(dimrXML)},
            {"rtcDataConfig".ToLower(),    typeof(RTCDataConfigXML)},
            {"rtcRuntimeConfig".ToLower(), typeof(RtcRuntimeConfigXML)},
            {"rtcToolsConfig".ToLower(),   typeof(RtcToolsConfigXML)},
            {"treeVectorFile".ToLower(),   typeof(TreeVectorFileXML)},
            {"TimeSeries".ToLower(),       typeof(TimeSeriesCollectionComplexType)}
        };

        public static object Convert(XmlReader file, string rootName, List<string> unsupportedFeatures)
        {
            if (file == null)
            {
                throw new ArgumentException("Reader cannot be null");
            }

            if (string.IsNullOrEmpty(rootName))
            {
                throw new ArgumentException("Rootname cannot be empty");
            }

            if (unsupportedFeatures == null)
            {
                throw new ArgumentException("Unsupported Features cannot be null");
            }

            Type serializerType;
            if (!LookupSerializer.TryGetValue(rootName.ToLower(), out serializerType))
            {
                throw new ArgumentException($"Can not find serializer for {rootName}");
            }

            var serializer = new XmlSerializer(serializerType);

            DelftXsdValidator.CollectUnsupportedFeatures(serializer, unsupportedFeatures);

            return serializer.Deserialize(file);
        }
    }
}