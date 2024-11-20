using DelftTools.Hydro.Structures;
using DelftTools.Utils.Validation.NameValidation;
using Deltares.Infrastructure.API.Validation;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using GeoAPI.Extensions.Feature;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.AttributeTableFeatureRowsTest
{
    [TestFixture]
    public class Pump2DRowTest
    {
        [Test]
        public void Constructor_WithNullPump2D_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new Pump2DRow(null, new NameValidator());
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
                new Pump2DRow(new Pump2D(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsPump2D()
        {
            // Arrange
            var pump2D = new Pump2D();
            var row = new Pump2DRow(pump2D, new NameValidator());

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(pump2D));
        }

        [Test]
        public void WhenPump2DPropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var pump2D = new Pump2D();
            var row = new Pump2DRow(pump2D, new NameValidator());
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            pump2D.Name = "some_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void SetName_SetsPump2DName()
        {
            // Arrange
            var pump2D = new Pump2D();
            var row = new Pump2DRow(pump2D, new NameValidator());

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(pump2D.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsPump2DName()
        {
            // Arrange
            var pump2D = new Pump2D { Name = "some_name" };
            var row = new Pump2DRow(pump2D, new NameValidator());

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

            var pump2D = new Pump2D { Name = "some_name" };
            var row = new Pump2DRow(pump2D, nameValidator);

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

            var pump2D = new Pump2D { Name = "some_name" };
            var row = new Pump2DRow(pump2D, nameValidator);

            // Act
            row.Name = "some_valid_name";

            // Assert
            Assert.That(row.Name, Is.EqualTo("some_valid_name"));
        }

        [Test]
        public void SetLongName_SetsPump2DLongName()
        {
            // Arrange
            var pump2D = new Pump2D();
            var row = new Pump2DRow(pump2D, new NameValidator());

            // Act
            row.LongName = "some_long_name";

            // Assert
            Assert.That(pump2D.LongName, Is.EqualTo("some_long_name"));
        }

        [Test]
        public void GetLongName_GetsPump2DLongName()
        {
            // Arrange
            var pump2D = new Pump2D { LongName = "some_long_name" };
            var row = new Pump2DRow(pump2D, new NameValidator());

            // Act
            string result = row.LongName;

            // Assert
            Assert.AreEqual(result, "some_long_name");
        }

        [Test]
        public void GetBranch_ReturnsEmptyString()
        {
            // Arrange
            var pump2D = new Pump2D();
            var row = new Pump2DRow(pump2D, new NameValidator());

            // Act
            string result = row.Branch;

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void SetPositiveDirection_SetsPump2DDirectionIsPositive([Values] bool directionIsPositive)
        {
            // Arrange
            var pump2D = new Pump2D();
            var row = new Pump2DRow(pump2D, new NameValidator());

            // Act
            row.PositiveDirection = directionIsPositive;

            // Assert
            Assert.That(pump2D.DirectionIsPositive, Is.EqualTo(directionIsPositive));
        }

        [Test]
        public void GetPositiveDirection_GetsPump2DDirectionIsPositive([Values] bool directionIsPositive)
        {
            // Arrange
            var pump2D = new Pump2D { DirectionIsPositive = directionIsPositive };
            var row = new Pump2DRow(pump2D, new NameValidator());

            // Act
            bool result = row.PositiveDirection;

            // Assert
            Assert.That(result, Is.EqualTo(directionIsPositive));
        }

        [Test]
        public void SetCapacity_SetsPump2DCapacity()
        {
            // Arrange
            var pump2D = new Pump2D();
            var row = new Pump2DRow(pump2D, new NameValidator());

            // Act
            row.Capacity = 1.23;

            // Assert
            Assert.That(pump2D.Capacity, Is.EqualTo(1.23));
        }

        [Test]
        public void GetCapacity_GetsPump2DCapacity()
        {
            // Arrange
            var pump2D = new Pump2D { Capacity = 1.23 };
            var row = new Pump2DRow(pump2D, new NameValidator());

            // Act
            double result = row.Capacity;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void SetStartDelivery_SetsPump2DStartDelivery()
        {
            // Arrange
            var pump2D = new Pump2D();
            var row = new Pump2DRow(pump2D, new NameValidator());

            // Act
            row.StartDelivery = 1.23;

            // Assert
            Assert.That(pump2D.StartDelivery, Is.EqualTo(1.23));
        }

        [Test]
        public void GetStartDelivery_GetsPump2DStartDelivery()
        {
            // Arrange
            var pump2D = new Pump2D { StartDelivery = 1.23 };
            var row = new Pump2DRow(pump2D, new NameValidator());

            // Act
            double result = row.StartDelivery;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void SetStopDelivery_SetsPump2DStopDelivery()
        {
            // Arrange
            var pump2D = new Pump2D();
            var row = new Pump2DRow(pump2D, new NameValidator());

            // Act
            row.StopDelivery = 1.23;

            // Assert
            Assert.That(pump2D.StopDelivery, Is.EqualTo(1.23));
        }

        [Test]
        public void GetStopDelivery_GetsPump2DStopDelivery()
        {
            // Arrange
            var pump2D = new Pump2D { StopDelivery = 1.23 };
            var row = new Pump2DRow(pump2D, new NameValidator());

            // Act
            double result = row.StopDelivery;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void SetStartSuction_SetsPump2DStartSuction()
        {
            // Arrange
            var pump2D = new Pump2D();
            var row = new Pump2DRow(pump2D, new NameValidator());

            // Act
            row.StartSuction = 1.23;

            // Assert
            Assert.That(pump2D.StartSuction, Is.EqualTo(1.23));
        }

        [Test]
        public void GetStartSuction_GetsPump2DStartSuction()
        {
            // Arrange
            var pump2D = new Pump2D { StartSuction = 1.23 };
            var row = new Pump2DRow(pump2D, new NameValidator());

            // Act
            double result = row.StartSuction;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void SetStopSuction_SetsPump2DStopSuction()
        {
            // Arrange
            var pump2D = new Pump2D();
            var row = new Pump2DRow(pump2D, new NameValidator());

            // Act
            row.StopSuction = 1.23;

            // Assert
            Assert.That(pump2D.StopSuction, Is.EqualTo(1.23));
        }

        [Test]
        public void GetStopSuction_GetsPump2DStopSuction()
        {
            // Arrange
            var pump2D = new Pump2D { StopSuction = 1.23 };
            var row = new Pump2DRow(pump2D, new NameValidator());

            // Act
            double result = row.StopSuction;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void SetControlOn_SetsPump2DControlOn([Values] PumpControlDirection pumpControlDirection)
        {
            // Arrange
            var pump2D = new Pump2D();
            var row = new Pump2DRow(pump2D, new NameValidator());

            // Act
            row.ControlOn = pumpControlDirection;

            // Assert
            Assert.That(pump2D.ControlDirection, Is.EqualTo(pumpControlDirection));
        }

        [Test]
        public void GetControlOn_GetsPump2DControlOn([Values] PumpControlDirection pumpControlDirection)
        {
            // Arrange
            var pump2D = new Pump2D { ControlDirection = pumpControlDirection };
            var row = new Pump2DRow(pump2D, new NameValidator());

            // Act
            PumpControlDirection result = row.ControlOn;

            // Assert
            Assert.That(result, Is.EqualTo(pumpControlDirection));
        }

        [Test]
        public void SetGroupName_SetsPump2DGroupName()
        {
            // Arrange
            var pump2D = new Pump2D();
            var row = new Pump2DRow(pump2D, new NameValidator());

            // Act
            row.GroupName = "some_group_name";

            // Assert
            Assert.That(pump2D.GroupName, Is.EqualTo("some_group_name"));
        }

        [Test]
        public void GetGroupName_GetsPump2DGroupName()
        {
            // Arrange
            var pump2D = new Pump2D { GroupName = "some_group_name" };
            var row = new Pump2DRow(pump2D, new NameValidator());

            // Act
            string result = row.GroupName;

            // Assert
            Assert.AreEqual(result, "some_group_name");
        }
    }
}