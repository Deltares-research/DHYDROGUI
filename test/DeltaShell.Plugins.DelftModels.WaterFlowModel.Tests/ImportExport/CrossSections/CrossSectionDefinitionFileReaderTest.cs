using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.Collections.Extensions;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.CrossSections
{
    [TestFixture]
    public class CrossSectionDefinitionFileReaderTest
    {
        private IList<string> errorReport;
        private CrossSectionDefinitionFileReader reader;
        private Action<string, IList<string>> createAndAddErrorReport;

        [SetUp]
        public void SetUp()
        {
            createAndAddErrorReport =
                (header, errorMessages) => errorReport.AddRange(errorMessages);
            reader = new CrossSectionDefinitionFileReader(createAndAddErrorReport);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAValidCrossSectionDefinitionFileWithTypeYZ_WhenReading_ThenCrossSectionDefinitionsShouldBeReturnedWithoutErrors()
        {
            errorReport = new List<string>();

            var filePath = TestHelper.GetTestFilePath(@"ImportCrossSections\CrossSectionDefinitions_YZ.ini");

            Assert.That(File.Exists(filePath));

            var network = new HydroNetwork();
            network.CrossSectionSectionTypes.Clear();

            var definitions = reader.Read(filePath, network);

            Assert.AreEqual(2, definitions.Count);
            Assert.AreEqual(0, errorReport.Count);
            Assert.AreEqual(2, definitions.First().Sections.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAValidCrossSectionDefinitionFileWithTypeZW_WhenReading_ThenCrossSectionDefinitionsShouldBeReturnedWithoutErrors()
        {
            errorReport = new List<string>();

            var filePath = TestHelper.GetTestFilePath(@"ImportCrossSections\CrossSectionDefinitions_ZW.ini");

            Assert.That(File.Exists(filePath));

            var network = new HydroNetwork();

            var definitions = reader.Read(filePath, network);

            Assert.AreEqual(1, definitions.Count);
            Assert.AreEqual(0, errorReport.Count);
            Assert.AreEqual(2, definitions.First().Sections.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAValidCrossSectionDefinitionFileWithTypeStandard_WhenReading_ThenCrossSectionDefinitionsShouldBeReturnedWithoutErrors()
        {
            errorReport = new List<string>();

            var filePath = TestHelper.GetTestFilePath(@"ImportCrossSections\CrossSectionDefinitions_Standard.ini");

            Assert.That(File.Exists(filePath));

            var network = new HydroNetwork();

            var definitions = reader.Read(filePath, network);

            Assert.AreEqual(1, definitions.Count);
            Assert.AreEqual(0, errorReport.Count);
            Assert.AreEqual(1, definitions.First().Sections.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAValidCrossSectionDefinitionFile_WhenReadingGroundLayerData_ThenGroundLayerDataObjectShouldBeReturnedWithoutErrors()
        {
            // Given
            var filePath = TestHelper.GetTestFilePath(@"ImportCrossSections\CrossSectionDefinitions_Standard.ini");
            Assert.That(File.Exists(filePath));

            // When
            var groundLayerDataObjects = CrossSectionDefinitionFileReader.ReadGroundLayerData(filePath).ToArray();

            // Then
            Assert.That(groundLayerDataObjects.Length, Is.EqualTo(1));
            var groundLayerData = groundLayerDataObjects.FirstOrDefault();
            Assert.IsNotNull(groundLayerData);
            Assert.That(groundLayerData.CrossSectionDefinitionId, Is.EqualTo("CrossSection1"));
            Assert.IsTrue(groundLayerData.GroundLayerUsed, "GroundLayer is not used");
            Assert.That(groundLayerData.GroundLayerThickness, Is.EqualTo(2.0));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCase(@"ImportCrossSections\CrossSectionDefinitions_Standard_NoRoughnessNames.ini", "There was no roughness defined in the cross section definition file.")]
        [TestCase(@"ImportCrossSections\CrossSectionDefinitions_Standard_TwoRoughnessNames.ini", "There can only be one roughness defined on a standard cross section definition.")]
        public void GivenAValidCrossSectionDefinitionFileWithTypeStandardWithoutExactlyOneRoughnessSection_WhenReading_ThenErrorShouldBeReported(string partialFilePath, string expectedMessage)
        {
            errorReport = new List<string>();

            var filePath = TestHelper.GetTestFilePath(partialFilePath);

            Assert.That(File.Exists(filePath));

            var network = new HydroNetwork();

            var definitions = reader.Read(filePath, network);

            Assert.AreEqual(1, definitions.Count);
            Assert.AreEqual(1, errorReport.Count);
            Assert.AreEqual(0, definitions.First().Sections.Count);
            Assert.That(errorReport.Any(e => e.Equals(expectedMessage)));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenACrossSectionDefinitionFileWithDuplicateIds_WhenReading_ThenErrorShouldBeReported()
        {
            errorReport = new List<string>();

            var filePath = TestHelper.GetTestFilePath(@"ImportCrossSections\CrossSectionDefinitions_YZ_DuplicateId.ini");

            Assert.That(File.Exists(filePath));

            var network = new HydroNetwork();

            var defintions = reader.Read(filePath, network);

            Assert.AreEqual(1, defintions.Count);

            var definition = defintions.First();

            Assert.AreEqual(1, errorReport.Count);
            Assert.That(errorReport.Any(e => e.Equals($"Cross section definition with id {definition.Name} already exists, there cannot be any duplicate cross section definition ids")));

            Assert.AreEqual(2, definition.Sections.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCase(@"ImportCrossSections\CrossSectionDefinitions_YZ_Roughness.ini", "Incorrect number of roughness positions in cross section definition file: should be one more than the number of roughness sections.")]
        [TestCase(@"ImportCrossSections\CrossSectionDefinitions_YZ_NoRoughnessNames.ini", "There were no roughness names defined in the cross section definition file.")]
        [TestCase(@"ImportCrossSections\CrossSectionDefinitions_YZ_NoRoughnessPositions.ini", " is not a valid value for Double.")]
        public void GivenACrossSectionDefinitionFileWithInvalidNumberRoughnessPositions_WhenReading_ThenErrorShouldBeReported(string partialFilePath, string expectedMessage)
        {
            errorReport = new List<string>();

            var filePath = TestHelper.GetTestFilePath(partialFilePath);

            Assert.That(File.Exists(filePath));

            var network = new HydroNetwork();

            var definitions = reader.Read(filePath, network);

            Assert.AreEqual(1, definitions.Count);

            var definition = definitions.First();

            Assert.AreEqual(1, errorReport.Count);
            Assert.That(errorReport.Any(e => e.Equals(expectedMessage)));

            Assert.AreEqual(0, definition.Sections.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenANonExistingCrossSectionDefinitionFile_WhenReading_ThenErrorShouldBeReported()
        {
            errorReport = new List<string>();

            var filePath = TestHelper.GetTestFilePath(@"ImportCrossSections\idonotexist.ini");

            Assert.That(!File.Exists(filePath));

            var definitions = reader.Read(filePath, null);

            Assert.AreEqual(0, definitions.Count);

            Assert.AreEqual(1, errorReport.Count);
            Assert.That(errorReport.Any(e => e.Equals($"Could not read file {filePath} properly, it doesn't exist.")));

        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenACrossSectionDefinitionFileWithMissingProperty_WhenReading_ThenErrorShouldBeReported()
        {
            errorReport = new List<string>();

            const string missingPropertyName = "type";

            var filePath = TestHelper.GetTestFilePath(@"ImportCrossSections\CrossSectionDefinitions_YZ_MissingProperty.ini");

            Assert.That(File.Exists(filePath));

            var network = new HydroNetwork();

            var defintions = reader.Read(filePath, network);

            Assert.AreEqual(1, defintions.Count);
            Assert.AreEqual(1, errorReport.Count);
            Assert.That(errorReport.Any(e => e.Equals($"Property {missingPropertyName} is not found in the file")));
            Assert.AreEqual(2, defintions.First().Sections.Count);
        }
    }
}
