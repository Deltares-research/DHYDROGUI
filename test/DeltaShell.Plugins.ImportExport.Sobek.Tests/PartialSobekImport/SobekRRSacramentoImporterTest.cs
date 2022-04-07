using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class SobekRRSacramentoImporterTest
    {
        [Test]
        public void ImportDrainageBasinWithSacramentoCatchments()
        {
            var pathToNetwork = TestHelper.GetTestDataDirectory() + @"\TEST_SAC.lit\2\NETWORK.TP";
            var rrDrainageBasinImporter = new SobekRRDrainageBasinImporter() { TargetObject = new DrainageBasin(), PathSobek = pathToNetwork };
            rrDrainageBasinImporter.Import();

            var basin = (DrainageBasin)rrDrainageBasinImporter.TargetObject;
        
            Assert.AreEqual(8, basin.Catchments.Count(c => Equals(c.CatchmentType, CatchmentType.Sacramento)));
            Assert.AreEqual(1e+06, basin.Catchments.First(c => c.Name == "2").GeometryArea, 0.1);
        }

        [Test]
        public void ImportDrainageBasinWithHbvCatchments()
        {
            var pathToNetwork = TestHelper.GetTestDataDirectory() + @"\TEST_HBV.lit\2\NETWORK.TP";
            var rrDrainageBasinImporter = new SobekRRDrainageBasinImporter()
                {
                    TargetObject = new DrainageBasin(),
                    PathSobek = pathToNetwork
                };
            rrDrainageBasinImporter.Import();

            var basin = (IDrainageBasin) rrDrainageBasinImporter.TargetObject;
            Assert.AreEqual(9, basin.Catchments.Count(c => Equals(c.CatchmentType, CatchmentType.Hbv)));
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ImportModelWithSacramentoCatchmentData()
        {
            var pathToSobekModel = TestHelper.GetTestFilePath(@"TEST_SAC.lit\2\NETWORK.TP");

            var builder = new HydroModelBuilder();
            var hydroModel = builder.BuildModel(ModelGroup.All); //sobek only
            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekModel, hydroModel);

            importer.Import();

            var rrModel = hydroModel.Activities.OfType<RainfallRunoffModel>().First();
            var sacramentoCatchment = rrModel.Basin.Catchments.First(c => c.Name == "5");
            var sacramentoData = rrModel.GetCatchmentModelData(sacramentoCatchment) as SacramentoData;

            Assert.IsNotNull(sacramentoData);
            Assert.AreEqual("5", sacramentoData.Name, "name");
            Assert.AreEqual(1e+06, sacramentoData.Area, "runoff area"); 
            Assert.AreEqual(1e+06, sacramentoData.CalculationArea, "calculation area");
            Assert.AreEqual(1, sacramentoData.AreaAdjustmentFactor, "area adjustment factor");
            Assert.AreEqual(0.2, sacramentoData.UpperZoneFreeWaterDrainageRate, 0.000001, "Upper zone free water drainage rate");
            Assert.AreEqual(3.5, sacramentoData.HydrographValues[4], "value in hydrograph");
            Assert.AreEqual(1, sacramentoData.HydrographStep, "hydrograph timestep");
            Assert.AreEqual(0.2, sacramentoData.PercolatedWaterFraction, "percolated water fraction pfree");
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ImportModelWithHbvCatchmentData()
        {
            var pathToSobekModel = TestHelper.GetTestFilePath(@"TEST_HBV.lit\2\NETWORK.TP");

            var builder = new HydroModelBuilder();
            var hydroModel = builder.BuildModel(ModelGroup.All); //sobek only
            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekModel, hydroModel);

            importer.Import();

            var rrModel = hydroModel.Activities.OfType<RainfallRunoffModel>().First();
            var hbvCatchment = rrModel.Basin.Catchments.First(c => c.Name == "2");
            var hbvData = rrModel.GetCatchmentModelData(hbvCatchment) as HbvData;

            Assert.IsNotNull(hbvData);
            Assert.AreEqual("2", hbvData.Name, "name");
            Assert.AreEqual(0.01 ,hbvData.BaseFlowReservoirConstant, "base flow reservoir constant");
            Assert.AreEqual(0.95, hbvData.FreezingEfficiency, "freezing efficiency");
            Assert.AreEqual(200.0, hbvData.FieldCapacity, "field capacity");
            Assert.AreEqual(45.0, hbvData.InitialUpperZoneContent, "initial upper zone content");
        }
    }
}