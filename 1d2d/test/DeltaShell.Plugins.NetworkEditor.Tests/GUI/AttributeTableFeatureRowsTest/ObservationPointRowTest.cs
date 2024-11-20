using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Utils.Validation.NameValidation;
using Deltares.Infrastructure.API.Validation;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Networks;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.AttributeTableFeatureRowsTest
{
    [TestFixture]
    public class ObservationPointRowTest
    {
        [Test]
        public void Constructor_WithNullObservationPoint_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new ObservationPointRow(null, new NameValidator());
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
                new ObservationPointRow(Substitute.For<IObservationPoint, INotifyPropertyChanged>(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsObservationPoint()
        {
            // Arrange
            var observationPoint = new ObservationPoint();
            var row = new ObservationPointRow(observationPoint, new NameValidator());

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(observationPoint));
        }

        [Test]
        public void WhenObservationPointPropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var observationPoint = new ObservationPoint();
            var row = new ObservationPointRow(observationPoint, new NameValidator());
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            observationPoint.Name = "some_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void SetName_SetsObservationPointName()
        {
            // Arrange
            var observationPoint = new ObservationPoint();
            var row = new ObservationPointRow(observationPoint, new NameValidator());

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(observationPoint.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsObservationPointName()
        {
            // Arrange
            var observationPoint = new ObservationPoint { Name = "some_name" };
            var row = new ObservationPointRow(observationPoint, new NameValidator());

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

            var observationPoint = new ObservationPoint { Name = "some_name" };
            var row = new ObservationPointRow(observationPoint, nameValidator);

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

            var observationPoint = new ObservationPoint { Name = "some_name" };
            var row = new ObservationPointRow(observationPoint, nameValidator);

            // Act
            row.Name = "some_valid_name";

            // Assert
            Assert.That(row.Name, Is.EqualTo("some_valid_name"));
        }

        [Test]
        public void SetLongName_SetsObservationPointLongName()
        {
            // Arrange
            var observationPoint = new ObservationPoint();
            var row = new ObservationPointRow(observationPoint, new NameValidator());

            // Act
            row.LongName = "some_long_name";

            // Assert
            Assert.That(observationPoint.LongName, Is.EqualTo("some_long_name"));
        }

        [Test]
        public void GetLongName_GetsObservationPointLongName()
        {
            // Arrange
            var observationPoint = new ObservationPoint { LongName = "some_long_name" };
            var row = new ObservationPointRow(observationPoint, new NameValidator());

            // Act
            string result = row.LongName;

            // Assert
            Assert.AreEqual(result, "some_long_name");
        }

        [Test]
        public void GetBranch_GetsObservationPointBranchName()
        {
            // Arrange
            var branch = new Branch { Name = "some_branch_name" };
            var observationPoint = new ObservationPoint { Branch = branch };
            var row = new ObservationPointRow(observationPoint, new NameValidator());

            // Act
            string result = row.Branch;

            // Assert
            Assert.AreEqual(result, "some_branch_name");
        }

        [Test]
        public void GetChainage_GetsObservationPointChainage()
        {
            // Arrange
            var observationPoint = new ObservationPoint { Chainage = 123.45 };
            var row = new ObservationPointRow(observationPoint, new NameValidator());

            // Act
            double result = row.Chainage;

            // Assert
            Assert.That(result, Is.EqualTo(123.45));
        }
    }
}