using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Validation.NameValidation;
using Deltares.Infrastructure.API.Validation;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.AttributeTableFeatureRowsTest
{
    [TestFixture]
    public class HydroNodeRowTest
    {
        [Test]
        public void Constructor_WithNullHydroNode_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new HydroNodeRow(null, new NameValidator());
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
                new HydroNodeRow(new HydroNode(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsHydroNode()
        {
            // Arrange
            var hydroNode = new HydroNode();
            var row = new HydroNodeRow(hydroNode, new NameValidator());

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(hydroNode));
        }

        [Test]
        public void WhenHydroNodePropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var hydroNode = new HydroNode();
            var row = new HydroNodeRow(hydroNode, new NameValidator());
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            hydroNode.Name = "some_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void SetName_SetsHydroNodeName()
        {
            // Arrange
            var hydroNode = new HydroNode();
            var row = new HydroNodeRow(hydroNode, new NameValidator());

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(hydroNode.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsHydroNodeName()
        {
            // Arrange
            var hydroNode = new HydroNode { Name = "some_name" };
            var row = new HydroNodeRow(hydroNode, new NameValidator());

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

            var hydroNode = new HydroNode { Name = "some_name" };
            var row = new HydroNodeRow(hydroNode, nameValidator);

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

            var hydroNode = new HydroNode { Name = "some_name" };
            var row = new HydroNodeRow(hydroNode, nameValidator);

            // Act
            row.Name = "some_valid_name";

            // Assert
            Assert.That(row.Name, Is.EqualTo("some_valid_name"));
        }

        [Test]
        public void SetLongName_SetsHydroNodeLongName()
        {
            // Arrange
            var hydroNode = new HydroNode();
            var row = new HydroNodeRow(hydroNode, new NameValidator());

            // Act
            row.LongName = "some_long_name";

            // Assert
            Assert.That(hydroNode.LongName, Is.EqualTo("some_long_name"));
        }

        [Test]
        public void GetLongName_GetsHydroNodeLongName()
        {
            // Arrange
            var hydroNode = new HydroNode { LongName = "some_long_name" };
            var row = new HydroNodeRow(hydroNode, new NameValidator());

            // Act
            string result = row.LongName;

            // Assert
            Assert.AreEqual(result, "some_long_name");
        }

        [Test]
        public void GetXCoordinate_GetsHydroNodeXCoordinate()
        {
            // Arrange
            var point = Substitute.For<IPoint>();
            point.Coordinate.Returns(new Coordinate(1.23, 4.56));
            var hydroNode = new HydroNode { Geometry = point };
            var row = new HydroNodeRow(hydroNode, new NameValidator());

            // Act
            double result = row.XCoordinate;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void GetYCoordinate_GetsHydroNodeYCoordinate()
        {
            // Arrange
            var point = Substitute.For<IPoint>();
            point.Coordinate.Returns(new Coordinate(1.23, 4.56));
            var hydroNode = new HydroNode { Geometry = point };
            var row = new HydroNodeRow(hydroNode, new NameValidator());

            // Act
            double result = row.YCoordinate;

            // Assert
            Assert.That(result, Is.EqualTo(4.56));
        }

        [Test]
        public void GetIncomingBranchesCount_GetsHydroNodeIncomingBranchesCount()
        {
            // Arrange
            var hydroNode = new HydroNode { IncomingBranches = new EventedList<IBranch>(new IBranch[3]) };
            var row = new HydroNodeRow(hydroNode, new NameValidator());

            // Act
            int result = row.IncomingBranchesCount;

            // Assert
            Assert.That(result, Is.EqualTo(3));
        }

        [Test]
        public void GetOutgoingBranchesCount_GetsHydroNodeOutgoingBranchesCount()
        {
            // Arrange
            var hydroNode = new HydroNode { OutgoingBranches = new EventedList<IBranch>(new IBranch[3]) };
            var row = new HydroNodeRow(hydroNode, new NameValidator());

            // Act
            double result = row.OutgoingBranchesCount;

            // Assert
            Assert.That(result, Is.EqualTo(3));
        }

        [Test]
        public void GetIsOnSingleBranch_GetsHydroNodeIsOnSingleBranch([Values] bool isOnSingleBranch)
        {
            // Arrange
            IHydroNode hydroNode = Substitute.For<IHydroNode, INotifyPropertyChanged>();
            hydroNode.IsOnSingleBranch.Returns(isOnSingleBranch);
            var row = new HydroNodeRow(hydroNode, new NameValidator());

            // Act
            bool result = row.IsOnSingleBranch;

            // Assert
            Assert.That(result, Is.EqualTo(isOnSingleBranch));
        }
    }
}