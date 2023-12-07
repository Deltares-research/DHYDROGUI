using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Validation.Common;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.AttributeTableFeatureRowsTest
{
    [TestFixture]
    public class SewerConnectionRowTest
    {
        [Test]
        public void Constructor_WithNullSewerConnection_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new SewerConnectionRow(null, new NameValidator());
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
                new SewerConnectionRow(Substitute.For<ISewerConnection>(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsSewerConnection()
        {
            // Arrange
            var sewerConnection = new SewerConnection();
            var row = new SewerConnectionRow(sewerConnection, new NameValidator());

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(sewerConnection));
        }

        [Test]
        public void SetName_SetsSewerConnectionName()
        {
            // Arrange
            var sewerConnection = new SewerConnection();
            var row = new SewerConnectionRow(sewerConnection, new NameValidator());

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(sewerConnection.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsSewerConnectionName()
        {
            // Arrange
            var sewerConnection = new SewerConnection { Name = "some_name" };
            var row = new SewerConnectionRow(sewerConnection, new NameValidator());

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

            var sewerConnection = new SewerConnection { Name = "some_name" };
            var row = new SewerConnectionRow(sewerConnection, nameValidator);

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

            var sewerConnection = new SewerConnection { Name = "some_name" };
            var row = new SewerConnectionRow(sewerConnection, nameValidator);

            // Act
            row.Name = "some_valid_name";

            // Assert
            Assert.That(row.Name, Is.EqualTo("some_valid_name"));
        }

        [Test]
        public void GetFromManhole_GetsNameOfSourceNode()
        {
            // Arrange
            var node = Substitute.For<INode>();
            node.Name = "some_node";
            var sewerConnection = Substitute.For<ISewerConnection>();
            sewerConnection.Source.Returns(node);
            var row = new SewerConnectionRow(sewerConnection, new NameValidator());

            // Act
            string result = row.FromManhole;

            // Assert
            Assert.That(result, Is.EqualTo("some_node"));
        }

        [Test]
        public void GetToManhole_SetsNameOfSourceNode()
        {
            // Arrange
            var node = Substitute.For<INode>();
            node.Name = "some_node";
            var sewerConnection = Substitute.For<ISewerConnection>();
            sewerConnection.Target.Returns(node);
            var row = new SewerConnectionRow(sewerConnection, new NameValidator());

            // Act
            string result = row.ToManhole;

            // Assert
            Assert.That(result, Is.EqualTo("some_node"));
        }

        [Test]
        public void GetFromCompartment_GetsNameOfSourceCompartment()
        {
            // Arrange
            var compartment = new Compartment { Name = "some_compartment" };
            var sewerConnection = Substitute.For<ISewerConnection>();
            sewerConnection.SourceCompartment.Returns(compartment);
            var row = new SewerConnectionRow(sewerConnection, new NameValidator());

            // Act
            string result = row.FromCompartment;

            // Assert
            Assert.That(result, Is.EqualTo("some_compartment"));
        }

        [Test]
        public void GetToCompartment_GetsNameOfTargetCompartment()
        {
            // Arrange
            var compartment = new Compartment { Name = "some_compartment" };
            var sewerConnection = Substitute.For<ISewerConnection>();
            sewerConnection.TargetCompartment.Returns(compartment);
            var row = new SewerConnectionRow(sewerConnection, new NameValidator());

            // Act
            string result = row.ToCompartment;

            // Assert
            Assert.That(result, Is.EqualTo("some_compartment"));
        }

        [Test]
        public void GetGeometryLength_GetsSewerConnectionGeometryLength()
        {
            // Arrange
            var geometry = Substitute.For<IGeometry>();
            geometry.Length.Returns(123.45);
            var sewerConnection = new SewerConnection { Geometry = geometry };
            var row = new SewerConnectionRow(sewerConnection, new NameValidator());

            // Act
            double result = row.GeometryLength;

            // Assert
            Assert.That(result, Is.EqualTo(123.45));
        }

        [Test]
        public void SetLength_SetsSewerConnectionLength()
        {
            // Arrange
            var sewerConnection = new SewerConnection();
            var row = new SewerConnectionRow(sewerConnection, new NameValidator());

            // Act
            row.Length = 45.67;

            // Assert
            Assert.That(sewerConnection.Length, Is.EqualTo(45.67));
        }

        [Test]
        public void GetLength_GetsSewerConnectionLength()
        {
            // Arrange
            var sewerConnection = new SewerConnection { Length = 45.67 };
            var row = new SewerConnectionRow(sewerConnection, new NameValidator());

            // Act
            double result = row.Length;

            // Assert
            Assert.That(result, Is.EqualTo(45.67));
        }

        [Test]
        public void SetOrderNumber_SetsSewerConnectionOrderNumber()
        {
            // Arrange
            var sewerConnection = new SewerConnection();
            var row = new SewerConnectionRow(sewerConnection, new NameValidator());

            // Act
            row.OrderNumber = 3;

            // Assert
            Assert.That(sewerConnection.OrderNumber, Is.EqualTo(3));
        }

        [Test]
        public void GetOrderNumber_GetsSewerConnectionOrderNumber()
        {
            // Arrange
            var sewerConnection = new SewerConnection { OrderNumber = 3 };
            var row = new SewerConnectionRow(sewerConnection, new NameValidator());

            // Act
            int result = row.OrderNumber;

            // Assert
            Assert.That(result, Is.EqualTo(3));
        }

        [Test]
        public void SetInvertLevelFrom_SetsSewerConnectionInvertLevelFrom()
        {
            // Arrange
            var sewerConnection = new SewerConnection();
            var row = new SewerConnectionRow(sewerConnection, new NameValidator());

            // Act
            row.InvertLevelFrom = 10.5;

            // Assert
            Assert.That(sewerConnection.LevelSource, Is.EqualTo(10.5));
        }

        [Test]
        public void GetInvertLevelFrom_GetsSewerConnectionInvertLevelFrom()
        {
            // Arrange
            var sewerConnection = new SewerConnection { LevelSource = 10.5 };
            var row = new SewerConnectionRow(sewerConnection, new NameValidator());

            // Act
            double result = row.InvertLevelFrom;

            // Assert
            Assert.That(result, Is.EqualTo(10.5));
        }

        [Test]
        public void SetInvertLevelTo_SetsSewerConnectionInvertLevelTo()
        {
            // Arrange
            var sewerConnection = new SewerConnection();
            var row = new SewerConnectionRow(sewerConnection, new NameValidator());

            // Act
            row.InvertLevelTo = 8.9;

            // Assert
            Assert.That(sewerConnection.LevelTarget, Is.EqualTo(8.9));
        }

        [Test]
        public void GetInvertLevelTo_GetsSewerConnectionInvertLevelTo()
        {
            // Arrange
            var sewerConnection = new SewerConnection { LevelTarget = 8.9 };
            var row = new SewerConnectionRow(sewerConnection, new NameValidator());

            // Act
            double result = row.InvertLevelTo;

            // Assert
            Assert.That(result, Is.EqualTo(8.9));
        }

        [Test]
        public void SetSewerType_SetsSewerConnectionSewerType([Values] SewerConnectionWaterType sewerConnectionWaterType)
        {
            // Arrange
            var sewerConnection = new SewerConnection();
            var row = new SewerConnectionRow(sewerConnection, new NameValidator());

            // Act
            row.SewerType = sewerConnectionWaterType;

            // Assert
            Assert.That(sewerConnection.WaterType, Is.EqualTo(sewerConnectionWaterType));
        }

        [Test]
        public void GetSewerType_GetsSewerConnectionSewerType([Values] SewerConnectionWaterType sewerConnectionWaterType)
        {
            // Arrange
            var sewerConnection = new SewerConnection { WaterType = sewerConnectionWaterType };
            var row = new SewerConnectionRow(sewerConnection, new NameValidator());

            // Act
            SewerConnectionWaterType result = row.SewerType;

            // Assert
            Assert.That(result, Is.EqualTo(sewerConnectionWaterType));
        }

        [Test]
        public void GetSewerSpecialConnectionType_GetsSewerConnectionSpecialConnectionType([Values] SewerConnectionSpecialConnectionType sewerConnectionSpecialConnectionType)
        {
            // Arrange
            var sewerConnection = Substitute.For<ISewerConnection>();
            sewerConnection.SpecialConnectionType.Returns(sewerConnectionSpecialConnectionType);
            var row = new SewerConnectionRow(sewerConnection, new NameValidator());

            // Act
            SewerConnectionSpecialConnectionType result = row.SewerSpecialConnectionType;

            // Assert
            Assert.That(result, Is.EqualTo(sewerConnectionSpecialConnectionType));
        }

        [Test]
        public void GetDefinitionName_GetsPipeDefinitionName()
        {
            // Arrange
            var sewerConnection = Substitute.For<ISewerConnection>();
            sewerConnection.DefinitionName.Returns("some_definition");
            var row = new SewerConnectionRow(sewerConnection, new NameValidator());

            // Act
            string result = row.DefinitionName;

            // Assert
            Assert.That(result, Is.EqualTo("some_definition"));
        }
    }
}