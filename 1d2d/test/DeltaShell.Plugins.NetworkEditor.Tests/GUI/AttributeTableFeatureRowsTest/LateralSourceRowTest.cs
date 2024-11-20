using System.ComponentModel;
using DelftTools.Hydro;
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
    public class LateralSourceRowTest
    {
        [Test]
        public void Constructor_WithNullLateralSource_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new LateralSourceRow(null, new NameValidator());
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
                new LateralSourceRow(Substitute.For<ILateralSource, INotifyPropertyChanged>(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsLateralSource()
        {
            // Arrange
            var lateralSource = new LateralSource();
            var row = new LateralSourceRow(lateralSource, new NameValidator());

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(lateralSource));
        }

        [Test]
        public void WhenLateralSourcePropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var lateralSource = new LateralSource();
            var row = new LateralSourceRow(lateralSource, new NameValidator());
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            lateralSource.Name = "some_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void SetName_SetsLateralSourceName()
        {
            // Arrange
            var lateralSource = new LateralSource();
            var row = new LateralSourceRow(lateralSource, new NameValidator());

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(lateralSource.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsLateralSourceName()
        {
            // Arrange
            var lateralSource = new LateralSource { Name = "some_name" };
            var row = new LateralSourceRow(lateralSource, new NameValidator());

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

            var lateralSource = new LateralSource { Name = "some_name" };
            var row = new LateralSourceRow(lateralSource, nameValidator);

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

            var lateralSource = new LateralSource { Name = "some_name" };
            var row = new LateralSourceRow(lateralSource, nameValidator);

            // Act
            row.Name = "some_valid_name";

            // Assert
            Assert.That(row.Name, Is.EqualTo("some_valid_name"));
        }

        [Test]
        public void SetLongName_SetsLateralSourceLongName()
        {
            // Arrange
            var lateralSource = new LateralSource();
            var row = new LateralSourceRow(lateralSource, new NameValidator());

            // Act
            row.LongName = "some_long_name";

            // Assert
            Assert.That(lateralSource.LongName, Is.EqualTo("some_long_name"));
        }

        [Test]
        public void GetLongName_GetsLateralSourceLongName()
        {
            // Arrange
            var lateralSource = new LateralSource { LongName = "some_long_name" };
            var row = new LateralSourceRow(lateralSource, new NameValidator());

            // Act
            string result = row.LongName;

            // Assert
            Assert.AreEqual(result, "some_long_name");
        }

        [Test]
        public void GetBranch_GetsLateralSourceBranch()
        {
            // Arrange
            var branch = Substitute.For<IBranch>();
            branch.Name = "some_branch_name";
            var lateralSource = new LateralSource { Branch = branch };
            var row = new LateralSourceRow(lateralSource, new NameValidator());

            // Act
            string result = row.Branch;

            // Assert
            Assert.That(result, Is.EqualTo("some_branch_name"));
        }

        [Test]
        public void SetChainage_SetsLateralSourceChainage()
        {
            // Arrange
            var lateralSource = new LateralSource();
            var row = new LateralSourceRow(lateralSource, new NameValidator());

            // Act
            row.Chainage = 1.23;

            // Assert
            Assert.That(lateralSource.Chainage, Is.EqualTo(1.23));
        }

        [Test]
        public void GetChainage_GetsLateralSourceChainage()
        {
            // Arrange
            var lateralSource = new LateralSource { Chainage = 1.23 };
            var row = new LateralSourceRow(lateralSource, new NameValidator());

            // Act
            double result = row.Chainage;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void GetDiffuseLateral_GetsLateralSourceDiffuseLateral([Values] bool isDiffuse)
        {
            // Arrange
            ILateralSource lateralSource = Substitute.For<ILateralSource, INotifyPropertyChanged>();
            lateralSource.IsDiffuse.Returns(isDiffuse);
            var row = new LateralSourceRow(lateralSource, new NameValidator());

            // Act
            bool result = row.DiffuseLateral;

            // Assert
            Assert.That(result, Is.EqualTo(isDiffuse));
        }

        [Test]
        public void GetLength_GetsLateralSourceLength()
        {
            // Arrange
            var lateralSource = new LateralSource { Length = 4.56 };
            var row = new LateralSourceRow(lateralSource, new NameValidator());

            // Act
            double result = row.Length;

            // Assert
            Assert.That(result, Is.EqualTo(4.56));
        }
    }
}