using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Common.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [TestFixture]
    public class CrossSectionDefinitionStandardFileTest
    {
        private const string TestDirectoryName = "myDir";
        private const string CsDefIniFileName = "myCrossSectionDefinition.ini";
        private string testDir;
        private string filePath;
        private CrossSectionDefinitionStandardFile writer;

        [SetUp]
        public void Setup()
        {
            testDir = Path.Combine(Path.GetTempPath(), TestDirectoryName);
            filePath = Path.Combine(testDir, CsDefIniFileName);
            writer = new CrossSectionDefinitionStandardFile();
        }

        [TearDown]
        public void TearDown()
        {
            FileUtils.DeleteIfExists(testDir);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenCrossSectionDefinitionStandardFile_WhenWriting_ThenIniFileIsCreatedAtGivenLocation()
        {
            var csDefinitions = new List<CrossSectionDefinitionStandard> {new CrossSectionDefinitionStandard()};
            writer.Write(filePath, csDefinitions);

            Assert.IsTrue(File.Exists(filePath));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenCsDefinitionStandardRound_WhenWriting_ThenFileIsInRightFormat()
        {
            var csDefinitions = new List<CrossSectionDefinitionStandard> {
                new CrossSectionDefinitionStandard(new CrossSectionStandardShapeCircle { Diameter = 0.45 })
                {
                    Name = "myCrossSectionDefinition",
                }
            };

            writer.Write(filePath, csDefinitions);
            var text = File.ReadAllText(filePath);
            var dimensionText = GetIniFileLine("Diameter", "0,45");
            var expectedText = ConstructExpectedFileContent(csDefinitions.First(), dimensionText);
            Assert.That(text, Is.EqualTo(expectedText));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenCsDefinitionStandardRectangle_WhenWriting_ThenFileIsInRightFormat()
        {
            var csDefinitions = new List<CrossSectionDefinitionStandard> {
                new CrossSectionDefinitionStandard(new CrossSectionStandardShapeRectangle { Width = 5.6, Height = 1.6 })
                {
                    Name = "myCrossSectionDefinition",
                }
            };

            writer.Write(filePath, csDefinitions);
            var text = File.ReadAllText(filePath);
            var dimensionText =
                GetIniFileLine("Width", "5,60") +
                GetIniFileLine("Height", "1,60");
            var expectedText = ConstructExpectedFileContent(csDefinitions.First(), dimensionText);
            Assert.That(text, Is.EqualTo(expectedText));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenCsDefinitionStandardArch_WhenWriting_ThenFileIsInRightFormat()
        {
            var csDefinitions = new List<CrossSectionDefinitionStandard> {
                new CrossSectionDefinitionStandard(new CrossSectionStandardShapeArch { Width = 1.23, Height = 1.89, ArcHeight = 0.67})
                {
                    Name = "myCrossSectionDefinition",
                }
            };

            writer.Write(filePath, csDefinitions);
            var text = File.ReadAllText(filePath);
            var dimensionText =
                GetIniFileLine("Width", "1,23") +
                GetIniFileLine("Height", "1,89") +
                GetIniFileLine("ArcHeight", "0,67");
            var expectedText = ConstructExpectedFileContent(csDefinitions.First(), dimensionText);
            Assert.That(text, Is.EqualTo(expectedText));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenCsDefinitionStandardCunette_WhenWriting_ThenFileIsInRightFormat()
        {
            var csDefinitions = new List<CrossSectionDefinitionStandard> {
                new CrossSectionDefinitionStandard(CrossSectionStandardShapeCunette.CreateDefault())
                {
                    Name = "myCrossSectionDefinition",
                }
            };

            writer.Write(filePath, csDefinitions);
            var text = File.ReadAllText(filePath);
            var dimensionText =
                GetIniFileLine("Width", "1,00") +
                GetIniFileLine("Height", "0,634");
            var expectedText = ConstructExpectedFileContent(csDefinitions.First(), dimensionText);
            Assert.That(text, Is.EqualTo(expectedText));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenCsDefinitionStandardSteelCunette_WhenWriting_ThenFileIsInRightFormat()
        {
            var csDefinitions = new List<CrossSectionDefinitionStandard> {
                new CrossSectionDefinitionStandard(CrossSectionStandardShapeSteelCunette.CreateDefault())
                {
                    Name = "myCrossSectionDefinition",
                }
            };

            writer.Write(filePath, csDefinitions);
            var text = File.ReadAllText(filePath);
            var dimensionText =
                GetIniFileLine("Height", "0,78") +
                GetIniFileLine("RadiusR", "0,50") +
                GetIniFileLine("RadiusR1", "0,80") +
                GetIniFileLine("RadiusR2", "0,20") +
                GetIniFileLine("RadiusR3", "0,00") +
                GetIniFileLine("AngleA", "28,00") +
                GetIniFileLine("AngleA1", "0,00");
            var expectedText = ConstructExpectedFileContent(csDefinitions.First(), dimensionText);
            Assert.That(text, Is.EqualTo(expectedText));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenCsDefinitionStandardEgg_WhenWriting_ThenFileIsInRightFormat()
        {
            var csDefinitions = new List<CrossSectionDefinitionStandard> {
                new CrossSectionDefinitionStandard(CrossSectionStandardShapeEgg.CreateDefault())
                {
                    Name = "myCrossSectionDefinition",
                }
            };

            writer.Write(filePath, csDefinitions);
            var text = File.ReadAllText(filePath);
            var dimensionText =
                GetIniFileLine("Width", "2,00") +
                GetIniFileLine("Height", "3,00");
            var expectedText = ConstructExpectedFileContent(csDefinitions.First(), dimensionText);
            Assert.That(text, Is.EqualTo(expectedText));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenCsDefinitionStandardElliptical_WhenWriting_ThenFileIsInRightFormat()
        {
            var csDefinitions = new List<CrossSectionDefinitionStandard> {
                new CrossSectionDefinitionStandard(CrossSectionStandardShapeElliptical.CreateDefault())
                {
                    Name = "myCrossSectionDefinition",
                }
            };

            writer.Write(filePath, csDefinitions);
            var text = File.ReadAllText(filePath);
            var dimensionText =
                GetIniFileLine("Width", "1,00") +
                GetIniFileLine("Height", "1,00");
            var expectedText = ConstructExpectedFileContent(csDefinitions.First(), dimensionText);
            Assert.That(text, Is.EqualTo(expectedText));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenCsDefinitionStandardTrapezium_WhenWriting_ThenFileIsInRightFormat()
        {
            var csDefinitions = new List<CrossSectionDefinitionStandard> {
                new CrossSectionDefinitionStandard(CrossSectionStandardShapeTrapezium.CreateDefault())
                {
                    Name = "myCrossSectionDefinition",
                }
            };

            writer.Write(filePath, csDefinitions);
            var text = File.ReadAllText(filePath);
            var dimensionText =
                GetIniFileLine("Slope", "2,00") +
                GetIniFileLine("BottomWidthB", "10,00") +
                GetIniFileLine("MaximumFlowWidth", "20,00");
            var expectedText = ConstructExpectedFileContent(csDefinitions.First(), dimensionText);
            Assert.That(text, Is.EqualTo(expectedText));
        }

        private static string ConstructExpectedFileContent(CrossSectionDefinitionStandard csDefinition, string dimensionLines)
        {
            var expectedText = new StringBuilder();
            expectedText.Append("[Definition]" + Environment.NewLine);
            expectedText.Append(GetIniFileLine("Id", "myCrossSectionDefinition"));
            expectedText.Append(GetIniFileLine("Type", csDefinition.ShapeType.ToString()));
            expectedText.Append(dimensionLines);
            expectedText.Append(GetIniFileLine("Closed", "1"));
            expectedText.Append(GetIniFileLine("GroundLayerUsed", "0"));
            expectedText.Append(GetIniFileLine("RoughnessNames", "Main"));
            expectedText.Append(Environment.NewLine);

            return expectedText.ToString();
        }

        private static string GetIniFileLine(string key, string value)
        {
            return $"    {key,-22}= {value,-20}" + Environment.NewLine;
        }
    }
}