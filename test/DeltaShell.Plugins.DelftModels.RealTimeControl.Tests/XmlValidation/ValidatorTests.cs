using System.Collections.Generic;
using System.Xml;
using DelftTools.TestUtils;
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
            var validator = new Validator(new List<string> {TestHelper.GetTestWorkingDirectory(@"..\XmlValidation\XMLValidationTest.xsd")});
            validator.Validate(TestHelper.GetTestWorkingDirectory(@"..\XmlValidation\XMLTest.xml")); 
        }

        [Test]
        public void TestThatXmlInvalidatedBySuppliedSchemaReturnsTrue()
        {
            var validator = new Validator(new List<string> { TestHelper.GetTestWorkingDirectory(@"..\XmlValidation\XMLValidationTest.xsd")});
            Assert.IsTrue(validator.IsValid(TestHelper.GetTestWorkingDirectory(@"..\XmlValidation\XMLTest.xml")));
        }

        [Test]
        public void TestThatXmlIsInvalidatedBySuppliedSchemaThrowsException()
        {
            var validator = new Validator(new List<string> { TestHelper.GetTestWorkingDirectory(@"..\XmlValidation\XMLValidationTest.xsd") });

            Assert.Throws<XmlException>(() =>
            {
                validator.Validate(TestHelper.GetTestWorkingDirectory(@"..\XmlValidation\XMLTestFail.xml"));
            });
        }

        [Test]
        public void TestThatXmlIsInvalidatedBySuppliedSchemaReturnsFalse()
        {
            var validator = new Validator(new List<string> { TestHelper.GetTestWorkingDirectory(@"..\XmlValidation\XMLValidationTest.xsd")});
            Assert.IsFalse(validator.IsValid(TestHelper.GetTestWorkingDirectory(@"..\XmlValidation\XMLTestFail.xml")));
        }
    }
}
