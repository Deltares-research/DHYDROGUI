using System.Collections.Generic;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.LeveeBreachFormula;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Structures
{
    [TestFixture]
    public class LeveeBreachTest
    {
        [Test]
        public void DefaultBreachLocation_ShouldBeCentered_StraightLineAsDamBreakLine()
        {
            var damBreak = new LeveeBreach();
            var coordinates = new List<Coordinate> {new Coordinate(0, 0), new Coordinate(10, 10)};
            var line = new LineString(coordinates.ToArray());

            var expectedBreachLocationX = 5.0;
            var expectedBreachLocationY = 5.0;

            damBreak.Geometry = line;

            Assert.AreEqual(expectedBreachLocationX, damBreak.BreachLocationX);
            Assert.AreEqual(expectedBreachLocationY, damBreak.BreachLocationY);
        }
        
        [Test]
        public void BreachLocationXandY_ShouldBeSameAsBreachLocation()
        {
            var leveeBreach = new LeveeBreach
            {
                Geometry = new LineString(new[] {new Coordinate(0, 0), new Coordinate(10, 10)})
            };
            
            Assert.AreEqual(leveeBreach.BreachLocationX, leveeBreach.BreachLocation.X);
            Assert.AreEqual(leveeBreach.BreachLocationY, leveeBreach.BreachLocation.Y);
        }

        [Test]
        public void DefaultBreachLocation_ShouldBeCentered_StairsShapedLineAsDamBreakLine()
        {
            var damBreak = new LeveeBreach();
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

        [Test]
        [TestCase(LeveeBreachGrowthFormula.UserDefinedBreach, typeof(UserDefinedBreachSettings))]
        [TestCase(LeveeBreachGrowthFormula.VerheijvdKnaap2002, typeof(VerheijVdKnaap2002BreachSettings))]
        public void LeveeBreachWithGrowthFormula_GettingsSettings_ShouldReturnCorrectType<T>(LeveeBreachGrowthFormula growthFormula, T type)
        {
            var leveeBreach = new LeveeBreach { LeveeBreachFormula = growthFormula };
            var settings = leveeBreach.GetActiveLeveeBreachSettings();
            Assert.AreEqual(type, settings.GetType());
        }

        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var leveeBreach = new LeveeBreach();

            // Assert
            Assert.That(leveeBreach.Name, Is.EqualTo("LeveeBreach"));
        }
    }
}
