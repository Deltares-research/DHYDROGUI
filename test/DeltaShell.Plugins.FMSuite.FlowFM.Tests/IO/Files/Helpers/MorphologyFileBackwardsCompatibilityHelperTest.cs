using System;
using DeltaShell.NGHS.IO.Handlers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.Helpers
{
    [TestFixture]
    public class MorphologyFileBackwardsCompatibilityHelperTest
    {
        /// <summary>
        /// GIVEN a property Name for which a mapping exists
        /// WHEN GetUpdatedPropertyName is called
        /// THEN the correct mapping is returned
        /// </summary>
        [TestCase("bslhd", "Bshld")]
        [TestCase("BSlhd", "Bshld")]
        public void GivenAPropertyNameForWhichAMappingExists_WhenGetUpdatedPropertyNameIsCalled_ThenTheCorrectMappingIsReturned(string propertyName, string expectedMapping)
        {
            string result = MorphologyFileBackwardsCompatibilityHelper.GetUpdatedPropertyName(propertyName);
            Assert.That(result, Is.EqualTo(expectedMapping), $"Expected a different mapping:");
        }

        /// <summary>
        /// GIVEN a property Name for which no mapping exists
        /// WHEN GetUpdatedPropertyName is called
        /// THEN null is returned
        /// </summary>
        [Test]
        public void GivenAPropertyNameForWhichNoMappingExists_WhenGetUpdatedPropertyNameIsCalled_ThenNullIsReturned()
        {
            string result = MorphologyFileBackwardsCompatibilityHelper.GetUpdatedPropertyName("Hamburger");
            Assert.That(result, Is.Null, "Expected no mapping to be found.");
        }

        /// <summary>
        /// WHEN GetUpdatedPropertyName is called with null
        /// THEN an ArgumentNullException is thrown
        /// </summary>
        [Test]
        public void WhenGetUpdatedPropertyNameIsCalledWithNull_ThenAnArgumentNullExceptionIsThrown()
        {
            Assert.Throws<ArgumentNullException>(
                () => MorphologyFileBackwardsCompatibilityHelper.GetUpdatedPropertyName(null));
        }

        /// <summary>
        /// GIVEN a property Name for which a mapping exists
        ///   AND a ILogHandler
        /// WHEN GetUpdatedPropertyName is called
        /// THEN a warning message is reported with the mapping
        /// </summary>
        [TestCase("bslhd", "Bshld")]
        [TestCase("BSlhd", "Bshld")]
        public void GivenAPropertyNameForWhichAMappingExistsAndAILogHandler_WhenGetUpdatedPropertyNameIsCalled_ThenAWarningMessageIsReportedWithTheMapping(string propertyName, string expectedMapping)
        {
            // Given
            var logHandlerMock = MockRepository.GenerateStrictMock<ILogHandler>();
            logHandlerMock.Expect(l => l.ReportWarningFormat(
                                      Arg<string>.Matches(fp => fp.Equals(Resources.MorphologyFileBackwardsCompatibilityHelper_GetUpdatedPropertyName_Backwards_Compatibility____0___has_been_updated_to___1__)),
                                      Arg<object[]>.Matches(fs => fs.Length == 2 &&
                                                                  fs[0].Equals(propertyName) && 
                                                                  fs[1].Equals(expectedMapping))))
                          .Repeat.Once();

            // When
            logHandlerMock.Replay();
            MorphologyFileBackwardsCompatibilityHelper.GetUpdatedPropertyName(propertyName, 
                                                                              logHandlerMock);

            // Then
            logHandlerMock.VerifyAllExpectations();
        }

        /// <summary>
        /// GIVEN a property Name for which no mapping exists
        ///   AND a ILogHandler
        /// WHEN GetUpdatedPropertyName is called
        /// THEN no message is reported
        /// </summary>
        [Test]
        public void GivenAPropertyNameForWhichNoMappingExistsAndAILogHandler_WhenGetUpdatedPropertyNameIsCalled_ThenNoMessageIsReported()
        {
            var logHandlerMock = MockRepository.GenerateStrictMock<ILogHandler>();

            // When
            logHandlerMock.Replay();
            MorphologyFileBackwardsCompatibilityHelper.GetUpdatedPropertyName("Bacon");

            // Then
            logHandlerMock.VerifyAllExpectations();

        }

    }
}
