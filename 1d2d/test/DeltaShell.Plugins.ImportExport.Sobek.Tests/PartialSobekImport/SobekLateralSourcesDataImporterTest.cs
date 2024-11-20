using System;
using System.Linq;
using DelftTools.Functions;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.FMSuite.FlowFM;
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
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\JAMM2010.sbk\40\DEFTOP.1";
            var waterFlowFmModel = new WaterFlowFMModel("waterflowfm");

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowFmModel, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekLateralSourcesImporter(), new SobekLateralSourcesDataImporter() });

            importer.Import();

            Assert.IsNotNull(waterFlowFmModel.LateralSourcesData);
            Assert.AreEqual(80, waterFlowFmModel.LateralSourcesData.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ImportLateralSourcesDataOnExistingModel()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\JAMM2010.sbk\40\DEFTOP.1";
            var waterFlowModel1DModel = new WaterFlowFMModel("water flow fm");

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowModel1DModel, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekLateralSourcesImporter(), new SobekLateralSourcesDataImporter() });

            importer.Import();

            Assert.IsNotNull(waterFlowModel1DModel.LateralSourcesData);
            Assert.AreEqual(80, waterFlowModel1DModel.LateralSourcesData.Count);

            var timeSeries = (TimeSeries)waterFlowModel1DModel.LateralSourcesData.First().Data;

            var n = timeSeries.Time.Values.Count;

            timeSeries[DateTime.Now] = 0.0;

            Assert.AreNotEqual(n,timeSeries.Time.Values.Count);

            importer.Import();

            Assert.IsNotNull(waterFlowModel1DModel.LateralSourcesData);
            Assert.AreEqual(80, waterFlowModel1DModel.LateralSourcesData.Count);

            timeSeries = (TimeSeries) waterFlowModel1DModel.LateralSourcesData.First().Data;

            Assert.AreEqual(n, timeSeries.Time.Values.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportLateralSourcesWithDischarges()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\263_000.lit\1\NETWORK.TP";
            var waterFlowFmModel = new WaterFlowFMModel();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowFmModel, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekLateralSourcesImporter(), new SobekLateralSourcesDataImporter() });

            importer.Import();

            Assert.AreEqual(0.1d, waterFlowFmModel.LateralSourcesData.First(x => x.Name.StartsWith(@"T4_x=0m")).Flow);
            Assert.AreEqual(-1.2d, waterFlowFmModel.LateralSourcesData.First(x => x.Name.StartsWith(@"T5_x=0m")).Flow);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportDiffuseLateralSourcesData_Testbench_272()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\272_000.lit\NETWORK.TP";
            var waterFlowFmModel = new WaterFlowFMModel("waterflowfm");

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowFmModel, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekLateralSourcesImporter(), new SobekLateralSourcesDataImporter() });

            importer.Import();

            Assert.IsNotNull(waterFlowFmModel.LateralSourcesData);
            Assert.AreEqual(13, waterFlowFmModel.LateralSourcesData.Count); // 2 * lateral source data not diffuse and 11 for diffuse:  All diffuse lateralsources are merged to one diffuse lateralsource per branch

            //not diffuse and constant
            var lsd_T7_Qlat1 = waterFlowFmModel.LateralSourcesData.FirstOrDefault(lsd => lsd.Name.StartsWith("T7_Qlat1"));

            Assert.IsNotNull(lsd_T7_Qlat1);
            Assert.IsFalse(lsd_T7_Qlat1.Feature.IsDiffuse);
            Assert.AreEqual(Model1DLateralDataType.FlowConstant,lsd_T7_Qlat1.DataType);
            Assert.AreEqual(1.0, lsd_T7_Qlat1.Flow);

            var lsd_T7_Qlat2 = waterFlowFmModel.LateralSourcesData.FirstOrDefault(lsd => lsd.Name.StartsWith("T7_Qlat2"));

            Assert.IsNotNull(lsd_T7_Qlat2);
            Assert.IsFalse(lsd_T7_Qlat2.Feature.IsDiffuse);
            Assert.AreEqual(Model1DLateralDataType.FlowConstant, lsd_T7_Qlat2.DataType);
            Assert.AreEqual(2.0, lsd_T7_Qlat2.Flow);

            //diffuse and constant
            var lsd_31 = waterFlowFmModel.LateralSourcesData.FirstOrDefault(lsd => lsd.Name.StartsWith("3")); //diffuse lateral source 31 -> branch 8. All diffuse lateralsources are merged to one diffuse lateralsource per branch

            Assert.IsNotNull(lsd_31);
            Assert.AreEqual(Model1DLateralDataType.FlowConstant, lsd_31.DataType);
            Assert.AreEqual(1.0, lsd_31.Flow); //0.002 * lengthe 500 = 1 (m2/s/m -> m3/m)
        }
    }
}
