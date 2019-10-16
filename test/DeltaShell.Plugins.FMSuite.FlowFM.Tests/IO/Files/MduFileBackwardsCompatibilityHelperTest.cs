using System;
using DeltaShell.NGHS.IO.Handlers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files
{
    [TestFixture]
    public class MduFileBackwardsCompatibilityHelperTest
    {
        [Test]
        public void GetUpdatedPropertyName_WithLegacyName_ThenUpdatedNameIsReturned()
        {
            // Call
            string updatedName = MduFileBackwardsCompatibilityHelper.GetUpdatedPropertyName("model");

            // Assert
            Assert.That(updatedName, Is.EqualTo("General"));
        }

        [Test]
        public void GetUpdatedPropertyName_WithLegacyNameAndLogHandler_ThenWarningIsLogged()
        {
            // Setup
            var logHandler = MockRepository.GenerateStrictMock<ILogHandler>();
            logHandler.Expect(l => l.ReportWarningFormat(
                                  Arg<string>.Matches(fp => fp.Equals("Backwards Compatibility: '{0}' has been updated to '{1}'")),
                                  Arg<object[]>.Matches(fs => fs.Length == 2 &&
                                                              fs[0].Equals("model") &&
                                                              fs[1].Equals("General"))))
                      .Repeat.Once();

            // Call
            MduFileBackwardsCompatibilityHelper.GetUpdatedPropertyName("model", logHandler);

            // Assert
            logHandler.VerifyAllExpectations();
        }

        [Test]
        public void GetUpdatedPropertyName_WithNonLegacyName_ThenSameNameIsReturned()
        {
            // Call
            string updatedName = MduFileBackwardsCompatibilityHelper.GetUpdatedPropertyName("myNonLegacyName");

            // Assert
            Assert.That(updatedName, Is.EqualTo("myNonLegacyName"));
        }

        [Test]
        public void GetUpdatedPropertyName_WithNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => MduFileBackwardsCompatibilityHelper.GetUpdatedPropertyName(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("propertyName"));
            Assert.That(exception.Message, Is.EqualTo("Value cannot be null.\r\nParameter name: propertyName"));
        }
    }
}