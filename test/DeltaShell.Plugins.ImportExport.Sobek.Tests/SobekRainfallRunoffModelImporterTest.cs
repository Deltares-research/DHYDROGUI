using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    [TestFixture]
    public class SobekRainfallRunoffModelImporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.VerySlow)]
        [Category("Quarantine")]
        [Ignore("Sobek RR is not implemented yet.")]
        public void ImportTholenGeneralCheck()
        {
            string pathToSobekModel = TestHelper.GetTestDataDirectory() + @"\Tholen.lit\29\NETWORK.TP";

            var hydroModel = CreateHydroModelWithRR();
            var rainfallRunoffModel = hydroModel.Activities.OfType<RainfallRunoffModel>().First();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekModel, hydroModel);
            importer.Import();

            //Settings
            Assert.AreEqual(true, rainfallRunoffModel.CapSim, "CapSim");
            Assert.AreEqual(RainfallRunoffEnums.CapsimInitOptions.AtEquilibriumMoisture, rainfallRunoffModel.CapSimInitOption, "CapSim init option");
            
            //no setting in file, but this is the default in sobek 2:
            Assert.AreEqual(RainfallRunoffEnums.CapsimCropAreaOptions.PerCropArea, rainfallRunoffModel.CapSimCropAreaOption, "CapSim crop method"); 

            //Types
            Assert.AreEqual(328, rainfallRunoffModel.Basin.Catchments.Count(c => c.CatchmentType == CatchmentType.Unpaved), "unpaved");
            Assert.AreEqual(48, rainfallRunoffModel.Basin.Catchments.Count(c => c.CatchmentType == CatchmentType.Paved), "paved");

            Assert.AreEqual(328, rainfallRunoffModel.ModelData.OfType<UnpavedData>().Count(), "unpaved model");
            Assert.AreEqual(48, rainfallRunoffModel.ModelData.OfType<PavedData>().Count(), "paved model");
            
            //Meteo
            Assert.AreEqual(MeteoDataDistributionType.Global, rainfallRunoffModel.Precipitation.DataDistributionType);
            Assert.AreEqual(145,rainfallRunoffModel.Precipitation.Data.Arguments[0].Values.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        [Category("Quarantine")]
        [Ignore("Sobek RR is not implemented yet.")]
        public void ImportRRBoundariesAndConditions()
        {
            string pathToModel = TestHelper.GetTestDataDirectory() + @"\RR.Lit\1\Network.TP";

            var rrModel = new RainfallRunoffModel();
            var rrImporter = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToModel, rrModel);

            rrImporter.Import();

            Assert.AreEqual(2, rrModel.BoundaryData.Count);

            Assert.IsFalse(rrModel.BoundaryData[0].Series.IsConstant);
            Assert.AreEqual(1.0, rrModel.BoundaryData[0].Series.Evaluate(new DateTime(2000, 1, 1))); //series

            Assert.IsTrue(rrModel.BoundaryData[1].Series.IsConstant);
            Assert.AreEqual(0.5, rrModel.BoundaryData[1].Series.Evaluate(new DateTime(2000, 1, 1))); //constant
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.VerySlow)]
        [Category("Quarantine")]
        [Ignore("Sobek RR is not implemented yet.")]
        public void ImportTholenUnpavedLinkCheck()
        {
            string pathToSobekModel = TestHelper.GetTestDataDirectory() + @"\Tholen.lit\29\NETWORK.TP";

            var hydroModel = CreateHydroModelWithRR();
            var rainfallRunoffModel = hydroModel.Activities.OfType<RainfallRunoffModel>().First();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekModel, hydroModel);
            importer.Import();
            
            var unpavedData =
                rainfallRunoffModel.GetAllModelData().OfType<UnpavedData>().FirstOrDefault();

            Assert.IsNotNull(unpavedData);

            Assert.AreNotEqual(0, unpavedData.Catchment.Links.Count);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category("Quarantine")]
        [Ignore("Sobek RR is not implemented yet.")]
        public void ImportRrStandaloneWithFlowConnectionNode()
        {
            // TOOLS-20516
            string pathToModel = TestHelper.GetTestDataDirectory() + @"\019_011.lit\2\NETWORK.TP";
            var hydroModel = CreateHydroModelWithRR();
            var rainfallRunoffModel = hydroModel.Activities.OfType<RainfallRunoffModel>().First();
            var rrImporter = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToModel, hydroModel);
            rrImporter.Import();

            Assert.True(rainfallRunoffModel.Basin.Boundaries.Count == 1);
            Assert.True(rainfallRunoffModel.BoundaryData.Count == 1);
        }

        private static HydroModel CreateHydroModelWithRR()
        {
            var hydroModel = new HydroModel();
            var network = new HydroNetwork();
            hydroModel.Region.SubRegions.Add(network);
            var basin = new DrainageBasin();
            hydroModel.Region.SubRegions.Add(basin);

            var rainfallRunoffModel = new RainfallRunoffModel();
            hydroModel.Activities.Add(rainfallRunoffModel);

            rainfallRunoffModel.GetDataItemByValue(rainfallRunoffModel.Basin).LinkTo(hydroModel.GetDataItemByValue(basin));
            return hydroModel;
        }

    }
}
