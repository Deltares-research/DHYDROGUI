using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.FileWriters.SpatialData;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport
{
    [Category(TestCategory.DataAccess)]
    [TestFixture]
    public class ModelDefinitionsFileWriterTest
    {
        [Test]
        public void TestModelDefinitionsFileWriter()
        {
            var expectedFile = TestHelper.GetTestFilePath(@"FileWriters/ModelDefinitions_expected.txt");
            
            var targetPath = Path.Combine(Environment.CurrentDirectory, FileWriterTestHelper.RelativeTargetDirectory);
            ModelDefinitionFileWriter.RoughnessFiles = "roughness-1.ini;roughness-2.ini";
            ModelDefinitionFileWriter.WriteFile(FileWriterTestHelper.ModelFileNames.LateralDischarge, null);
            
            string errorMessage;
            var relativePathActualFile = Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename);
            Assert.IsTrue(FileComparer.Compare(expectedFile, relativePathActualFile, out errorMessage, true),
                          string.Format("Generated ModelDefinitions file does not match template!{0}{1}", Environment.NewLine, errorMessage));
        }

        [Test]
        public void TestModelDefinitionsFileWriter_Files()
        {
            var targetPath = Path.Combine(Environment.CurrentDirectory, FileWriterTestHelper.RelativeTargetDirectory);
            var modelDefinitionFile = Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename);

            ModelDefinitionFileWriter.RoughnessFiles = "roughness-1.ini;roughness-2.ini";
            ModelDefinitionFileWriter.WriteFile(FileWriterTestHelper.ModelFileNames.LateralDischarge, null);

            var modelFileNames = new ModelFileNames();

            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(modelDefinitionFile);

            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(1, categories.Count(op => op.Name == ModelDefinitionsRegion.FilesIniHeader));

            var content = categories.Where(c => c.Name == ModelDefinitionsRegion.FilesIniHeader).ToList().First();

            var networkFileProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.NetworkFile.Key);
            Assert.AreEqual(modelFileNames.Network, networkFileProperty.Value);

            var crossSectionLocationsFile = content.Properties.First(p => p.Name == ModelDefinitionsRegion.CrossSectionLocationsFile.Key);
            Assert.AreEqual(modelFileNames.CrossSectionLocations, crossSectionLocationsFile.Value);

            var crossSectionDefinitionsFile = content.Properties.First(p => p.Name == ModelDefinitionsRegion.CrossSectionDefinitionsFile.Key);
            Assert.AreEqual(modelFileNames.CrossSectionDefinitions, crossSectionDefinitionsFile.Value);

            var structuresFile = content.Properties.First(p => p.Name == ModelDefinitionsRegion.StructuresFile.Key);
            Assert.AreEqual(modelFileNames.Structures, structuresFile.Value);

            var observationPointLocationsFile = content.Properties.First(p => p.Name == ModelDefinitionsRegion.ObservationPointsFile.Key);
            Assert.AreEqual(modelFileNames.ObservationPoints, observationPointLocationsFile.Value);
            
            var initialWaterLevelFile = content.Properties.First(p => p.Name == ModelDefinitionsRegion.InitialWaterLevelFile.Key);
            Assert.AreEqual(SpatialDataFileNames.InitialWaterLevel, initialWaterLevelFile.Value);

            var initialWaterDepthFile = content.Properties.First(p => p.Name == ModelDefinitionsRegion.InitialWaterDepthFile.Key);
            Assert.AreEqual(SpatialDataFileNames.InitialWaterDepth, initialWaterDepthFile.Value);

            var initialDischargeFile = content.Properties.First(p => p.Name == ModelDefinitionsRegion.InitialDischargeFile.Key);
            Assert.AreEqual(SpatialDataFileNames.InitialDischarge, initialDischargeFile.Value);

            var initialSalinityFile = content.Properties.First(p => p.Name == ModelDefinitionsRegion.InitialSalinityFile.Key);
            Assert.AreEqual(SpatialDataFileNames.InitialSalinity, initialSalinityFile.Value);

            var windShieldingFile = content.Properties.First(p => p.Name == ModelDefinitionsRegion.WindShieldingFile.Key);
            Assert.AreEqual(SpatialDataFileNames.WindShielding, windShieldingFile.Value);

            // TODO: according to the definition document, roughness files should be added to a "Roughness" directory
            var roughnessFiles = content.Properties.First(p => p.Name == ModelDefinitionsRegion.RoughnessFile.Key);
            Assert.AreEqual(ModelDefinitionFileWriter.RoughnessFiles, roughnessFiles.Value);
            

            var boundaryLocationsFile = content.Properties.First(p => p.Name == ModelDefinitionsRegion.BoundaryLocationsFile.Key);
            Assert.AreEqual(modelFileNames.BoundaryLocations, boundaryLocationsFile.Value);

            var lateralDischargeLocationsFile = content.Properties.First(p => p.Name == ModelDefinitionsRegion.LateralDischargeLocationsFile.Key);
            Assert.AreEqual(modelFileNames.LateralDischarge, lateralDischargeLocationsFile.Value);

            var boundaryConditionsFile = content.Properties.First(p => p.Name == ModelDefinitionsRegion.BoundaryConditionsFile.Key);
            Assert.AreEqual(modelFileNames.BoundaryConditions, boundaryConditionsFile.Value);

            var retentionFile = content.Properties.First(p => p.Name == ModelDefinitionsRegion.RetentionFile.Key);
            Assert.AreEqual(modelFileNames.Retention, retentionFile.Value);
        }

        [Test]
        public void TestModelDefinitionsFileWriter_F3File()
        {
            var content = (IDelftIniCategory) TypeUtils.CallPrivateStaticMethod(typeof(ModelDefinitionFileWriter), "GenerateFilesRegion", true, true, null);
            var f3FileProperty = content.Properties.FirstOrDefault(p => p.Name == ModelDefinitionsRegion.DispersionF3File.Key);
            Assert.NotNull(f3FileProperty);
            Assert.AreEqual(SpatialDataFileNames.DispersionF3, f3FileProperty.Value);
        }

        [Test]
        public void TestModelDefinitionsFileWriter_F4File()
        {
            var content = (IDelftIniCategory) TypeUtils.CallPrivateStaticMethod(typeof(ModelDefinitionFileWriter), "GenerateFilesRegion", true, true, null);
            var f4FileProperty = content.Properties.FirstOrDefault(p => p.Name == ModelDefinitionsRegion.DispersionF4File.Key);
            Assert.NotNull(f4FileProperty);
            Assert.AreEqual(SpatialDataFileNames.DispersionF4, f4FileProperty.Value);
        }
        
        [Test]
        public void TestModelDefinitionsFileWriter_GlobalValues_1()
        {
            // water level (no initial discharge/initial salinity / dispersion)
            WaterFlowModel1D waterFlowModel1D = new WaterFlowModel1D
            {
                InitialConditionsType = InitialConditionsType.WaterLevel,
                DefaultInitialWaterLevel = 17.0,
                DefaultInitialDepth = 7.0
            };
            var targetDirectory = Path.Combine(Environment.CurrentDirectory, FileWriterTestHelper.RelativeTargetDirectory);
            var modelDefinitionFile = Path.Combine(targetDirectory, ModelFileNames.ModelDefinitionFilename);

            ModelDefinitionFileWriter.WriteFile(modelDefinitionFile, waterFlowModel1D);
            string resultFileContent = File.ReadAllText(modelDefinitionFile).Replace(" ","");
            Assert.IsFalse(resultFileContent.Contains("UseInitialWaterDepth"));
            Assert.IsTrue(resultFileContent.Contains("InitialWaterLevel=17"));
            Assert.IsTrue(resultFileContent.Contains("InitialWaterDepth=7"));
            Assert.IsTrue(resultFileContent.Contains("InitialDischarge=0"));
            Assert.IsFalse(resultFileContent.Contains("InitialSalinity="));
            //Assert.IsFalse(resultFileContent.Contains("Dispersion=")); // test can't be done anymore because on result ResultsBranches we set this parameter.
            //Changed to this to check the dispersion for global values:
            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(modelDefinitionFile);
            Assert.AreEqual(1, categories.Count(g => g.Name == ModelDefinitionsRegion.GlobalValuesHeader));
            var contentGlobalValues = categories.Where(c => c.Name == ModelDefinitionsRegion.GlobalValuesHeader).ToList().First();
            var dispersionPropertyInGlobalValues = contentGlobalValues.Properties.FirstOrDefault(p => p.Name == ModelDefinitionsRegion.Dispersion.Key);
            Assert.AreEqual(null, dispersionPropertyInGlobalValues); //does the same test, but now as we agreed to write our tests...

            // water depth instead of water level
            waterFlowModel1D.InitialConditionsType = InitialConditionsType.Depth;
            ModelDefinitionFileWriter.WriteFile(modelDefinitionFile, waterFlowModel1D);
            resultFileContent = File.ReadAllText(modelDefinitionFile).Replace(" ", "");
            Assert.IsTrue(resultFileContent.Contains("UseInitialWaterDepth=1"));
        }

        [Test]
        public void TestModelDefinitionsFileWriter_GlobalValues_2()
        {
            // initial salinity / dispersion
            WaterFlowModel1D waterFlowModel1D = new WaterFlowModel1D();
            waterFlowModel1D.UseSalt = true;
            waterFlowModel1D.InitialSaltConcentration.DefaultValue = 23;
            waterFlowModel1D.DispersionCoverage.DefaultValue = 41;

            var targetPath = Path.Combine(Environment.CurrentDirectory, FileWriterTestHelper.RelativeTargetDirectory);
            var modelDefinitionFile = Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename);

            ModelDefinitionFileWriter.WriteFile(modelDefinitionFile, waterFlowModel1D);
            string resultFileContent = File.ReadAllText(modelDefinitionFile).Replace(" ", "");
            Assert.IsTrue(resultFileContent.Contains("InitialSalinity=23"));
            Assert.IsTrue(resultFileContent.Contains("Dispersion=41"));
        }
        
        [Test]
        public void TestModelDefinitionFileWriter_TimeValues()
        {
            WaterFlowModel1D waterFlowModel1D = new WaterFlowModel1D();
            waterFlowModel1D.StartTime = new DateTime(2015, 12, 1, 12, 0, 0);
            waterFlowModel1D.StopTime = new DateTime(2015, 12, 2, 12, 0, 0);

            var timeStep = new TimeSpan(0, 1, 0, 0);
            waterFlowModel1D.TimeStep = timeStep;
            waterFlowModel1D.OutputSettings.GridOutputTimeStep = timeStep;
            waterFlowModel1D.OutputSettings.StructureOutputTimeStep = timeStep;

            var targetPath = Path.Combine(Environment.CurrentDirectory, FileWriterTestHelper.RelativeTargetDirectory);
            var modelDefinitionFile = Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename);

            ModelDefinitionFileWriter.WriteFile(modelDefinitionFile, waterFlowModel1D);
            string resultFileContent = File.ReadAllText(modelDefinitionFile).Replace(" ", "");

            var formattedStartTime = waterFlowModel1D.StartTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture).Replace(" ", "");
            Assert.IsTrue(resultFileContent.Contains("StartTime=" + formattedStartTime));

            var formattedStopTime = waterFlowModel1D.StopTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture).Replace(" ", "");
            Assert.IsTrue(resultFileContent.Contains("StopTime=" + formattedStopTime));

            Assert.IsTrue(resultFileContent.Contains("TimeStep=" + timeStep.TotalSeconds.ToString(ModelDefinitionsRegion.TimeStep.Format, CultureInfo.InvariantCulture)));
            Assert.IsTrue(resultFileContent.Contains("MapOutputTimeStep=" + timeStep.TotalSeconds.ToString(ModelDefinitionsRegion.MapOutputTimeStep.Format, CultureInfo.InvariantCulture)));
            Assert.IsTrue(resultFileContent.Contains("HisOutputTimeStep=" + timeStep.TotalSeconds.ToString(ModelDefinitionsRegion.HisOutputTimeStep.Format, CultureInfo.InvariantCulture)));
        }

        [Test]
        public void TestModelDefinitionFileWriter_InitialConditions()
        {
            var waterFlowModel1D = new WaterFlowModel1D();

            // Retrieve values from Model
            var expectedValue = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.InitialEmptyWells.Key);

            // Write Md1d File
            var targetPath = Path.Combine(Environment.CurrentDirectory, FileWriterTestHelper.RelativeTargetDirectory);
            var modelDefinitionFile = Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename);
            ModelDefinitionFileWriter.WriteFile(modelDefinitionFile, waterFlowModel1D);

            // Read Md1d File
            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(modelDefinitionFile);

            // Check category is present in file
            var content = categories.Where(c => c.Name == ModelDefinitionsRegion.InitialConditionsValuesHeader).ToList().First();
            Assert.NotNull(content);

            // Check values are present in category
            var initialEmptyWellsProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.InitialEmptyWells.Key);
            Assert.AreEqual(Convert.ToBoolean(expectedValue) ? "1" : "0", initialEmptyWellsProperty.Value);
        }

        [Test]
        public void TestModelDefinitionFileWriter_ResultsNodes()
        {
            var waterFlowModel1D = new WaterFlowModel1D();

            // Retrieve values from Model
           
            // Write Md1d File
            var targetPath = Path.Combine(Environment.CurrentDirectory, FileWriterTestHelper.RelativeTargetDirectory);
            var modelDefinitionFile = Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename);
            ModelDefinitionFileWriter.WriteFile(modelDefinitionFile, waterFlowModel1D);

            // Read Md1d File
            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(modelDefinitionFile);

            // Check category is present in file
            var content = categories.FirstOrDefault(c => c.Name == ModelDefinitionsRegion.ResultsNodesHeader);
            Assert.NotNull(content);

            // TODO: LevelFromStreetLevel (not implemented yet)

            // TODO: RunOff (not implemented yet)
            
            // TODO: TimeWaterOnStreet (not implemented yet)

            // TODO: VolumeError (not implemented yet)

            // TODO: VolumesOnStreet (not implemented yet)

            // TODO: WaterOnStreet (not implemented yet)
        }

        [Test]
        public void TestModelDefinitionFileWriter_ResultsBranches()
        {
            var waterFlowModel1D = new WaterFlowModel1D();

            // Write Md1d File
            var targetPath = Path.Combine(Environment.CurrentDirectory, FileWriterTestHelper.RelativeTargetDirectory);
            var modelDefinitionFile = Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename);
            ModelDefinitionFileWriter.WriteFile(modelDefinitionFile, waterFlowModel1D);

            // Read Md1d File
            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(modelDefinitionFile);

            // Check category is present in file
            var content = categories.FirstOrDefault(c => c.Name == ModelDefinitionsRegion.ResultsBranchesHeader);
            Assert.NotNull(content);
            // TODO: EnergyHeadMethod (not implemented yet)

            // TODO: Fwind (not implemented yet)

            // TODO: InfiltrationPipes (not implemented yet)

            // TODO: Levelsoutputonpipes (not implemented yet)

            // TODO: SedimentFrijlink (not implemented yet)

            // TODO: SedimentVanRijn (not implemented yet)
            
            // TODO: Twind (not implemented yet)
        }

        [Test]
        public void TestModelDefinitionFileWriter_ResultsStructures()
        {
            var waterFlowModel1D = new WaterFlowModel1D();

            var crestLevelEngineParameter = waterFlowModel1D.OutputSettings.EngineParameters.FirstOrDefault(
                ep => ep.ElementSet == ElementSet.Structures && ep.QuantityType == QuantityType.CrestLevel);
            Assert.NotNull(crestLevelEngineParameter);
            crestLevelEngineParameter.AggregationOptions = AggregationOptions.Current;

            var crestWidthEngineParameter = waterFlowModel1D.OutputSettings.EngineParameters.FirstOrDefault(
                ep => ep.ElementSet == ElementSet.Structures && ep.QuantityType == QuantityType.CrestWidth);
            Assert.NotNull(crestWidthEngineParameter);
            crestWidthEngineParameter.AggregationOptions = AggregationOptions.Current;

            var gateLowerEdgeLevelEngineParameter = waterFlowModel1D.OutputSettings.EngineParameters.FirstOrDefault(
                ep => ep.ElementSet == ElementSet.Structures && ep.QuantityType == QuantityType.GateLowerEdgeLevel);
            Assert.NotNull(gateLowerEdgeLevelEngineParameter);
            gateLowerEdgeLevelEngineParameter.AggregationOptions = AggregationOptions.Current;

            var gateOpeningHeightEngineParameter = waterFlowModel1D.OutputSettings.EngineParameters.FirstOrDefault(
                ep => ep.ElementSet == ElementSet.Structures && ep.QuantityType == QuantityType.GateOpeningHeight);
            Assert.NotNull(gateOpeningHeightEngineParameter);
            gateOpeningHeightEngineParameter.AggregationOptions = AggregationOptions.Current;
            
            // Write Md1d File
            var targetPath = Path.Combine(Environment.CurrentDirectory, FileWriterTestHelper.RelativeTargetDirectory);
            var modelDefinitionFile = Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename);
            ModelDefinitionFileWriter.WriteFile(modelDefinitionFile, waterFlowModel1D);

            // Read Md1d File
            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(modelDefinitionFile);

            // Check category is present in file
            var content = categories.FirstOrDefault(c => c.Name == "ResultsStructures");
            Assert.NotNull(content);

            // Check values are present in category
            var crestLevelProperty = content.Properties.FirstOrDefault(p => p.Name == QuantityType.CrestLevel.ToString());
            Assert.AreEqual(Enum.GetName(typeof(AggregationOptions), AggregationOptions.Current), crestLevelProperty.Value);

            var crestWidthProperty = content.Properties.FirstOrDefault(p => p.Name == QuantityType.CrestWidth.ToString());
            Assert.AreEqual(Enum.GetName(typeof(AggregationOptions), AggregationOptions.Current), crestWidthProperty.Value);

            var gateLowerEdgeLevelProperty = content.Properties.FirstOrDefault(p => p.Name == QuantityType.GateLowerEdgeLevel.ToString());
            Assert.AreEqual(Enum.GetName(typeof(AggregationOptions), AggregationOptions.Current), gateLowerEdgeLevelProperty.Value);

            var openingHeightProperty = content.Properties.FirstOrDefault(p => p.Name == QuantityType.GateOpeningHeight.ToString());
            Assert.AreEqual(Enum.GetName(typeof(AggregationOptions), AggregationOptions.Current), openingHeightProperty.Value);
        }

        [TestCase(QuantityType.SuctionSideLevel)]
        [TestCase(QuantityType.DeliverySideLevel)]
        [TestCase(QuantityType.PumpHead)]
        [TestCase(QuantityType.ActualPumpStage)]
        [TestCase(QuantityType.PumpCapacity)]
        [TestCase(QuantityType.ReductionFactor)]
        [TestCase(QuantityType.PumpDischarge)]
        public void TestModelDefinitionFileWriter_ResultsPumps(QuantityType quantityType)
        {
            var waterFlowModel1D = new WaterFlowModel1D();

            var pumpDischargeEngineParameter = waterFlowModel1D.OutputSettings.EngineParameters.FirstOrDefault(
                ep => ep.ElementSet == ElementSet.Pumps && ep.QuantityType == quantityType);
            Assert.NotNull(pumpDischargeEngineParameter);
            pumpDischargeEngineParameter.AggregationOptions = AggregationOptions.Current;

            // Write Md1d File
            var targetPath = Path.Combine(Environment.CurrentDirectory, FileWriterTestHelper.RelativeTargetDirectory);
            var modelDefinitionFile = Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename);
            ModelDefinitionFileWriter.WriteFile(modelDefinitionFile, waterFlowModel1D);

            // Read Md1d File
            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(modelDefinitionFile);

            // Check category is present in file
            var content = categories.FirstOrDefault(c => c.Name == "ResultsPumps");
            Assert.NotNull(content);

            // Check values are present in category
            var pumpDischargeProperty = content.Properties.First(p => p.Name == quantityType.ToString());
            Assert.AreEqual(Enum.GetName(typeof(AggregationOptions), AggregationOptions.Current), pumpDischargeProperty.Value);
        }

        [Test]
        public void TestModelDefinitionFileWriter_ResultsWaterBalance()
        {
            var waterFlowModel1D = new WaterFlowModel1D();

            // Retrieve values from Model
            var pumpResultsEngineParameter = waterFlowModel1D.OutputSettings.EngineParameters.FirstOrDefault(
                ep => ep.ElementSet == ElementSet.ModelWide && ep.QuantityType == QuantityType.BalVolume);
            Assert.NotNull(pumpResultsEngineParameter);
            pumpResultsEngineParameter.AggregationOptions = AggregationOptions.Current;

            // Write Md1d File
            var targetPath = Path.Combine(Environment.CurrentDirectory, FileWriterTestHelper.RelativeTargetDirectory);
            var modelDefinitionFile = Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename);
            ModelDefinitionFileWriter.WriteFile(modelDefinitionFile, waterFlowModel1D);

            // Read Md1d File
            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(modelDefinitionFile);

            // Check category is present in file
            var content = categories.FirstOrDefault(c => c.Name == "ResultsWaterBalance");
            Assert.NotNull(content);

            // Check values are present in category
            var balVolumeProperty = content.Properties.First(p => p.Name == QuantityType.BalVolume.ToString());
            Assert.AreEqual(Enum.GetName(typeof(AggregationOptions), AggregationOptions.Current), balVolumeProperty.Value);
        }

        [Test]
        public void TestModelDefinitionFileWriter_Sediment()
        {
            var waterFlowModel1D = new WaterFlowModel1D {D50 = 0.0006, D90 = 0.004, DepthUsedForSediment = 0.02};

            // Write Md1d File
            var targetPath = Path.Combine(Environment.CurrentDirectory, FileWriterTestHelper.RelativeTargetDirectory);
            var modelDefinitionFile = Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename);
            ModelDefinitionFileWriter.WriteFile(modelDefinitionFile, waterFlowModel1D);

            // Read Md1d File
            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(modelDefinitionFile);

            // Check category is present in file
            var content = categories.Where(c => c.Name == ModelDefinitionsRegion.SedimentValuesHeader).ToList().First();
            Assert.NotNull(content);

            // Check values are present in category
            var d50 = content.Properties.First(p => p.Name == ModelDefinitionsRegion.D50.Key);
            Assert.AreEqual("0.0006", d50.Value);
            var d90 = content.Properties.First(p => p.Name == ModelDefinitionsRegion.D90.Key);
            Assert.AreEqual("0.004", d90.Value);
            var depthUsedForSediment = content.Properties.First(p => p.Name == ModelDefinitionsRegion.DepthUsedForSediment.Key);
            Assert.AreEqual("0.02", depthUsedForSediment.Value);
        }

        [Test]
        public void TestModelDefinitionFileWriter_Specials()
        {
            var waterFlowModel1D = new WaterFlowModel1D(){DesignFactorDlg = 1.5};

            // Write Md1d File
            var targetPath = Path.Combine(Environment.CurrentDirectory, FileWriterTestHelper.RelativeTargetDirectory);
            var modelDefinitionFile = Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename);
            ModelDefinitionFileWriter.WriteFile(modelDefinitionFile, waterFlowModel1D);

            // Read Md1d File
            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(modelDefinitionFile);

            // Check category is present in file
            var content = categories.Where(c => c.Name == ModelDefinitionsRegion.SpecialsValuesHeader).ToList().First();
            Assert.NotNull(content);

            // Check values are present in category
            var designFactorDlg = content.Properties.First(p => p.Name == ModelDefinitionsRegion.DesignFactorDlg.Key);
            Assert.AreEqual("1.5", designFactorDlg.Value);
        }

        [Test]
        public void TestModelDefinitionFileWriter_NumericalParameters()
        {
            var waterFlowModel1D = new WaterFlowModel1D();

            // Retrieve values from Model
            var expectedAccelerationTermFactor = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.AccelerationTermFactor.Key);
            var expectedAccurateVersusSpeed = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.AccurateVersusSpeed.Key);
            var expectedCourantNumber = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.CourantNumber.Key);
            var expectedDtMinimum = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.DtMinimum.Key);
            var expectedEpsilonValueVolume = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.EpsilonValueVolume.Key);
            var expectedEpsilonValueWaterDepth = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.EpsilonValueWaterDepth.Key);
            var expectedGravity = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.Gravity.Key);
            var expectedMaxDegree = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.MaxDegree.Key);
            var expectedMaxIterations = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.MaxIterations.Key);
            var expectedMinimumSurfaceatStreet = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.MinimumSurfaceatStreet.Key);
            var expectedMinimumSurfaceinNode = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.MinimumSurfaceinNode.Key);
            var expectedMinimumLength = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.MinimumLength.Key);
            var expectedRelaxationFactor = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.RelaxationFactor.Key);
            var expectedRho = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.Rho.Key);
            var expectedStructureInertiaDampingFactor = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.StructureInertiaDampingFactor.Key);
            var expectedTheta = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.Theta.Key);
            var expectedThresholdValueFlooding = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.ThresholdValueFlooding.Key);
            var expectedUseTimeStepReducerStructures = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.UseTimeStepReducerStructures.Key);

            // Write Md1d File
            var targetPath = Path.Combine(Environment.CurrentDirectory, FileWriterTestHelper.RelativeTargetDirectory);
            var modelDefinitionFile = Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename);
            ModelDefinitionFileWriter.WriteFile(modelDefinitionFile, waterFlowModel1D);

            // Read Md1d File
            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(modelDefinitionFile);

            // Check category is present in file
            var content = categories.Where(c => c.Name == ModelDefinitionsRegion.NumericalParametersValuesHeader).ToList().First();
            Assert.NotNull(content);

            // Check values are present in category
            var accelerationTermFactorProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.AccelerationTermFactor.Key);
            Assert.AreEqual(expectedAccelerationTermFactor, accelerationTermFactorProperty.Value);

            var accurateVersusSpeedProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.AccurateVersusSpeed.Key);
            Assert.AreEqual(expectedAccurateVersusSpeed, accurateVersusSpeedProperty.Value);

            var courantNumberProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.CourantNumber.Key);
            Assert.AreEqual(expectedCourantNumber, courantNumberProperty.Value);

            var dtMinimumProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.DtMinimum.Key);
            Assert.AreEqual(expectedDtMinimum, dtMinimumProperty.Value);

            var epsilonValueVolumeProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.EpsilonValueVolume.Key);
            Assert.AreEqual(expectedEpsilonValueVolume, epsilonValueVolumeProperty.Value);

            var epsilonValueWaterDepthProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.EpsilonValueWaterDepth.Key);
            Assert.AreEqual(expectedEpsilonValueWaterDepth, epsilonValueWaterDepthProperty.Value);

            // TODO: FloodingDividedByDrying (not implemented yet)

            var gravityProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.Gravity.Key);
            Assert.AreEqual(expectedGravity, gravityProperty.Value);

            var maxDegreeProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.MaxDegree.Key);
            Assert.AreEqual(expectedMaxDegree, maxDegreeProperty.Value);

            var maxIterationsProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.MaxIterations.Key);
            Assert.AreEqual(expectedMaxIterations, maxIterationsProperty.Value);

            // TODO: MaxTimeStep (not implemented yet)

            var minimumSurfaceatStreetProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.MinimumSurfaceatStreet.Key);
            Assert.AreEqual(expectedMinimumSurfaceatStreet, minimumSurfaceatStreetProperty.Value);

            var minimumSurfaceinNodeProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.MinimumSurfaceinNode.Key);
            Assert.AreEqual(expectedMinimumSurfaceinNode, minimumSurfaceinNodeProperty.Value);

            var minimumLengthProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.MinimumLength.Key);
            Assert.AreEqual(expectedMinimumLength, minimumLengthProperty.Value);

            var relaxationFactorProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.RelaxationFactor.Key);
            Assert.AreEqual(expectedRelaxationFactor, relaxationFactorProperty.Value);

            var rhoProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.Rho.Key);
            Assert.AreEqual(expectedRho, rhoProperty.Value);

            var structureInertiaDampingFactorProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.StructureInertiaDampingFactor.Key);
            Assert.AreEqual(expectedStructureInertiaDampingFactor, structureInertiaDampingFactorProperty.Value);

            var thetaProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.Theta.Key);
            Assert.AreEqual(expectedTheta, thetaProperty.Value);

            var thresholdValueFloodingProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.ThresholdValueFlooding.Key);
            Assert.AreEqual(expectedThresholdValueFlooding, thresholdValueFloodingProperty.Value);

            // TODO: UseOmp (not implemented yet)

            var useTimeStepReducerStructuresProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.UseTimeStepReducerStructures.Key);
            Assert.AreEqual(Convert.ToBoolean(expectedUseTimeStepReducerStructures) ? "1" : "0", useTimeStepReducerStructuresProperty.Value);
        }

        [Test]
        public void TestModelDefinitionFileWriter_SimulationOptions()
        {
            var waterFlowModel1D = new WaterFlowModel1D();
            
            // Retrieve values from Model
            var expectedDebug = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.Debug.Key);
            var expectedDebugTime = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.DebugTime.Key);
            var expectedDispMaxFactor = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.DispMaxFactor.Key);
            var expectedDumpInput = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.DumpInput.Key);
            var expectedIadvec1D = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.Iadvec1D.Key);
            var expectedLimtyphu1D = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.Limtyphu1D.Key);
            var expectedMomdilution1D = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.Momdilution1D.Key);
            var expectedMorphology = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.Morphology.Key);
            var expectedTimersOutputFrequency = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.TimersOutputFrequency.Key);
            var expectedUseTimers = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.UseTimers.Key);
            
            // Write Md1d File
            var targetPath = Path.Combine(Environment.CurrentDirectory, FileWriterTestHelper.RelativeTargetDirectory);
            var modelDefinitionFile = Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename);
            ModelDefinitionFileWriter.WriteFile(modelDefinitionFile, waterFlowModel1D);

            // Read Md1d File
            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(modelDefinitionFile);

            // Check category is present in file
            var content = categories.Where(c => c.Name == ModelDefinitionsRegion.SimulationOptionsValuesHeader).ToList().First();
            Assert.NotNull(content);

            // Check values are present in category

            // TODO: allowablelargertimestep (not implemented yet)

            // TODO: allowabletimesteplimiter (not implemented yet)

            // TODO: AllowableVolumeError (not implemented yet)

            // TODO: AllowCrestLevelBelowBottom (not implemented yet)

            // TODO: Cflcheckalllinks (not implemented yet)

            // TODO: Channel (not implemented yet)

            // TODO: CheckFuru (not implemented yet)

            // TODO: CheckFuruMode (not implemented yet)

            var debugProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.Debug.Key);
            Assert.AreEqual(Convert.ToBoolean(expectedDebug) ? "1" : "0", debugProperty.Value);

            var debugTimeProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.DebugTime.Key);
            Assert.AreEqual(expectedDebugTime, debugTimeProperty.Value);

            // TODO: DepthsBelowBobs (not implemented yet)

            var dispMaxFactorProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.DispMaxFactor.Key);
            Assert.AreEqual(expectedDispMaxFactor, dispMaxFactorProperty.Value);

            var dumpInputProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.DumpInput.Key);
            Assert.AreEqual(Convert.ToBoolean(expectedDumpInput) ? "1" : "0", dumpInputProperty.Value);

            var iadvec1DProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.Iadvec1D.Key);
            Assert.AreEqual(expectedIadvec1D, iadvec1DProperty.Value);

            // TODO: Jchecknans (not implemented yet)

            // TODO: Junctionadvection (not implemented yet)

            // TODO: LaboratoryTest (not implemented yet)

            // TODO: LaboratoryTimeStep (not implemented yet)

            // TODO: LaboratoryTotalStep (not implemented yet)

            var limtyphu1DProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.Limtyphu1D.Key);
            Assert.AreEqual(expectedLimtyphu1D, limtyphu1DProperty.Value);

            // TODO: LoggingLevel (not implemented yet)

            // TODO: Manhloss (not implemented yet)

            // TODO: ManholeLosses (not implemented yet)

            // TODO: MissingValue (not implemented yet)

            var momdilution1DProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.Momdilution1D.Key);
            Assert.AreEqual(expectedMomdilution1D, momdilution1DProperty.Value);

            var morphologyProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.Morphology.Key);
            Assert.AreEqual(Convert.ToBoolean(expectedMorphology) ? "1" : "0", morphologyProperty.Value);

            // TODO: PreissmannMinClosedManholes (not implemented yet)

            // TODO: QDrestart (not implemented yet)

            // TODO: River (not implemented yet)

            // TODO: Sewer (not implemented yet)

            // TODO: SiphonUpstreamThresholdSwitchOff (not implemented yet)

            // TODO: StrucAlfa (not implemented yet)

            // TODO: StructureDynamicsFactor (not implemented yet)

            // TODO: StructureStabilityFactor (not implemented yet)

            // TODO: ThresholdForSummerDike (not implemented yet)

            var timersOutputFrequencyProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.TimersOutputFrequency.Key);
            Assert.AreEqual(expectedTimersOutputFrequency, timersOutputFrequencyProperty.Value);

            // TODO: use1d2dcoupling (not implemented yet)

            // TODO: UseEnergyHeadStructures (not implemented yet)
            
            var useTimersProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.UseTimers.Key);
            Assert.AreEqual(Convert.ToBoolean(expectedUseTimers) ? "1" : "0", useTimersProperty.Value);
            
            // TODO: Usevariableteta (not implemented yet)

            // TODO: VolumeCheck (not implemented yet)

            // TODO: VolumeCorrection (not implemented yet)

            // TODO: WaterQualityInUse (not implemented yet)

            var writeNetCdfProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.WriteNetCDF.Key);
            Assert.AreEqual("1", writeNetCdfProperty.Value);
        }

        [Test]
        public void TestModelDefinitionFileWriter_SimulationOptions_DontHaveRestartBools()
        {
            var waterFlowModel1D = new WaterFlowModel1D();
            
            // Write Md1d File
            var targetPath = Path.Combine(Environment.CurrentDirectory, FileWriterTestHelper.RelativeTargetDirectory);
            var modelDefinitionFile = Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename);
            ModelDefinitionFileWriter.WriteFile(modelDefinitionFile, waterFlowModel1D);

            // Read Md1d File
            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(modelDefinitionFile);

            // Check category is present in file
            var content = categories.Where(c => c.Name == ModelDefinitionsRegion.SimulationOptionsValuesHeader).ToList().First();
            Assert.NotNull(content);
            Assert.That(content.Properties.FirstOrDefault(p => p.Name == ModelDefinitionsRegion.UseRestart.Key), Is.Null);
            Assert.That(content.Properties.FirstOrDefault(p => p.Name == ModelDefinitionsRegion.WriteRestart.Key), Is.Null);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestModelDefinitionFileWriter_RestartOptions(bool useSaveStateTimeRange)
        {
            var waterFlowModel1D = new WaterFlowModel1D();
            waterFlowModel1D.SaveStateStartTime = new DateTime(2015, 12, 1, 12, 0, 0);
            waterFlowModel1D.SaveStateStopTime = new DateTime(2015, 12, 2, 12, 0, 0);

            var timeStep = new TimeSpan(0, 1, 0, 0);
            waterFlowModel1D.SaveStateTimeStep = timeStep;
            
            waterFlowModel1D.UseRestart = true;
            waterFlowModel1D.WriteRestart = true;

            // Retrieve values from Model
            var expectedUseRestart = true;
            var expectedWriteRestart = true;

            // Write Md1d File
            var targetPath = Path.Combine(Environment.CurrentDirectory, FileWriterTestHelper.RelativeTargetDirectory);
            var modelDefinitionFile = Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename);
            ModelDefinitionFileWriter.WriteFile(modelDefinitionFile, waterFlowModel1D);

            // Read Md1d File
            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(modelDefinitionFile);

            // Check category is present in file
            var content = categories.Where(c => c.Name == ModelDefinitionsRegion.RestartHeader).ToList().First();
            Assert.NotNull(content);
            
            var useRestartProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.UseRestart.Key);
            Assert.AreEqual(Convert.ToBoolean(expectedUseRestart) ? "1" : "0", useRestartProperty.Value);

            var writeRestartProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.WriteRestart.Key);
            Assert.AreEqual(Convert.ToBoolean(expectedWriteRestart) ? "1" : "0", writeRestartProperty.Value);
            if (Convert.ToBoolean(expectedWriteRestart))
            {
                var restartStartTime = content.Properties.First(p => p.Name == ModelDefinitionsRegion.RestartStartTime.Key);
                var formattedExpectedRestartStartTime = waterFlowModel1D.SaveStateStartTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                Assert.AreEqual(formattedExpectedRestartStartTime, restartStartTime.Value);

                var restartStopTime = content.Properties.First(p => p.Name == ModelDefinitionsRegion.RestartStopTime.Key);
                var formattedExpectedRestartStopTime = waterFlowModel1D.SaveStateStopTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture); 
                Assert.AreEqual(formattedExpectedRestartStopTime, restartStopTime.Value);

                var restartTimeStep = content.Properties.FirstOrDefault(p => p.Name == ModelDefinitionsRegion.RestartTimeStep.Key);
                Assert.AreEqual(int.Parse(waterFlowModel1D.SaveStateTimeStep.TotalSeconds.ToString(CultureInfo.InvariantCulture)).ToString(), restartTimeStep.Value);

            }
            else
            {
                Assert.That(content.Properties.FirstOrDefault(p => p.Name == ModelDefinitionsRegion.RestartStartTime.Key), Is.Null);
                Assert.That(content.Properties.FirstOrDefault(p => p.Name == ModelDefinitionsRegion.RestartStopTime.Key), Is.Null);
                Assert.That(content.Properties.FirstOrDefault(p => p.Name == ModelDefinitionsRegion.RestartTimeStep.Key), Is.Null);
            }
        }

        [Test]
        public void TestModelDefinitionFileWriter_TransportComputation()
        {
            var waterFlowModel1D = new WaterFlowModel1D();

            // retrieve values from Model
            var expectedUseTemperature = waterFlowModel1D.UseTemperature ? 1 : 0;
            var expectedDensity = waterFlowModel1D.DensityTypeParameter.Value;
            var expectedHeatTransferModel = waterFlowModel1D.TemperatureModelTypeParameter.Value;

            // Write Md1d File
            var targetPath = Path.Combine(Environment.CurrentDirectory, FileWriterTestHelper.RelativeTargetDirectory);
            var modelDefinitionFile = Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename);
            ModelDefinitionFileWriter.WriteFile(modelDefinitionFile, waterFlowModel1D);

            // Read Md1d File
            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(modelDefinitionFile);

            // Check category is present in file
            var content = categories.Where(c => c.Name == ModelDefinitionsRegion.TransportComputationValuesHeader).ToList().First();
            Assert.NotNull(content);

            // Check values are present in category
            var useTemperatureProperty =
                content.Properties.First(p => p.Name == ModelDefinitionsRegion.UseTemperature.Key);
            Assert.AreEqual(expectedUseTemperature.ToString(), useTemperatureProperty.Value);

            var densityProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.Density.Key);
            Assert.AreEqual(expectedDensity, densityProperty.Value);

            var heatTransferModelProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.HeatTransferModel.Key);
            Assert.AreEqual(expectedHeatTransferModel, heatTransferModelProperty.Value);
        }

        [Test]
        public void TestModelDefinitionFileWriter_AdvancedOptions()
        {
            var waterFlowModel1D = new WaterFlowModel1D();

            // Retrieve values from Model
            var expectedExtraResistanceGeneralStructure = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.ExtraResistanceGeneralStructure.Key);
            var expectedFillCulvertsWithGL = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.FillCulvertsWithGL.Key);
            var expectedLateralLocation = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.LateralLocation.Key);
            var expectedMaxLoweringCrossAtCulvert = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.MaxLoweringCrossAtCulvert.Key);
            var expectedMaxVolFact = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.MaxVolFact.Key);
            var expectedNoNegativeQlatWhenThereIsNoWater = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.NoNegativeQlatWhenThereIsNoWater.Key);
            var expectedTransitionHeightSD = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.TransitionHeightSD.Key);
            var expectedLatitude = waterFlowModel1D.Latitude;
            var expectedLongitude = waterFlowModel1D.Longitude;
            
            // Write Md1d File
            var targetPath = Path.Combine(Environment.CurrentDirectory, FileWriterTestHelper.RelativeTargetDirectory);
            var modelDefinitionFile = Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename);
            ModelDefinitionFileWriter.WriteFile(modelDefinitionFile, waterFlowModel1D);

            // Read Md1d File
            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(modelDefinitionFile);

            // Check category is present in file
            var content = categories.Where(c => c.Name == ModelDefinitionsRegion.AdvancedOptionsHeader).ToList().First();
            Assert.NotNull(content);

            // Check values are present in category

            // TODO: CalculateDelwaqOutput (not implemented yet)

            var extraResistanceGeneralStructureProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.ExtraResistanceGeneralStructure.Key);
            Assert.AreEqual(expectedExtraResistanceGeneralStructure, extraResistanceGeneralStructureProperty.Value);

            var fillCulvertsWithGL = content.Properties.First(p => p.Name == ModelDefinitionsRegion.FillCulvertsWithGL.Key);
            Assert.AreEqual(Convert.ToBoolean(expectedFillCulvertsWithGL) ? "1" : "0", fillCulvertsWithGL.Value);

            var lateralLocationProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.LateralLocation.Key);
            Assert.AreEqual(expectedLateralLocation, lateralLocationProperty.Value);

            var maxLoweringCrossAtCulvertProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.MaxLoweringCrossAtCulvert.Key);
            Assert.AreEqual(expectedMaxLoweringCrossAtCulvert, maxLoweringCrossAtCulvertProperty.Value);

            var maxVolFactProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.MaxVolFact.Key);
            Assert.AreEqual(expectedMaxVolFact, maxVolFactProperty.Value);

            var noNegativeQlatWhenThereIsNoWaterProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.NoNegativeQlatWhenThereIsNoWater.Key);
            Assert.AreEqual(Convert.ToBoolean(expectedNoNegativeQlatWhenThereIsNoWater) ? "1" : "0", noNegativeQlatWhenThereIsNoWaterProperty.Value);

            var transitionHeightSDProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.TransitionHeightSD.Key);
            Assert.AreEqual(expectedTransitionHeightSD, transitionHeightSDProperty.Value);
            var latitudeProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.Latitude.Key);
            Assert.AreEqual(expectedLatitude.ToString("e7", CultureInfo.InvariantCulture), latitudeProperty.Value);

            var LongitudeProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.Longitude.Key);
            Assert.AreEqual(expectedLongitude.ToString("e7", CultureInfo.InvariantCulture), LongitudeProperty.Value);
        }

        [Test]
        public void TestModelDefinitionFileWriter_Salinity()
        {
            var waterFlowModel1D = new WaterFlowModel1D();

            // Retrieve values from Model
            var expectedDiffusionAtBoundaries = SetModelParameter(waterFlowModel1D, ModelDefinitionsRegion.DiffusionAtBoundaries.Key);
            
            // Write Md1d File
            var targetPath = Path.Combine(Environment.CurrentDirectory, FileWriterTestHelper.RelativeTargetDirectory);
            var modelDefinitionFile = Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename);

            ModelDefinitionFileWriter.WriteFile(modelDefinitionFile, waterFlowModel1D);

            // Read Md1d File
            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(modelDefinitionFile);

            // Check category is present in file
            var content = categories.Where(c => c.Name == ModelDefinitionsRegion.SalinityValuesHeader).ToList().First();
            Assert.NotNull(content);

            // Check values are present in category
            var saltComputationProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.SaltComputation.Key);
            Assert.AreEqual(waterFlowModel1D.UseSaltInCalculation ? "1" : "0", saltComputationProperty.Value);

            var diffusionAtBoundariesrProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.DiffusionAtBoundaries.Key);
            Assert.AreEqual(Convert.ToBoolean(expectedDiffusionAtBoundaries) ? "1" : "0", diffusionAtBoundariesrProperty.Value);
        }

        [Test]
        public void TestModelDefinitionFileWriter_Temperature()
        {
            var waterFlowModel1D = new WaterFlowModel1D();

            // retrieve values from Model
            var expectedBackgroundTemperature = waterFlowModel1D.BackgroundTemperature;
            var expectedSurfaceArea = waterFlowModel1D.SurfaceArea;
            var expectedAtmosphericPressure = waterFlowModel1D.AtmosphericPressure;
            var expectedDaltonNumber = waterFlowModel1D.DaltonNumber;
            var expectedStantonNumber = waterFlowModel1D.StantonNumber;
            var expectedHeatCapacity = waterFlowModel1D.HeatCapacityWater;

            // Write Md1d File
            var targetPath = Path.Combine(Environment.CurrentDirectory, FileWriterTestHelper.RelativeTargetDirectory);
            var modelDefinitionFile = Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename);
            ModelDefinitionFileWriter.WriteFile(modelDefinitionFile, waterFlowModel1D);

            // Read Md1d File
            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(modelDefinitionFile);

            // Check category is present in file
            var content = categories.Where(c => c.Name == ModelDefinitionsRegion.TemperatureValuesHeader).ToList().First();
            Assert.NotNull(content);

            // Check values are present in category
            var backgroundTemperatureProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.BackgroundTemperature.Key);
            Assert.AreEqual(expectedBackgroundTemperature.ToString("e7", CultureInfo.InvariantCulture), backgroundTemperatureProperty.Value);

            var surfaceAreaProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.SurfaceArea.Key);
            Assert.AreEqual(expectedSurfaceArea.ToString("e7", CultureInfo.InvariantCulture), surfaceAreaProperty.Value);

            var atmosphericPressureProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.AtmosphericPressure.Key);
            Assert.AreEqual(expectedAtmosphericPressure.ToString("e7", CultureInfo.InvariantCulture), atmosphericPressureProperty.Value);

            var daltonNumberProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.DaltonNumber.Key);
            Assert.AreEqual(expectedDaltonNumber.ToString("e7", CultureInfo.InvariantCulture), daltonNumberProperty.Value);

            var stantonNumberProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.StantonNumber.Key);
            Assert.AreEqual(expectedStantonNumber.ToString("e7", CultureInfo.InvariantCulture), stantonNumberProperty.Value);

            var heatCapacityProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.HeatCapacity.Key);
            Assert.AreEqual(expectedHeatCapacity.ToString("e7", CultureInfo.InvariantCulture), heatCapacityProperty.Value);
        }

        [Test]
        public void TestModelDefinitionFileWriter_Morphology()
        {
            var waterFlowModel1D = new WaterFlowModel1D();

            // Set values in Model
            waterFlowModel1D.UseMorphology = !waterFlowModel1D.UseMorphology;
            waterFlowModel1D.AdditionalMorphologyOutput = !waterFlowModel1D.AdditionalMorphologyOutput;
            waterFlowModel1D.SedimentPath = "TestSediment.sed";
            waterFlowModel1D.MorphologyPath = "TestMorphology.mor";

            // Write Md1d File
            var targetPath = Path.Combine(Environment.CurrentDirectory, FileWriterTestHelper.RelativeTargetDirectory);
            var modelDefinitionFile = Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename);
            ModelDefinitionFileWriter.WriteFile(modelDefinitionFile, waterFlowModel1D);

            // Read Md1d File
            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(modelDefinitionFile);

            // Check category is present in file
            var content = categories.Where(c => c.Name == ModelDefinitionsRegion.MorphologyValuesHeader).ToList().First();
            Assert.NotNull(content);

            // Check values are present in category
            var calculateMorphologyProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.CalculateMorphology.Key);
            Assert.AreEqual(waterFlowModel1D.UseMorphology ? "1" : "0", calculateMorphologyProperty.Value);

            var additionalOutputProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.AdditionalOutput.Key);
            Assert.AreEqual(waterFlowModel1D.AdditionalMorphologyOutput ? "1" : "0", additionalOutputProperty.Value);

            // Files will be copied locally to the run dir
            var sedimentInputFileProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.SedimentInputFile.Key);
            Assert.AreEqual("TestSediment.sed", sedimentInputFileProperty.Value);

            var morphologyInputFileProperty = content.Properties.First(p => p.Name == ModelDefinitionsRegion.MorphologyInputFile.Key);
            Assert.AreEqual("TestMorphology.mor", morphologyInputFileProperty.Value); 
        }

        [Test]
        public void TestModelDefinitionFileWriter_OutputSettings()
        {
            var waterFlowModel1D = new WaterFlowModel1D();

            // Write Md1d File
            var targetPath = Path.Combine(Environment.CurrentDirectory, FileWriterTestHelper.RelativeTargetDirectory);
            var modelDefinitionFile = Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename);

            ModelDefinitionFileWriter.WriteFile(modelDefinitionFile, waterFlowModel1D);

            // Read Md1d File
            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(modelDefinitionFile);

            var listGridPoints = new List<string>()
            {
                "WaterLevel","WaterDepth","Volume","TotalArea","TotalWidth","Salinity","Density","Lateral1D2D","NegativeDepth","NoIteration", "Temperature"
            };

            var listReachSegments = new List<string>()
            {
                "Discharge","Velocity","FlowArea","FlowHydrad","FlowConv","FlowChezy","WaterLevelGradient","Froude","DischargeMain","ChezyMain","AreaMain",
                "WidthMain","HydradMain","DischargeFP1","ChezyFP1","AreaFP1","WidthFP1","HydradFP1","DischargeFP2","ChezyFP2","AreaFP2","WidthFP2","HydradFP2",
                "TimeStepEstimation"
            };
            var listStructures = new List<string>()
            {
                "Discharge","Velocity","FlowArea","WaterlevelUp","WaterlevelDown","Head","PressureDifference","WaterLevelAtCrest"
            };
            var listObsPoints = new List<string>()
            {
                "WaterLevel","WaterDepth","Discharge","Velocity","Salinity", "Temperature"
            };
            var listRetentions = new List<string>()
            {
                "WaterLevel", "Volume"
            };
            var listLatSources = new List<string>()
            {
                "ActualDischarge", "DefinedDischarge", "LateralDifference", "WaterLevel"
            };
            var listVolumeGrid = new List<string>()
            {
                "FiniteGridType"
            };
            var listSimulation = new List<string>()
            {
                "BalVolume","BalError","BalStorage","BalBoundariesIn","BalBoundariesOut","BalBoundariesTot","BalLatIn","BalLatOut",
                "BalLatTot","Bal2d1dIn","Bal2d1dOut","Bal2d1dTot"
            };

            checkAllPropertiesInCategory(listGridPoints, categories.FirstOrDefault(c => c.Name.Equals("ResultsNodes")));
            checkAllPropertiesInCategory(listReachSegments, categories.FirstOrDefault(c => c.Name.Equals("ResultsBranches")));
            checkAllPropertiesInCategory(listStructures, categories.FirstOrDefault(c => c.Name.Equals("ResultsStructures")));
            checkAllPropertiesInCategory(listObsPoints, categories.FirstOrDefault(c => c.Name.Equals("ResultsObservationPoints")));
            checkAllPropertiesInCategory(listRetentions, categories.FirstOrDefault(c => c.Name.Equals("ResultsRetentions")));
            checkAllPropertiesInCategory(listLatSources, categories.FirstOrDefault(c => c.Name.Equals("ResultsLaterals")));
            checkAllPropertiesInCategory(listVolumeGrid, categories.FirstOrDefault(c => c.Name.Equals("FiniteVolumeGridOnGridPoints")));
            checkAllPropertiesInCategory(listSimulation, categories.FirstOrDefault(c => c.Name.Equals("ResultsWaterBalance")));
        }

        private void checkAllPropertiesInCategory(List<string> listElements, DelftIniCategory category )
        {
            Assert.NotNull(category);
            var propertiesList = category.Properties;
            foreach (var element in listElements)
            {
                Assert.IsNotEmpty(propertiesList.Where(gp => gp.Name.Equals(element)).ToList());
            }
            Assert.That(propertiesList.Where(
                p => listElements.Contains(p.Name) 
                        && (p.Value.Equals(Enum.GetName(typeof(AggregationOptions), AggregationOptions.None)) || p.Value.Equals(Enum.GetName(typeof(AggregationOptions), AggregationOptions.Current)))).ToList().Count, Is.EqualTo(listElements.Count));
        }

        private string getCategoryName(string categoryName)
        {
            string newName = "";
            if (!WaterFlowModel1DOutputSettingData.CategoryMap.TryGetValue(categoryName, out newName))
            {
                return categoryName;
            }
            return newName;
        }

        private string SetModelParameter(WaterFlowModel1D model, string name, string category = null)
        {
            // add a ModelApiParameter to the model if one by the same name does not already exist
            var retVal = model.ParameterSettings.FirstOrDefault(ps => ps.Name.Equals(name) && (category == null || ps.Category.ToString().Equals(category)))
                ?? AddModelApiParameter(model, name, true);

            EngineParameter param = null;
            foreach (var ep in model.OutputSettings.EngineParameters)
            {
                if (getCategoryName(ep.ElementSet.ToString()).Equals(retVal.Category.ToString()) && ep.QuantityType.ToString().Equals(name))
                {
                    param = ep;
                    break;
                }
            }
            if (param != null)
            {
                param.AggregationOptions = Convert.ToBoolean(retVal.Value) ? AggregationOptions.Maximum : AggregationOptions.None; // Any will work as only 'None' will set it to 0, the rest to 1.
            }

            return retVal.Value;
        }

        private ModelApiParameter AddModelApiParameter(WaterFlowModel1D model, string name, bool value)
        {
            var parameter = new ModelApiParameter { Name = name, Value = value.ToString() };
            model.ParameterSettings.Add(parameter);
            return parameter;
        }
    }
}