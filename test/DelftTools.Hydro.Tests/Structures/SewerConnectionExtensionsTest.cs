using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Structures
{
    [TestFixture]
    public class SewerConnectionExtensionsTest
    {
        [Test]
        [TestCase(0, 0, 100, 100, 50, 50, 70.710678)]
        [TestCase(5, 5, 5, 5, 5, 5, 0)]
        public void WhenAddingStructureToBranch_AddStructureInMiddleOfBranch(double point1X, double point1Y, double point2X, double point2Y, double expectedX, double expectedY, double chainage)
        {
            var sourceCompartment = new Compartment { Geometry = new Point(point1X, point1Y) };
            var targetCompartment = new Compartment { Geometry = new Point(point2X, point2Y) };
            var sourceManhole = new Manhole("1") {Compartments = new EventedList<Compartment> {sourceCompartment}};
            var targetManhole = new Manhole("2") {Compartments = new EventedList<Compartment> {targetCompartment}};
            var sewerConnection = new SewerConnection
            {
                SourceCompartment = sourceCompartment,
                TargetCompartment = targetCompartment
            };

            var newStructure = sewerConnection.AddStructureToBranch(new Pump()); // Add structure mock instead of new pump
            Assert.AreEqual(expectedX, newStructure.Geometry.Coordinate.X);
            Assert.AreEqual(expectedY, newStructure.Geometry.Coordinate.Y);
            Assert.AreEqual(chainage, newStructure.Chainage, 1e-5);
        }

    }
}