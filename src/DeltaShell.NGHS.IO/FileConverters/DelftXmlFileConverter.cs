using System;
using System.Collections.Generic;
using System.Xml;
using DeltaShell.Dimr.xsd;
using DeltaShell.NGHS.IO.Factories;

namespace DeltaShell.NGHS.IO.FileConverters
{
    public static class DelftXmlFileConverter
    {
        public static object Convert(XmlReader reader, string rootName, List<string> errorMessages)
        {
            var selector = new DelftConfigXmlSerializerSelector();

            var serializer = DelftXsdValidator.ValidateDataObjectModel(selector.ReturnSerializer(rootName), errorMessages);
 
            var dataAccessObject = serializer.Deserialize(reader);


            return dataAccessObject;
        }
    }
}