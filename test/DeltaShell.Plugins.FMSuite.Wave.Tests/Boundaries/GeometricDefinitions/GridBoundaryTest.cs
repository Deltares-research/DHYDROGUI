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
            var grid = Substitute.For<IDiscreteGridPointCoverage>();

            // Call
            var gridBoundary = new GridBoundary(grid);

            // Assert
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
    }
}