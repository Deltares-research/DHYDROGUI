using System.Collections.Generic;
using System.Xml;
using DeltaShell.Plugins.DelftModels.RealTimeControl.XmlValidation;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.XmlValidation
{
    [TestFixture]
    public class ValidatorTests
    {
        [Test]
        public void TestThatXmlIsValidatedBySuppliedSchema()
        {
            var validator = new Validator(new List<string> {@"XmlValidation\XMLValidationTest.xsd"});
            validator.Validate(@"XmlValidation\XMLTest.xml");
        }

        [Test]
        public void TestThatXmlInvalidatedBySuppliedSchemaReturnsTrue()
        {
            var validator = new Validator(new List<string> {@"XmlValidation\XMLValidationTest.xsd"});
            Assert.IsTrue(validator.IsValid(@"XmlValidation\XMLTest.xml"));
        }

        [Test]
        [ExpectedException(typeof(XmlException))]
        public void TestThatXmlIsInvalidatedBySuppliedSchemaThrowsException()
        {
            var validator = new Validator(new List<string> {@"XmlValidation\XMLValidationTest.xsd"});
            validator.Validate(@"XmlValidation\XMLTestFail.xml");
        }

        [Test]
        public void TestThatXmlIsInvalidatedBySuppliedSchemaReturnsFalse()
        {
            var validator = new Validator(new List<string> {@"XmlValidation\XMLValidationTest.xsd"});
            Assert.IsFalse(validator.IsValid(@"XmlValidation\XMLTestFail.xml"));
        }
    }
}