using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using DelftTools.Utils.Reflection;
using DeltaShell.Dimr.xsd;
using DeltaShell.NGHS.IO.FileConverters;
using log4net;

namespace DeltaShell.NGHS.IO.FileReaders
{
    /// <summary>
    /// Reads the contents of an imported xml configuration file and returns data access model object.
    /// </summary>
    public static class DelftConfigXmlFileParser
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DelftConfigXmlFileParser));

        private static readonly Dictionary<string, Type> LookupSerializer = new Dictionary<string, Type>
        {
            {"dimrConfig".ToLower(),       typeof(dimrXML)},
            {"rtcDataConfig".ToLower(),    typeof(RTCDataConfigXML)},
            {"rtcRuntimeConfig".ToLower(), typeof(RtcRuntimeConfigXML)},
            {"rtcToolsConfig".ToLower(),   typeof(RtcToolsConfigXML)},
            {"treeVectorFile".ToLower(),   typeof(TreeVectorFileXML)},
            {"TimeSeries".ToLower(),       typeof(TimeSeriesCollectionComplexType)}
        };

        /// <summary>
        /// Reads an <see cref="IXmlParsedObject"/> from the <param name="xmlFileSource"/> file
        /// </summary>
        /// <param name="xmlFileSource">Path to the xml file</param>
        /// <returns>De-serialized <see cref="IXmlParsedObject"/> object</returns>
        public static IXmlParsedObject Read(string xmlFileSource)
        {
            var rootElement = GetXmlConfigFileRootElement(xmlFileSource);
            if (rootElement == null)
            {
                throw new ArgumentException("Root element cannot be found");
            }

            using (var reader = rootElement.CreateReader())
            {
                var rootName = rootElement.Name.LocalName;

                if (string.IsNullOrEmpty(rootName))
                {
                    throw new ArgumentException("Rootname cannot be empty");
                }

                if (!LookupSerializer.TryGetValue(rootName.ToLower(), out var serializerType))
                {
                    throw new ArgumentException($"Can not find serializer for {rootName}");
                }

                var unsupportedFeatures = new List<string>();

                var dataAccessModel = TypeUtils.CallStaticGenericMethod(typeof(DelftXmlFileConverter), nameof(DelftXmlFileConverter.Convert), serializerType, reader, unsupportedFeatures);

                LogMissingFeatures(unsupportedFeatures, xmlFileSource);

                return (IXmlParsedObject)dataAccessModel;
            }
        }

        private static XElement GetXmlConfigFileRootElement(string xmlFileSource)
        {
            if (string.IsNullOrEmpty(xmlFileSource))
            {
                throw new FileReadingException($"Configuration file {xmlFileSource} cannot be found");
            }

            XDocument xmlConfigFile;

            try
            {
                xmlConfigFile = XDocument.Load(xmlFileSource);
            }
            catch
            {
                throw new XmlException("Unable to read the file due to invalid file format");
            }

            return xmlConfigFile?.Root;
        }

        private static void LogMissingFeatures(List<string> unsupportedFeatures, string xmlFileSource)
        {
            if (unsupportedFeatures.Count == 0) return;

            var fileName = xmlFileSource?.Split('\\').LastOrDefault() ?? "";
            var message = string.Join(Environment.NewLine, unsupportedFeatures);
                
            Log.InfoFormat($"The following features in the {fileName} file are not conforming with the xsd file: {message}");
        }
    }
}
