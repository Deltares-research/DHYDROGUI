using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class WaterFlowModel1DFileReaderTest
    {
        private string tempFolderPath;
        
        [TestFixtureSetUp]
        public void FixtureSetup()
        {
           var testFolder = TestHelper.GetTestDataPath(Assembly.GetExecutingAssembly(), @"Md1dReading");
           tempFolderPath = TestHelper.CreateLocalCopy(testFolder);
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            FileUtils.DeleteIfExists(tempFolderPath);
        }
        
        [Test]
        public void GivenAnMd1dFile_WhenReadingTheAttachedNetworkDefinitionFile_ThenAModelIsReturned()
        {
            var md1dFilePath = Path.Combine(tempFolderPath, "Md1dExport.md1d");

            var waterFlowModel1D = WaterFlowModel1DFileReader.Read(md1dFilePath);
            Assert.IsNotNull(waterFlowModel1D);
        }

        [Test]
        public void GivenAnMd1dFileWithSediment_WhenReadingTheAttachedNetworkDefinitionFile_ThenAModelIsReturned()
        {
            var md1dFilePath = Path.Combine(tempFolderPath, "Md1dExportWithSediment.md1d");

            var waterFlowModel1D = WaterFlowModel1DFileReader.Read(md1dFilePath);
            Assert.IsNotNull(waterFlowModel1D);
        }

        [Test]
        public void GivenAnMd1dFileWithReversedRoughnessSectionDefined_WhenReadingFlow1DModel_ThenModelUsesReversedRoughness()
        {
            var md1dFilePath = Path.Combine(tempFolderPath, "Md1dExportWithReversedRoughnessSection.md1d");

            var waterFlowModel1D = WaterFlowModel1DFileReader.Read(md1dFilePath);
            Assert.IsNotNull(waterFlowModel1D);
            Assert.IsTrue(waterFlowModel1D.UseReverseRoughness);
            Assert.IsTrue(waterFlowModel1D.UseReverseRoughnessInCalculation);
        }

        [TestCase("InitialWaterLevel.ini")]
        [TestCase("Dispersion.ini")]
        [TestCase("InitialDischarge.ini")]
        [TestCase("InitialSalinity.ini")]
        [TestCase("InitialTemperature.ini")]
        [TestCase("InitialWaterLevel.ini")]
        [TestCase("WindShielding.ini")]
        [Category(TestCategory.Slow)]
        public void GivenAnMd1dFile_WhenReadingAndWriting_ThenTheWrittenFilesAreEqualToReadFiles(string spatialIniFile)
        {
            // Given
            var md1dFilePath = TestHelper.GetTestFilePath(@"ImportSpatialData\water flow 1d.md1d");
            var testDirectory = FileUtils.CreateTempDirectory();
            var sourceFile = TestHelper.GetTestFilePath($@"ImportSpatialData\{spatialIniFile}");
            var errorMessage = "Files not equal";
            var targetFilePath = Path.Combine(testDirectory, "FileWriters");

            try
            {
                //When
                var waterFlowModel1D = WaterFlowModel1DFileReader.Read(md1dFilePath);
                Assert.IsNotNull(waterFlowModel1D);
                WaterFlowModel1DFileWriter.Write(Path.Combine(targetFilePath, ModelFileNames.ModelDefinitionFilename),
                    waterFlowModel1D);

                // Then
                var iniFilePath = Path.Combine(targetFilePath, spatialIniFile);
                Assert.IsTrue(FileComparer.Compare(iniFilePath, sourceFile, out errorMessage));
            }
            finally
            {
                FileUtils.DeleteIfExists(testDirectory);
            }
        }
        
        [Test]
        public void GivenAnMd1dFile_WhenReadingTheNetworkDefinitionFileWithABadNode_ThenNullIsReturned()
        {
            var md1dFilePath = Path.Combine(tempFolderPath, "Md1dExportBadNode.md1d");

            var waterFlowModel1D = WaterFlowModel1DFileReader.Read(md1dFilePath);
            Assert.IsNull(waterFlowModel1D);
        }

        [Test]
        public void GivenAnMd1dFile_WhenReadingTheNetworkDefinitionFileWithABadBranch_ThenNullIsReturned()
        {
            var md1dFilePath = Path.Combine(tempFolderPath, "Md1dExportBadBranch.md1d");

            var waterFlowModel1D = WaterFlowModel1DFileReader.Read(md1dFilePath);
            Assert.IsNull(waterFlowModel1D);
        }

        [Test]
        public void GivenBadlyFormattedMd1dFile_WhenReadingMd1dfFile_ThenFormatExceptionIsThrown()
        {
            // Given
            var md1dFilePath = Path.Combine(tempFolderPath, "ModelDefinitionsFileWithBadFormat.md1d");

            // When - Then
            Assert.Throws<FormatException>(() => WaterFlowModel1DFileReader.Read(md1dFilePath));
        }

        [Test]
        public void GivenMd1dFileWithMandatoryFileNameMissing_WhenReadingMd1dfFile_ThenPropertyNotFoundInFileExceptionIsThrown()
        {
            // Given
            var md1dFilePath = Path.Combine(tempFolderPath, "ModelDefinitionsFileWithMissingMandatoryFileProperty.md1d");

            // When - Then
            Assert.Throws<PropertyNotFoundInFileException>(() => WaterFlowModel1DFileReader.Read(md1dFilePath));
        }

        [Test]
        public void GivenAnMd1dFile_WhenReadingTheNetworkDefinitionFileWithABadNetworkDiscretization_ThenNullIsReturned()
        {
            var md1dFilePath = Path.Combine(tempFolderPath, "Md1dExportBadNetworkDiscretization.md1d");

            var waterFlowModel1D = WaterFlowModel1DFileReader.Read(md1dFilePath);
            Assert.IsNull(waterFlowModel1D);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void GivenAnMd1dFile_WhenReading_ThenAModelIsReturned()
        {
            var md1dFilePath = TestHelper.GetTestFilePath(@"ImportSpatialData\water flow 1d.md1d");

            var waterFlowModel1D = WaterFlowModel1DFileReader.Read(md1dFilePath);
            Assert.IsNotNull(waterFlowModel1D.Network);

            Assert.AreEqual(267, waterFlowModel1D.Network.Branches.Count);
            Assert.AreEqual(212, waterFlowModel1D.Network.HydroNodes.Count());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void GivenAnMd1dFile_WhenReadingAnIncorrectSpatialDataFile_ThenNullIsReturned()
        {
            var md1dFilePath = TestHelper.GetTestFilePath(@"ImportSpatialData\water flow 1dIncorrect.md1d");

            var waterFlowModel1D = WaterFlowModel1DFileReader.Read(md1dFilePath);
            Assert.IsNull(waterFlowModel1D);
        }
    }
}
