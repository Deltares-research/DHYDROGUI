using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM;
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
        [Category("Quarantine")]
        public void ImportSalt()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\20110331_NDB.sbk\6\DEFTOP.1";
            var waterFlowFmModel = new WaterFlowFMModel();
            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowFmModel, new IPartialSobekImporter[] { new SobekBranchesImporter(),new SobekLateralSourcesImporter(), new SobekBoundaryConditionsImporter(), new SobekLateralSourcesDataImporter()});

            importer.Import();

            // Assert.IsNotNull(waterFlowFmModel.DispersionCoverage);
            //
            // var boundary = waterFlowFmModel.BoundaryConditions.FirstOrDefault(bc => bc.Node.Name == "1");
            //
            // Assert.IsNotNull(boundary);
            // Assert.AreEqual(SaltBoundaryConditionType.Constant, boundary.SaltConditionType);
            // Assert.AreEqual(31.0, boundary.SaltConcentrationConstant);
            //
            // Assert.IsTrue(waterFlowFmModel.UseSalt);
            // Assert.IsTrue(waterFlowFmModel.UseSaltInCalculation);
            //
            // Assert.IsTrue(waterFlowFmModel.DispersionFormulationType == DispersionFormulationType.Constant);
        }

        [Test]
        public void ImportModelWithoutSalt()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\SW_max_1.lit\3\Network.TP";
            var waterFlowFmModel = new WaterFlowFMModel("waterflowfm");
            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowFmModel, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekLateralSourcesImporter(), new SobekBoundaryConditionsImporter(), new SobekLateralSourcesDataImporter()});

            importer.Import();

            // Assert.IsFalse(waterFlowFmModel.UseSalt);
            // Assert.IsFalse(waterFlowFmModel.UseSaltInCalculation);
            // Assert.That(waterFlowFmModel.DispersionFormulationType == DispersionFormulationType.Constant);
        }

        [Test]
        [Category(TestCategory.Slow)]
        [Category("Quarantine")]
        public void ImportModelWithSaltCheckSaltBoundariesAndSaltLaterals()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\301_00.lit\2\Network.TP";
            var waterFlowFmModel = new WaterFlowFMModel("water flow fm");
            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowFmModel, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekLateralSourcesImporter(), new SobekBoundaryConditionsImporter(), new SobekLateralSourcesDataImporter()});

            importer.Import();

            // Assert.IsTrue(waterFlowFmModel.UseSalt);
            // Assert.IsTrue(waterFlowFmModel.UseSaltInCalculation);
            //
            // var saltBoundaries =
            //     waterFlowFmModel.BoundaryConditions.Where(b => b.UseSalt).ToList();
            // Assert.AreEqual(2, saltBoundaries.Count());
            // Assert.AreEqual(new[] { 33.0, 0.01 }, saltBoundaries.OrderBy(sb => sb.Id).Select(sb => sb.SaltConcentrationConstant).ToArray());
            //
            // var saltLaterals =
            //     waterFlowFmModel.LateralSourceData.Where(b => b.UseSalt).ToList();
            // Assert.AreEqual(2, saltLaterals.Count());
            // var sdhfsdhgjkldfghdfsg = saltLaterals.OrderBy(sb => sb.Id).Select(sb => sb.SaltConcentrationDischargeConstant).ToArray();
            // Assert.AreEqual(new[] { 33.0, 33.0 }, saltLaterals.OrderBy(sb => sb.Id).Select(sb => sb.SaltConcentrationDischargeConstant).ToArray());
        }
    }
}
