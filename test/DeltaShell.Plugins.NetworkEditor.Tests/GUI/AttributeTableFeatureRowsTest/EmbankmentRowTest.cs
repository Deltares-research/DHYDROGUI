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
    public class EmbankmentRowTest
    {
        [Test]
        public void Constructor_WithNullEmbankment_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new EmbankmentRow(null, new NameValidator());
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
                new EmbankmentRow(new Embankment(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsEmbankment()
        {
            // Arrange
            var embankment = new Embankment();
            var row = new EmbankmentRow(embankment, new NameValidator());

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(embankment));
        }

        [Test]
        public void WhenEmbankmentPropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var embankment = new Embankment();
            var row = new EmbankmentRow(embankment, new NameValidator());
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            embankment.Name = "some_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void SetName_SetsEmbankmentName()
        {
            // Arrange
            var embankment = new Embankment();
            var row = new EmbankmentRow(embankment, new NameValidator());

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(embankment.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsEmbankmentName()
        {
            // Arrange
            var embankment = new Embankment { Name = "some_name" };
            var row = new EmbankmentRow(embankment, new NameValidator());

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

            var embankment = new Embankment { Name = "some_name" };
            var row = new EmbankmentRow(embankment, nameValidator);

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

            var embankment = new Embankment { Name = "some_name" };
            var row = new EmbankmentRow(embankment, nameValidator);

            // Act
            row.Name = "some_valid_name";

            // Assert
            Assert.That(row.Name, Is.EqualTo("some_valid_name"));
        }
    }
}