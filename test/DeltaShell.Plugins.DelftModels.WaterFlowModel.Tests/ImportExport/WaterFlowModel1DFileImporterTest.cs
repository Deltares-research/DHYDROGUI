using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport
{
    [TestFixture]
    public class WaterFlowModel1DFileImporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void GivenAnMd1dFile_WhenImportingFileAndExporting_ThenTheInputFilesAreTheSameAsTheOutputFiles11()
        {
            // TODO: Add BoundaryConditions.bc, CrossSectionDefinitions.ini, CrossSectionLocations.ini, NetworkDefinition.ini, ObservationPoints.ini & the md1d file
            var md1dFilePath = TestHelper.GetTestFilePath(@"ImportSpatialData\water flow 1d.md1d");
            var testDirectory = FileUtils.CreateTempDirectory();
            var errorMessage = "Files not equal";
            var targetFilePath = Path.Combine(testDirectory, "FileWriters");
            var fileCollection = new List<string>
            {
                "BoundaryLocations.ini",
                "Dispersion.ini",
                "DispersionF3.ini",
                "DispersionF4.ini",
                "InitialDischarge.ini",
                "InitialSalinity.ini",
                "InitialTemperature.ini",
                "InitialWaterLevel.ini",
                "LateralDischargeLocations.ini",
                "Retention.ini",
                "roughness-FloodPlain1 (Reversed).ini",
                "roughness-FloodPlain1.ini",
                "roughness-Main (Reversed).ini",
                "roughness-Main.ini",
                "Salinity.ini",
                "sobeksim.fnm",
                "SobekSim.ini",
                "Structures.ini",
                "WindShielding.ini"
            };

            try
            {
                var importer = new WaterFlowModel1DFileImporter
                {
                    ProgressChanged = (name, step, steps) => { }
                };
                var model = importer.ImportItem(md1dFilePath) as WaterFlowModel1D;
                Assert.IsNotNull(model);

                WaterFlowModel1DFileWriter.Write(Path.Combine(targetFilePath, ModelFileNames.ModelDefinitionFilename), model);

                foreach (var file in fileCollection)
                {
                    var iniFilePath = Path.Combine(targetFilePath, file);
                    Assert.IsTrue(FileComparer.Compare(iniFilePath,
                        TestHelper.GetTestFilePath($@"ImportSpatialData\{file}"), out errorMessage));
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(testDirectory);
            }
        }
    }
}