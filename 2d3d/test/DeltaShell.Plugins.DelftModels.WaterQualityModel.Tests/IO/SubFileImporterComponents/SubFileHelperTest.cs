using System;
using System.Linq;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO.SubFileImporterComponents;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO.SubFileImporterComponents
{
    [TestFixture]
    public class SubFileHelperTest
    {
        [Test]
        public void GetRegexPattern_PropertyRegexInfosNull_ThrowsArgumentNullException()
        {
            // Call
            TestDelegate call = () => SubFileHelper.GetRegexPattern(null, string.Empty);

            // Assert
            Assert.That(call, Throws.TypeOf<ArgumentNullException>()
                                    .With.Property(nameof(ArgumentNullException.ParamName))
                                    .EqualTo("propertyRegexInfos"));
        }

        [Test]
        public void GetRegexPattern_PropertySeparatorNull_ThrowsArgumentNullException()
        {
            // Call
            TestDelegate call = () => SubFileHelper.GetRegexPattern(Enumerable.Empty<SubFilePropertyRegexInfo>(), null);

            // Assert
            Assert.That(call, Throws.TypeOf<ArgumentNullException>()
                                    .With.Property(nameof(ArgumentNullException.ParamName))
                                    .EqualTo("propertySeparator"));
        }

        [Test]
        public void GetRegexPattern_WithRegexInfosEmpty_ReturnsEmptyPattern()
        {
            // Setup
            const string propertySeparator = @"\s*\n";

            // Call
            string pattern = SubFileHelper.GetRegexPattern(Enumerable.Empty<SubFilePropertyRegexInfo>(), propertySeparator);

            // Assert
            Assert.That(pattern, Is.Empty);
        }

        [Test]
        public void GetRegexPattern_WithValidArguments_ReturnsExpectedPattern()
        {
            // Setup
            const string propertySeparator = @"\s*\n";

            var variableOne = new SubFilePropertyRegexInfo("PropertyNameOne", "CaptureGroupNameOne", "CaptureGroupCharactersOne");
            var variableTwo = new SubFilePropertyRegexInfo("PropertyNameTwo", "CaptureGroupNameTwo", "CaptureGroupCharactersTwo");

            // Call
            string pattern = SubFileHelper.GetRegexPattern(new[]
            {
                variableOne,
                variableTwo
            }, propertySeparator);

            // Assert
            string expectedPattern =
                $@"\s*{variableOne.PropertyName}\s*'(?<{variableOne.CaptureGroupName}>{variableOne.CaptureGroupPattern})'{propertySeparator}" +
                $@"\s*{variableTwo.PropertyName}\s*'(?<{variableTwo.CaptureGroupName}>{variableTwo.CaptureGroupPattern})'{propertySeparator}";

            Assert.That(pattern, Is.EqualTo(expectedPattern));
        }
    }
}