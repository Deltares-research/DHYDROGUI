using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.IntegrationTests.NHibernate
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    public class NHibernateDrainageBasinIntegrationTest : NHibernateHydroRegionTestBase
    {
        [Test]
        public void SaveLoadDefaultGeometryCatchment()
        {
            // add catchment
            var catchment = new Catchment {IsGeometryDerivedFromAreaSize = true};
            catchment.SetAreaSize(500);

            var path = "catch.dsproj";

            var network = new DrainageBasin();
            network.Catchments.Add(catchment);

            DrainageBasin retrievedNetwork = SaveLoadObject(network, path);

            Assert.AreEqual(1, retrievedNetwork.Catchments.Count);
            Catchment retrievedCatchment = retrievedNetwork.Catchments.First();

            double oldArea = retrievedCatchment.Geometry.Area;

            retrievedCatchment.SetAreaSize(retrievedCatchment.AreaSize * 2);

            Assert.AreNotEqual(oldArea, retrievedCatchment.Geometry.Area); //check if change in area, changes geometry (when IsGeometryDerivedFromAreaSize)
        }

        [Test]
        public void SaveLoadHydroLink()
        {
            var catchment = new Catchment();
            var wwtp = new WasteWaterTreatmentPlant();
            var basin = new DrainageBasin
            {
                Catchments = {catchment},
                WasteWaterTreatmentPlants = {wwtp}
            };

            catchment.LinkTo(wwtp);

            DrainageBasin retrievedBasin = SaveLoadObject(basin, "link.dsproj");

            Assert.AreEqual(1, retrievedBasin.Links.Count);

            HydroLink retrievedLink = retrievedBasin.Links.First();
            var retrievedCatchment = (Catchment) retrievedLink.Source;
            var retrievedWwtp = (WasteWaterTreatmentPlant) retrievedLink.Target;

            Assert.AreEqual(1, retrievedCatchment.Links.Count);
            Assert.AreEqual(1, retrievedWwtp.Links.Count);
            Assert.AreSame(retrievedLink, retrievedWwtp.Links.First());
        }

        [Test]
        public void SaveLoadWasteWaterTreatmentPlant()
        {
            var wwtp = new WasteWaterTreatmentPlant
            {
                Name = "testName",
                Description = "testDescr",
                Geometry = new Point(55, 33)
            };

            var path = "wwtp.dsproj";

            var basin = new DrainageBasin();
            basin.WasteWaterTreatmentPlants.Add(wwtp);

            DrainageBasin retrievedBasin = SaveLoadObject(basin, path);

            Assert.AreEqual(1, retrievedBasin.WasteWaterTreatmentPlants.Count);
            WasteWaterTreatmentPlant retrievedWwtp = retrievedBasin.WasteWaterTreatmentPlants.First();
            Assert.AreEqual(wwtp.Geometry, retrievedWwtp.Geometry);
            Assert.AreEqual(wwtp.Name, retrievedWwtp.Name);
            Assert.AreEqual(basin, retrievedWwtp.Basin);
            Assert.AreEqual(wwtp.Description, retrievedWwtp.Description);
        }

        [Test]
        public void SaveLoadRunoffBoundary()
        {
            var boundary = new RunoffBoundary
            {
                Name = "testName",
                Description = "testDescr",
                Geometry = new Point(55, 33)
            };

            var path = "rb.dsproj";

            var basin = new DrainageBasin();
            basin.Boundaries.Add(boundary);

            DrainageBasin retrievedBasin = SaveLoadObject(basin, path);

            Assert.AreEqual(1, retrievedBasin.Boundaries.Count);
            RunoffBoundary retrievedBoundary = retrievedBasin.Boundaries.First();
            Assert.AreEqual(boundary.Geometry, retrievedBoundary.Geometry);
            Assert.AreEqual(boundary.Name, retrievedBoundary.Name);
            Assert.AreEqual(basin, retrievedBoundary.Basin);
            Assert.AreEqual(boundary.Description, retrievedBoundary.Description);
        }
    }
}