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
        // TODO: BoundaryConditions.bc, CrossSectionDefinitions.ini, CrossSectionLocations.ini, NetworkDefinition.ini, ObservationPoints.ini & the md1d file
        [TestCase("BoundaryLocations.ini")]
        [TestCase("Dispersion.ini")]
        [TestCase("DispersionF3.ini")]
        [TestCase("DispersionF4.ini")]
        [TestCase("InitialDischarge.ini")]
        [TestCase("InitialSalinity.ini")]
        [TestCase("InitialTemperature.ini")]
        [TestCase("InitialWaterLevel.ini")]
        [TestCase("LateralDischargeLocations.ini")]
        [TestCase("Retention.ini")]
        [TestCase("roughness-FloodPlain1 (Reversed).ini")]
        [TestCase("roughness-FloodPlain1.ini")]
        [TestCase("roughness-Main (Reversed).ini")]
        [TestCase("roughness-Main.ini")]
        [TestCase("Salinity.ini")]
        [TestCase("sobeksim.fnm")]
        [TestCase("SobekSim.ini")]
        [TestCase("Structures.ini")]
        [TestCase("WindShielding.ini")]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void GivenAnMd1dFile_WhenImportingFileAndExporting_ThenTheInputFilesAreTheSameAsTheOutputFiles11(
            string spatialIniFile)
        {
            var md1dFilePath = TestHelper.GetTestFilePath(@"ImportSpatialData\water flow 1d.md1d");
            var testDirectory = FileUtils.CreateTempDirectory();
            var sourceFile = TestHelper.GetTestFilePath($@"ImportSpatialData\{spatialIniFile}");
            var errorMessage = "Files not equal";
            var targetFilePath = Path.Combine(testDirectory, "FileWriters");

            try
            {
                var importer = new WaterFlowModel1DFileImporter
                {
                    ProgressChanged = (name, step, steps) => { }
                };
                var model = importer.ImportItem(md1dFilePath) as WaterFlowModel1D;
                Assert.IsNotNull(model);

                WaterFlowModel1DFileWriter.Write(Path.Combine(targetFilePath, ModelFileNames.ModelDefinitionFilename),
                    model);

                var iniFilePath = Path.Combine(targetFilePath, spatialIniFile);
                Assert.IsTrue(FileComparer.Compare(iniFilePath, sourceFile, out errorMessage));
            }
            finally
            {
                FileUtils.DeleteIfExists(testDirectory);
            }
        }
    }
}