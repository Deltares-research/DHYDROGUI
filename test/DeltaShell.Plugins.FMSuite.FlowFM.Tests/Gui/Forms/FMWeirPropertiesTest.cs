using DelftTools.Hydro.Structures;
using DelftTools.Utils.Validation.Common;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Forms
{
    [TestFixture]
    public class FMWeirPropertiesTest
    {
        [Test]
        public void CrestLevel_WeirIsUsingTimeSeriesForCrestLevel_ReturnsTimeSeriesString()
        {
            // Setup
            var weir = Substitute.For<IWeir>();
            weir.IsUsingTimeSeriesForCrestLevel().Returns(true);

            var fmWeirProperties = new FMWeirProperties() { Data = weir };

            // Call
            string result = fmWeirProperties.CrestLevel;
            
            // Assert
            const string expectedResult = "Time series";
            Assert.That(result, Is.EqualTo(expectedResult));
        }
        
        [Test]
        public void CrestLevel_WeirIsNotUsingTimeSeriesForCrestLevel_ReturnsCrestLevelValueAsString()
        {
            // Setup
            const double randomCrestLevel = 123.456;
            
            var weir = Substitute.For<IWeir>();
            weir.IsUsingTimeSeriesForCrestLevel().Returns(false);
            weir.CrestLevel.Returns(randomCrestLevel);

            var fmWeirProperties = new FMWeirProperties() { Data = weir };

            // Call
            string result = fmWeirProperties.CrestLevel;
            
            // Assert
            Assert.That(result, Is.EqualTo(randomCrestLevel.ToString()));
        }

        [Test]
        public void SetName_InvalidName_OriginalNameIsPreserved()
        {
            // Arrange
            var validator = Substitute.For<IValidator<string>>();
            validator.Validate("some_invalid_name").Returns(ValidationResult.Fail("message"));

            var data = new Weir { Name = "some_name" };
            data.AttachNameValidator(validator);
            var properties = new FMWeirProperties { Data = data };

            // Act
            properties.Name = "some_invalid_name";

            // Assert
            Assert.That(properties.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void SetName_ValidName_NameIsUpdated()
        {
            // Arrange
            var validator = Substitute.For<IValidator<string>>();
            validator.Validate("some_valid_name").Returns(ValidationResult.Success);

            var data = new Weir { Name = "some_name" };
            data.AttachNameValidator(validator);
            var properties = new FMWeirProperties { Data = data };

            // Act
            properties.Name = "some_valid_name";

            // Assert
            Assert.That(properties.Name, Is.EqualTo("some_valid_name"));
        }
    }
}