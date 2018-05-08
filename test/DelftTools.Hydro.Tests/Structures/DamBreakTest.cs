using System.Collections.Generic;
using DelftTools.Hydro.Structures;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Structures
{
    [TestFixture]
    public class DamBreakTest
    {
        [Test]
        public void DefaultBreachLocation_ShouldBeCentered_StraightLineAsDamBreakLine()
        {
            var damBreak = new DamBreak();
            var coordinates = new List<Coordinate>();
            coordinates.Add(new Coordinate(0,0));
            coordinates.Add(new Coordinate(10, 10));
            var line = new LineString(coordinates.ToArray());

            var expectedBreachLocationX = 5.0;
            var expectedBreachLocationY = 5.0;

            damBreak.Geometry = line;

            Assert.AreEqual(expectedBreachLocationX, damBreak.BreachLocationX);
            Assert.AreEqual(expectedBreachLocationY, damBreak.BreachLocationY);
        }

        [Test]
        public void DefaultBreachLocation_ShouldBeCentered_StairsShapedLineAsDamBreakLine()
        {
            var damBreak = new DamBreak();
            var coordinates = new List<Coordinate>();
            coordinates.Add(new Coordinate(0, 0));
            coordinates.Add(new Coordinate(1, 0));
            coordinates.Add(new Coordinate(1, 1));
            coordinates.Add(new Coordinate(2, 1));
            coordinates.Add(new Coordinate(2, 2));
            coordinates.Add(new Coordinate(3, 3));
            coordinates.Add(new Coordinate(4, 3));
            coordinates.Add(new Coordinate(4, 4));
            coordinates.Add(new Coordinate(5, 4));
            coordinates.Add(new Coordinate(5, 5));

            //
            //
            //          _|
            //        _|
            //       /
            //     _|
            //   _|
            //   
            //

            var line = new LineString(coordinates.ToArray());

            var expectedBreachLocationX = 2.5;
            var expectedBreachLocationY = 2.5;

            damBreak.Geometry = line;

            Assert.AreEqual(expectedBreachLocationX, damBreak.BreachLocationX);
            Assert.AreEqual(expectedBreachLocationY, damBreak.BreachLocationY);
        }
    }
}
