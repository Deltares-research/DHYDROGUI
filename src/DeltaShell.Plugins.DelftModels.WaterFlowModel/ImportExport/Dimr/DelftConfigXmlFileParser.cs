using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Dimr
{
    public static class DelftConfigXmlFileParser
    {
        public static XDocument Parse(string xmlFileSource)
        {
            if (string.IsNullOrEmpty(xmlFileSource)) { throw new ArgumentException("Configuration file cannot be found"); }

            XDocument configurationFile;
            try
            {
                configurationFile = XDocument.Load(Path.Combine(xmlFileSource));
            }
            catch
            {
                throw new XmlException("Unable to parse file");
            }

            return configurationFile;
        }
    }

    public static class DelftIniXmlFileParser2
    {
        public static XDocument Parse(string xmlFileSource)
        {
            if (string.IsNullOrEmpty(xmlFileSource)) { throw new ArgumentException("Configuration file cannot be found"); }

            XDocument configurationFile;
            try
            {
                configurationFile = XDocument.Load(Path.Combine(xmlFileSource));
            }
            catch
            {
                throw new XmlException("Unable to parse file");
            }

            return configurationFile;
        }
    }
}
