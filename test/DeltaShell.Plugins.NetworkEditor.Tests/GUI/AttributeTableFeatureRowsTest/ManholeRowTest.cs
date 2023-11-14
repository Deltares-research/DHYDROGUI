using System.ComponentModel;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Validation.Common;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.AttributeTableFeatureRowsTest
{
    [TestFixture]
    public class ManholeRowTest
    {
        [Test]
        public void Constructor_WithNullManhole_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new ManholeRow(null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsManhole()
        {
            // Arrange
            var manhole = new Manhole();
            var row = new ManholeRow(manhole);

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(manhole));
        }

        [Test]
        public void WhenManholePropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var manhole = new Manhole();
            var row = new ManholeRow(manhole);
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            manhole.Name = "some_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void SetName_SetsManholeName()
        {
            // Arrange
            var manhole = new Manhole();
            var row = new ManholeRow(manhole);

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(manhole.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsManholeName()
        {
            // Arrange
            var manhole = new Manhole { Name = "some_name" };
            var row = new ManholeRow(manhole);

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

            var manhole = new Manhole { Name = "some_name" };
            manhole.AttachNameValidator(validator);
            var row = new ManholeRow(manhole);

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

            var manhole = new Manhole { Name = "some_name" };
            manhole.AttachNameValidator(validator);
            var row = new ManholeRow(manhole);

            // Act
            row.Name = "some_valid_name";

            // Assert
            Assert.That(row.Name, Is.EqualTo("some_valid_name"));
        }

        [Test]
        public void GetCompartmentCount_GetsManholeCompartmentCount()
        {
            // Arrange
            IManhole manhole = Substitute.For<IManhole, INotifyPropertyChanged>();
            manhole.Compartments.Returns(new EventedList<ICompartment>(new ICompartment[3]));
            var row = new ManholeRow(manhole);

            // Act
            int result = row.CompartmentCount;

            // Assert
            Assert.That(result, Is.EqualTo(3));
        }

        [Test]
        public void GetXCoordinate_GetsManholeXCoordinate()
        {
            // Arrange
            var point = Substitute.For<IPoint>();
            point.Coordinate.Returns(new Coordinate(1.23, 4.56));
            var manhole = new Manhole { Geometry = point };
            var row = new ManholeRow(manhole);

            // Act
            double result = row.XCoordinate;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void GetYCoordinate_GetsManholeYCoordinate()
        {
            // Arrange
            var point = Substitute.For<IPoint>();
            point.Coordinate.Returns(new Coordinate(1.23, 4.56));
            var manhole = new Manhole { Geometry = point };
            var row = new ManholeRow(manhole);

            // Act
            double result = row.YCoordinate;

            // Assert
            Assert.That(result, Is.EqualTo(4.56));
        }

        [Test]
        public void GetIsOnSingleBranch_GetsManholeIsOnSingleBranch([Values] bool isOnSingleBranch)
        {
            // 
            IManhole manhole = Substitute.For<IManhole, INotifyPropertyChanged>();
            manhole.IsOnSingleBranch.Returns(isOnSingleBranch);
            var row = new ManholeRow(manhole);

            // Act
            bool result = row.IsOnSingleBranch;

            // Assert
            Assert.That(result, Is.EqualTo(isOnSingleBranch));
        }
    }
}