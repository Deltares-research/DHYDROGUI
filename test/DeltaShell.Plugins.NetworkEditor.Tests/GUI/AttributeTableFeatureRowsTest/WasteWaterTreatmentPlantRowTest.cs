using DelftTools.Hydro;
using DelftTools.Utils.Validation.Common;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using GeoAPI.Extensions.Feature;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.AttributeTableFeatureRowsTest
{
    [TestFixture]
    public class WasteWaterTreatmentPlantRowTest
    {
        [Test]
        public void Constructor_WithNullWasteWaterTreatmentPlant_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new WasteWaterTreatmentPlantRow(null, new NameValidator());
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_WithNullNameValidator_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new WasteWaterTreatmentPlantRow(new WasteWaterTreatmentPlant(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsWasteWaterTreatmentPlant()
        {
            // Arrange
            var wasteWaterTreatmentPlant = new WasteWaterTreatmentPlant();
            var row = new WasteWaterTreatmentPlantRow(wasteWaterTreatmentPlant, new NameValidator());

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(wasteWaterTreatmentPlant));
        }

        [Test]
        public void WhenWasteWaterTreatmentPlantPropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var wasteWaterTreatmentPlant = new WasteWaterTreatmentPlant();
            var row = new WasteWaterTreatmentPlantRow(wasteWaterTreatmentPlant, new NameValidator());
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            wasteWaterTreatmentPlant.Name = "some_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void SetName_SetsWasteWaterTreatmentPlantName()
        {
            // Arrange
            var wasteWaterTreatmentPlant = new WasteWaterTreatmentPlant();
            var row = new WasteWaterTreatmentPlantRow(wasteWaterTreatmentPlant, new NameValidator());

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(wasteWaterTreatmentPlant.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsWasteWaterTreatmentPlantName()
        {
            // Arrange
            var wasteWaterTreatmentPlant = new WasteWaterTreatmentPlant { Name = "some_name" };
            var row = new WasteWaterTreatmentPlantRow(wasteWaterTreatmentPlant, new NameValidator());

            // Act
            string result = row.Name;

            // Assert
            Assert.AreEqual(result, "some_name");
        }

        [Test]
        public void SetName_InvalidName_OriginalNameIsPreserved()
        {
            // Arrange
            var validator = Substitute.For<IValidator<string>>();
            validator.Validate("some_invalid_name").Returns(ValidationResult.Fail("message"));
            var nameValidator = new NameValidator();
            nameValidator.AddValidator(validator);

            var wasteWaterTreatmentPlant = new WasteWaterTreatmentPlant { Name = "some_name" };
            var row = new WasteWaterTreatmentPlantRow(wasteWaterTreatmentPlant, nameValidator);

            // Act
            row.Name = "some_invalid_name";

            // Assert
            Assert.That(row.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void SetName_ValidName_NameIsUpdated()
        {
            // Arrange
            var validator = Substitute.For<IValidator<string>>();
            validator.Validate("some_valid_name").Returns(ValidationResult.Success);
            var nameValidator = new NameValidator();
            nameValidator.AddValidator(validator);

            var wasteWaterTreatmentPlant = new WasteWaterTreatmentPlant { Name = "some_name" };
            var row = new WasteWaterTreatmentPlantRow(wasteWaterTreatmentPlant, nameValidator);

            // Act
            row.Name = "some_valid_name";

            // Assert
            Assert.That(row.Name, Is.EqualTo("some_valid_name"));
        }

        [Test]
        public void SetDescription_SetsWasteWaterTreatmentPlantDescription()
        {
            // Arrange
            var wasteWaterTreatmentPlant = new WasteWaterTreatmentPlant();
            var row = new WasteWaterTreatmentPlantRow(wasteWaterTreatmentPlant, new NameValidator());

            // Act
            row.Description = "some_name";

            // Assert
            Assert.That(wasteWaterTreatmentPlant.Description, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetDescription_GetsWasteWaterTreatmentPlantDescription()
        {
            // Arrange
            var wasteWaterTreatmentPlant = new WasteWaterTreatmentPlant { Description = "some_name" };
            var row = new WasteWaterTreatmentPlantRow(wasteWaterTreatmentPlant, new NameValidator());

            // Act
            string result = row.Description;

            // Assert
            Assert.AreEqual(result, "some_name");
        }

        [Test]
        public void SetLongName_SetsWasteWaterTreatmentPlantLongName()
        {
            // Arrange
            var wasteWaterTreatmentPlant = new WasteWaterTreatmentPlant();
            var row = new WasteWaterTreatmentPlantRow(wasteWaterTreatmentPlant, new NameValidator());

            // Act
            row.LongName = "some_name";

            // Assert
            Assert.That(wasteWaterTreatmentPlant.LongName, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetLongName_GetsWasteWaterTreatmentPlantLongName()
        {
            // Arrange
            var wasteWaterTreatmentPlant = new WasteWaterTreatmentPlant { LongName = "some_name" };
            var row = new WasteWaterTreatmentPlantRow(wasteWaterTreatmentPlant, new NameValidator());

            // Act
            string result = row.LongName;

            // Assert
            Assert.AreEqual(result, "some_name");
        }
    }
}