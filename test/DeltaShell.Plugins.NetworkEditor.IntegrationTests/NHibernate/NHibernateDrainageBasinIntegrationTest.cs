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
    }
}