using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DeltaShell.Dimr.xsd;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests
{
    [TestFixture]
    public class DelftXsdValidatorTest
    {
        [SetUp]
        public void Setup()
        {
           
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Serializer cannot be null")]
        public void ValidateWithNoSerializer()
        {
            var errors = new List<string>();
            DelftXsdValidator.ValidateDataObjectModel(null, errors);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Serializer cannot be null")]
        public void ValidateWithoutNoList()
        {
            var serializer = new XmlSerializer(typeof(dimrXML));
            DelftXsdValidator.ValidateDataObjectModel(serializer, null);
        }


        [Test]
        public void ValidateWithDimrSerializer()
        {
            var serializer = new XmlSerializer(typeof(dimrXML));
            var errors = new List<string>();
            var result = DelftXsdValidator.ValidateDataObjectModel(serializer, errors);
            serializer.UnknownAttribute += delegate(object sender, XmlAttributeEventArgs e)
            {
                errors.Add(e.Attr.Name);
            };

        }
    }
}
