using System;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport
{
    [TestFixture]
    public class SobekSettingsImporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportSettingsRe()
        {
            var pathToSobekNetwork = TestHelper.GetDataDir() + @"\ReModels\LATERALS.sbk\2\DEFTOP.1";
            var waterFlowModel1DModel = new WaterFlowModel1D("water flow 1d");
            waterFlowModel1DModel.StopTime = waterFlowModel1DModel.StartTime;

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowModel1DModel, new IPartialSobekImporter[] { new SobekSettingsImporter() });

            importer.Import();

            Assert.Greater(waterFlowModel1DModel.StopTime,waterFlowModel1DModel.StartTime);
            Assert.AreEqual(waterFlowModel1DModel.OutputSettings.GridOutputTimeStep, waterFlowModel1DModel.OutputSettings.StructureOutputTimeStep);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportSettingsFlow()
        {
            var pathToSobekNetwork = TestHelper.GetDataDir() + @"\301_00.lit\2\NETWORK.TP";
            var waterFlowModel1DModel = new WaterFlowModel1D("water flow 1d");
            waterFlowModel1DModel.StopTime = waterFlowModel1DModel.StartTime;

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowModel1DModel, new IPartialSobekImporter[] { new SobekSettingsImporter() });

            importer.Import();

            // Assert that the output settings are imported correctly from settings.dat.  
            Assert.That(Equals(waterFlowModel1DModel.OutputSettings.GetEngineParameter(QuantityType.WaterLevel, ElementSet.GridpointsOnBranches).AggregationOptions, AggregationOptions.Current));
            Assert.That(Equals(waterFlowModel1DModel.OutputSettings.GetEngineParameter(QuantityType.Froude, ElementSet.ReachSegElmSet).AggregationOptions, AggregationOptions.None));
            Assert.That(Equals(waterFlowModel1DModel.OutputSettings.GetEngineParameter(QuantityType.Discharge,ElementSet.Structures).AggregationOptions,AggregationOptions.Current));
            Assert.That(Equals(waterFlowModel1DModel.OutputSettings.GetEngineParameter(QuantityType.WaterlevelUp, ElementSet.Structures).AggregationOptions, AggregationOptions.Current));
            Assert.That(Equals(waterFlowModel1DModel.OutputSettings.GetEngineParameter(QuantityType.WaterlevelDown, ElementSet.Structures).AggregationOptions, AggregationOptions.Current));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenSobek2FileBase_WhenImporting_ThenSaveStateTimePropertiesAreAsExpected()
        {
            // Given
            var pathToSobekNetwork = TestHelper.GetDataDir() + @"\301_00.lit\2\NETWORK.TP";
            var waterFlowModel1D = new WaterFlowModel1D("water flow 1d");
            var initialModelStopTime = waterFlowModel1D.StopTime;

            // When
            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowModel1D, new IPartialSobekImporter[] { new SobekSettingsImporter() });
            importer.Import();

            // Then
            Assert.That(waterFlowModel1D.StopTime, Is.Not.EqualTo(initialModelStopTime)); // Check that the importer actually changed the model stop time
            Assert.That(waterFlowModel1D.SaveStateStartTime, Is.EqualTo(waterFlowModel1D.StopTime));
            Assert.That(waterFlowModel1D.SaveStateStopTime, Is.EqualTo(waterFlowModel1D.StopTime));

            var oneDayTimeSpan = new TimeSpan(1, 0, 0, 0);
            Assert.That(waterFlowModel1D.SaveStateTimeStep, Is.EqualTo(oneDayTimeSpan));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportSettingsChezyBecomesConveyance()
        {
            var pathToSobekNetwork = TestHelper.GetDataDir() + @"\chezyBecomesConveyance\1\NETWORK.TP";
            var waterFlowModel1DModel = new WaterFlowModel1D("water flow 1d");
            waterFlowModel1DModel.StopTime = waterFlowModel1DModel.StartTime;

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowModel1DModel, new IPartialSobekImporter[] { new SobekSettingsImporter() });

            Assert.That(Equals(waterFlowModel1DModel.OutputSettings.GetEngineParameter(QuantityType.FlowConv, ElementSet.ReachSegElmSet).AggregationOptions, AggregationOptions.None));

            importer.Import();

            // Assert that the output setting for Chezys in SOBEK 2 is converted into Conveyance, TOOLS-22143
            // In SOBEK 2 file settings.dat: Chezy=-1
            // In SOBEK 3 model results on branches: Conveyance = Current
            Assert.That(Equals(waterFlowModel1DModel.OutputSettings.GetEngineParameter(QuantityType.FlowConv,ElementSet.ReachSegElmSet).AggregationOptions, AggregationOptions.Current));
        }
    }
}
