using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using DelftModelApi.Net;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    public class ModelApiParameterTest
    {
        [Test]
        public void SerializeToAndFromXml()
        {
            var parameters = new List<ModelApiParameter>();
            parameters.Add(new ModelApiParameter
                               {
                                   Description = "desc",
                                   Id = "1",
                                   Type = "typeof(int)",
                                   Value = "2",
                                   Visible = false
                               });
            //serialize
            var serializer = new XmlSerializer(typeof(List<ModelApiParameter>));
            const string path = "parameters.xml";
            TextWriter tw = new StreamWriter(path); 
            serializer.Serialize(tw,parameters); 
            tw.Close();

            //deserialize
            var deserializer = new XmlSerializer(typeof(List<ModelApiParameter>));
            TextReader tr = new StreamReader(path);
            var retrievedParameters = (List<ModelApiParameter>)deserializer.Deserialize(tr);
            tr.Close(); 

            //check the parameters
            Assert.AreEqual(1,retrievedParameters.Count);
            var retrievedParameter = retrievedParameters[0];
            Assert.AreEqual("desc",retrievedParameter.Description);
            Assert.AreEqual("1", retrievedParameter.Id);
            Assert.AreEqual("typeof(int)", retrievedParameter.Type);
            Assert.AreEqual("2", retrievedParameter.Value);
            Assert.AreEqual(false, retrievedParameter.Visible);
        }
    }
}