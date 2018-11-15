using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.CrossSections
{
    [TestFixture]
    class CrossSectionDefinitionFileReaderTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAValidCrossSectionDefinitionFile_WhenReading_ThenCrossSectionDefinitionsShouldBeReturnedWithoutErrors()
        {
            var errorReport = new List<string>();

            Action<string, IList<string>> createAndAddErrorReport =
                (header, errorMessages) => errorReport.AddRange(errorMessages);

            var filePath = TestHelper.GetTestFilePath(@"ImportCrossSections\CrossSectionDefinitions_YZ.ini");

            Assert.That(File.Exists(filePath));

            var reader = new CrossSectionDefinitionFileReader(createAndAddErrorReport);

            var network = new HydroNetwork();

            var defintions = reader.Read(filePath, network);

            Assert.AreEqual(2, defintions.Count);
            Assert.AreEqual(0, errorReport.Count);
            Assert.AreEqual(2, defintions.First().Sections.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenACrossSectionDefinitionFileWithDuplicateIds_WhenReading_ThenErrorShouldBeReported()
        {
            var errorReport = new List<string>();

            Action<string, IList<string>> createAndAddErrorReport =
                (header, errorMessages) => errorReport.AddRange(errorMessages);

            var filePath = TestHelper.GetTestFilePath(@"ImportCrossSections\CrossSectionDefinitions_YZ_DuplicateId.ini");

            Assert.That(File.Exists(filePath));

            var reader = new CrossSectionDefinitionFileReader(createAndAddErrorReport);

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
        public void GivenACrossSectionDefinitionFileWithMissingProperty_WhenReading_ThenErrorShouldBeReported()
        {
            var errorReport = new List<string>();

            Action<string, IList<string>> createAndAddErrorReport = 
                (header, errorMessages) => errorReport.AddRange(errorMessages);

            const string missingPropertyName = "type";

            var filePath = TestHelper.GetTestFilePath(@"ImportCrossSections\CrossSectionDefinitions_YZ_MissingProperty.ini");

            Assert.That(File.Exists(filePath));

            var reader = new CrossSectionDefinitionFileReader(createAndAddErrorReport);

            var network = new HydroNetwork();

            var defintions = reader.Read(filePath, network);

            Assert.AreEqual(1, defintions.Count);
            Assert.AreEqual(1, errorReport.Count);
            Assert.That(errorReport.Any(e => e.Equals($"Property {missingPropertyName} is not found in the file")));
            Assert.AreEqual(2, defintions.First().Sections.Count);
        }
    }
}
