using DelftTools.Hydro;
using DelftTools.Utils.Validation.NameValidation;
using Deltares.Infrastructure.API.Validation;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.AttributeTableFeatureRowsTest
{
    [TestFixture]
    public class GullyRowTest
    {
        [Test]
        public void Constructor_WithNullGully_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new GullyRow(null, new NameValidator());
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
                new GullyRow(new Gully(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsGully()
        {
            // Arrange
            var gully = new Gully();
            var row = new GullyRow(gully, new NameValidator());

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(gully));
        }

        [Test]
        public void WhenGullyPropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var gully = new Gully();
            var row = new GullyRow(gully, new NameValidator());
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            gully.GroupName = "some_group_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void SetGroupName_SetsGullyGroupName()
        {
            // Arrange
            var gully = new Gully();
            var row = new GullyRow(gully, new NameValidator());

            // Act
            row.GroupName = "some_group_name";

            // Assert
            Assert.That(gully.GroupName, Is.EqualTo("some_group_name"));
        }

        [Test]
        public void GetGroupName_GetsGullyGroupName()
        {
            // Arrange
            var gully = new Gully { GroupName = "some_group_name" };
            var row = new GullyRow(gully, new NameValidator());

            // Act
            string result = row.GroupName;

            // Assert
            Assert.AreEqual(result, "some_group_name");
        }

        [Test]
        public void SetName_SetsGullyName()
        {
            // Arrange
            var gully = new Gully();
            var row = new GullyRow(gully, new NameValidator());

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(gully.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsGullyName()
        {
            // Arrange
            var gully = new Gully { Name = "some_name" };
            var row = new GullyRow(gully, new NameValidator());

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

            var gully = new Gully { Name = "some_name" };
            var row = new GullyRow(gully, nameValidator);

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

            var gully = new Gully { Name = "some_name" };
            var row = new GullyRow(gully, nameValidator);

            // Act
            row.Name = "some_valid_name";

            // Assert
            Assert.That(row.Name, Is.EqualTo("some_valid_name"));
        }

        [Test]
        public void GetX_GetsGullyX()
        {
            // Arrange
            var point = Substitute.For<IPoint>();
            point.Coordinate.Returns(new Coordinate(1.23, 4.56));
            var gully = new Gully { Geometry = point };
            var row = new GullyRow(gully, new NameValidator());

            // Act
            double result = row.X;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void GetY_GetsGullyY()
        {
            // Arrange
            var point = Substitute.For<IPoint>();
            point.Coordinate.Returns(new Coordinate(1.23, 4.56));
            var gully = new Gully { Geometry = point };
            var row = new GullyRow(gully, new NameValidator());

            // Act
            double result = row.Y;

            // Assert
            Assert.That(result, Is.EqualTo(4.56));
        }
    }
}