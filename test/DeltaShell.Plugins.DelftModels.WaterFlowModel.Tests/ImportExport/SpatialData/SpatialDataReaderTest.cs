using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileWriters.SpatialData;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.SpatialData;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.SpatialData
{
    [TestFixture]
    public class SpatialDataReaderTest
    {
        [Test]
        public void GivenNonExistingFilePath_WhenReadingSpatialData_ThenErrorMessageIsReturned()
        {
            // Given
            var nonExistentFilePath = "NonExistentFilePath.ini";
            var filePaths = new[] { nonExistentFilePath };
            var model = new WaterFlowModel1D();
            var errorMessages = new List<string>();

            // When
            SpatialDataReader.ReadSpatialData(filePaths, model, (s, list) => {errorMessages.AddRange(list);});

            // Then
            Assert.That(errorMessages.Count, Is.EqualTo(1));
            Assert.Contains($"Could not read file {nonExistentFilePath} properly, it doesn't exist.", errorMessages);
        }

        [Test]
        public void GivenInitialWaterLevelFile_WhenReadingDataWithSpatialDataReader_ThenInitialConditionsHaveBeenSetOnModel()
        {
            // Given
            var filePaths = GetFilePaths(SpatialDataFileNames.InitialWaterLevel);
            var model = new WaterFlowModel1D
            {
                InitialConditionsType = InitialConditionsType.WaterLevel
            };
            model.Network.Branches.Add(new Channel {Name = "Maasmond", Geometry = new LineString(new[]{new Coordinate(1,1), new Coordinate(2,2) })});

            // When
            SpatialDataReader.ReadSpatialData(filePaths, model, (s, list) => { });

            // Then
            Assert.IsNotEmpty(model.InitialConditions.Arguments[0].Values);
            Assert.IsNotEmpty(model.InitialConditions.Components[0].Values);
        }

        private static string[] GetFilePaths(string fileName)
        {
            var filePath = TestHelper.GetTestFilePath(Path.Combine("FileReaders", "SpatialData", fileName));
            var tempDirectory = FileUtils.CreateTempDirectory();
            var testFilePath = Path.Combine(tempDirectory, fileName);
            File.Copy(filePath, testFilePath);

            var filePaths = new[] {testFilePath};
            return filePaths;
        }

        [Test]
        public void GivenInitialWaterDepthFile_WhenReadingDataWithSpatialDataReader_ThenInitialConditionsHaveBeenSetOnModel()
        {
            // Given
            var filePaths = GetFilePaths(SpatialDataFileNames.InitialWaterDepth);
            var model = new WaterFlowModel1D
            {
                InitialConditionsType = InitialConditionsType.Depth
            };
            model.Network.Branches.Add(GetBasicChannel());

            // When
            SpatialDataReader.ReadSpatialData(filePaths, model, (s, list) => { });

            // Then
            Assert.IsNotEmpty(model.InitialConditions.Arguments[0].Values);
            Assert.IsNotEmpty(model.InitialConditions.Components[0].Values);
        }

        [Test]
        public void GivenInitialSalinityFile_WhenReadingDataWithSpatialDataReader_ThenInitialSaltConcentrationHasBeenSetOnModel()
        {
            // Given
            var filePaths = GetFilePaths(SpatialDataFileNames.InitialSalinity);
            var model = new WaterFlowModel1D
            {
                UseSalt = true
            };
            model.Network.Branches.Add(GetBasicChannel());

            // When
            SpatialDataReader.ReadSpatialData(filePaths, model, (s, list) => { });

            // Then
            Assert.IsNotEmpty(model.InitialSaltConcentration.Arguments[0].Values);
            Assert.IsNotEmpty(model.InitialSaltConcentration.Components[0].Values);
        }

        [Test]
        public void GivenInitialDischargeFile_WhenReadingDataWithSpatialDataReader_ThenInitialFlowHasBeenSetOnModel()
        {
            // Given
            var filePaths = GetFilePaths(SpatialDataFileNames.InitialDischarge);
            var model = new WaterFlowModel1D();
            model.Network.Branches.Add(GetBasicChannel());

            // When
            SpatialDataReader.ReadSpatialData(filePaths, model, (s, list) => { });

            // Then
            Assert.IsNotEmpty(model.InitialFlow.Arguments[0].Values);
            Assert.IsNotEmpty(model.InitialFlow.Components[0].Values);
        }

        [Test]
        public void GivenInitialTemperatureFile_WhenReadingDataWithSpatialDataReader_ThenInitialTemperatureHasBeenSetOnModel()
        {
            // Given
            var filePaths = GetFilePaths(SpatialDataFileNames.InitialTemperature);
            var model = new WaterFlowModel1D
            {
                UseTemperature = true
            };
            model.Network.Branches.Add(GetBasicChannel());

            // When
            SpatialDataReader.ReadSpatialData(filePaths, model, (s, list) => { });

            // Then
            Assert.IsNotEmpty(model.InitialTemperature.Arguments[0].Values);
            Assert.IsNotEmpty(model.InitialTemperature.Components[0].Values);
        }

        [Test]
        public void GivenDispersionFile_WhenReadingDataWithSpatialDataReader_ThenDispersionCoverageHasBeenSetOnModel()
        {
            // Given
            var filePaths = GetFilePaths(SpatialDataFileNames.Dispersion);
            var model = new WaterFlowModel1D
            {
                UseSalt = true
            };
            model.Network.Branches.Add(GetBasicChannel());

            // When
            SpatialDataReader.ReadSpatialData(filePaths, model, (s, list) => { });

            // Then
            Assert.IsNotEmpty(model.DispersionCoverage.Arguments[0].Values);
            Assert.IsNotEmpty(model.DispersionCoverage.Components[0].Values);
        }

        [Test]
        public void GivenDispersionF3File_WhenReadingDataWithSpatialDataReader_ThenDispersionF3CoverageHasBeenSetOnModel()
        {
            // Given
            var filePaths = GetFilePaths(SpatialDataFileNames.DispersionF3);
            var model = new WaterFlowModel1D
            {
                DispersionFormulationType = DispersionFormulationType.KuijperVanRijnPrismatic
            };
            model.Network.Branches.Add(GetBasicChannel());

            // When
            SpatialDataReader.ReadSpatialData(filePaths, model, (s, list) => { });

            // Then
            Assert.IsNotEmpty(model.DispersionF3Coverage.Arguments[0].Values);
            Assert.IsNotEmpty(model.DispersionF3Coverage.Components[0].Values);
        }

        [Test]
        public void GivenDispersionF4File_WhenReadingDataWithSpatialDataReader_ThenDispersionF4CoverageHasBeenSetOnModel()
        {
            // Given
            var filePaths = GetFilePaths(SpatialDataFileNames.DispersionF4);
            var model = new WaterFlowModel1D
            {
                DispersionFormulationType = DispersionFormulationType.KuijperVanRijnPrismatic
            };
            model.Network.Branches.Add(GetBasicChannel());

            // When
            SpatialDataReader.ReadSpatialData(filePaths, model, (s, list) => { });

            // Then
            Assert.IsNotEmpty(model.DispersionF4Coverage.Arguments[0].Values);
            Assert.IsNotEmpty(model.DispersionF4Coverage.Components[0].Values);
        }

        [Test]
        public void GivenWindShieldingFile_WhenReadingDataWithSpatialDataReader_ThenWindShieldingCoverageHasBeenSetOnModel()
        {
            // Given
            var filePaths = GetFilePaths(SpatialDataFileNames.WindShielding);
            var model = new WaterFlowModel1D();
            model.Network.Branches.Add(GetBasicChannel());

            // When
            SpatialDataReader.ReadSpatialData(filePaths, model, (s, list) => { });

            // Then
            Assert.IsNotEmpty(model.WindShielding.Arguments[0].Values);
            Assert.IsNotEmpty(model.WindShielding.Components[0].Values);
        }

        [Test]
        public void GivenCorrectSpatialDataFileWithUnknownFileName_WhenReadingDataWithSpatialDataReader_ThenMessageIsLogged()
        {
            // Given
            var filePaths = GetFilePaths("UnknownFileName.ini");
            var model = new WaterFlowModel1D();
            model.Network.Branches.Add(GetBasicChannel());

            // When
            var expectedLogMessage = string.Format(
                Resources.SpatialDataReader_SetModelSpatialDataOnModel_Could_not_find_any_spatial_data_to_set_on_the_model__The_file___0__does_not_have_a_correct_name_,
                filePaths[0]);
            TestHelper.AssertLogMessageIsGenerated(() => SpatialDataReader.ReadSpatialData(filePaths, model, (s, list) => { }), expectedLogMessage, 1);
        }

        private static Channel GetBasicChannel()
        {
            return new Channel { Name = "Maasmond", Geometry = new LineString(new[] { new Coordinate(1, 1), new Coordinate(2, 2) }) };
        }
    }
}