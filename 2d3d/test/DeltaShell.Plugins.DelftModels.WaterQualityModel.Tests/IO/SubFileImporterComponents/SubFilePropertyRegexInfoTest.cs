using System;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO.SubFileImporterComponents;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO.SubFileImporterComponents
{
    [TestFixture]
    public class SubFilePropertyRegexInfoTest
    {
        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void Constructor_InvalidPropertyName_ThrowsArgumentException(string invalidPropertyName)
        {
            // Call
            TestDelegate call = () => new SubFilePropertyRegexInfo(invalidPropertyName, "group", string.Empty);

            // Assert
            const string expectedMessage = "'propertyName' cannot be null, empty or consist of only whitespace.";
            Assert.That(call, Throws.ArgumentException
                                    .With.Message.EqualTo(expectedMessage));
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void Constructor_InvalidCaptureGroupName_ThrowsArgumentException(string invalidGroupName)
        {
            // Call
            TestDelegate call = () => new SubFilePropertyRegexInfo("property", invalidGroupName, string.Empty);

            // Assert
            const string expectedMessage = "'captureGroupName' cannot be null, empty or consist of only whitespace.";
            Assert.That(call, Throws.ArgumentException
                                    .With.Message.EqualTo(expectedMessage));
        }

        [Test]
        public void Constructor_CaptureGroupPatternNull_ThrowsArgumentNullException()
        {
            // Call
            TestDelegate call = () => new SubFilePropertyRegexInfo("property", "group", null);

            // Assert
            Assert.That(call, Throws.TypeOf<ArgumentNullException>()
                                    .With.Property(nameof(ArgumentNullException.ParamName))
                                    .EqualTo("captureGroupPattern"));
        }

        [Test]
        public void Constructor_WithArguments_ExpectedValues()
        {
            // Setup
            const string propertyName = "propertyName";
            const string captureGroupName = "captureGroupName";
            const string captureGroupPattern = "pattern";

            // Call
            var info = new SubFilePropertyRegexInfo(propertyName, captureGroupName, captureGroupPattern);

            // Assert
            Assert.That(info.PropertyName, Is.EqualTo(propertyName));
            Assert.That(info.CaptureGroupName, Is.EqualTo(captureGroupName));
            Assert.That(info.CaptureGroupPattern, Is.EqualTo(captureGroupPattern));
        }
    }
}