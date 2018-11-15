using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.CrossSections
{
    [TestFixture]
    public class CrossSectionLocationFileReaderTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAValidCrossSectionLocationFile_WhenReading_ThenCrossSectionLocationsShouldBeReturnedWithoutErrors ()
        {
            var errorReport = new List<string>();
            Action<string, IList<string>> createAndAddErrorReport = (header, errorMessages) =>
                errorReport.AddRange(errorMessages);

            var filePath = TestHelper.GetTestFilePath(@"ImportCrossSections\CrossSectionLocations_YZ.ini");

            Assert.That(File.Exists(filePath));

            var reader = new CrossSectionLocationFileReader(createAndAddErrorReport);

            var locations = reader.Read(filePath);

            Assert.AreEqual(2, locations.Count());
            Assert.AreEqual(0, errorReport.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenACrossSectionLocationFileWithDuplicateIds_WhenReading_ThenErrorShouldBeReported()
        {
            var errorReport = new List<string>();
            Action<string, IList<string>> createAndAddErrorReport = (header, errorMessages) =>
                errorReport.AddRange(errorMessages);

            var filePath = TestHelper.GetTestFilePath(@"ImportCrossSections\CrossSectionLocations_YZ_DuplicateId.ini");

            Assert.That(File.Exists(filePath));

            var reader = new CrossSectionLocationFileReader(createAndAddErrorReport);

            var locations = reader.Read(filePath);

            Assert.AreEqual(1, locations.Count());

            var location = locations.First();

            Assert.AreEqual(1, errorReport.Count);
            Assert.That(errorReport.Any(e => e.Equals($"Cross section location with id {location.Name} already exists, there cannot be any duplicate cross section location ids")));

        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenACrossSectionLocationFileWithMissingProperty_WhenReading_ThenErrorShouldBeReported()
        {
            var errorReport = new List<string>();
            Action<string, IList<string>> createAndAddErrorReport = (header, errorMessages) =>
                errorReport.AddRange(errorMessages);

            const string missingPropertyName = "branchid";

            var filePath = TestHelper.GetTestFilePath(@"ImportCrossSections\CrossSectionLocations_YZ_MissingProperty.ini");

            Assert.That(File.Exists(filePath));

            var reader = new CrossSectionLocationFileReader(createAndAddErrorReport);

            var locations = reader.Read(filePath);

            Assert.AreEqual(1, locations.Count());
            Assert.AreEqual(1, errorReport.Count);
            Assert.That(errorReport.Any(e => e.Equals($"Property {missingPropertyName} is not found in the file")));
        }
    }
}
