using System.Collections.Generic;
using System.Xml.Serialization;
using DeltaShell.Dimr.xsd;
using DeltaShell.NGHS.IO.FileConverters;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    [TestFixture]
    public class DelftXsdValidatorTest
    {
        private XmlSerializer serializer;
        [SetUp]
        public void Setup()
        {
            var mocks = new MockRepository();
            serializer = new XmlSerializer(typeof(dimrXML));
        }

        [Test]
        public void Test1()
        {
            var unsupportedFeatures = new List<string>();
            //var result = DelftXsdValidator.CollectUnsupportedFeatures(serializer, unsupportedFeatures);

            //Assert.That(result, Is.TypeOf<XmlSerializer>());
        }
    }
}
