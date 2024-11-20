using System.Collections.Generic;
using System.IO;
using System.Xml;
using DeltaShell.Plugins.DelftModels.RealTimeControl.XmlValidation;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.XmlValidation
{
    [TestFixture]
    public class ValidatorTests
    {
        private static readonly string xmlValidationTestXsd = Path.Combine(TestContext.CurrentContext.TestDirectory, @"XmlValidation\XMLValidationTest.xsd");
        private static readonly string xmlTestFile = Path.Combine(TestContext.CurrentContext.TestDirectory, @"XmlValidation\XMLTest.xml");
        private static readonly string xmlTestFailFile = Path.Combine(TestContext.CurrentContext.TestDirectory, @"XmlValidation\XMLTestFail.xml");

        [Test]
        public void TestThatXmlIsValidatedBySuppliedSchema()
        {
            var validator = new Validator(new List<string> {xmlValidationTestXsd});
            validator.Validate(xmlTestFile);
        }

        [Test]
        public void TestThatXmlInvalidatedBySuppliedSchemaReturnsTrue()
        {
            var validator = new Validator(new List<string> {xmlValidationTestXsd});
            Assert.IsTrue(validator.IsValid(xmlTestFile));
        }

        [Test]
        public void TestThatXmlIsInvalidatedBySuppliedSchemaThrowsException()
        {
            var validator = new Validator(new List<string> {xmlValidationTestXsd});
            Assert.That(() => validator.Validate(xmlTestFailFile), Throws.InstanceOf<XmlException>());
        }

        [Test]
        public void TestThatXmlIsInvalidatedBySuppliedSchemaReturnsFalse()
        {
            var validator = new Validator(new List<string> {xmlValidationTestXsd});
            Assert.IsFalse(validator.IsValid(xmlTestFailFile));
        }
    }
}