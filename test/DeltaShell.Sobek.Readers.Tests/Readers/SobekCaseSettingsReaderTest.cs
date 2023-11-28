using System;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekCaseSettingsReaderTest
    {
        private SobekCaseSettingsReader settingsReader;

        [SetUp]
        public void SetUp()
        {
            settingsReader = new SobekCaseSettingsReader();
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadCaseSettings()
        {
            var path = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowFMModelImporterTest).Assembly, @"SW_max_1.lit\3\SETTINGS.DAT");

            var sobekCaseSettings = settingsReader.GetSobekCaseSettings(path);

            Assert.AreEqual(new DateTime(2007, 1, 15, 1, 0, 0), sobekCaseSettings.StartTime);
            Assert.AreEqual(new DateTime(2007, 1, 17, 1, 0, 0), sobekCaseSettings.StopTime);
            Assert.AreEqual(new TimeSpan(0, 0, 10, 0), sobekCaseSettings.TimeStep);
            Assert.AreEqual(new TimeSpan(0, 1, 0, 0), sobekCaseSettings.OutPutTimeStep);

            Assert.IsFalse(sobekCaseSettings.FromNetter);
            Assert.IsTrue(sobekCaseSettings.FromValuesSelected);
            Assert.IsFalse(sobekCaseSettings.FromRestart);
            Assert.IsFalse(sobekCaseSettings.InitialLevel);
            Assert.IsTrue(sobekCaseSettings.InitialDepth);
            Assert.AreEqual(0.0, sobekCaseSettings.InitialFlowValue, 1.0e-6);
            Assert.AreEqual(2.0, sobekCaseSettings.InitialLevelValue, 1.0e-6);
            Assert.AreEqual(3.0, sobekCaseSettings.InitialDepthValue, 1.0e-6);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadCaseSettingsPoorMansFloat()
        {
            var path = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowFMModelImporterTest).Assembly, @"QHBound.lit\1\SETTINGS.DAT");

            var sobekCaseSettings = settingsReader.GetSobekCaseSettings(path);

            Assert.AreEqual(0.1, sobekCaseSettings.InitialDepthValue, 1.0e-6);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadCaseSettingsWithOptionalResultsPumps()
        {
            var path = TestHelper.GetTestFilePath("SETTINGS_with_resultspumps.DAT");

            var sobekCaseSettings = settingsReader.GetSobekCaseSettings(path);

            Assert.IsTrue(sobekCaseSettings.PumpResults);
        }

        [Test]
        public void ReadCorruptFile()
        {
            var path = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowFMModelImporterTest).Assembly, @"SW_max_1.lit\3\NETWORK.TP"); 
            Assert.Throws<FormatException>(() =>
            {
                settingsReader.GetSobekCaseSettings(path);
            });
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadCaseSettingsSteadyViaSettingsDatFile()
        {
            var path =TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowFMModelImporterTest).Assembly, @"steady.lit\2\SETTINGS.DAT");

            var sobekCaseSettings = settingsReader.GetSobekCaseSettings(path);
            //ramdon samples
            Assert.AreEqual(0.1, sobekCaseSettings.InitialDepthValue, 1.0e-6);
            Assert.AreEqual(2.0, sobekCaseSettings.CourantNumber, 1.0e-6);
            Assert.AreEqual(0.001, sobekCaseSettings.DtMinimum, 1.0e-6);


            Assert.AreEqual(0.0, sobekCaseSettings.InitialFlowValue, 1.0e-6);
            Assert.AreEqual(2.0, sobekCaseSettings.InitialLevelValue, 1.0e-6);
            Assert.AreEqual(0.1, sobekCaseSettings.InitialDepthValue, 1.0e-6);
            //support EmptyWells

            //data found in settings.dat.xls
            //simulation
            Assert.AreEqual(0, sobekCaseSettings.LateralLocation);                      // not in file; default
            Assert.AreEqual(false, sobekCaseSettings.NoNegativeQlatWhenThereIsNoWater); // not in file; default
            Assert.AreEqual(0.0, sobekCaseSettings.MaxLoweringCrossAtCulvert);
            //flow Parameters
            Assert.AreEqual(9.81, sobekCaseSettings.GravityAcceleration, 1.0e-6);
            Assert.AreEqual(1.0, sobekCaseSettings.Theta, 1.0e-6);
            Assert.AreEqual(1000.0, sobekCaseSettings.Rho, 1.0e-6);
            Assert.AreEqual(1.0, sobekCaseSettings.RelaxationFactor, 1.0e-6);
            Assert.AreEqual(2.0, sobekCaseSettings.CourantNumber, 1.0e-6);
            Assert.AreEqual(6, sobekCaseSettings.MaxDegree);
            Assert.AreEqual(8, sobekCaseSettings.MaxIterations);
            Assert.AreEqual(0.001, sobekCaseSettings.DtMinimum, 1.0e-6);
            Assert.AreEqual(0.0001, sobekCaseSettings.EpsilonValueVolume, 1.0e-6);
            Assert.AreEqual(0.0001, sobekCaseSettings.EpsilonValueWaterDepth, 1.0e-6);
            Assert.AreEqual(1.0, sobekCaseSettings.StructureDynamicsFactor, 1.0e-6);
            Assert.AreEqual(0.01, sobekCaseSettings.ThresholdValueFlooding, 1.0e-6);
            Assert.AreEqual(0.001, sobekCaseSettings.ThresholdValueFloodingFLS, 1.0e-6);
            Assert.AreEqual(1.0, sobekCaseSettings.MinimumLength, 1.0e-6);
            Assert.AreEqual(3, sobekCaseSettings.AccurateVersusSpeed);
            Assert.AreEqual(1.0, sobekCaseSettings.StructureInertiaDampingFactor, 1.0e-6);
            Assert.AreEqual(0.1, sobekCaseSettings.MinimumSurfaceinNode, 1.0e-6);
            Assert.AreEqual(0.1, sobekCaseSettings.MinimumSurfaceatStreet, 1.0e-6);
            Assert.AreEqual(0.0, sobekCaseSettings.ExtraResistanceGeneralStructure, 1.0e-6);
            Assert.AreEqual(1.0, sobekCaseSettings.AccelerationTermFactor, 1.0e-6);
            Assert.AreEqual(false, sobekCaseSettings.UseTimeStepReducerStructures);

            //ResultsGeneral
            Assert.AreEqual(true, sobekCaseSettings.ActualValue);
            Assert.AreEqual(false, sobekCaseSettings.MeanValue);
            Assert.AreEqual(false, sobekCaseSettings.MaximumValue);
            //ResultsNodes
            Assert.AreEqual(true, sobekCaseSettings.Freeboard);
            Assert.AreEqual(false, sobekCaseSettings.TotalArea); // not in file; default
            Assert.AreEqual(false, sobekCaseSettings.TotalWidth); // not in file; default
            Assert.AreEqual(false, sobekCaseSettings.Volume);
            Assert.AreEqual(true, sobekCaseSettings.WaterDepth);
            Assert.AreEqual(true, sobekCaseSettings.WaterLevelOnResultsNodes);
            Assert.AreEqual(false, sobekCaseSettings.LateralOnNodes);
            //ResultsBranches
            Assert.AreEqual(false, sobekCaseSettings.Chezy);
            Assert.AreEqual(false, sobekCaseSettings.Froude); // not in file; default
            Assert.AreEqual(false, sobekCaseSettings.RiverSubsectionParameters); // not in file; default
            Assert.AreEqual(false, sobekCaseSettings.WaterLevelSlope);
            Assert.AreEqual(false, sobekCaseSettings.Wind);
            Assert.AreEqual(true, sobekCaseSettings.DischargeOnResultsBranches);
            Assert.AreEqual(true, sobekCaseSettings.VelocityOnResultsBranches);
            //ResultsStructures
            Assert.AreEqual(true, sobekCaseSettings.CrestLevel);
            Assert.AreEqual(false, sobekCaseSettings.CrestWidth);
            Assert.AreEqual(true, sobekCaseSettings.GateLowerEdgeLevel);
            Assert.AreEqual(false, sobekCaseSettings.Head);
            Assert.AreEqual(false, sobekCaseSettings.OpeningsArea);
            Assert.AreEqual(false, sobekCaseSettings.PressureDifference);
            Assert.AreEqual(false, sobekCaseSettings.WaterlevelOnCrest);
            Assert.AreEqual(true, sobekCaseSettings.DischargeOnResultsStructures);
            Assert.AreEqual(false, sobekCaseSettings.VelocityOnResultsStructures);
            Assert.AreEqual(true, sobekCaseSettings.WaterLevelOnResultsStructures);
            //ResultsPumps
            Assert.AreEqual(false, sobekCaseSettings.PumpResults);
            //RiverOptions
            Assert.AreEqual(1.0, sobekCaseSettings.TransitionHeightSummerDike, 1.0e-6);

        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadCaseSettingsWithout_Iadvec1D_Limtyphu1D_Momdilution1D()
        {
            var path = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowFMModelImporterTest).Assembly, @"steady.lit\2\SETTINGS.DAT");

            var sobekCaseSettings = settingsReader.GetSobekCaseSettings(path);

            Assert.AreEqual(null, sobekCaseSettings.Iadvec1D);
            Assert.AreEqual(null, sobekCaseSettings.Limtyphu1D);
            Assert.AreEqual(null, sobekCaseSettings.Momdilution1D);

        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadCaseSettingsWith_Iadvec1D_Limtyphu1D_Momdilution1D()
        {
            var path = TestHelper.GetTestFilePath("SETTINGS_WITH_IADVEC1D.DAT");

            var sobekCaseSettings = settingsReader.GetSobekCaseSettings(path);

            Assert.AreEqual(2.0, sobekCaseSettings.Iadvec1D);
            Assert.AreEqual(2.0, sobekCaseSettings.Limtyphu1D);
            Assert.AreEqual(1.0, sobekCaseSettings.Momdilution1D);

        }

        [Test, Category(TestCategory.DataAccess)]
        public void ReadCaseSettingsWithWaterQualitySettings()
        {
            var path = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowFMModelImporterTest).Assembly, @"SobekModelWithWaterQualityData\Sobek212\Test\1\SETTINGS.DAT");

            var sobekCaseSettings = settingsReader.GetSobekCaseSettings(path);

            Assert.AreEqual(sobekCaseSettings.MeasurementFile, "");
            Assert.AreEqual(sobekCaseSettings.Fraction, true);
            Assert.AreEqual(sobekCaseSettings.PeriodFromEvent, true); // Note that this is read from the Simulation block, not from the Water Quality block
            Assert.AreEqual(sobekCaseSettings.HistoryOutputInterval, 2);
            Assert.AreEqual(sobekCaseSettings.BalanceOutputInterval, 1);
            Assert.AreEqual(sobekCaseSettings.HisPeriodFromSimulation, false);
            Assert.AreEqual(sobekCaseSettings.BalPeriodFromSimulation, true);
            Assert.AreEqual(sobekCaseSettings.PeriodFromFlow, true);
            Assert.AreEqual(sobekCaseSettings.ActiveProcess, true);
            Assert.AreEqual(sobekCaseSettings.UseOldQuantityResults, false);
            Assert.AreEqual(sobekCaseSettings.LumpProcessesContributions, true);
            Assert.AreEqual(sobekCaseSettings.LumpBoundaryContributions, true);
            Assert.AreEqual(sobekCaseSettings.SumOfMonitoringAreas, true);
            Assert.AreEqual(sobekCaseSettings.SuppressTimeDependentOutput, true);
            Assert.AreEqual(sobekCaseSettings.LumpInternalTransport, true);
            Assert.AreEqual(sobekCaseSettings.MapOutputInterval, 1);
            Assert.AreEqual(sobekCaseSettings.MapPeriodFromSimulation, true);
            Assert.AreEqual(sobekCaseSettings.OutputLocationsType, 0);
            Assert.AreEqual(sobekCaseSettings.OutputHisVarType , 0);
            Assert.AreEqual(sobekCaseSettings.OutputHisMapType , 0);
            Assert.AreEqual(sobekCaseSettings.SubstateFile, @"\SOBEK212\FIXED\DELWAQ\Eutrof_simple.0");
            Assert.AreEqual(sobekCaseSettings.SubstateFileOption, 0);
            Assert.AreEqual(sobekCaseSettings.UseTatcherHarlemanTimeLag, false);
            Assert.AreEqual(sobekCaseSettings.TatcherHarlemanTimeLag, 0);
        }
    }
}
