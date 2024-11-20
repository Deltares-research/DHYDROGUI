using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.XmlValidation
{
    public static class FileToObject
    {
        public static XmlSchema ConvertToXmlSchema(string xmlSchemaPath)
        {
            if (!File.Exists(xmlSchemaPath))
            {
                return null;
            }

            var reader = new XmlTextReader(xmlSchemaPath);
            XmlSchema schema = XmlSchema.Read(reader, ValidationCallBack);
            reader.Close();
            return schema;
        }

        public static XDocument ConvertToXDocument(string xmlDocumentPath)
        {
            return XDocument.Load(xmlDocumentPath);
        }

        private static void ValidationCallBack(object sender, ValidationEventArgs e)
        {
            throw new XmlException(e.Message, e.Exception);
        }
    }
}