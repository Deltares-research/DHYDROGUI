using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeltaShell.Dimr.xsd;
using DeltaShell.NGHS.IO.FileConverters;
using DeltaShell.NGHS.IO.Handlers;

namespace DeltaShell.NGHS.IO.FileReaders
{
    /// <summary>
    /// Reads the contents of an imported xml configuration file and returns data access model object.
    /// </summary>
    public class DelftConfigXmlFileParser
    {
        private readonly ILogHandler logHandler;
        private readonly string separator = Environment.NewLine + "\t";

        public DelftConfigXmlFileParser(ILogHandler logHandler)
        {
            this.logHandler = logHandler;
        }

        /// <summary>
        /// Reads an <see cref="IXmlParsedObject"/> from the <param name="xmlFileSource"/> file
        /// </summary>
        /// <param name="xmlFileSource">Path to the xml file</param>
        /// <typeparam name="T"><see cref="IXmlParsedObject"/> object to parse from the <see cref="xmlFileSource"/></typeparam>
        /// <returns>De-serialized <see cref="IXmlParsedObject"/> object</returns>
        /// <exception cref="FileNotFoundException">When the path to the file or the file does not exist.</exception>
        public T Read<T>(string xmlFileSource) where T : class, IXmlParsedObject
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

        private void LogMissingFeatures(List<string> unsupportedFeatures, string xmlFileSource)
        {
            if (unsupportedFeatures.Count == 0) return;

            var fileName = xmlFileSource?.Split('\\').LastOrDefault() ?? "";
            var formattedMessages = separator + string.Join(separator, unsupportedFeatures);
            logHandler.ReportInfo($"The following features in the {fileName} file are not conforming with the xsd file: {formattedMessages}");
        }
    }
}
