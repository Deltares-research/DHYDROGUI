using System.Collections.Generic;
using DeltaShell.Dimr.dimr_xsd;
using DeltaShell.Dimr.DimrXsd;
using DeltaShell.Dimr.Properties;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Dimr.Tests.dimr_xsd
{
    [TestFixture]
    public class DimrXmlValidatorTest
    {
        private const string pathName = "location/somewhere";
        private const string documentation = "Documentation";
        private const string control = "Control";
        private const string component = "Component";
        private const string coupler = "Coupler";

        [Test]
        public void Constructor_ArgNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new DimrXmlValidator(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }
        
        [Test]
        public void IsValid_ArgDimrXmlNull_ThrowsArgumentNullException()
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var validator = new DimrXmlValidator(logHandler);
            
            // Call
            void Call() => validator.IsValid(null, "path");

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }
        
        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void IsValid_ArgPathNullOrWhiteSpace_ThrowsArgumentException(string path)
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var validator = new DimrXmlValidator(logHandler);
            
            // Call
            void Call() => validator.IsValid(new dimrXML(), path);

            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }
        
        [Test]
        public void GivenValidDimrXml_WhenIsValid_ExpectValidReturned()
        {
            // Arrange
            dimrXML dimrXml = GetFilledInDimrXml();
            var logHandler = Substitute.For<ILogHandler>();
            var validator = new DimrXmlValidator(logHandler);

            // Act
            bool isValid = validator.IsValid(dimrXml, pathName);

            // Assert
            Assert.That(isValid, Is.True);
        }

        [Test]
        [TestCaseSource(nameof(DimrXmlData))]
        public void GivenInvalidDimrXml_WhenIsValid_ExpectLoggingCalled(dimrXML dimrXML, string expectedLogMessage)
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var validator = new DimrXmlValidator(logHandler);

            // Act
            bool isValid = validator.IsValid(dimrXML, pathName);

            // Assert
            logHandler.Received(1).ReportError(expectedLogMessage);
            Assert.That(isValid, Is.False);
        }

        private static IEnumerable<TestCaseData> DimrXmlData()
        {
            var dimrXml = new dimrXML();
            string multipleMissingElements = $"{documentation}, {control}, {component}";
            string expectedLogMessage = $"DIMR xml {pathName} is not valid. It is missing the following element(s): {multipleMissingElements}";
            yield return new TestCaseData(dimrXml, expectedLogMessage).SetName("Dimr Xml File empty");

            dimrXml = GetFilledInDimrXml();
            dimrXml.documentation = null;
            expectedLogMessage = $"DIMR xml {pathName} is not valid. It is missing the following element(s): {documentation}";
            yield return new TestCaseData(dimrXml, expectedLogMessage).SetName("Documentation missing in Dimr Xml File");

            dimrXml = GetFilledInDimrXml();
            dimrXml.control = null;
            expectedLogMessage = $"DIMR xml {pathName} is not valid. It is missing the following element(s): {control}";
            yield return new TestCaseData(dimrXml, expectedLogMessage).SetName("Control missing in Dimr Xml File");

            dimrXml = GetFilledInDimrXml();
            dimrXml.control = new object[] {};
            expectedLogMessage = $"DIMR xml {pathName} is not valid. It is missing the following element(s): {control}";
            yield return new TestCaseData(dimrXml, expectedLogMessage).SetName("Control empty in Dimr Xml File");

            dimrXml = GetFilledInDimrXml();
            dimrXml.component = null;
            expectedLogMessage = $"DIMR xml {pathName} is not valid. It is missing the following element(s): {component}";
            yield return new TestCaseData(dimrXml, expectedLogMessage).SetName("Component missing in Dimr Xml File");

            dimrXml = GetFilledInDimrXml();
            dimrXml.component = new dimrComponentXML[] {};
            expectedLogMessage = $"DIMR xml {pathName} is not valid. It is missing the following element(s): {component}";
            yield return new TestCaseData(dimrXml, expectedLogMessage).SetName("Component empty in Dimr Xml File");

            dimrXml = GetFilledInDimrXml();
            dimrXml.coupler = null;
            expectedLogMessage = $"DIMR xml {pathName} is not valid. It is missing the following element(s): {coupler}";
            yield return new TestCaseData(dimrXml, expectedLogMessage).SetName("Coupler missing in Dimr Xml File");

            dimrXml = GetFilledInDimrXml();
            dimrXml.coupler = new dimrCouplerXML[] {};
            expectedLogMessage = $"DIMR xml {pathName} is not valid. It is missing the following element(s): {coupler}";
            yield return new TestCaseData(dimrXml, expectedLogMessage).SetName("Coupler empty in Dimr Xml File");
        }

        private static dimrXML GetFilledInDimrXml()
        {
            return new dimrXML
            {
                documentation = new dimrDocumentationXML(),
                control = new object[] { "something" },
                component = new[] { new dimrComponentXML(), new dimrComponentXML() },
                coupler = new[] { new dimrCouplerXML(), new dimrCouplerXML() }
            };
        }
    }
}