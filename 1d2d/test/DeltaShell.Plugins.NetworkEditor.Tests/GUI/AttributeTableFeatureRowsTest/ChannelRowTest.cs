using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
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
    public class ChannelRowTest
    {
        [Test]
        public void Constructor_WithNullChannel_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new ChannelRow(null, new NameValidator());
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
                new ChannelRow(Substitute.For<IChannel, INotifyPropertyChanged>(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsChannel()
        {
            // Arrange
            var channel = new Channel();
            var row = new ChannelRow(channel, new NameValidator());

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(channel));
        }

        [Test]
        public void WhenChannelPropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var channel = new Channel();
            var row = new ChannelRow(channel, new NameValidator());
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            channel.Name = "some_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void SetName_SetsChannelName()
        {
            // Arrange
            var channel = new Channel();
            var row = new ChannelRow(channel, new NameValidator());

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(channel.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsChannelName()
        {
            // Arrange
            var channel = new Channel { Name = "some_name" };
            var row = new ChannelRow(channel, new NameValidator());

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

            var channel = new Channel { Name = "some_name" };
            var row = new ChannelRow(channel, nameValidator);

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

            var channel = new Channel { Name = "some_name" };
            var row = new ChannelRow(channel, nameValidator);

            // Act
            row.Name = "some_valid_name";

            // Assert
            Assert.That(row.Name, Is.EqualTo("some_valid_name"));
        }

        [Test]
        public void SetLongName_SetsChannelLongName()
        {
            // Arrange
            var channel = new Channel();
            var row = new ChannelRow(channel, new NameValidator());

            // Act
            row.LongName = "some_long_name";

            // Assert
            Assert.That(channel.LongName, Is.EqualTo("some_long_name"));
        }

        [Test]
        public void GetLongName_GetsChannelLongName()
        {
            // Arrange
            var channel = new Channel { LongName = "some_long_name" };
            var row = new ChannelRow(channel, new NameValidator());

            // Act
            string result = row.LongName;

            // Assert
            Assert.That(result, Is.EqualTo("some_long_name"));
        }

        [Test]
        public void GetSource_GetsChannelSource()
        {
            // Arrange
            var node = Substitute.For<INode>();
            var channel = new Channel { Source = node };
            var row = new ChannelRow(channel, new NameValidator());

            // Act
            INode result = row.Source;

            // Assert
            Assert.That(result, Is.SameAs(node));
        }

        [Test]
        public void GetTarget_GetsChannelTarget()
        {
            // Arrange
            var node = Substitute.For<INode>();
            var channel = new Channel { Target = node };
            var row = new ChannelRow(channel, new NameValidator());

            // Act
            INode result = row.Target;

            // Assert
            Assert.That(result, Is.SameAs(node));
        }

        [Test]
        public void SetIsLengthCustom_SetsChannelIsLengthCustom([Values] bool isLengthCustom)
        {
            // Arrange
            IChannel channel = Substitute.For<IChannel, INotifyPropertyChanged>();
            var row = new ChannelRow(channel, new NameValidator());

            // Act
            row.IsLengthCustom = isLengthCustom;

            // Assert
            Assert.That(channel.IsLengthCustom, Is.EqualTo(isLengthCustom));
        }

        [Test]
        public void GetIsLengthCustom_GetsChannelIsLengthCustom([Values] bool isLengthCustom)
        {
            // Arrange
            IChannel channel = Substitute.For<IChannel, INotifyPropertyChanged>();
            channel.IsLengthCustom = isLengthCustom;
            var row = new ChannelRow(channel, new NameValidator());

            // Act
            bool result = row.IsLengthCustom;

            // Assert
            Assert.That(result, Is.EqualTo(isLengthCustom));
        }

        [Test]
        public void SetLength_SetsChannelLength()
        {
            // Arrange
            var channel = new Channel();
            var row = new ChannelRow(channel, new NameValidator());

            // Act
            row.Length = 1.23;

            // Assert
            Assert.That(channel.Length, Is.EqualTo(1.23));
        }

        [Test]
        public void GetLength_GetsChannelLength()
        {
            // Arrange
            var channel = new Channel { Length = 1.23 };
            var row = new ChannelRow(channel, new NameValidator());

            // Act
            double result = row.Length;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void GetGeometryLength_GetsChannelGeometryLength()
        {
            // Arrange
            var geometry = Substitute.For<IGeometry>();
            geometry.Length.Returns(1.23);
            var channel = new Channel { Geometry = geometry };
            var row = new ChannelRow(channel, new NameValidator());

            // Act
            double result = row.GeometryLength;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void SetOrderNumber_SetsChannelOrderNumber()
        {
            // Arrange
            var channel = new Channel();
            var row = new ChannelRow(channel, new NameValidator());

            // Act
            row.OrderNumber = 123;

            // Assert
            Assert.That(channel.OrderNumber, Is.EqualTo(123));
        }

        [Test]
        public void GetOrderNumber_GetsChannelOrderNumber()
        {
            // Arrange
            var channel = new Channel { OrderNumber = 123 };
            var row = new ChannelRow(channel, new NameValidator());

            // Act
            double result = row.OrderNumber;

            // Assert
            Assert.That(result, Is.EqualTo(123));
        }

        [Test]
        public void GetCrossSectionCount_GetsNumberOfChannelCrossSections()
        {
            // Arrange
            IChannel channel = Substitute.For<IChannel, INotifyPropertyChanged>();
            channel.CrossSections.Returns(new ICrossSection[3]);
            var row = new ChannelRow(channel, new NameValidator());

            // Act
            int result = row.CrossSectionCount;

            // Assert
            Assert.That(result, Is.EqualTo(3));
        }

        [Test]
        public void GetStructureCount_GetsNumberOfChannelStructures()
        {
            // Arrange
            IChannel channel = Substitute.For<IChannel, INotifyPropertyChanged>();
            channel.Structures.Returns(new IStructure1D[3]);
            var row = new ChannelRow(channel, new NameValidator());

            // Act
            int result = row.StructureCount;

            // Assert
            Assert.That(result, Is.EqualTo(3));
        }

        [Test]
        public void GetPumpCount_GetsNumberOfChannelPumps()
        {
            // Arrange
            IChannel channel = Substitute.For<IChannel, INotifyPropertyChanged>();
            channel.Pumps.Returns(new IPump[3]);
            var row = new ChannelRow(channel, new NameValidator());

            // Act
            int result = row.PumpCount;

            // Assert
            Assert.That(result, Is.EqualTo(3));
        }

        [Test]
        public void GetCulvertCount_GetsNumberOfChannelCulverts()
        {
            // Arrange
            IChannel channel = Substitute.For<IChannel, INotifyPropertyChanged>();
            channel.Culverts.Returns(new ICulvert[3]);
            var row = new ChannelRow(channel, new NameValidator());

            // Act
            int result = row.CulvertCount;

            // Assert
            Assert.That(result, Is.EqualTo(3));
        }

        [Test]
        public void GetBridgeCount_GetsNumberOfChannelBridges()
        {
            // Arrange
            IChannel channel = Substitute.For<IChannel, INotifyPropertyChanged>();
            channel.Bridges.Returns(new IBridge[3]);
            var row = new ChannelRow(channel, new NameValidator());

            // Act
            int result = row.BridgeCount;

            // Assert
            Assert.That(result, Is.EqualTo(3));
        }

        [Test]
        public void GetWeirCount_GetsNumberOfChannelWeirs()
        {
            // Arrange
            IChannel channel = Substitute.For<IChannel, INotifyPropertyChanged>();
            channel.Weirs.Returns(new IWeir[3]);
            var row = new ChannelRow(channel, new NameValidator());

            // Act
            int result = row.WeirCount;

            // Assert
            Assert.That(result, Is.EqualTo(3));
        }

        [Test]
        public void GetGateCount_GetsNumberOfChannelGates()
        {
            // Arrange
            IChannel channel = Substitute.For<IChannel, INotifyPropertyChanged>();
            channel.Gates.Returns(new IGate[3]);
            var row = new ChannelRow(channel, new NameValidator());

            // Act
            int result = row.GateCount;

            // Assert
            Assert.That(result, Is.EqualTo(3));
        }

        [Test]
        public void GetLateralSourcesCount_GetsNumberOfChannelBranchSources()
        {
            // Arrange
            IChannel channel = Substitute.For<IChannel, INotifyPropertyChanged>();
            channel.BranchSources.Returns(new LateralSource[3]);
            var row = new ChannelRow(channel, new NameValidator());

            // Act
            int result = row.LateralSourcesCount;

            // Assert
            Assert.That(result, Is.EqualTo(3));
        }
    }
}