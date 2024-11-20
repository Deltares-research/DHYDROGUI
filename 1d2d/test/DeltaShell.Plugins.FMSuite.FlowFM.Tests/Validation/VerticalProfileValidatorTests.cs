using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation
{
    [TestFixture]
    public class VerticalProfileValidatorTests
    {
        [Test]
        public void ValidateNoIssuesTest()
        {
            // arrange 
            var profile = new VerticalProfileDefinition(VerticalProfileType.PercentageFromSurface, 100);

            // act
            var issues = VerticalProfileValidator.ValidateVerticalProfile("", profile, null, "").ToArray();

            // assert
            Assert.AreEqual(0, issues.Length);
        }

        [Test]
        public void ValidateDuplicatePointDepthValuesTest()
        {
            // arrange
            var profile = new VerticalProfileDefinition(VerticalProfileType.PercentageFromSurface, 100);

            profile.PointDepths.Add(0.0);
            profile.PointDepths.Add(0.0);

            // act
            var issues = VerticalProfileValidator.ValidateVerticalProfile("bc01", profile, null, "bc01_001").ToArray();
            
            // assert
            Assert.AreEqual(1, issues.Length);
            Assert.AreEqual("Duplicate profile depths detected for bc01 at bc01_001", issues[0].Message);
            Assert.AreEqual(ValidationSeverity.Error, issues[0].Severity);
        }

        [TestCase(VerticalProfileType.PercentageFromBed, "Vertical profile point below bed detected", "Vertical profile point above surface detected")]
        [TestCase(VerticalProfileType.PercentageFromSurface, "Vertical profile point above surface detected", "Vertical profile point below bed detected")]
        public void ValidateProfileForPercentageFromBedAndSurfaceReturnIssuesTest(VerticalProfileType profileType, string firstWarning, string secondWarning)
        {
            // arrange
            var profile = new VerticalProfileDefinition(profileType, 0);

            profile.PointDepths.Add(-1.0);
            profile.PointDepths.Add(101.0);

            // act
            var issues = VerticalProfileValidator.ValidateVerticalProfile("bc01", profile, null, "bc01_001").ToArray();

            // assert
            Assert.AreEqual(2, issues.Length);
            Assert.AreEqual(firstWarning + " for bc01 at bc01_001", issues[0].Message);
            Assert.AreEqual(ValidationSeverity.Warning, issues[0].Severity);
            Assert.AreEqual(secondWarning + " for bc01 at bc01_001", issues[1].Message);
            Assert.AreEqual(ValidationSeverity.Warning, issues[1].Severity);
        }

        [TestCase(VerticalProfileType.ZFromBed, "Vertical profile point below bed detected")]
        [TestCase(VerticalProfileType.ZFromSurface, "Vertical profile point above surface detected")]
        public void ValidateProfileForZFromBadAndSurfaceReturnIssues(VerticalProfileType profileType, string warningMessage)
        {
            // arrange
            var profile = new VerticalProfileDefinition(profileType, 0);

            profile.PointDepths.Add(-1.0);

            // act
            var issues = VerticalProfileValidator.ValidateVerticalProfile("bc01", profile, null, "bc01_001").ToArray();

            // assert
            Assert.AreEqual(1, issues.Length);
            Assert.AreEqual(warningMessage + " for bc01 at bc01_001", issues[0].Message);
            Assert.AreEqual(ValidationSeverity.Warning, issues[0].Severity);
        }
    }
}
