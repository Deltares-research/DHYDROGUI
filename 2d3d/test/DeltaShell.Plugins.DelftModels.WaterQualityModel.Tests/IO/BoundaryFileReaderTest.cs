using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using GeoAPI.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    public class BoundaryFileReaderTest
    {
        private string commonFilePath;

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            commonFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO");
        }

        [Test]
        public void ReadNonExistentFileThrowsInvalidOperationException()
        {
            // setup
            var boundariesFile = new FileInfo("nonexistentfile.bnd");

            // call
            TestDelegate call = () => BoundaryFileReader.ReadAll(boundariesFile);

            // assert
            var exception = Assert.Throws<InvalidOperationException>(call);
            Assert.AreEqual("Cannot find boundaries file (" + boundariesFile.FullName + ").",
                            exception.Message);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFromFlowFMBoundariesFileReturnsAllReadBoundaries()
        {
            // setup
            var boundariesFile = new FileInfo(Path.Combine(commonFilePath, "uni3d.bnd"));

            // call
            IDictionary<WaterQualityBoundary, int[]> boundaries = BoundaryFileReader.ReadAll(boundariesFile);

            // assert
            var expectedBoundaries = new[]
            {
                "sea_002.pli",
                "sacra_001.pli",
                "sanjoa_001.pli",
                "yolo_001.pli",
                "CC.pli",
                "tracy.pli"
            };
            int[] expectedCoordinateCounts = new[]
            {
                2 * 105,
                2 * 4,
                2 * 3,
                2 * 24,
                2 * 1,
                2 * 1
            };
            var expectedCoordinates = new[]
            {
                null,
                new[]
                {
                    new Coordinate(629751.48224218, 4273342.11616655),
                    new Coordinate(629652.98730354, 4273299.40077900),
                    new Coordinate(630053.41258012, 4273366.14930817),
                    new Coordinate(629970.36795947, 4273375.18785627),
                    new Coordinate(629865.45841468, 4273373.01908793),
                    new Coordinate(629751.48224218, 4273342.11616655),
                    new Coordinate(629970.36795947, 4273375.18785627),
                    new Coordinate(629865.45841468, 4273373.01908793)
                },
                new[]
                {
                    new Coordinate(649711.71641119, 4182564.05982326),
                    new Coordinate(649744.14162055, 4182574.88896024),
                    new Coordinate(649687.29369493, 4182553.93225919),
                    new Coordinate(649711.71641119, 4182564.05982326),
                    new Coordinate(649665.52921436, 4182544.28489453),
                    new Coordinate(649687.29369493, 4182553.93225919)
                },
                null,
                new[]
                {
                    new Coordinate(623810.48544239, 4188405.03473900),
                    new Coordinate(623822.73600663, 4188038.21285109)
                },
                new[]
                {
                    new Coordinate(626755.25000000, 4185725.75000000),
                    new Coordinate(626924.12500000, 4185768.25000000)
                }
            };
            // Note: There is no requirement for the boundary node ID's to be consecutive negative numbers. 
            //       They just happen to be that way for this file.
            int[][] expectedBoundaryNodeIDs = new[]
            {
                Enumerable.Range(1, 105).ToArray(),
                Enumerable.Range(106, 4).ToArray(),
                Enumerable.Range(110, 3).ToArray(),
                Enumerable.Range(113, 24).ToArray(),
                new[]
                {
                    137
                },
                new[]
                {
                    138
                }
            };

            Assert.AreEqual(expectedBoundaries.Length, boundaries.Count);
            for (var index = 0; index < expectedBoundaries.Length; index++)
            {
                string boundaryName = expectedBoundaries[index];
                WaterQualityBoundary boundary = boundaries.Keys.Single(b => b.Name == boundaryName);
                Assert.IsNotNull(boundary);
                Assert.AreEqual(expectedCoordinateCounts[index], boundary.Geometry.NumPoints);
                Assert.AreEqual(expectedCoordinateCounts[index], boundary.Geometry.Coordinates.Length);
                if (expectedCoordinates[index] != null)
                {
                    for (var i = 0; i < expectedCoordinates[index].Length; i++)
                    {
                        Assert.IsTrue(expectedCoordinates[index][i].Equals2D(boundary.Geometry.Coordinates[i]));
                    }
                }

                CollectionAssert.AreEqual(expectedBoundaryNodeIDs[index], boundaries[boundary]);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFromEmptyFlowFMBoundaryFileReturnsEmptyCollection()
        {
            // setup
            var boundariesFile = new FileInfo(Path.Combine(commonFilePath, "square", "square.bnd"));

            // call
            IDictionary<WaterQualityBoundary, int[]> boundaries = BoundaryFileReader.ReadAll(boundariesFile);

            // assert
            Assert.AreEqual(0, boundaries.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFromMalformattedEmptyFlowFMBoundaryFileThrowsFormatException()
        {
            // setup
            var boundariesFile = new FileInfo(Path.Combine(commonFilePath, "faultyFiles", "completelyEmpty.bnd"));

            // call
            TestDelegate call = () => BoundaryFileReader.ReadAll(boundariesFile);

            // assert
            var exception = Assert.Throws<FormatException>(call);
            Assert.AreEqual("Error reading file: Missing statement of the number of boundaries.", exception.Message);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFromMalformattedNonIntegerFlowFMBoundaryFileThrowsFormatException()
        {
            // setup
            var boundariesFile = new FileInfo(Path.Combine(commonFilePath, "faultyFiles", "NumberOfBoundariesNotInteger.bnd"));

            // call
            TestDelegate call = () => BoundaryFileReader.ReadAll(boundariesFile);

            // assert
            var exception = Assert.Throws<FormatException>(call);
            Assert.AreEqual("Error reading file: Statement of the number of boundaries is not an integer.", exception.Message);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFromMalformattedMissingBoundaryFlowFMBoundaryFileThrowsFormatException()
        {
            // setup
            var boundariesFile = new FileInfo(Path.Combine(commonFilePath, "faultyFiles", "MissingBoundaries.bnd"));

            // call
            TestDelegate call = () => BoundaryFileReader.ReadAll(boundariesFile);

            // assert
            var exception = Assert.Throws<FormatException>(call);
            Assert.AreEqual("Error reading file: Expected number of boundaries: 2; But read: 1.", exception.Message);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFromMalformattedMissingNumberOfBoundaryNodeIdsFlowFMBoundaryFileThrowsFormatException()
        {
            // setup
            var boundariesFile = new FileInfo(Path.Combine(commonFilePath, "faultyFiles", "MissingNumberForNrOfBoundariesNodeIds.bnd"));

            // call
            TestDelegate call = () => BoundaryFileReader.ReadAll(boundariesFile);

            // assert
            var exception = Assert.Throws<FormatException>(call);
            Assert.AreEqual("Error reading file: Statement of number of boundary node ID's for boundary 'sacra_001.pli' is not an integer.", exception.Message);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFromMalformattedEarlyEndOfBoundaryNodeDataFlowFMBoundaryFileThrowsFormatException()
        {
            // setup
            var boundariesFile = new FileInfo(Path.Combine(commonFilePath, "faultyFiles", "EarlyEndOfBoundary.bnd"));

            // call
            TestDelegate call = () => BoundaryFileReader.ReadAll(boundariesFile);

            // assert
            var exception = Assert.Throws<FormatException>(call);
            Assert.AreEqual("Error reading file: Unexpected end of file while reading boundary 'sea_002.pli'.", exception.Message);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFromMalformattedBoundaryNodeDataFlowFMBoundaryFileThrowsFormatException()
        {
            // setup
            var boundariesFile = new FileInfo(Path.Combine(commonFilePath, "faultyFiles", "MalformattedBoundaryNodeDataLine.bnd"));

            // call
            TestDelegate call = () => BoundaryFileReader.ReadAll(boundariesFile);

            // assert
            var exception = Assert.Throws<FormatException>(call);
            Assert.AreEqual("Error reading file: Boundary node data line of boundary 'sea_002.pli' is not in valid format. (Expected '-<integer> <double> <double> <double> <double>', but was '-a   5243#8.84689238  4146590.925%7682   523*49.36646870  4147808.7095^919')", exception.Message);
        }
    }
}