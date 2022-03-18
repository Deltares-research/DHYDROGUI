using DelftTools.Hydro;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Helpers
{
    [TestFixture]
    public class BranchChainageExtensionsTest
    {
        [Test]
        [TestCase(5, 5)]
        [TestCase(12, 10)]
        [TestCase(-10, 0)]
        public void GivenBranchChainageExtensions_GetBranchSnappedChainage_ShouldReturnAValidChainage(double chainage, double expectedChainage)
        {
            //Arrange
            var branch = new Channel
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(10, 0)
                })
            };

            // Act & Assert
            Assert.AreEqual(expectedChainage, branch.GetBranchSnappedChainage(chainage));
        }

        [Test]
        public void GivenBranchChainageExtensions_GetBranchSnappedChainage_ShouldReturnAValidChainageWithCustomLength()
        {
            //Arrange
            var branch = new Channel
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(10, 0)
                }),
                IsLengthCustom = true,
                Length = 100
            };

            // Act & Assert
            Assert.AreEqual(15, branch.GetBranchSnappedChainage(15));
            Assert.AreEqual(100, branch.GetBranchSnappedChainage(150));
        }
    }
}