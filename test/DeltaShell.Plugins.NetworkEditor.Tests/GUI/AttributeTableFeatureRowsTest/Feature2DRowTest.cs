using DelftTools.Utils.Validation.Common;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.AttributeTableFeatureRowsTest
{
    [TestFixture]
    public class Feature2DRowTest
    {
        [Test]
        public void Constructor_WithNullFeature2D_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new Feature2DRow(null, new NameValidator());
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
                new Feature2DRow(new Feature2D(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsFeature2D()
        {
            // Arrange
            var feature2D = new Feature2D();
            var row = new Feature2DRow(feature2D, new NameValidator());

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(feature2D));
        }

        [Test]
        public void WhenFeature2DPropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var feature2D = new Feature2D();
            var row = new Feature2DRow(feature2D, new NameValidator());
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            feature2D.Name = "some_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void SetName_SetsFeature2DName()
        {
            // Arrange
            var feature2D = new Feature2D();
            var row = new Feature2DRow(feature2D, new NameValidator());

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(feature2D.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsFeature2DName()
        {
            // Arrange
            var feature2D = new Feature2D { Name = "some_name" };
            var row = new Feature2DRow(feature2D, new NameValidator());

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

            var feature2D = new Feature2D { Name = "some_name" };
            var row = new Feature2DRow(feature2D, nameValidator);

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

            var feature2D = new Feature2D { Name = "some_name" };
            var row = new Feature2DRow(feature2D, nameValidator);

            // Act
            row.Name = "some_valid_name";

            // Assert
            Assert.That(row.Name, Is.EqualTo("some_valid_name"));
        }
    }
}