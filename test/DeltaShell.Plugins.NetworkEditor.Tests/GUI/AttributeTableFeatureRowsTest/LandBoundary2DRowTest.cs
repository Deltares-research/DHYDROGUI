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
    public class LandBoundary2DRowTest
    {
        [Test]
        public void Constructor_WithNullLandBoundary2D_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new LandBoundary2DRow(null, new NameValidator());
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
                new LandBoundary2DRow(new LandBoundary2D(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsLandBoundary2D()
        {
            // Arrange
            var landBoundary2D = new LandBoundary2D();
            var row = new LandBoundary2DRow(landBoundary2D, new NameValidator());

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(landBoundary2D));
        }

        [Test]
        public void WhenLandBoundary2DPropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var landBoundary2D = new LandBoundary2D();
            var row = new LandBoundary2DRow(landBoundary2D, new NameValidator());
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            landBoundary2D.Name = "some_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void SetName_SetsLandBoundary2DName()
        {
            // Arrange
            var landBoundary2D = new LandBoundary2D();
            var row = new LandBoundary2DRow(landBoundary2D, new NameValidator());

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(landBoundary2D.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsLandBoundary2DName()
        {
            // Arrange
            var landBoundary2D = new LandBoundary2D { Name = "some_name" };
            var row = new LandBoundary2DRow(landBoundary2D, new NameValidator());

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

            var landBoundary2D = new LandBoundary2D { Name = "some_name" };
            var row = new LandBoundary2DRow(landBoundary2D, nameValidator);

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

            var landBoundary2D = new LandBoundary2D { Name = "some_name" };
            var row = new LandBoundary2DRow(landBoundary2D, nameValidator);

            // Act
            row.Name = "some_valid_name";

            // Assert
            Assert.That(row.Name, Is.EqualTo("some_valid_name"));
        }

        [Test]
        public void SetGroupName_SetsLandBoundary2DGroupName()
        {
            // Arrange
            var landBoundary2D = new LandBoundary2D();
            var row = new LandBoundary2DRow(landBoundary2D, new NameValidator());

            // Act
            row.GroupName = "some_group_name";

            // Assert
            Assert.That(landBoundary2D.GroupName, Is.EqualTo("some_group_name"));
        }

        [Test]
        public void GetGroupName_GetsLandBoundary2DGroupName()
        {
            // Arrange
            var landBoundary2D = new LandBoundary2D { GroupName = "some_group_name" };
            var row = new LandBoundary2DRow(landBoundary2D, new NameValidator());

            // Act
            string result = row.GroupName;

            // Assert
            Assert.AreEqual(result, "some_group_name");
        }
    }
}