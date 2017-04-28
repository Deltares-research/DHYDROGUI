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
    [Category(TestCategory.DataAccess)]
    public class SobekSaltImporterTest
    {
        [Test]
        [Category(TestCategory.Slow)]
        public void ImportSalt()
        {
            var pathToSobekNetwork = TestHelper.GetDataDir() + @"\ReModels\20110331_NDB.sbk\6\DEFTOP.1";
            var waterFlowModel1DModel = new WaterFlowModel1D("water flow 1d");
            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowModel1DModel, new IPartialSobekImporter[] { new SobekBranchesImporter(),new SobekLateralSourcesImporter(), new SobekBoundaryConditionsImporter(), new SobekLateralSourcesDataImporter(), new SobekSaltImporter() });

            importer.Import();

            Assert.IsNotNull(waterFlowModel1DModel.DispersionCoverage);
            
            var boundary = waterFlowModel1DModel.BoundaryConditions.FirstOrDefault(bc => bc.Node.Name == "1");
            
            Assert.IsNotNull(boundary);
            Assert.AreEqual(SaltBoundaryConditionType.Constant, boundary.SaltConditionType);
            Assert.AreEqual(31.0, boundary.SaltConcentrationConstant);

            Assert.IsTrue(waterFlowModel1DModel.UseSalt);
            Assert.IsTrue(waterFlowModel1DModel.UseSaltInCalculation);

            Assert.IsTrue(waterFlowModel1DModel.DispersionFormulationType == DispersionFormulationType.ThatcherHarleman);
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ImportSaltWithTatcherHarlemanF1F3F4()
        {
            var pathToSobekNetwork = TestHelper.GetDataDir() + @"\030_NDB_zout_grotere_DX.lit\3\Network.TP";
            var waterFlowModel1DModel = new WaterFlowModel1D("water flow 1d");
            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowModel1DModel, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekLateralSourcesImporter(), new SobekBoundaryConditionsImporter(), new SobekLateralSourcesDataImporter(), new SobekSaltImporter() });

            importer.Import();

            Assert.IsTrue(waterFlowModel1DModel.UseSalt);
            Assert.IsTrue(waterFlowModel1DModel.UseSaltInCalculation);
            Assert.IsTrue(waterFlowModel1DModel.DispersionFormulationType == DispersionFormulationType.ThatcherHarleman);

            var dispersion = waterFlowModel1DModel.DispersionCoverage;
            var dispersionF3 = waterFlowModel1DModel.DispersionF3Coverage;
            Assert.AreEqual(87, dispersionF3.Locations.Values.Count);

            var nl1 = dispersionF3.Locations.Values.First(l => l.Branch.Name == "R_10");
            var nl2 = dispersionF3.Locations.Values.First(l => l.Branch.Name == "R_47");
            var nl3 = dispersionF3.Locations.Values.First(l => l.Branch.Name == "R_87");

            Assert.AreEqual(new[] { 2500.0 }, dispersion.GetAllComponentValues(nl1));
            Assert.AreEqual(new[] { 100.0 }, dispersion.GetAllComponentValues(nl2));
            Assert.AreEqual(new[] { 50.0 }, dispersion.GetAllComponentValues(nl3));

            Assert.AreEqual(new[] { 0.0 }, dispersionF3.GetAllComponentValues(nl1));
            Assert.AreEqual(new[] { 1.0 }, dispersionF3.GetAllComponentValues(nl2));
            Assert.AreEqual(new[] { 0.0 }, dispersionF3.GetAllComponentValues(nl3));
        }

        [Test]
        public void ImportModelWithoutSalt()
        {
            var pathToSobekNetwork = TestHelper.GetDataDir() + @"\SW_max_1.lit\3\Network.TP";
            var waterFlowModel1DModel = new WaterFlowModel1D("water flow 1d");
            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowModel1DModel, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekLateralSourcesImporter(), new SobekBoundaryConditionsImporter(), new SobekLateralSourcesDataImporter(), new SobekSaltImporter() });

            importer.Import();

            Assert.IsFalse(waterFlowModel1DModel.UseSalt);
            Assert.IsFalse(waterFlowModel1DModel.UseSaltInCalculation);
            Assert.That(waterFlowModel1DModel.DispersionFormulationType == DispersionFormulationType.Constant);
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ImportModelWithSaltCheckSaltBoundariesAndSaltLaterals()
        {
            var pathToSobekNetwork = TestHelper.GetDataDir() + @"\301_00.lit\2\Network.TP";
            var waterFlowModel1DModel = new WaterFlowModel1D("water flow 1d");
            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowModel1DModel, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekLateralSourcesImporter(), new SobekBoundaryConditionsImporter(), new SobekLateralSourcesDataImporter(), new SobekSaltImporter() });

            importer.Import();

            Assert.IsTrue(waterFlowModel1DModel.UseSalt);
            Assert.IsTrue(waterFlowModel1DModel.UseSaltInCalculation);

            var saltBoundaries =
                waterFlowModel1DModel.BoundaryConditions.Where(b => b.UseSalt).ToList();
            Assert.AreEqual(2, saltBoundaries.Count());
            Assert.AreEqual(new[] { 33.0, 0.01 }, saltBoundaries.OrderBy(sb => sb.Id).Select(sb => sb.SaltConcentrationConstant).ToArray());

            var saltLaterals =
                waterFlowModel1DModel.LateralSourceData.Where(b => b.UseSalt).ToList();
            Assert.AreEqual(2, saltLaterals.Count());
            var sdhfsdhgjkldfghdfsg = saltLaterals.OrderBy(sb => sb.Id).Select(sb => sb.SaltConcentrationDischargeConstant).ToArray();
            Assert.AreEqual(new[] { 33.0, 33.0 }, saltLaterals.OrderBy(sb => sb.Id).Select(sb => sb.SaltConcentrationDischargeConstant).ToArray());
        }
    }
}
