using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using DeltaShell.Dimr.xsd;
using DeltaShell.NGHS.IO.FileConverters;

namespace DeltaShell.NGHS.IO.FileReaders.ConfigXml
{
    public static class DelftConfigXmlFileReader
    {
        private static List<string> errorMessages { get; set;  }
         
        public static object Read(string xmlFileSource)
        {
            if (string.IsNullOrEmpty(xmlFileSource)) { throw new FileReadingException("Configuration file cannot be found"); }

            object dataAccessModel;
            errorMessages = new List<string>();
            XDocument xmlConfigFile;
            string rootName;

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

            dataAccessModel = DelftXmlFileConverter.Convert(reader, rootName);
            var convertedObject = (dimrXML)dataAccessModel;

            errorMessages.Add($"The following elements are missing {convertedObject.UnKnownElements}");
            errorMessages.Add($"The following attributes are missing {convertedObject.UnKnownAttributes}");

            return dataAccessModel;
        }
    }
}
