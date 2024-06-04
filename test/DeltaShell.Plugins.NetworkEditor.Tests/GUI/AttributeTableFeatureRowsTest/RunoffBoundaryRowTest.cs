using DelftTools.Hydro;
using DelftTools.Utils.Validation.NameValidation;
using Deltares.Infrastructure.API.Validation;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using GeoAPI.Extensions.Feature;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.AttributeTableFeatureRowsTest
{
    [TestFixture]
    public class RunoffBoundaryRowTest
    {
        [Test]
        public void Constructor_WithNullRunoffBoundary_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new RunoffBoundaryRow(null, new NameValidator());
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
                new RunoffBoundaryRow(new RunoffBoundary(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsRunoffBoundary()
        {
            // Arrange
            var runoffBoundary = new RunoffBoundary();
            var row = new RunoffBoundaryRow(runoffBoundary, new NameValidator());

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(runoffBoundary));
        }

        [Test]
        public void WhenRetentionPropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var runoffBoundary = new RunoffBoundary();
            var row = new RunoffBoundaryRow(runoffBoundary, new NameValidator());
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            runoffBoundary.Name = "some_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void SetName_SetsRunoffBoundaryName()
        {
            // Arrange
            var runoffBoundary = new RunoffBoundary();
            var row = new RunoffBoundaryRow(runoffBoundary, new NameValidator());

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(runoffBoundary.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsRunoffBoundaryName()
        {
            // Arrange
            var runoffBoundary = new RunoffBoundary { Name = "some_name" };
            var row = new RunoffBoundaryRow(runoffBoundary, new NameValidator());

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

            var runoffBoundary = new RunoffBoundary { Name = "some_name" };
            var row = new RunoffBoundaryRow(runoffBoundary, nameValidator);

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

            var runoffBoundary = new RunoffBoundary { Name = "some_name" };
            var row = new RunoffBoundaryRow(runoffBoundary, nameValidator);

            // Act
            row.Name = "some_valid_name";

            // Assert
            Assert.That(row.Name, Is.EqualTo("some_valid_name"));
        }

        [Test]
        public void SetDescription_SetsRunoffBoundaryDescription()
        {
            // Arrange
            var runoffBoundary = new RunoffBoundary();
            var row = new RunoffBoundaryRow(runoffBoundary, new NameValidator());

            // Act
            row.Description = "some_description";

            // Assert
            Assert.That(runoffBoundary.Description, Is.EqualTo("some_description"));
        }

        [Test]
        public void GetDescription_GetsRunoffBoundaryDescription()
        {
            // Arrange
            var runoffBoundary = new RunoffBoundary { Description = "some_description" };
            var row = new RunoffBoundaryRow(runoffBoundary, new NameValidator());

            // Act
            string result = row.Description;

            // Assert
            Assert.AreEqual(result, "some_description");
        }

        [Test]
        public void SetLongName_SetsRunoffBoundaryLongName()
        {
            // Arrange
            var runoffBoundary = new RunoffBoundary();
            var row = new RunoffBoundaryRow(runoffBoundary, new NameValidator());

            // Act
            row.LongName = "some_long_name";

            // Assert
            Assert.That(runoffBoundary.LongName, Is.EqualTo("some_long_name"));
        }

        [Test]
        public void GetLongName_GetsRunoffBoundaryLongName()
        {
            // Arrange
            var runoffBoundary = new RunoffBoundary { LongName = "some_long_name" };
            var row = new RunoffBoundaryRow(runoffBoundary, new NameValidator());

            // Act
            string result = row.LongName;

            // Assert
            Assert.AreEqual(result, "some_long_name");
        }
    }
}