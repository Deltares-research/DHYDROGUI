using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Serialization;
using DeltaShell.Dimr.xsd;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Dimr
{
    public static class DelftConfigXmlFileConverter
    {
        private static XmlSerializer serializer;

        private static XmlSerializer Serializer
        {
            get
            {
                if ((serializer == null))
                {
                    serializer = new XmlSerializer(typeof(dimrXML));
                }

                return serializer;
            }
        }

        public static object Convert(XDocument configurationFile, string xmlFileSourcePath)
        {
            if (configurationFile == null) { throw new ArgumentException("Configuration file does not exist"); }

            if (xmlFileSourcePath == null) { throw new ArgumentException("File path does not exist"); }

            if (configurationFile.Root?.Name.LocalName == "dimrConfig")
            {
                var streamReader = new System.IO.StreamReader(xmlFileSourcePath);
                var dimrDataAccessModel = ((dimrXML)(Serializer.Deserialize(System.Xml.XmlReader.Create(streamReader))));

                ValidateWithXsd(); // maybe seperate validator based on factory

                return dimrDataAccessModel;
            }

            if (configurationFile.Root?.Name.LocalName == "rtcConfig")
            {
                return "this is a rtcConfig file";

            }

            return null;
        }

        private static object ValidateWithXsd()
        {
            return null;
        }
    }
}
