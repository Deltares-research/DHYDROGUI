using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
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
    public class CompositeBranchStructureRowTest
    {
        [Test]
        public void Constructor_WithNullCompositeBranchStructure_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new CompositeBranchStructureRow(null, new NameValidator());
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
                new CompositeBranchStructureRow(Substitute.For<ICompositeBranchStructure, INotifyPropertyChanged>(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsCompositeBranchStructure()
        {
            // Arrange
            var compositeBranchStructure = new CompositeBranchStructure();
            var row = new CompositeBranchStructureRow(compositeBranchStructure, new NameValidator());

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(compositeBranchStructure));
        }

        [Test]
        public void WhenCompositeBranchStructurePropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var compositeBranchStructure = new CompositeBranchStructure();
            var row = new CompositeBranchStructureRow(compositeBranchStructure, new NameValidator());
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            compositeBranchStructure.Name = "some_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void SetName_SetsCompositeBranchStructureName()
        {
            // Arrange
            var compositeBranchStructure = new CompositeBranchStructure();
            var row = new CompositeBranchStructureRow(compositeBranchStructure, new NameValidator());

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(compositeBranchStructure.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsCompositeBranchStructureName()
        {
            // Arrange
            var compositeBranchStructure = new CompositeBranchStructure { Name = "some_name" };
            var row = new CompositeBranchStructureRow(compositeBranchStructure, new NameValidator());

            // Act
            string result = row.Name;

            // Assert
            Assert.That(result, Is.EqualTo("some_name"));
        }

        [Test]
        public void SetName_InvalidName_OriginalNameIsPreserved()
        {
            // Arrange
            var validator = Substitute.For<IValidator<string>>();
            validator.Validate("some_invalid_name").Returns(ValidationResult.Fail("message"));
            var nameValidator = new NameValidator();
            nameValidator.AddValidator(validator);

            var compositeBranchStructure = new CompositeBranchStructure { Name = "some_name" };
            var row = new CompositeBranchStructureRow(compositeBranchStructure, nameValidator);

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

            var compositeBranchStructure = new CompositeBranchStructure { Name = "some_name" };
            var row = new CompositeBranchStructureRow(compositeBranchStructure, nameValidator);

            // Act
            row.Name = "some_valid_name";

            // Assert
            Assert.That(row.Name, Is.EqualTo("some_valid_name"));
        }

        [Test]
        public void SetLongName_SetsCompositeBranchStructureNLongName()
        {
            // Arrange
            var compositeBranchStructure = new CompositeBranchStructure();
            var row = new CompositeBranchStructureRow(compositeBranchStructure, new NameValidator());

            // Act
            row.LongName = "some_name";

            // Assert
            Assert.That(compositeBranchStructure.LongName, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetLongName_GetsCompositeBranchStructureLongName()
        {
            // Arrange
            var compositeBranchStructure = new CompositeBranchStructure { LongName = "some_name" };
            var row = new CompositeBranchStructureRow(compositeBranchStructure, new NameValidator());

            // Act
            string result = row.LongName;

            // Assert
            Assert.That(result, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetBranch_GetsCompositeBranchStructureBranchName()
        {
            // Arrange
            var branch = Substitute.For<IBranch>();
            branch.Name = "some_branch_name";
            var compositeBranchStructure = new CompositeBranchStructure { Branch = branch };
            var row = new CompositeBranchStructureRow(compositeBranchStructure, new NameValidator());

            // Act
            string result = row.Branch;

            // Assert
            Assert.That(result, Is.EqualTo("some_branch_name"));
        }

        [Test]
        public void GetStructures_GetsNumberOfStructuresOfCompositeBranchStructure()
        {
            // Arrange
            var compositeBranchStructure = new CompositeBranchStructure();
            compositeBranchStructure.Structures = new EventedList<IStructure1D>(new IStructure1D[3]);
            var row = new CompositeBranchStructureRow(compositeBranchStructure, new NameValidator());

            // Act
            int result = row.Structures;

            // Assert
            Assert.That(result, Is.EqualTo(3));
        }
    }
}