using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using log4net.Core;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class UnstructuredGridCoverageExtensionsTest
    {
        [Test]
        public void VertexCoverageToPointCloudTest()
        {
            // unstructured grid: two triangles in a square
            // 2 +-----+ 3
            //   |   / |
            //   | /   |
            // 1 +-----+ 4

            IList<Coordinate> vertices = new[]
            {
                new Coordinate(0, 0),
                new Coordinate(0, 10),
                new Coordinate(10, 10),
                new Coordinate(10, 0)
            };

            var edges = new[,]
            {
                {
                    1,
                    2
                },
                {
                    2,
                    3
                },
                {
                    3,
                    4
                },
                {
                    4,
                    1
                },
                {
                    1,
                    3
                }
            };

            UnstructuredGrid grid = UnstructuredGridFactory.CreateFromVertexAndEdgeList(vertices, edges);

            var coverage = new UnstructuredGridVertexCoverage(grid, false);
            coverage.SetValues(new[]
            {
                1.0,
                2.0,
                3.0,
                4.0
            });
            coverage.Components[0].NoDataValue = -999.0;

            IPointCloud cloud = coverage.ToPointCloud();
            Assert.AreEqual(4, cloud.PointValues.Count);
        }

        [Test]
        public void CellCenterCoverageToPointCloudTest()
        {
            int[,] edges;
            int[,] cellIndices;
            IList<Coordinate> vertices = TwoTrianglesInASquareUnstructuredGrid(out edges, out cellIndices);

            UnstructuredGrid grid = UnstructuredGridFactory.CreateFromVertexAndEdgeList(vertices, edges, cellIndices);
            var coverage = new UnstructuredGridCellCoverage(grid, false);
            coverage[0] = 1.0;
            coverage[1] = 2.0;
            coverage.Components[0].NoDataValue = -999.0;

            IPointCloud cloud = coverage.ToPointCloud();
            Assert.AreEqual(2, cloud.PointValues.Count);
            Assert.AreEqual(cloud.PointValues[0].Value, 1.0);
            Assert.AreEqual(coverage.Coordinates.First(), new Coordinate(cloud.PointValues[0].X, cloud.PointValues[0].Y));
            Assert.AreEqual(cloud.PointValues[1].Value, 2.0);
            Assert.AreEqual(coverage.Coordinates.Last(), new Coordinate(cloud.PointValues[1].X, cloud.PointValues[1].Y));
        }

        [Test]
        public void ConvertingTimeDependentSpatialDataToSamplesIsNotSupportedTest()
        {
            int[,] edges;
            int[,] cellIndices;
            IList<Coordinate> vertices = TwoTrianglesInASquareUnstructuredGrid(out edges, out cellIndices);

            UnstructuredGrid grid = UnstructuredGridFactory.CreateFromVertexAndEdgeList(vertices, edges, cellIndices);
            var coverage = new UnstructuredGridCellCoverage(grid, true);
            try
            {
                coverage.ToPointCloud();
                Assert.Fail("It should have failed because of being TimeDependent");
            }
            catch (NotSupportedException e)
            {
                //All good, we want to hit this :) 
                Assert.AreEqual(
                    Resources.UnstructuredGridCoverageExtensions_ToPointCloud_Converting_time_dependent_spatial_data_to_samples_is_not_supported,
                    e.Message);
            }
            catch (Exception e)
            {
                Assert.Fail("An unexpected exception was given: {0}", e.Message);
            }
        }

        [Test]
        public void ConvertingNonDoubleValuedCoverageComponentIsNotSupportedTest()
        {
            int[,] edges;
            int[,] cellIndices;
            IList<Coordinate> vertices = TwoTrianglesInASquareUnstructuredGrid(out edges, out cellIndices);
            UnstructuredGrid grid = UnstructuredGridFactory.CreateFromVertexAndEdgeList(vertices, edges, cellIndices);

            var coverage = new UnstructuredGridCellCoverage(grid, false);
            coverage.Components.RemoveAt(0);
            coverage.Components.Add(new Variable<string>());
            try
            {
                coverage.ToPointCloud();
                Assert.Fail("It should have failed because of being DoubleValued Coverage Component");
            }
            catch (NotSupportedException e)
            {
                //All good, we want to hit this :) 
                Assert.AreEqual(
                    Resources.UnstructuredGridCoverageExtensions_ToPointCloud_Converting_a_non_double_valued_coverage_component_to_a_point_cloud_is_not_supported,
                    e.Message);
            }
            catch (Exception e)
            {
                Assert.Fail("An unexpected exception was given: {0}", e.Message);
            }
        }

        [Test]
        public void SpatialDataNeedsToBeConsistentCoordinatesMatchValuesTest()
        {
            int[,] edges;
            int[,] cellIndices;
            IList<Coordinate> vertices = TwoTrianglesInASquareUnstructuredGrid(out edges, out cellIndices);

            UnstructuredGrid grid = UnstructuredGridFactory.CreateFromVertexAndEdgeList(vertices, edges, cellIndices);
            var coverage = new UnstructuredGridCellCoverage(grid, false);
            coverage.Components[0].NoDataValue = -999.0;
            coverage.Components[0].Values.RemoveAt(0);

            try
            {
                coverage.ToPointCloud();
                Assert.Fail("It should have failed because of being not being consistent");
            }
            catch (InvalidOperationException e)
            {
                //All good, we want to hit this :) 
                Assert.AreEqual(
                    Resources.UnstructuredGridCoverageExtensions_ToPointCloud_Spatial_data_is_not_consistent__number_of_coordinate_does_not_match_number_of_values,
                    e.Message);
            }
            catch (Exception e)
            {
                Assert.Fail("An unexpected exception was given: {0}", e.Message);
            }
        }

        [TestCase(-1d)]
        [TestCase(7d)]
        public void ReplaceMissingValuesWithDefaultValues_ReplacesNoDataValuesWithDefaultValues(double defaultValue)
        {
            // Setup
            const double noDataValue = -1d;

            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 1, 1);
            var coverage = new UnstructuredGridCellCoverage(grid, false);

            IVariable component = coverage.Components[0];

            component.NoDataValue = noDataValue;
            component.DefaultValue = defaultValue;

            component.Values[0] = noDataValue;
            component.Values[1] = 1d;
            component.Values[2] = noDataValue;
            component.Values[3] = 3d;

            // Call
            coverage.ReplaceMissingValuesWithDefaultValues();

            // Assert
            Assert.That(coverage.Components[0].Values[0], Is.EqualTo(defaultValue));
            Assert.That(coverage.Components[0].Values[1], Is.EqualTo(1d));
            Assert.That(coverage.Components[0].Values[2], Is.EqualTo(defaultValue));
            Assert.That(coverage.Components[0].Values[3], Is.EqualTo(3d));
        }

        [Test]
        public void ReplaceMissingValuesWithDefaultValues_CoverageNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => ((UnstructuredGridCoverage)null).ReplaceMissingValuesWithDefaultValues();

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("coverage"));
        }

        private static IList<Coordinate> TwoTrianglesInASquareUnstructuredGrid(out int[,] edges, out int[,] cellIndices)
        {
            // unstructured grid: two triangles in a square
            // 2 +-----+ 3
            //   |   / |
            //   | /   |
            // 1 +-----+ 4

            IList<Coordinate> vertices = new[]
            {
                new Coordinate(0, 0),
                new Coordinate(0, 10),
                new Coordinate(10, 10),
                new Coordinate(10, 0)
            };

            edges = new[,]
            {
                {
                    1,
                    2
                },
                {
                    2,
                    3
                },
                {
                    3,
                    4
                },
                {
                    4,
                    1
                },
                {
                    1,
                    3
                }
            };

            cellIndices = new[,]
            {
                {
                    1,
                    2,
                    3
                },
                {
                    1,
                    3,
                    4
                }
            };
            return vertices;
        }

        [Test]
        public void LoadBathymetry_UnstructuredGridCellCoverageNull_ThrowsArgumentNullException()
        {
            UnstructuredGridCellCoverage coverage = null;
            var grid = new UnstructuredGrid();

            void Call() =>
                UnstructuredGridCoverageExtensions.LoadBathymetry(coverage, grid, "some/path.nc", -999.0);

            Assert.Throws<ArgumentNullException>(Call);
        }

        [Test]
        public void LoadBathymetry_UnstructuredGridVertexCoverageNull_ThrowsArgumentNullException()
        {
            UnstructuredGridVertexCoverage coverage = null;
            var grid = new UnstructuredGrid();

            void Call() =>
                UnstructuredGridCoverageExtensions.LoadBathymetry(coverage, grid, -999.0);

            Assert.Throws<ArgumentNullException>(Call);
        }

        [Test]
        public void LoadBathymetry_UnstructuredGridCellCoverage_GridNull_ThrowsArgumentNullException()
        {
            var grid = new UnstructuredGrid();
            var coverage = new UnstructuredGridCellCoverage(grid, false);

            void Call() =>
                UnstructuredGridCoverageExtensions.LoadBathymetry(coverage, null, "some/path.nc", -999.0);

            Assert.Throws<ArgumentNullException>(Call);
        }

        [Test]
        public void LoadBathymetry_UnstructuredGridVertexCoverage_GridNull_ThrowsArgumentNullException()
        {
            var grid = new UnstructuredGrid();
            var coverage = new UnstructuredGridVertexCoverage(grid, false);

            void Call() =>
                UnstructuredGridCoverageExtensions.LoadBathymetry(coverage, null, -999.0);

            Assert.Throws<ArgumentNullException>(Call);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void LoadBathymetry_UnstructuredGridCellCoverage_PathInvalid_ThrowsArgumentException(string path)
        {
            var grid = new UnstructuredGrid();
            var coverage = new UnstructuredGridCellCoverage(grid, false);

            void Call() =>
                UnstructuredGridCoverageExtensions.LoadBathymetry(coverage, grid, path, -999.0);

            Assert.Throws<ArgumentException>(Call);
        }

        [Test]
        public void LoadBathymetry_UnstructuredGridVertexCoverage_ExpectedResults()
        {
            // Setup
            var fixture = new Fixture();

            var grid = new UnstructuredGrid();
            var coverage = new UnstructuredGridVertexCoverage(grid, false);

            UnstructuredGrid newGrid = UnstructuredGridTestHelper.GenerateRegularGrid(4, 4, 10D, 10D);

            foreach (Coordinate vertex in newGrid.Vertices)
            {
                vertex.Z = fixture.Create<double>();
            }

            // Call
            coverage.LoadBathymetry(newGrid);

            // Assert
            Assert.That(coverage.Grid, Is.SameAs(newGrid));

            double[] retrievedValues = GetBathymetryValuesFromCoverage(coverage);

            Assert.That(retrievedValues, Is.EqualTo(newGrid.Vertices.Select(v => v.Z)));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void LoadBathymetry_UnstructuredGridCellCoverage_ExpectedResults()
        {
            // Setup
            var grid = new UnstructuredGrid();
            var coverage = new UnstructuredGridCellCoverage(grid, false);

            using (var tempDir = new TemporaryDirectory())
            {
                UnstructuredGrid newGrid = UnstructuredGridTestHelper.GenerateRegularGrid(7, 7, 10D, 10D);

                string gridSourcePath = TestHelper.GetTestFilePath("WaterFlowFMModel.MorphologicalGrid/replacement_grid.nc");
                string gridLocalPath = tempDir.CopyTestDataFileToTempDirectory(TestHelper.GetTestFilePath(gridSourcePath));

                // Call
                coverage.LoadBathymetry(newGrid, gridLocalPath);

                // Assert
                Assert.That(coverage.Grid, Is.SameAs(newGrid));

                double[] expectedValues =
                    UnstructuredGridFileHelper.ReadZValues(gridLocalPath,
                                                           UnstructuredGridFileHelper.BedLevelLocation.Faces);

                double[] retrievedValues = GetBathymetryValuesFromCoverage(coverage);

                Assert.That(retrievedValues, Is.EqualTo(expectedValues));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void LoadBathymetry_UnstructuredGridCellCoverage_NoDataInFile_MissingDataValueWrittenForAllBathymetryValues()
        {
            // Setup
            var grid = new UnstructuredGrid();
            var coverage = new UnstructuredGridCellCoverage(grid, false);

            using (var tempDir = new TemporaryDirectory())
            {
                UnstructuredGrid newGrid = UnstructuredGridTestHelper.GenerateRegularGrid(7, 7, 10D, 10D);

                string gridSourcePath = TestHelper.GetTestFilePath("WaterFlowFMModel.MorphologicalGrid/grid_without_face_data.nc");
                string gridLocalPath = tempDir.CopyTestDataFileToTempDirectory(TestHelper.GetTestFilePath(gridSourcePath));

                // Call
                void Call() => coverage.LoadBathymetry(newGrid, gridLocalPath);
                IEnumerable<string> messages = TestHelper.GetAllRenderedMessages(Call, Level.Warn);

                // Assert
                Assert.That(messages, Has.Member("No bathymetry data was found, the default D-Flow FM (-999) will be used instead."));
                Assert.That(coverage.Grid, Is.SameAs(newGrid));

                double[] retrievedValues = GetBathymetryValuesFromCoverage(coverage);

                Assert.That(retrievedValues.Length, Is.EqualTo(coverage.GetCoordinatesForGrid(newGrid).Count()));
                Assert.That(retrievedValues, Has.All.EqualTo(-999.0));
            }
        }

        public static double[] GetBathymetryValuesFromCoverage(UnstructuredGridCoverage coverage)
        {
            IMultiDimensionalArray dataSource = coverage.Components[0].Values;
            var dataReturn = new double[dataSource.Count];
            dataSource.CopyTo(dataReturn, 0);

            return dataReturn;
        }
    }
}