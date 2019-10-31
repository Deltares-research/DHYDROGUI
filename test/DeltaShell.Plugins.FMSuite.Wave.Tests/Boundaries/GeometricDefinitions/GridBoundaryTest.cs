using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using GeoAPI.Extensions.Coverages;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.GeometricDefinitions
{
    [TestFixture]
    public class GridBoundaryTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            const int expectedX = 5;
            const int expectedY = 6;

            var grid = Substitute.For<IDiscreteGridPointCoverage>();
            grid.Size1.Returns(expectedX);
            grid.Size2.Returns(expectedY);

            // Call
            var gridBoundary = new GridBoundary(grid);

            // Assert
            Assert.That(gridBoundary[GridSide.West], Has.Count.EqualTo(expectedY), 
                        $"Expected the west boundary to have {expectedY} values");
            for (var i = 0; i < gridBoundary[GridSide.West].Count; i++)
            {
                Assert.That(gridBoundary[GridSide.West][i].X, Is.EqualTo(0));
                Assert.That(gridBoundary[GridSide.West][i].Y, Is.EqualTo(i));
            }

            Assert.That(gridBoundary[GridSide.North], Has.Count.EqualTo(expectedX), 
                        $"Expected the west boundary to have {expectedX} values");
            for (var i = 0; i < gridBoundary[GridSide.North].Count; i++)
            {
                Assert.That(gridBoundary[GridSide.North][i].X, Is.EqualTo(i));
                Assert.That(gridBoundary[GridSide.North][i].Y, Is.EqualTo(expectedY - 1));
            }

            Assert.That(gridBoundary[GridSide.East], Has.Count.EqualTo(expectedY), 
                        $"Expected the west boundary to have {expectedY} values");
            for (var i = 0; i < gridBoundary[GridSide.East].Count; i++)
            {
                Assert.That(gridBoundary[GridSide.East][i].X, Is.EqualTo(expectedX - 1));
                Assert.That(gridBoundary[GridSide.East][i].Y, Is.EqualTo(expectedY - 1 - i));
            }

            Assert.That(gridBoundary[GridSide.South], Has.Count.EqualTo(expectedX), 
                        $"Expected the west boundary to have {expectedX} values");
            for (var i = 0; i < gridBoundary[GridSide.South].Count; i++)
            {
                Assert.That(gridBoundary[GridSide.South][i].X, Is.EqualTo(expectedX - 1 - i));
                Assert.That(gridBoundary[GridSide.South][i].Y, Is.EqualTo(0));
            }
        }

        [Test]
        public void Constructor_GridNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new GridBoundary(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, 
                        Has.Property("ParamName").EqualTo("grid"));
            
        }

        [Test]
        [TestCase(1, 2)]
        [TestCase(2, 1)]
        [TestCase(0, 2)]
        [TestCase(2, 0)]
        [TestCase(-1, 2)]
        [TestCase(2, -1)]
        public void Constructor_GridWithInvalidDimensions_ThrowsArgumentException(int xVal, int yVal)
        {
            // Setup
            var grid = Substitute.For<IDiscreteGridPointCoverage>();
            grid.Size1.Returns(xVal);
            grid.Size2.Returns(yVal);

            // Call
            void Call() => new GridBoundary(grid);

            // Assert
            var exception = Assert.Throws<ArgumentException>(Call);
            Assert.That(exception, 
                        Has.Message.EqualTo("grid should contain at least 2 points in each dimension."));
        }
    }
}