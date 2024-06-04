using System.ComponentModel;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Validation.NameValidation;
using Deltares.Infrastructure.API.Validation;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.AttributeTableFeatureRowsTest
{
    [TestFixture]
    public class PumpRowTest
    {
        [Test]
        public void Constructor_WithNullPump_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new PumpRow(null, new NameValidator());
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
                new PumpRow(Substitute.For<IPump, INotifyPropertyChanged>(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsPump()
        {
            // Arrange
            var pump = new Pump();
            var row = new PumpRow(pump, new NameValidator());

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(pump));
        }

        [Test]
        public void WhenPumpPropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var pump = new Pump();
            var row = new PumpRow(pump, new NameValidator());
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            pump.Name = "some_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void SetName_SetsPumpName()
        {
            // Arrange
            var pump = new Pump();
            var row = new PumpRow(pump, new NameValidator());

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(pump.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsPumpName()
        {
            // Arrange
            var pump = new Pump { Name = "some_name" };
            var row = new PumpRow(pump, new NameValidator());

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

            var pump = new Pump { Name = "some_name" };
            var row = new PumpRow(pump, nameValidator);

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

            var pump = new Pump { Name = "some_name" };
            var row = new PumpRow(pump, nameValidator);

            // Act
            row.Name = "some_valid_name";

            // Assert
            Assert.That(row.Name, Is.EqualTo("some_valid_name"));
        }

        [Test]
        public void SetLongName_SetsPumpLongName()
        {
            // Arrange
            var pump = new Pump();
            var row = new PumpRow(pump, new NameValidator());

            // Act
            row.LongName = "some_long_name";

            // Assert
            Assert.That(pump.LongName, Is.EqualTo("some_long_name"));
        }

        [Test]
        public void GetLongName_GetsPumpLongName()
        {
            // Arrange
            var pump = new Pump { LongName = "some_long_name" };
            var row = new PumpRow(pump, new NameValidator());

            // Act
            string result = row.LongName;

            // Assert
            Assert.AreEqual(result, "some_long_name");
        }

        [Test]
        public void GetBranch_ReturnsPumpBranchName()
        {
            // Arrange
            var branch = Substitute.For<IBranch>();
            branch.Name = "some_branch_name";
            var pump = new Pump { Branch = branch };
            var row = new PumpRow(pump, new NameValidator());

            // Act
            string result = row.Branch;

            // Assert
            Assert.That(result, Is.EqualTo("some_branch_name"));
        }

        [Test]
        public void SetPositiveDirection_SetsPumpDirectionIsPositive([Values] bool directionIsPositive)
        {
            // Arrange
            var pump = new Pump();
            var row = new PumpRow(pump, new NameValidator());

            // Act
            row.PositiveDirection = directionIsPositive;

            // Assert
            Assert.That(pump.DirectionIsPositive, Is.EqualTo(directionIsPositive));
        }

        [Test]
        public void GetPositiveDirection_GetsPumpDirectionIsPositive([Values] bool directionIsPositive)
        {
            // Arrange
            var pump = new Pump { DirectionIsPositive = directionIsPositive };
            var row = new PumpRow(pump, new NameValidator());

            // Act
            bool result = row.PositiveDirection;

            // Assert
            Assert.That(result, Is.EqualTo(directionIsPositive));
        }

        [Test]
        public void SetCapacity_SetsPumpCapacity()
        {
            // Arrange
            var pump = new Pump();
            var row = new PumpRow(pump, new NameValidator());

            // Act
            row.Capacity = 1.23;

            // Assert
            Assert.That(pump.Capacity, Is.EqualTo(1.23));
        }

        [Test]
        public void GetCapacity_GetsPumpCapacity()
        {
            // Arrange
            var pump = new Pump { Capacity = 1.23 };
            var row = new PumpRow(pump, new NameValidator());

            // Act
            double result = row.Capacity;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void SetStartDelivery_SetsPumpStartDelivery()
        {
            // Arrange
            var pump = new Pump();
            var row = new PumpRow(pump, new NameValidator());

            // Act
            row.StartDelivery = 1.23;

            // Assert
            Assert.That(pump.StartDelivery, Is.EqualTo(1.23));
        }

        [Test]
        public void GetStartDelivery_GetsPumpStartDelivery()
        {
            // Arrange
            var pump = new Pump { StartDelivery = 1.23 };
            var row = new PumpRow(pump, new NameValidator());

            // Act
            double result = row.StartDelivery;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void SetStopDelivery_SetsPumpStopDelivery()
        {
            // Arrange
            var pump = new Pump();
            var row = new PumpRow(pump, new NameValidator());

            // Act
            row.StopDelivery = 1.23;

            // Assert
            Assert.That(pump.StopDelivery, Is.EqualTo(1.23));
        }

        [Test]
        public void GetStopDelivery_GetsPumpStopDelivery()
        {
            // Arrange
            var pump = new Pump { StopDelivery = 1.23 };
            var row = new PumpRow(pump, new NameValidator());

            // Act
            double result = row.StopDelivery;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void SetStartSuction_SetsPumpStartSuction()
        {
            // Arrange
            var pump = new Pump();
            var row = new PumpRow(pump, new NameValidator());

            // Act
            row.StartSuction = 1.23;

            // Assert
            Assert.That(pump.StartSuction, Is.EqualTo(1.23));
        }

        [Test]
        public void GetStartSuction_GetsPumpStartSuction()
        {
            // Arrange
            var pump = new Pump { StartSuction = 1.23 };
            var row = new PumpRow(pump, new NameValidator());

            // Act
            double result = row.StartSuction;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void SetStopSuction_SetsPumpStopSuction()
        {
            // Arrange
            var pump = new Pump();
            var row = new PumpRow(pump, new NameValidator());

            // Act
            row.StopSuction = 1.23;

            // Assert
            Assert.That(pump.StopSuction, Is.EqualTo(1.23));
        }

        [Test]
        public void GetStopSuction_GetsPumpStopSuction()
        {
            // Arrange
            var pump = new Pump { StopSuction = 1.23 };
            var row = new PumpRow(pump, new NameValidator());

            // Act
            double result = row.StopSuction;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void SetControlOn_SetsPumpControlOn([Values] PumpControlDirection pumpControlDirection)
        {
            // Arrange
            var pump = new Pump();
            var row = new PumpRow(pump, new NameValidator());

            // Act
            row.ControlOn = pumpControlDirection;

            // Assert
            Assert.That(pump.ControlDirection, Is.EqualTo(pumpControlDirection));
        }

        [Test]
        public void GetControlOn_GetsPumpControlOn([Values] PumpControlDirection pumpControlDirection)
        {
            // Arrange
            var pump = new Pump { ControlDirection = pumpControlDirection };
            var row = new PumpRow(pump, new NameValidator());

            // Act
            PumpControlDirection result = row.ControlOn;

            // Assert
            Assert.That(result, Is.EqualTo(pumpControlDirection));
        }
    }
}