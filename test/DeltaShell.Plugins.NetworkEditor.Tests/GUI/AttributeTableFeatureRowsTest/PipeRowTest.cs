using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Validation.Common;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.AttributeTableFeatureRowsTest
{
    [TestFixture]
    public class PipeRowTest
    {
        [Test]
        public void Constructor_WithNullPipe_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new PipeRow(null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsPipe()
        {
            // Arrange
            var pipe = new Pipe();
            var row = new PipeRow(pipe);

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(pipe));
        }

        [Test]
        public void WhenPipePropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var pipe = new Pipe();
            var row = new PipeRow(pipe);
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            pipe.Name = "some_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void SetName_SetsPipeName()
        {
            // Arrange
            var pipe = new Pipe();
            var row = new PipeRow(pipe);

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(pipe.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsPipeName()
        {
            // Arrange
            var pipe = new Pipe { Name = "some_name" };
            var row = new PipeRow(pipe);

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

            var pipe = new Pipe { Name = "some_name" };
            pipe.AttachNameValidator(validator);
            var row = new PipeRow(pipe);

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

            var pipe = new Pipe { Name = "some_name" };
            pipe.AttachNameValidator(validator);
            var row = new PipeRow(pipe);

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
            var pipe = Substitute.For<IPipe>();
            pipe.Source.Returns(node);
            var row = new PipeRow(pipe);

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
            var pipe = Substitute.For<IPipe>();
            pipe.Target.Returns(node);
            var row = new PipeRow(pipe);

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
            var pipe = Substitute.For<IPipe>();
            pipe.SourceCompartment.Returns(compartment);
            var row = new PipeRow(pipe);

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
            var pipe = Substitute.For<IPipe>();
            pipe.TargetCompartment.Returns(compartment);
            var row = new PipeRow(pipe);

            // Act
            string result = row.ToCompartment;

            // Assert
            Assert.That(result, Is.EqualTo("some_compartment"));
        }

        [Test]
        public void GetGeometryLength_GetsPipeGeometryLength()
        {
            // Arrange
            var geometry = Substitute.For<IGeometry>();
            geometry.Length.Returns(150.7);
            var pipe = new Pipe { Geometry = geometry };
            var row = new PipeRow(pipe);

            // Act
            double result = row.GeometryLength;

            // Assert
            Assert.That(result, Is.EqualTo(150.7));
        }

        [Test]
        public void SetLength_SetsPipeLength()
        {
            // Arrange
            var pipe = new Pipe();
            var row = new PipeRow(pipe);

            // Act
            row.Length = 15.0;

            // Assert
            Assert.That(pipe.Length, Is.EqualTo(15.0));
        }

        [Test]
        public void GetLength_GetsPipeLength()
        {
            // Arrange
            var pipe = new Pipe { Length = 12.5 };
            var row = new PipeRow(pipe);

            // Act
            double result = row.Length;

            // Assert
            Assert.That(result, Is.EqualTo(12.5));
        }

        [Test]
        public void GetOrderNumber_GetsPipeOrderNumber()
        {
            // Arrange
            var pipe = new Pipe { OrderNumber = 42 };
            var row = new PipeRow(pipe);

            // Act
            int result = row.OrderNumber;

            // Assert
            Assert.That(result, Is.EqualTo(42));
        }

        [Test]
        public void SetOrderNumber_SetsPipeOrderNumber()
        {
            // Arrange
            var pipe = new Pipe();
            var row = new PipeRow(pipe);

            // Act
            row.OrderNumber = 10;

            // Assert
            Assert.That(pipe.OrderNumber, Is.EqualTo(10));
        }

        [Test]
        public void GetInvertLevelFrom_GetsPipeInvertLevelFrom()
        {
            // Arrange
            var pipe = new Pipe { LevelSource = 5.3 };
            var row = new PipeRow(pipe);

            // Act
            double result = row.InvertLevelFrom;

            // Assert
            Assert.That(result, Is.EqualTo(5.3));
        }

        [Test]
        public void SetInvertLevelFrom_SetsPipeInvertLevelFrom()
        {
            // Arrange
            var pipe = new Pipe();
            var row = new PipeRow(pipe);

            // Act
            row.InvertLevelFrom = 7.1;

            // Assert
            Assert.That(pipe.LevelSource, Is.EqualTo(7.1));
        }

        [Test]
        public void GetInvertLevelTo_GetsPipeInvertLevelTo()
        {
            // Arrange
            var pipe = new Pipe { LevelTarget = 8.7 };
            var row = new PipeRow(pipe);

            // Act
            double result = row.InvertLevelTo;

            // Assert
            Assert.That(result, Is.EqualTo(8.7));
        }

        [Test]
        public void SetInvertLevelTo_SetsPipeInvertLevelTo()
        {
            // Arrange
            var pipe = new Pipe();
            var row = new PipeRow(pipe);

            // Act
            row.InvertLevelTo = 9.2;

            // Assert
            Assert.That(pipe.LevelTarget, Is.EqualTo(9.2));
        }

        [Test]
        public void GetSewerType_GetsPipeSewerType([Values] SewerConnectionWaterType sewerConnectionWaterType)
        {
            // Arrange
            var pipe = new Pipe { WaterType = sewerConnectionWaterType };
            var row = new PipeRow(pipe);

            // Act
            SewerConnectionWaterType result = row.SewerType;

            // Assert
            Assert.That(result, Is.EqualTo(sewerConnectionWaterType));
        }

        [Test]
        public void SetSewerType_SetsPipeSewerType([Values] SewerConnectionWaterType sewerConnectionWaterType)
        {
            // Arrange
            var pipe = new Pipe();
            var row = new PipeRow(pipe);

            // Act
            row.SewerType = sewerConnectionWaterType;

            // Assert
            Assert.That(pipe.WaterType, Is.EqualTo(sewerConnectionWaterType));
        }

        [Test]
        public void GetSewerSpecialConnectionType_GetsPipeSewerSpecialConnectionType([Values] SewerConnectionSpecialConnectionType sewerConnectionSpecialConnectionType)
        {
            // Arrange
            var pipe = Substitute.For<IPipe>();
            pipe.SpecialConnectionType.Returns(sewerConnectionSpecialConnectionType);
            var row = new PipeRow(pipe);

            // Act
            SewerConnectionSpecialConnectionType result = row.SewerSpecialConnectionType;

            // Assert
            Assert.That(result, Is.EqualTo(sewerConnectionSpecialConnectionType));
        }

        [Test]
        public void GetProfileType_GetsPipeProfileShapeType([Values] CrossSectionStandardShapeType crossSectionStandardShapeType)
        {
            // Arrange
            var profile = Substitute.For<CrossSectionDefinitionStandard>();
            profile.ShapeType = crossSectionStandardShapeType;
            var pipe = Substitute.For<IPipe>();
            pipe.Profile.Returns(profile);
            var row = new PipeRow(pipe);

            // Act
            CrossSectionStandardShapeType? result = row.ProfileType;

            // Assert
            Assert.That(result, Is.EqualTo(crossSectionStandardShapeType));
        }

        [Test]
        public void GetWidth_GetsPipeCrossSectionDefinitionWidth()
        {
            // Arrange
            var crossSectionDefinition = Substitute.For<ICrossSectionDefinition>();
            crossSectionDefinition.Width.Returns(2.5);
            var crossSection = Substitute.For<ICrossSection>();
            crossSection.Definition.Returns(crossSectionDefinition);

            var pipe = new Pipe { CrossSection = crossSection };
            var row = new PipeRow(pipe);

            // Act
            double result = row.Width;

            // Assert
            Assert.That(result, Is.EqualTo(2.5));
        }

        [Test]
        public void SetDefinitionName_SetsPipeDefinitionName()
        {
            // Arrange
            var pipe = Substitute.For<IPipe>();
            var row = new PipeRow(pipe);

            // Act
            row.DefinitionName = "some_definition";

            // Assert
            Assert.That(pipe.DefinitionName, Is.EqualTo("some_definition"));
        }

        [Test]
        public void GetDefinitionName_GetsPipeDefinitionName()
        {
            // Arrange
            var pipe = Substitute.For<IPipe>();
            pipe.DefinitionName.Returns("some_definition");
            var row = new PipeRow(pipe);

            // Act
            string result = row.DefinitionName;

            // Assert
            Assert.That(result, Is.EqualTo("some_definition"));
        }
    }
}