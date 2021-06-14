using System;
using System.Collections.Generic;
using DelftTools.Hydro.Geometries;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Geometries
{
    [TestFixture]
    public class BranchExtensionsTest
    {
        [Test]
        public void GetCoordinate_BranchNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => ((IBranch) null).GetCoordinate(10);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("branch"));
        }

        [Test]
        [TestCase(double.NegativeInfinity)]
        [TestCase(double.PositiveInfinity)]
        [TestCase(double.NaN)]
        [TestCase(-0.01)]
        [TestCase(-10)]
        public void GetCoordinate_ChainageInvalid_ThrowsArgumentOutOfRangeException(double chainage)
        {
            // Setup
            var branch = new Branch();

            // Call
            void Call() => branch.GetCoordinate(chainage);

            // Assert
            var e = Assert.Throws<ArgumentOutOfRangeException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("chainage"));
        }
        
        [Test]
        public void GetCoordinate_CannotDetermineNodeCoordinates_ThrowsArgumentException()
        {
            // Setup
            var branch = new Branch {Name = "some_branch"};

            // Call
            void Call() => branch.GetCoordinate(10);

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.Message, Is.EqualTo("Cannot determine the node coordinates of branch some_branch."));
        }

        [TestCaseSource(nameof(GetCoordinateCases))]
        public void GetCoordinate_ReturnsCorrectResult(double ax, double ay,
                                                       double bx, double by,
                                                       double chainage,
                                                       double expX, double expY)
        {
            // Setup
            var coordinateA = new Coordinate(ax, ay);
            var coordinateB = new Coordinate(bx, by);

            var nodeA = Substitute.For<INode>();
            var nodeB = Substitute.For<INode>();

            nodeA.Geometry = new Point(coordinateA);
            nodeB.Geometry = new Point(coordinateB);

            var branch = new Branch(nodeA, nodeB);

            // Call
            Coordinate result = branch.GetCoordinate(chainage);

            // Assert
            Assert.That(result.X, Is.EqualTo(expX).Within(0.0001));
            Assert.That(result.Y, Is.EqualTo(expY).Within(0.0001));
        }

        private static IEnumerable<TestCaseData> GetCoordinateCases()
        {
            yield return new TestCaseData(0, 0, 100, 100, 70.7107, 50, 50);
            yield return new TestCaseData(0, 0, -100, -100, 70.7107, -50, -50);
            yield return new TestCaseData(10, 60, 50, 260, 76.4853, 25, 135);
            yield return new TestCaseData(10, 60, 50, 260, 0, 10, 60);
        }
    }
}