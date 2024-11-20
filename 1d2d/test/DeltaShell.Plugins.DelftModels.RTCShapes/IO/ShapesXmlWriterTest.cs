using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RTCShapes.IO;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RTCShapes.Tests.IO
{
    [TestFixture]
    public class ShapesXmlWriterTest
    {
        private const string targetDirectory = "test_dir";

        private IShapeAccessor shapeAccessor;
        private MockFileSystem fileSystem;

        [SetUp]
        public void SetUp()
        {
            shapeAccessor = Substitute.For<IShapeAccessor>();
            fileSystem = new MockFileSystem();
        }

        [Test]
        public void Constructor_ShapeAccessorIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => new ShapesXmlWriter(null, fileSystem), Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_FileSystemIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => new ShapesXmlWriter(shapeAccessor, null), Throws.ArgumentNullException);
        }

        [Test]
        public void WriteToXml_ModelIsNull_ThrowsArgumentNullException()
        {
            ShapesXmlWriter writer = CreateWriter();

            Assert.That(() => writer.WriteToXml(null, targetDirectory), Throws.ArgumentNullException);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void WriteToXml_DirectoryIsNullOrEmpty_ThrowsArgumentException(string directory)
        {
            RealTimeControlModel model = CreateModel();
            ShapesXmlWriter writer = CreateWriter();

            Assert.That(() => writer.WriteToXml(model, directory), Throws.ArgumentException);
        }

        [Test]
        public void WriteToXml_DirectoryDoesNotExist_ThrowsDirectoryNotFoundException()
        {
            RealTimeControlModel model = CreateModel();
            ShapesXmlWriter writer = CreateWriter();

            Assert.That(() => writer.WriteToXml(model, targetDirectory), Throws.InstanceOf<DirectoryNotFoundException>());
        }

        [Test]
        public void WriteToXml_SourceXsdAndTargetXsdDoesNotExist_ThrowsIOException()
        {
            RealTimeControlModel model = CreateModel();
            ShapesXmlWriter writer = CreateWriter();

            CreateDirectory(targetDirectory);
            
            Assert.That(() => writer.WriteToXml(model, targetDirectory), Throws.InstanceOf<IOException>());
        }
        
        [Test]
        public void WriteToXml_SourceXsdExistsAndTargetXsdDoesNotExist_CopiesShapesConfigXsd()
        {
            RealTimeControlModel model = CreateModel();
            ShapesXmlWriter writer = CreateWriter();

            CreateDirectory(targetDirectory);
            CreateDefaultShapesConfigXsd();

            writer.WriteToXml(model, targetDirectory);

            MockFileData shapesConfigXsd = GetShapesConfigXsd(targetDirectory);
            Assert.That(shapesConfigXsd, Is.Not.Null);
        }
        
        [Test]
        public void WriteToXml_SourceXsdAndTargetXsdExists_CopiesShapesConfigXsd()
        {
            RealTimeControlModel model = CreateModel();
            ShapesXmlWriter writer = CreateWriter();

            CreateDirectory(targetDirectory);
            CreateShapesConfigXsd(targetDirectory);
            CreateDefaultShapesConfigXsd();

            writer.WriteToXml(model, targetDirectory);

            MockFileData shapesConfigXsd = GetShapesConfigXsd(targetDirectory);
            Assert.That(shapesConfigXsd, Is.Not.Null);
        }

        [Test]
        public void WriteToXml_EmptyControlGroups_DoesNotWriteRtcShapesConfigXml()
        {
            RealTimeControlModel model = CreateModel();
            ShapesXmlWriter writer = CreateWriter();

            CreateDirectory(targetDirectory);
            CreateDefaultShapesConfigXsd();

            writer.WriteToXml(model, targetDirectory);

            MockFileData shapesConfigXml = GetShapesConfigXml(targetDirectory);

            Assert.That(shapesConfigXml, Is.Null);
        }

        [Test]
        public void WriteToXml_ControlGroupsWithoutShapes_DoesNotWriteRtcShapesConfigXml()
        {
            RealTimeControlModel model = CreateModelWithControlGroups();
            ShapesXmlWriter writer = CreateWriter();

            CreateDirectory(targetDirectory);
            CreateDefaultShapesConfigXsd();

            writer.WriteToXml(model, targetDirectory);

            MockFileData shapesConfigXml = GetShapesConfigXml(targetDirectory);

            Assert.That(shapesConfigXml, Is.Null);
        }

        [Test]
        public void WriteToXml_ControlGroupsWithAndWithoutShapes_WritesRtcShapesConfigXml()
        {
            RealTimeControlModel model = CreateModelWithControlGroups();
            ShapesXmlWriter writer = CreateWriter();

            SetupControlGroupShapes(model.ControlGroups[0], CreateShapes());

            CreateDirectory(targetDirectory);
            CreateDefaultShapesConfigXsd();

            writer.WriteToXml(model, targetDirectory);

            MockFileData shapesConfigXml = GetShapesConfigXml(targetDirectory);

            const string expectedXml = @"<?xml version=""1.0""?>
<rtcShapesConfig xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.wldelft.nl/fews"">
  <groups>
    <group>
      <groupId>CG0</groupId>
      <shapes>
        <shape>
          <type>input</type>
          <x>0</x>
          <y>0</y>
          <width>100</width>
          <height>20</height>
          <title>TestShape1</title>
        </shape>
        <shape>
          <type>output</type>
          <x>40</x>
          <y>40</y>
          <width>100</width>
          <height>20</height>
          <title>TestShape2</title>
        </shape>
        <shape>
          <type>rule</type>
          <x>300</x>
          <y>100</y>
          <width>50</width>
          <height>50</height>
          <title>TestShape3</title>
        </shape>
        <shape>
          <type>signal</type>
          <x>150</x>
          <y>100</y>
          <width>50</width>
          <height>30</height>
          <title>TestShape4</title>
        </shape>
        <shape>
          <type>condition</type>
          <x>200</x>
          <y>300</y>
          <width>20</width>
          <height>20</height>
          <title>TestShape5</title>
        </shape>
        <shape>
          <type>expression</type>
          <x>400</x>
          <y>100</y>
          <width>120</width>
          <height>40</height>
          <title>TestShape6</title>
        </shape>
      </shapes>
    </group>
  </groups>
</rtcShapesConfig>";

            Assert.That(shapesConfigXml, Is.Not.Null);
            Assert.That(shapesConfigXml.TextContents, Is.EqualTo(expectedXml));
        }

        private ShapesXmlWriter CreateWriter()
        {
            return new ShapesXmlWriter(shapeAccessor, fileSystem);
        }

        private RealTimeControlModel CreateModel()
        {
            return new RealTimeControlModel();
        }

        private RealTimeControlModel CreateModelWithControlGroups(int count = 3)
        {
            RealTimeControlModel model = CreateModel();
            model.ControlGroups.AddRange(Enumerable.Range(0, count).Select(i => new ControlGroup { Name = $"CG{i}" }));
            return model;
        }

        private void SetupControlGroupShapes(ControlGroup controlGroup, IEnumerable<ShapeBase> shapes)
        {
            shapeAccessor.GetShapes(controlGroup).Returns(shapes);
        }

        private IEnumerable<ShapeBase> CreateShapes()
        {
            yield return new InputItemShape
            {
                X = 0,
                Y = 0,
                Width = 100,
                Height = 20,
                Title = "TestShape1"
            };

            yield return new OutputItemShape
            {
                X = 40,
                Y = 40,
                Width = 100,
                Height = 20,
                Title = "TestShape2"
            };

            yield return new RuleShape
            {
                X = 300,
                Y = 100,
                Width = 50,
                Height = 50,
                Title = "TestShape3"
            };

            yield return new SignalShape
            {
                X = 150,
                Y = 100,
                Width = 50,
                Height = 30,
                Title = "TestShape4"
            };

            yield return new ConditionShape
            {
                X = 200,
                Y = 300,
                Width = 20,
                Height = 20,
                Title = "TestShape5"
            };

            yield return new MathematicalExpressionShape
            {
                X = 400,
                Y = 100,
                Width = 120,
                Height = 40,
                Title = "TestShape6"
            };
        }

        private void CreateDirectory(string directory)
        {
            fileSystem.AddDirectory(directory);
        }

        private void CreateDefaultShapesConfigXsd()
        {
            fileSystem.AddEmptyFile(ShapesFilePaths.GetDefaultShapesConfigXsdPath());
        }
        
        private void CreateShapesConfigXsd(string directory)
        {
            fileSystem.AddEmptyFile(ShapesFilePaths.GetShapesConfigXsdPath(directory));
        }

        private MockFileData GetShapesConfigXsd(string directory)
        {
            return fileSystem.GetFile(ShapesFilePaths.GetShapesConfigXsdPath(directory));
        }

        private MockFileData GetShapesConfigXml(string directory)
        {
            return fileSystem.GetFile(ShapesFilePaths.GetShapesConfigXmlPath(directory));
        }
    }
}