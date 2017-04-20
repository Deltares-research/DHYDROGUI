using System;
using System.Linq;
using DelftTools.Functions;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport
{
    [TestFixture]
    public class SobekLateralSourcesDataImporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportLateralSourcesData()
        {
            var pathToSobekNetwork = TestHelper.GetDataDir() + @"\ReModels\JAMM2010.sbk\40\DEFTOP.1";
            var waterFlowModel1DModel = new WaterFlowModel1D("water flow 1d");

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowModel1DModel, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekLateralSourcesImporter(), new SobekLateralSourcesDataImporter() });

            importer.Import();

            Assert.IsNotNull(waterFlowModel1DModel.LateralSourceData);
            Assert.AreEqual(80, waterFlowModel1DModel.LateralSourceData.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ImportLateralSourcesDataOnExistingModel()
        {
            var pathToSobekNetwork = TestHelper.GetDataDir() + @"\ReModels\JAMM2010.sbk\40\DEFTOP.1";
            var waterFlowModel1DModel = new WaterFlowModel1D("water flow 1d");

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowModel1DModel, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekLateralSourcesImporter(), new SobekLateralSourcesDataImporter() });

            importer.Import();

            Assert.IsNotNull(waterFlowModel1DModel.LateralSourceData);
            Assert.AreEqual(80, waterFlowModel1DModel.LateralSourceData.Count);

            var timeSeries = (TimeSeries)waterFlowModel1DModel.LateralSourceData.First().Data;

            var n = timeSeries.Time.Values.Count;

            timeSeries[DateTime.Now] = 0.0;

            Assert.AreNotEqual(n,timeSeries.Time.Values.Count);

            importer.Import();

            Assert.IsNotNull(waterFlowModel1DModel.LateralSourceData);
            Assert.AreEqual(80, waterFlowModel1DModel.LateralSourceData.Count);

            timeSeries = (TimeSeries) waterFlowModel1DModel.LateralSourceData.First().Data;

            Assert.AreEqual(n, timeSeries.Time.Values.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportLateralSourcesWithDischarges()
        {
            string pathToSobekNetwork = TestHelper.GetDataDir() + @"\263_000.lit\1\NETWORK.TP";
            var flowModel1D = new WaterFlowModel1D();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, flowModel1D, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekLateralSourcesImporter(), new SobekLateralSourcesDataImporter() });

            importer.Import();

            Assert.AreEqual(0.1d, flowModel1D.LateralSourceData.First(x => x.Name.StartsWith(@"T4_x=0m")).Flow);
            Assert.AreEqual(-1.2d, flowModel1D.LateralSourceData.First(x => x.Name.StartsWith(@"T5_x=0m")).Flow);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportDiffuseLateralSourcesData_Testbench_272()
        {
            string pathToSobekNetwork = TestHelper.GetDataDir() + @"\272_000.lit\NETWORK.TP";
            var waterFlowModel1DModel = new WaterFlowModel1D("water flow 1d");

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowModel1DModel, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekLateralSourcesImporter(), new SobekLateralSourcesDataImporter() });

            importer.Import();

            Assert.IsNotNull(waterFlowModel1DModel.LateralSourceData);
            Assert.AreEqual(13, waterFlowModel1DModel.LateralSourceData.Count); // 2 * lateral source data not diffuse and 11 for diffuse:  All diffuse lateralsources are merged to one diffuse lateralsource per branch

            //not diffuse and constant
            var lsd_T7_Qlat1 = waterFlowModel1DModel.LateralSourceData.FirstOrDefault(lsd => lsd.Name.StartsWith("T7_Qlat1"));

            Assert.IsNotNull(lsd_T7_Qlat1);
            Assert.IsFalse(lsd_T7_Qlat1.Feature.IsDiffuse);
            Assert.AreEqual(WaterFlowModel1DLateralDataType.FlowConstant,lsd_T7_Qlat1.DataType);
            Assert.AreEqual(1.0, lsd_T7_Qlat1.Flow);

            var lsd_T7_Qlat2 = waterFlowModel1DModel.LateralSourceData.FirstOrDefault(lsd => lsd.Name.StartsWith("T7_Qlat2"));

            Assert.IsNotNull(lsd_T7_Qlat2);
            Assert.IsFalse(lsd_T7_Qlat2.Feature.IsDiffuse);
            Assert.AreEqual(WaterFlowModel1DLateralDataType.FlowConstant, lsd_T7_Qlat2.DataType);
            Assert.AreEqual(2.0, lsd_T7_Qlat2.Flow);

            //diffuse and constant
            var lsd_31 = waterFlowModel1DModel.LateralSourceData.FirstOrDefault(lsd => lsd.Name.StartsWith("3")); //diffuse lateral source 31 -> branch 8. All diffuse lateralsources are merged to one diffuse lateralsource per branch

            Assert.IsNotNull(lsd_31);
            Assert.AreEqual(WaterFlowModel1DLateralDataType.FlowConstant, lsd_31.DataType);
            Assert.AreEqual(1.0, lsd_31.Flow); //0.002 * lengthe 500 = 1 (m2/s/m -> m3/m)
        }
    }
}
