using System.ComponentModel;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Validation.Common;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.AttributeTableFeatureRowsTest
{
    [TestFixture]
    public class RetentionRowTest
    {
        [Test]
        public void Constructor_WithNullRetention_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new RetentionRow(null, new NameValidator());
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
                new RetentionRow(Substitute.For<IRetention, INotifyPropertyChanged>(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsRetention()
        {
            // Arrange
            var retention = new Retention();
            var row = new RetentionRow(retention, new NameValidator());

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(retention));
        }

        [Test]
        public void WhenRetentionPropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var retention = new Retention();
            var row = new RetentionRow(retention, new NameValidator());
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            retention.Name = "some_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void SetName_SetsRetentionName()
        {
            // Arrange
            var retention = new Retention();
            var row = new RetentionRow(retention, new NameValidator());

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(retention.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsRetentionName()
        {
            // Arrange
            var retention = new Retention { Name = "some_name" };
            var row = new RetentionRow(retention, new NameValidator());

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

            var retention = new Retention { Name = "some_name" };
            var row = new RetentionRow(retention, nameValidator);

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

            var retention = new Retention { Name = "some_name" };
            var row = new RetentionRow(retention, nameValidator);

            // Act
            row.Name = "some_valid_name";

            // Assert
            Assert.That(row.Name, Is.EqualTo("some_valid_name"));
        }

        [Test]
        public void SetLongName_SetsRetentionLongName()
        {
            // Arrange
            var retention = new Retention();
            var row = new RetentionRow(retention, new NameValidator());

            // Act
            row.LongName = "some_long_name";

            // Assert
            Assert.That(retention.LongName, Is.EqualTo("some_long_name"));
        }

        [Test]
        public void GetLongName_GetsRetentionLongName()
        {
            // Arrange
            var retention = new Retention { LongName = "some_long_name" };
            var row = new RetentionRow(retention, new NameValidator());

            // Act
            string result = row.LongName;

            // Assert
            Assert.AreEqual(result, "some_long_name");
        }

        [Test]
        public void GetBranch_GetsCulvertBranch()
        {
            // Arrange
            var branch = Substitute.For<IBranch>();
            branch.Name = "some_branch_name";
            var retention = new Retention { Branch = branch };
            var row = new RetentionRow(retention, new NameValidator());

            // Act
            string result = row.Branch;

            // Assert
            Assert.That(result, Is.EqualTo("some_branch_name"));
        }

        [Test]
        public void SetChainage_SetsRetentionChainage()
        {
            // Arrange
            var retention = new Retention();
            var row = new RetentionRow(retention, new NameValidator());

            // Act
            row.Chainage = 1.23;

            // Assert
            Assert.That(retention.Chainage, Is.EqualTo(1.23));
        }

        [Test]
        public void GetChainage_GetsRetentionChainage()
        {
            // Arrange
            var retention = new Retention { Chainage = 1.23 };
            var row = new RetentionRow(retention, new NameValidator());

            // Act
            double result = row.Chainage;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void SetType_SetsRetentionType([Values] RetentionType retentionType)
        {
            // Arrange
            var retention = new Retention();
            var row = new RetentionRow(retention, new NameValidator());

            // Act
            row.Type = retentionType;

            // Assert
            Assert.That(retention.Type, Is.EqualTo(retentionType));
        }

        [Test]
        public void GetType_GetsRetentionType([Values] RetentionType retentionType)
        {
            // Arrange
            var retention = new Retention { Type = retentionType };
            var row = new RetentionRow(retention, new NameValidator());

            // Act
            RetentionType result = row.Type;

            // Assert
            Assert.That(result, Is.EqualTo(retentionType));
        }

        [Test]
        public void SetStorageArea_SetsRetentionStorageArea()
        {
            // Arrange
            var retention = new Retention();
            var row = new RetentionRow(retention, new NameValidator());

            // Act
            row.StorageArea = 1.23;

            // Assert
            Assert.That(retention.StorageArea, Is.EqualTo(1.23));
        }

        [Test]
        public void GetStorageArea_GetsRetentionStorageArea()
        {
            // Arrange
            var retention = new Retention { StorageArea = 1.23 };
            var row = new RetentionRow(retention, new NameValidator());

            // Act
            double result = row.StorageArea;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void SetBedLevel_SetsRetentionBedLevel()
        {
            // Arrange
            var retention = new Retention();
            var row = new RetentionRow(retention, new NameValidator());

            // Act
            row.BedLevel = 1.23;

            // Assert
            Assert.That(retention.BedLevel, Is.EqualTo(1.23));
        }

        [Test]
        public void GetBedLevel_GetsRetentionBedLevel()
        {
            // Arrange
            var retention = new Retention { BedLevel = 1.23 };
            var row = new RetentionRow(retention, new NameValidator());

            // Act
            double result = row.BedLevel;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void SetUseTable_SetsRetentionUseTable()
        {
            // Arrange
            var retention = new Retention();
            var row = new RetentionRow(retention, new NameValidator());

            // Act
            row.UseTable = true;

            // Assert
            Assert.That(retention.UseTable, Is.EqualTo(true));
        }

        [Test]
        public void GetUseTable_GetsRetentionUseTable()
        {
            // Arrange
            var retention = new Retention { UseTable = true };
            var row = new RetentionRow(retention, new NameValidator());

            // Act
            bool result = row.UseTable;

            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void SetData_SetsRetentionData()
        {
            // Arrange
            var retention = new Retention();
            var row = new RetentionRow(retention, new NameValidator());
            var data = new Function();

            // Act
            row.Data = data;

            // Assert
            Assert.That(retention.Data, Is.SameAs(data));
        }

        [Test]
        public void GetData_GetsRetentionData()
        {
            // Arrange
            var function = new Function();
            var retention = new Retention { Data = function };
            var row = new RetentionRow(retention, new NameValidator());

            // Act
            IFunction result = row.Data;

            // Assert
            Assert.That(result, Is.SameAs(function));
        }

        [Test]
        public void SetInterpolationType_SetsRetentionInterpolationType([Values] InterpolationType interpolationType)
        {
            // Arrange
            var retention = new Retention();
            var row = new RetentionRow(retention, new NameValidator());

            // Act
            row.InterpolationType = interpolationType;

            // Assert
            Assert.That(retention.InterpolationType, Is.EqualTo(interpolationType));
        }

        [Test]
        public void GetInterpolationType_GetsRetentionInterpolationType([Values] InterpolationType interpolationType)
        {
            // Arrange
            var retention = new Retention { InterpolationType = interpolationType };
            var row = new RetentionRow(retention, new NameValidator());

            // Act
            InterpolationType result = row.InterpolationType;

            // Assert
            Assert.That(result, Is.EqualTo(interpolationType));
        }
    }
}