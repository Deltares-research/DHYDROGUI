using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
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

        public static object Read(string xmlFileSource)
        {
            if (string.IsNullOrEmpty(xmlFileSource)) {throw new FileReadingException("Configuration file cannot be found"); }

            XDocument xmlConfigFile;
            try
            {
                xmlConfigFile = XDocument.Load(xmlFileSource);
            }
            catch
            {
                throw new XmlException("Unable to read the file due to invalid file format");
            }

            var reader = xmlConfigFile?.Root?.CreateReader();
            var rootName = xmlConfigFile?.Root?.Name.LocalName;

            var unsupportedFeatures = new List<string>();
            var dataAccessModel = DelftXmlFileConverter.Convert(reader, rootName, unsupportedFeatures);

            var fileName = xmlFileSource.Split('\\').Last();
            LogMissingFeatures(unsupportedFeatures, fileName);
            return dataAccessModel;
        }

        private static void LogMissingFeatures(List<string> unsupportedFeatures, string fileName)
        {
            string message = string.Empty;
            if (unsupportedFeatures.Count != 0)
            {
                foreach (var feature in unsupportedFeatures)
                {
                    message += Environment.NewLine + feature;
                }
               
                Log.InfoFormat($"The following features in the {fileName} file are not conforming with the xsd file: {message}");
            }
        }
    }
}
