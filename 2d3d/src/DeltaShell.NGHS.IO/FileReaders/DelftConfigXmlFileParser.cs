using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.NGHS.IO.FileConverters;

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
        /// Reads an instance of <typeparamref name="T"/> from the <paramref name="xmlFileSource"/>
        /// file
        /// </summary>
        /// <param name="xmlFileSource">Path to the xml file</param>
        /// <typeparam name="T">The type to parse from the <paramref name="xmlFileSource"/></typeparam>
        /// <returns>De-serialized <typeparamref name="T"/> object</returns>
        /// <exception cref="FileNotFoundException">When the path to the file or the file does not exist.</exception>
        public T Read<T>(string xmlFileSource) where T : class
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
            if (unsupportedFeatures.Count == 0)
            {
                return;
            }

            string fileName = xmlFileSource?.Split('\\').LastOrDefault() ?? "";
            string formattedMessages = separator + string.Join(separator, unsupportedFeatures);
            logHandler.ReportInfo($"The following features in the {fileName} file are not conforming with the xsd file: {formattedMessages}");
        }
    }
}