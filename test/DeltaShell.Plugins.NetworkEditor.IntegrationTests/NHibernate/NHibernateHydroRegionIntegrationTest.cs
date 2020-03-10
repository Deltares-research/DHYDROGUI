using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.IntegrationTests.NHibernate
{
    [TestFixture]
    public class NHibernateHydroRegionIntegrationTest : NHibernateHydroRegionTestBase
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        [Category("Quarantine")]
        public void SaveLoadRegionWithLinks()
        {
            var catchment = new Catchment();
            var wwtp = new WasteWaterTreatmentPlant { Geometry = new Point(55, 33) };
            var basin = new DrainageBasin {Catchments = {catchment}, WasteWaterTreatmentPlants = {wwtp}};

            var path = "wwtp.dsproj";

            var network = HydroNetworkHelper.GetSnakeHydroNetwork(2);

            var lateralSource = new LateralSource();
            NetworkHelper.AddBranchFeatureToBranch(lateralSource, network.Branches.First(), 30);
            var boundary = network.HydroNodes.First();

            var region = new HydroRegion {SubRegions = {network, basin}};

            catchment.LinkTo(boundary);
            catchment.LinkTo(wwtp);
            wwtp.LinkTo(lateralSource);

            var retrievedRegion = SaveLoadObject(region, path);
            var retrievedNetwork = retrievedRegion.SubRegions.First() as HydroNetwork;
            var retrievedBasin = retrievedRegion.SubRegions.Last() as DrainageBasin;

            Assert.AreEqual(retrievedRegion, retrievedNetwork.Parent);
            Assert.AreEqual(2, retrievedRegion.Links.Count);
            Assert.AreEqual(0, retrievedNetwork.Links.Count);
            Assert.AreEqual(1, retrievedBasin.Links.Count);
            Assert.AreEqual(2, retrievedBasin.Catchments.First().Links.Count);
            Assert.AreEqual(2, retrievedBasin.WasteWaterTreatmentPlants.First().Links.Count);
            Assert.AreEqual(1, retrievedNetwork.HydroNodes.First().Links.Count);
            Assert.AreEqual(1, retrievedNetwork.LateralSources.First().Links.Count);
            
        }
    }
}