using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM;
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
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\LATERALS.sbk\2\DEFTOP.1";
            var waterFlowFmModel = new WaterFlowFMModel("waterflowfm");
            waterFlowFmModel.StopTime = waterFlowFmModel.StartTime;

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowFmModel, new IPartialSobekImporter[] { new SobekSettingsImporter() });

            importer.Import();

            // Assert.Greater(waterFlowFmModel.StopTime,waterFlowFmModel.StartTime);
            // Assert.AreEqual(waterFlowFmModel.OutputSettings.GridOutputTimeStep, waterFlowFmModel.OutputSettings.StructureOutputTimeStep);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportSettingsFlow()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\301_00.lit\2\NETWORK.TP";
            var waterFlowFmModel = new WaterFlowFMModel("waterflowfm");
            waterFlowFmModel.StopTime = waterFlowFmModel.StartTime;

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowFmModel, new IPartialSobekImporter[] { new SobekSettingsImporter() });

            importer.Import();

            // // Assert that the output settings are imported correctly from settings.dat.  
            // Assert.That(Equals(waterFlowFmModel.OutputSettings.GetEngineParameter(QuantityType.WaterLevel, ElementSet.GridpointsOnBranches).AggregationOptions, AggregationOptions.Current));
            // Assert.That(Equals(waterFlowFmModel.OutputSettings.GetEngineParameter(QuantityType.Froude, ElementSet.ReachSegElmSet).AggregationOptions, AggregationOptions.None));
            // Assert.That(Equals(waterFlowFmModel.OutputSettings.GetEngineParameter(QuantityType.Discharge,ElementSet.Structures).AggregationOptions,AggregationOptions.Current));
            // Assert.That(Equals(waterFlowFmModel.OutputSettings.GetEngineParameter(QuantityType.WaterlevelUp, ElementSet.Structures).AggregationOptions, AggregationOptions.Current));
            // Assert.That(Equals(waterFlowFmModel.OutputSettings.GetEngineParameter(QuantityType.WaterlevelDown, ElementSet.Structures).AggregationOptions, AggregationOptions.Current));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportSettingsChezyBecomesConveyance()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\chezyBecomesConveyance\1\NETWORK.TP";
            var waterFlowFmModel = new WaterFlowFMModel("waterflowfm");
            waterFlowFmModel.StopTime = waterFlowFmModel.StartTime;

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowFmModel, new IPartialSobekImporter[] { new SobekSettingsImporter() });

           // Assert.That(Equals(waterFlowFmModel.OutputSettings.GetEngineParameter(QuantityType.FlowConv, ElementSet.ReachSegElmSet).AggregationOptions, AggregationOptions.None));

            importer.Import();

            // Assert that the output setting for Chezys in SOBEK 2 is converted into Conveyance, TOOLS-22143
            // In SOBEK 2 file settings.dat: Chezy=-1
            // In SOBEK 3 model results on branches: Conveyance = Current
            // Assert.That(Equals(waterFlowFmModel.OutputSettings.GetEngineParameter(QuantityType.FlowConv,ElementSet.ReachSegElmSet).AggregationOptions, AggregationOptions.Current));
        }
    }
}
