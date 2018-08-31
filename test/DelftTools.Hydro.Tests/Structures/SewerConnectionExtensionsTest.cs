using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;

namespace DelftTools.Hydro.Tests.Structures
{
    [TestFixture]
    public class SewerConnectionExtensionsTest
    {
        private MockRepository mocks;

        [SetUp]
        public void SetUp()
        {
            mocks = new MockRepository();
        }

        [Test]
        [TestCase(0, 0, 100, 100, 50, 50, 70.710678)]
        [TestCase(5, 5, 5, 5, 5, 5, 0)]
        public void WhenAddingStructureToBranch_AddStructureInMiddleOfBranch(double point1X, double point1Y, double point2X, double point2Y, double expectedX, double expectedY, double chainage)
        {
            var sc = new SewerConnection(new Manhole("1") { Geometry = new Point(point1X, point1Y) }, new Manhole("2") { Geometry = new Point(point2X, point2Y) });
            
            var structure = mocks.DynamicMock<IStructure1D>();
            structure.Expect(s => s.Branch).Return(sc);
            mocks.ReplayAll();

            var newStructure = sc.AddStructureToBranch(new Pump()); // Add structure mock instead of new pump
            Assert.AreEqual(expectedX, newStructure.Geometry.Coordinate.X);
            Assert.AreEqual(expectedY, newStructure.Geometry.Coordinate.Y);
            Assert.AreEqual(chainage, newStructure.Chainage, 1e-5);
        }

    }
}