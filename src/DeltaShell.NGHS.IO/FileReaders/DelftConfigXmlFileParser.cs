using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using DeltaShell.NGHS.IO.FileConverters;
using log4net;

namespace DeltaShell.NGHS.IO.FileReaders
{
    public static class DelftConfigXmlFileParser
    {
        public static readonly ILog Log = LogManager.GetLogger(typeof(DelftConfigXmlFileParser));
        private static readonly Action<string, IList<string>> createAndAddErrorReport;
        private static object dataAccessModel;

        public static object Read(string xmlFileSource)
        {
            if (string.IsNullOrEmpty(xmlFileSource)) {throw new FileReadingException("Configuration file cannot be found"); }

            XDocument xmlConfigFile;
            string rootName;
            var errorMessages = new List<string>();
            try
            {
                xmlConfigFile = XDocument.Load(xmlFileSource);
                rootName = xmlConfigFile?.Root?.Name.LocalName;
            }
            catch
            {
                throw new XmlException("Unable to parse file");
            }

            var reader = xmlConfigFile?.Root?.CreateReader();

            dataAccessModel = DelftXmlFileConverter.Convert(reader, rootName, errorMessages );

            return dataAccessModel;
        }
    }
}
