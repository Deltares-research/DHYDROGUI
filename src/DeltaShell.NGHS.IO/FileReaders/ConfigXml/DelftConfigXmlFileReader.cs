using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using DeltaShell.Dimr.xsd;
using DeltaShell.NGHS.IO.Factories;
using DeltaShell.NGHS.IO.FileConverters;

namespace DeltaShell.NGHS.IO.FileReaders.ConfigXml
{
    public static class DelftConfigXmlFileReader
    {
        private static List<string> errorMessages { get; }
         
        public static object Read(string xmlFileSource)
        {
            if (string.IsNullOrEmpty(xmlFileSource)) { throw new ArgumentException("Configuration file cannot be found"); }

            object dataAccessModel;
            try
            {
                var xmlConfigFile = XDocument.Load(xmlFileSource);
                var rootName = xmlConfigFile?.Root?.Name.LocalName;

                var reader = xmlConfigFile?.Root?.CreateReader();


                dataAccessModel = DelftXmlFileConverter.Convert(reader, rootName);
                var convertedObject = (dimrXML) dataAccessModel;

                errorMessages.Add($"The following elements are missing {convertedObject.UnKnownElements}");
                errorMessages.Add($"The following attributes are missing {convertedObject.UnKnownAttributes}");


            }
            catch
            {
                throw new XmlException("Unable to parse file");
            }

            return dataAccessModel;
        }
    }
}
