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
        public void SaveLoadCatchment()
        {
            var catchment = new Catchment
            {
                Name = "testName",
                LongName = "longName",
                Geometry =
                    new Polygon(
                    new LinearRing(new []
                                                           {
                                                               new Coordinate(0, 0), new Coordinate(10, 0),
                                                               new Coordinate(10, 10), new Coordinate(0, 0)
                                                           })),
                IsGeometryDerivedFromAreaSize = true,
                CatchmentType = CatchmentType.GreenHouse,
                SubCatchments = {new Catchment()}
            };

            catchment.SetAreaSize(500);

            var basin = new DrainageBasin();
            basin.Catchments.Add(catchment);

            var retrievedBasin = SaveLoadObject(basin, "catch.dsproj");

            Assert.AreEqual(1, retrievedBasin.Catchments.Count);
            var retrievedCatchment = retrievedBasin.Catchments.First();
            Assert.AreEqual(catchment.Name, retrievedCatchment.Name);
            Assert.AreEqual(retrievedBasin, retrievedCatchment.Basin);
            Assert.AreEqual(catchment.LongName, retrievedCatchment.LongName);
            Assert.AreEqual(catchment.Geometry, retrievedCatchment.Geometry);
            Assert.AreEqual(catchment.AreaSize, retrievedCatchment.AreaSize);
            Assert.AreEqual(catchment.CatchmentType, retrievedCatchment.CatchmentType);
            Assert.AreEqual(catchment.SubCatchments.Count, retrievedCatchment.SubCatchments.Count);
        }

        [Test]
        public void SaveLoadDefaultGeometryCatchment()
        {
            // add catchment
            var catchment = new Catchment
            {
                IsGeometryDerivedFromAreaSize = true
            };
            catchment.SetAreaSize(500);

            var path = "catch.dsproj";

            var network = new DrainageBasin();
            network.Catchments.Add(catchment);

            var retrievedNetwork = SaveLoadObject(network, path);

            Assert.AreEqual(1, retrievedNetwork.Catchments.Count);
            var retrievedCatchment = retrievedNetwork.Catchments.First();

            var oldArea = retrievedCatchment.Geometry.Area;

            retrievedCatchment.SetAreaSize(retrievedCatchment.AreaSize * 2);

            Assert.AreNotEqual(oldArea, retrievedCatchment.Geometry.Area); //check if change in area, changes geometry (when IsGeometryDerivedFromAreaSize)
        }

        [Test]
        public void SaveLoadHydroLink()
        {
            var catchment = new Catchment();
            var wwtp = new WasteWaterTreatmentPlant();
            var basin = new DrainageBasin {Catchments = {catchment}, WasteWaterTreatmentPlants = {wwtp}};
            
            catchment.LinkTo(wwtp);

            var retrievedBasin = SaveLoadObject(basin, "link.dsproj");

            Assert.AreEqual(1, retrievedBasin.Links.Count);

            var retrievedLink = retrievedBasin.Links.First();
            var retrievedCatchment = (Catchment)retrievedLink.Source;
            var retrievedWwtp = (WasteWaterTreatmentPlant)retrievedLink.Target;

            Assert.AreEqual(1, retrievedCatchment.Links.Count);
            Assert.AreEqual(1, retrievedWwtp.Links.Count);
            Assert.AreSame(retrievedLink, retrievedWwtp.Links.First());
        }

        [Test]
        public void SaveLoadWasteWaterTreatmentPlant()
        {
            var wwtp = new WasteWaterTreatmentPlant { Name="testName", Description = "testDescr", Geometry = new Point(55, 33) };

            var path = "wwtp.dsproj";

            var basin = new DrainageBasin();
            basin.WasteWaterTreatmentPlants.Add(wwtp);

            var retrievedBasin = SaveLoadObject(basin, path);

            Assert.AreEqual(1, retrievedBasin.WasteWaterTreatmentPlants.Count);
            var retrievedWwtp = retrievedBasin.WasteWaterTreatmentPlants.First();
            Assert.AreEqual(wwtp.Geometry, retrievedWwtp.Geometry);
            Assert.AreEqual(wwtp.Name, retrievedWwtp.Name);
            Assert.AreEqual(basin, retrievedWwtp.Basin);
            Assert.AreEqual(wwtp.Description, retrievedWwtp.Description);
        }
        
        [Test]
        public void SaveLoadRunoffBoundary()
        {
            var boundary = new RunoffBoundary { Name = "testName", Description = "testDescr", Geometry = new Point(55, 33) };

            var path = "rb.dsproj";

            var basin = new DrainageBasin();
            basin.Boundaries.Add(boundary);

            var retrievedBasin = SaveLoadObject(basin, path);

            Assert.AreEqual(1, retrievedBasin.Boundaries.Count);
            var retrievedBoundary = retrievedBasin.Boundaries.First();
            Assert.AreEqual(boundary.Geometry, retrievedBoundary.Geometry);
            Assert.AreEqual(boundary.Name, retrievedBoundary.Name);
            Assert.AreEqual(basin, retrievedBoundary.Basin);
            Assert.AreEqual(boundary.Description, retrievedBoundary.Description);
        } 
    }
}