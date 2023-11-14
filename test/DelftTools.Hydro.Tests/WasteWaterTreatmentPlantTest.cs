using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Validation.Common;
using log4net.Core;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class WasteWaterTreatmentPlantTest
    {
        [Test]
        public void Clone()
        {
            var wwtp = new WasteWaterTreatmentPlant {Geometry = new Point(15, 15), Name = "aa", Basin = new DrainageBasin()};
            wwtp.Attributes.Add("Milage",15);

            var clone = wwtp.Clone();

            ReflectionTestHelper.AssertPublicPropertiesAreEqual(wwtp, clone);
        }

        [Test]
        public void SetNameIfValid_InvalidName_OriginalNameIsPreserved_WarningIsLogged()
        {
            // Arrange
            var validator = Substitute.For<IValidator<string>>();
            validator.Validate("some_invalid_name").Returns(ValidationResult.Fail("message"));

            var data = new WasteWaterTreatmentPlant { Name = "some_name" };
            data.AttachNameValidator(validator);

            // Act
            void Call() => data.SetNameIfValid("some_invalid_name");

            // Assert
            string error = TestHelper.GetAllRenderedMessages(Call, Level.Warn).Single();
            Assert.That(error, Is.EqualTo("message"));
            Assert.That(data.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void SetNameIfValid_ValidName_NameIsUpdated()
        {
            // Arrange
            var validator = Substitute.For<IValidator<string>>();
            validator.Validate("some_valid_name").Returns(ValidationResult.Success);

            var data = new WasteWaterTreatmentPlant { Name = "some_name" };
            data.AttachNameValidator(validator);

            // Act
            data.SetNameIfValid("some_valid_name");

            // Assert
            Assert.That(data.Name, Is.EqualTo("some_valid_name"));
        }

        [Test]
        public void AttachNameValidator_SubValidatorNull_ThrowsArgumentNullException()
        {
            // Arrange
            var data = new WasteWaterTreatmentPlant { Name = "some_name" };

            // Act
            void Call() => data.AttachNameValidator(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void DetachNameValidator_SubValidatorNull_ThrowsArgumentNullException()
        {
            // Arrange
            var data = new WasteWaterTreatmentPlant { Name = "some_name" };

            // Act
            void Call() => data.DetachNameValidator(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void DetachNameValidator_RemovesValidator()
        {
            // Arrange
            var validator = Substitute.For<IValidator<string>>();
            validator.Validate("some_invalid_name").Returns(ValidationResult.Fail("message"));

            var data = new WasteWaterTreatmentPlant { Name = "some_name" };
            data.AttachNameValidator(validator);

            // Pre-conditions
            data.SetNameIfValid("some_invalid_name");
            Assert.That(data.Name, Is.EqualTo("some_name"));

            // Act
            data.DetachNameValidator(validator);
            data.SetNameIfValid("some_invalid_name");

            // Assert
            Assert.That(data.Name, Is.EqualTo("some_invalid_name"));
        }
    }
}