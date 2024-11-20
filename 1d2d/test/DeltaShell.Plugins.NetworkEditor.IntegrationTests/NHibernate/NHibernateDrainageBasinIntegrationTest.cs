using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
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