using System.Collections.Generic;
using System.Xml;
using DeltaShell.NGHS.IO.Factories;

namespace DeltaShell.NGHS.IO.FileConverters
{
    public static class DelftXmlFileConverter
    {
        public static object Convert(XmlReader readerWithFile, string rootName, List<string> errorMessages)
        {
            var selector = new DelftConfigXmlSerializerSelector();

            var serializer = DelftXsdValidator.ValidateDataObjectModel(selector.ReturnSerializer(rootName), errorMessages);
 
            var dataAccessObject = serializer.Deserialize(readerWithFile);

            return dataAccessObject;
        }
    }
}