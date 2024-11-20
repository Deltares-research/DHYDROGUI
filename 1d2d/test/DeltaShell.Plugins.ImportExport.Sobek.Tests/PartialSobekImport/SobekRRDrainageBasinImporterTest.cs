using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    public class SobekRRDrainageBasinImporterTest
    {
        [Test]
        public void ImportRRNetwork()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\Tholen.Lit\29\Network.TP";

            var rrDrainageBasinImporter = new SobekRRDrainageBasinImporter
                {TargetObject = new DrainageBasin(), PathSobek = pathToSobekNetwork};

            rrDrainageBasinImporter.Import();
            
            var basin = (IDrainageBasin)rrDrainageBasinImporter.TargetObject;
            Assert.AreEqual(2, basin.WasteWaterTreatmentPlants.Count);

            var nCatchments = basin.Catchments.Count;
            Assert.Greater(nCatchments, 0);

            Assert.AreEqual(0,basin.Catchments.Count(c => !c.IsGeometryDerivedFromAreaSize));
        }

        [Test]
        public void ImportRRBoundaries()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\RR.Lit\1\Network.TP";

            var rrDrainageBasinImporter = new SobekRRDrainageBasinImporter { TargetObject = new DrainageBasin(), PathSobek = pathToSobekNetwork };

            rrDrainageBasinImporter.Import();

            var basin = (IDrainageBasin)rrDrainageBasinImporter.TargetObject;
            Assert.AreEqual(1, basin.WasteWaterTreatmentPlants.Count);
            Assert.AreEqual(2, basin.Boundaries.Count);
            Assert.AreEqual(3, basin.Catchments.Count);
            Assert.AreEqual(4, basin.Links.Count);
        }

        [Test]
        public void ImportRRNetworkToExistingNetwork()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\Tholen.Lit\29\Network.TP";

            var rrDrainageBasinImporter = new SobekRRDrainageBasinImporter();

            rrDrainageBasinImporter.TargetObject = new DrainageBasin();
            rrDrainageBasinImporter.PathSobek = pathToSobekNetwork;
            rrDrainageBasinImporter.Import();

            var basin = (IDrainageBasin)rrDrainageBasinImporter.TargetObject;
            Assert.AreEqual(2, basin.WasteWaterTreatmentPlants.Count);

            var nCatchments = basin.Catchments.Count;
            Assert.Greater(nCatchments,0);
            
            //second time

            rrDrainageBasinImporter.TargetObject = basin;
            rrDrainageBasinImporter.Import();

            Assert.AreEqual(2, basin.WasteWaterTreatmentPlants.Count);
            Assert.AreEqual(nCatchments, basin.Catchments.Count);
        }

        [Test]
        public void UpdateExistingRRNetwork()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\Tholen.Lit\29\Network.TP";

            var rrNetworkImporter = new SobekRRDrainageBasinImporter();

            rrNetworkImporter.TargetObject = new DrainageBasin();
            rrNetworkImporter.PathSobek = pathToSobekNetwork;
            rrNetworkImporter.Import();
            var basin = (IDrainageBasin)rrNetworkImporter.TargetObject;

            var firstCatchment = basin.Catchments.First();
            var hundredthCatchment = basin.Catchments[100];
            var firstWWTP = basin.WasteWaterTreatmentPlants.First();

            //get values
            var firstCatchmentArea = firstCatchment.GeometryArea;
            var hundredthCatchmentName = hundredthCatchment.Name;
            var firstWWTPLongName = firstWWTP.LongName;

            //do mutations
            firstCatchment.SetAreaSize(firstCatchment.GeometryArea + 1111111);
            basin.Catchments.Remove(hundredthCatchment);
            firstWWTP.LongName = "hahahahihihihohoho";

            //second time import
            rrNetworkImporter.TargetObject = basin;
            rrNetworkImporter.Import();

            Assert.AreEqual(firstCatchmentArea,basin.Catchments.First().GeometryArea);
            Assert.IsNotNull(basin.Catchments.FirstOrDefault(c => c.Name == hundredthCatchmentName));
            Assert.AreEqual(firstWWTPLongName, basin.WasteWaterTreatmentPlants.First().LongName);
        }

        [Test]
        public void CheckIfTholenCatchmentZRO652HasTwoLinks()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\Tholen.Lit\29\Network.TP";

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork,
                                                                                 new HydroRegion()
                                                                                     {
                                                                                         SubRegions =
                                                                                             {
                                                                                                 new HydroNetwork(),
                                                                                                 new DrainageBasin()
                                                                                             }
                                                                                     });

            importer.Import();
            var region = (HydroRegion)importer.TargetObject;

            var basin = region.SubRegions.OfType<IDrainageBasin>().First();
            var firstCatchment = basin.Catchments.First(c => c.Name == "ZRO652");

            Assert.AreEqual(2, firstCatchment.Links.Count);
            Assert.AreEqual("cGFE1045", firstCatchment.Links.First().Target.Name);
            Assert.AreEqual("ZRW3", firstCatchment.Links.Last().Target.Name);
        }
    }
}
