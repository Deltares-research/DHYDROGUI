using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using DeltaShell.Dimr.xsd;
using DeltaShell.NGHS.IO.Factories;

namespace DeltaShell.NGHS.IO.FileConverters
{
    public static class DelftXmlFileConverter
    {
        public static object Convert(XmlReader reader, string rootName)
        {
            var selector = new DelftConfigXmlSerializerSelector();

            var serializer = DelftXsdValidator.ValidateWithSerializer(selector.ReturnSerializer(rootName));
 
            var dataAccessObject = (dimrXML) serializer.Deserialize(reader);

            return dataAccessObject;
        }
    }
}