using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Xml;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RTCShapes.IO;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RTCShapes.Tests.IO
{
    [TestFixture]
    public class ShapesXmlReaderTest
    {
        private const string sourceDirectory = "test_dir";

        private IShapeSetter shapeSetter;
        private MockFileSystem fileSystem;

        [SetUp]
        public void SetUp()
        {
            shapeSetter = Substitute.For<IShapeSetter>();
            fileSystem = new MockFileSystem();
        }

        [Test]
        public void Constructor_ShapeAccessorIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => new ShapesXmlReader(null, fileSystem), Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_FileSystemIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => new ShapesXmlReader(shapeSetter, null), Throws.ArgumentNullException);
        }

        [Test]
        public void ReadFromXml_ModelIsNull_ThrowsArgumentNullException()
        {
            ShapesXmlReader reader = CreateReader();

            Assert.That(() => reader.ReadFromXml(null, sourceDirectory), Throws.ArgumentNullException);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void ReadFromXml_DirectoryIsNullOrEmpty_ThrowsArgumentException(string directory)
        {
            RealTimeControlModel model = CreateModel();
            ShapesXmlReader reader = CreateReader();

            Assert.That(() => reader.ReadFromXml(model, directory), Throws.ArgumentException);
        }

        [Test]
        public void ReadFromXml_DirectoryDoesNotExist_ThrowsDirectoryNotFoundException()
        {
            RealTimeControlModel model = CreateModel();
            ShapesXmlReader reader = CreateReader();

            Assert.That(() => reader.ReadFromXml(model, sourceDirectory), Throws.InstanceOf<DirectoryNotFoundException>());
        }

        [Test]
        [TestCaseSource(nameof(GetInvalidShapesConfigXmlTestCases))]
        public void ReadFromXml_InvalidShapesConfigXml_ThrowsXmlException(string xml)
        {
            RealTimeControlModel model = CreateModelWithControlGroups();
            ShapesXmlReader reader = CreateReader();

            CreateShapesConfigXml(sourceDirectory, xml);

            Assert.That(() => reader.ReadFromXml(model, sourceDirectory), Throws.InstanceOf<XmlException>());
        }

        private static IEnumerable<TestCaseData> GetInvalidShapesConfigXmlTestCases()
        {
            const string noShapesConfigElement = @"<?xml version=""1.0""?>";

            const string noGroupsElement = @"<?xml version=""1.0""?>
<rtcShapesConfig xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.wldelft.nl/fews"" />";

            const string emptyGroupsElement = @"<?xml version=""1.0""?>
<rtcShapesConfig xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.wldelft.nl/fews"">
  <groups/>
</rtcShapesConfig>";

            const string noShapesElement = @"<?xml version=""1.0""?>
<rtcShapesConfig xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.wldelft.nl/fews"">
  <groups>
    <group>
      <groupId>CG0</groupId>
    </group>
  </groups>
</rtcShapesConfig>";

            const string emptyShapesElement = @"<?xml version=""1.0""?>
<rtcShapesConfig xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.wldelft.nl/fews"">
  <groups>
    <group>
      <groupId>CG0</groupId>
      <shapes/>
    </group>
  </groups>
</rtcShapesConfig>";

            const string emptyShapeElement = @"<?xml version=""1.0""?>
<rtcShapesConfig xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.wldelft.nl/fews"">
  <groups>
    <group>
      <groupId>CG0</groupId>
      <shapes>
        <shape/>
      </shapes>
    </group>
  </groups>
</rtcShapesConfig>";

            const string emptyShapePropertyElements = @"<?xml version=""1.0""?>
<rtcShapesConfig xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.wldelft.nl/fews"">
  <groups>
    <group>
      <groupId>CG0</groupId>
      <shapes>
        <shape>
          <type/>
          <x/>
          <y/>
          <width/>
          <height/>
          <title/>
        </shape>
      </shapes>
    </group>
  </groups>
</rtcShapesConfig>";

            yield return new TestCaseData(string.Empty).SetName("Empty shapes config");
            yield return new TestCaseData(noShapesConfigElement).SetName("No shapes config element");
            yield return new TestCaseData(noGroupsElement).SetName("No groups element");
            yield return new TestCaseData(emptyGroupsElement).SetName("Empty groups element");
            yield return new TestCaseData(noShapesElement).SetName("No shapes element");
            yield return new TestCaseData(emptyShapesElement).SetName("Empty shapes element");
            yield return new TestCaseData(emptyShapeElement).SetName("Empty shape element");
            yield return new TestCaseData(emptyShapePropertyElements).SetName("Empty shape property elements");
        }

        [Test]
        public void ReadFromXml_ValidShapesConfigXml_AssignControlGroupShapes()
        {
            RealTimeControlModel model = CreateModelWithControlGroups();
            ShapesXmlReader reader = CreateReader();

            CreateShapesConfigXml(sourceDirectory, GetValidShapesConfigXml());

            reader.ReadFromXml(model, sourceDirectory);

            ControlGroup expectedControlGroup = model.ControlGroups.First();
            IEnumerable<ShapeBase> expectedShapes = GetExpectedShapes();

            shapeSetter.Received(1).SetShapes(
                Arg.Is<ControlGroup>(controlGroup => controlGroup == expectedControlGroup),
                Arg.Is<IEnumerable<ShapeBase>>(shapes => shapes.SequenceEqual(expectedShapes, new ShapeGeometryComparer())));
        }

        [Test]
        public void ReadFromXml_ValidShapesConfigXmlAndNoMatchingControlGroup_DoesNotAssignControlGroupShapes()
        {
            RealTimeControlModel model = CreateModelWithControlGroups(1);
            ShapesXmlReader reader = CreateReader();

            ControlGroup controlGroup = model.ControlGroups.First();
            controlGroup.Name = "ControlGroup";

            CreateShapesConfigXml(sourceDirectory, GetValidShapesConfigXml());

            reader.ReadFromXml(model, sourceDirectory);

            shapeSetter.DidNotReceive().SetShapes(
                Arg.Any<ControlGroup>(),
                Arg.Any<IEnumerable<ShapeBase>>());
        }

        private ShapesXmlReader CreateReader()
        {
            return new ShapesXmlReader(shapeSetter, fileSystem);
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

        private void CreateShapesConfigXml(string directory, string xml)
        {
            fileSystem.AddFile(ShapesFilePaths.GetShapesConfigXmlPath(directory), new MockFileData(xml));
        }
        
        private static string GetValidShapesConfigXml()
        {
            return @"<?xml version=""1.0""?>
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
          <title>S1</title>
        </shape>
        <shape>
          <type>output</type>
          <x>40</x>
          <y>40</y>
          <width>100</width>
          <height>20</height>
          <title>S2</title>
        </shape>
        <shape>
          <type>rule</type>
          <x>300</x>
          <y>100</y>
          <width>50</width>
          <height>50</height>
          <title>S3</title>
        </shape>
        <shape>
          <type>signal</type>
          <x>150</x>
          <y>100</y>
          <width>50</width>
          <height>30</height>
          <title>S4</title>
        </shape>
        <shape>
          <type>condition</type>
          <x>200</x>
          <y>300</y>
          <width>20</width>
          <height>20</height>
          <title>S5</title>
        </shape>
        <shape>
          <type>expression</type>
          <x>400</x>
          <y>100</y>
          <width>120</width>
          <height>40</height>
          <title>S6</title>
        </shape>
      </shapes>
    </group>
  </groups>
</rtcShapesConfig>";
        }

        private static IEnumerable<ShapeBase> GetExpectedShapes()
        {
            return new ShapeBase[]
            {
                new InputItemShape
                {
                    X = 0,
                    Y = 0,
                    Width = 100,
                    Height = 20,
                    Title = "S1"
                },
                new OutputItemShape
                {
                    X = 40,
                    Y = 40,
                    Width = 100,
                    Height = 20,
                    Title = "S2"
                },
                new RuleShape
                {
                    X = 300,
                    Y = 100,
                    Width = 50,
                    Height = 50,
                    Title = "S3"
                },
                new SignalShape
                {
                    X = 150,
                    Y = 100,
                    Width = 50,
                    Height = 30,
                    Title = "S4"
                },
                new ConditionShape
                {
                    X = 200,
                    Y = 300,
                    Width = 20,
                    Height = 20,
                    Title = "S5"
                },
                new MathematicalExpressionShape
                {
                    X = 400,
                    Y = 100,
                    Width = 120,
                    Height = 40,
                    Title = "S6"
                }
            };
        }
    }
}