using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
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

        /// <summary>
        /// Reads an <see cref="IXmlParsedObject"/> from the <param name="xmlFileSource"/> file
        /// </summary>
        /// <param name="xmlFileSource">Path to the xml file</param>
        /// <typeparam name="T"><see cref="IXmlParsedObject"/> object to parse from the <see cref="xmlFileSource"/></typeparam>
        /// <returns>De-serialized <see cref="IXmlParsedObject"/> object</returns>
        public static T Read<T>(string xmlFileSource) where T : class, IXmlParsedObject
        {
            if (string.IsNullOrEmpty(xmlFileSource) || !File.Exists(xmlFileSource))
            {
                throw new FileNotFoundException($"Configuration file {Path.GetFileName(xmlFileSource)} cannot be found");
            }
            
            using (var reader = new StreamReader(xmlFileSource, Encoding.UTF8))
            {
                var unsupportedFeatures = new List<string>();
                var dataAccessModel = DelftXmlFileConverter.Convert<T>(reader, unsupportedFeatures);

                LogMissingFeatures(unsupportedFeatures, xmlFileSource);

                return dataAccessModel;
            }
        }

        private static void LogMissingFeatures(List<string> unsupportedFeatures, string xmlFileSource)
        {
            if (unsupportedFeatures.Count == 0) return;

            var fileName = xmlFileSource?.Split('\\').LastOrDefault() ?? "";
            var message = string.Join(Environment.NewLine, unsupportedFeatures);

            Log.InfoFormat($"The following features in the {fileName} file are not conforming with the xsd file: {Environment.NewLine + message}");
        }
    }
}
