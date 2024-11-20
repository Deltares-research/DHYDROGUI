using System;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.Utils
{
    [TestFixture]
    public class PointToGridCellMapperTest
    {
        [Test]
        public void GetCellIndexWithoutGridSetThrowsInvalidOperationException()
        {
            // setup
            var relativeThicknesses = new[]
            {
                1.0
            };
            var mapper = new PointToGridCellMapper();
            mapper.SetSigmaLayers(relativeThicknesses);

            // call
            TestDelegate call = () => mapper.GetWaqSegmentIndex(4.5, 17.8, 0.5);

            // assert
            var exception = Assert.Throws<InvalidOperationException>(call);
            Assert.AreEqual("Cannot determine cell index as no grid was set.", exception.Message);
        }

        [Test]
        public void GetCellIndexWithoutLayersSetThrowsInvalidOperationException()
        {
            // setup
            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 10, 10);
            var mapper = new PointToGridCellMapper {Grid = grid};

            // call
            TestDelegate call = () => mapper.GetWaqSegmentIndex(4.5, 17.8, 0.5);

            // assert
            var exception = Assert.Throws<InvalidOperationException>(call);
            Assert.AreEqual("Cannot determine cell index as no layer data was provided.", exception.Message);
        }

        [Test]
        public void GetCellIndexForLocationOutsideGridThrowsArgumentException([Values(0.0 - double.Epsilon, 20.0 + double.Epsilon)]
                                                                              double x,
                                                                              [Values(0.0 - double.Epsilon, 20.0 + double.Epsilon)]
                                                                              double y)
        {
            // setup
            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 10, 10);
            var relativeThicknesses = new[]
            {
                1.0
            };
            var mapper = new PointToGridCellMapper {Grid = grid};
            mapper.SetSigmaLayers(relativeThicknesses);

            // call
            const double height = 0.5;
            TestDelegate call = () => mapper.GetWaqSegmentIndex(x, y, height);

            // assert
            var exception = Assert.Throws<ArgumentException>(call);
            string expectedMessage = string.Format("Point ({0}, {1}, {2}) is not within grid or has ambiguous location (on a grid edge or grid vertex).", x, y, height);
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        [Test]
        [TestCase(0.1, 3)]
        [TestCase(0.999 - 1e-4, 1)] // Boundary case
        [TestCase(1.001 + 1e-4, 1)] // Boundary case
        [TestCase(0.1, 20)]
        public void SetSigmaLayersWhereRelativeThicknessesDoNotAddUpToOneThrowsArgumentException(double relativeThickness, int nrOfLayers)
        {
            // setup
            var relativeThicknesses = new double[nrOfLayers];
            for (var i = 0; i < nrOfLayers; i++)
            {
                relativeThicknesses[i] = relativeThickness;
            }

            var mapper = new PointToGridCellMapper();

            // call
            TestDelegate call = () => mapper.SetSigmaLayers(relativeThicknesses);

            // assert
            var exception = Assert.Throws<ArgumentException>(call);
            string expectedMessage = string.Format("Sigma layers should add up to ~1.0, but was adding up to {0}.",
                                                   relativeThickness * nrOfLayers);
            expectedMessage += Environment.NewLine + string.Format("Parameter name: {0}", exception.ParamName);
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        [Test]
        [TestCase(0.999, 1)] // Boundary case
        [TestCase(0.25, 4)]
        [TestCase(1.001, 1)] // Boundary case
        public void SetSigmaLayersToValidRange(double relativeThickness, int nrOfLayers)
        {
            // setup
            var relativeThicknesses = new double[nrOfLayers];
            for (var i = 0; i < nrOfLayers; i++)
            {
                relativeThicknesses[i] = relativeThickness;
            }

            var mapper = new PointToGridCellMapper();

            // call
            TestDelegate call = () => mapper.SetSigmaLayers(relativeThicknesses);

            // assert
            Assert.DoesNotThrow(call);
        }

        [Test]
        [TestCase(0.1, 3)]
        [TestCase(0.999 - 1e-4, 1)] // Boundary case
        [TestCase(1.001 + 1e-4, 1)] // Boundary case
        [TestCase(0.1, 20)]
        public void SetZlayersWhereRelativeThicknessesDoNotAddUpToOneThrowsArgumentException(double relativeThickness, int nrOfLayers)
        {
            // setup
            var relativeThicknesses = new double[nrOfLayers];
            for (var i = 0; i < nrOfLayers; i++)
            {
                relativeThicknesses[i] = relativeThickness;
            }

            var mapper = new PointToGridCellMapper();

            // call
            TestDelegate call = () => mapper.SetZLayers(relativeThicknesses, 1.0, 7.7);

            // assert
            var exception = Assert.Throws<ArgumentException>(call);
            string expectedMessage = string.Format("Z layers should add up to ~1.0, but was adding up to {0}.",
                                                   relativeThickness * nrOfLayers);
            expectedMessage += Environment.NewLine + string.Format("Parameter name: {0}", exception.ParamName);
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        [Test]
        [TestCase(0.999, 1)] // Boundary case
        [TestCase(0.25, 4)]
        [TestCase(1.001, 1)] // Boundary case
        public void SetZLayersToValidRange(double relativeThickness, int nrOfLayers)
        {
            // setup
            var relativeThicknesses = new double[nrOfLayers];
            for (var i = 0; i < nrOfLayers; i++)
            {
                relativeThicknesses[i] = relativeThickness;
            }

            var mapper = new PointToGridCellMapper();

            // call
            TestDelegate call = () => mapper.SetZLayers(relativeThicknesses, 1.0, 7.7);

            // assert
            Assert.DoesNotThrow(call);
        }

        [Test]
        [TestCase(-2.5)]
        [TestCase(0.0 - 1e-6)]
        [TestCase(1.0 + 1e-6)]
        [TestCase(4.8)]
        public void GetCellIndexForSigmaModelWithInvalidZThrowsArgumentOutOfRangeException(double height)
        {
            // setup
            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 10, 10);
            var relativeThicknesses = new[]
            {
                1.0
            };
            var mapper = new PointToGridCellMapper {Grid = grid};
            mapper.SetSigmaLayers(relativeThicknesses);

            // call
            TestDelegate call = () => mapper.GetWaqSegmentIndex(4.5, 17.8, height);

            // assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(call);
            string expectedMessage = string.Format("Height of point must be in range [0, 1] for sigma models, but was {0}.", height);
            expectedMessage += Environment.NewLine + "Parameter name: z";
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        [Test]
        [TestCase(-2.5)]
        [TestCase(1.0 - 1e-6)]
        [TestCase(7.7 + 1e-6)]
        [TestCase(12.34)]
        public void GetCellIndexForZLayerModelWithInvalidZThrowsArgumentOutOfRangeException(double height)
        {
            // setup
            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 10, 10);
            var relativeThicknesses = new[]
            {
                1.0
            };
            var mapper = new PointToGridCellMapper {Grid = grid};
            mapper.SetZLayers(relativeThicknesses, 1.0, 7.7);

            // call
            TestDelegate call = () => mapper.GetWaqSegmentIndex(4.5, 17.8, height);

            // assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(call);
            string expectedMessage = string.Format("Height of point must be in range [1, 7.7] for Z-layer models, but was {0}.", height);
            expectedMessage += Environment.NewLine + "Parameter name: z";
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        [Test]
        [TestCase(0.0)]
        [TestCase(0.33)]
        [TestCase(1.0)]
        public void GetCellIndexFor2DSigmaModelForVariousZTest(double height)
        {
            // setup
            // Grid:
            // O----O----O
            // | 3  | 4  |
            // |    |    |
            // O----O----O
            // |  1 | 2  |
            // |    |    |
            // O----O----O

            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 10, 10);
            var relativeThicknesses = new[]
            {
                1.0
            };
            var mapper = new PointToGridCellMapper {Grid = grid};
            mapper.SetSigmaLayers(relativeThicknesses);

            // call
            int index = mapper.GetWaqSegmentIndex(4.5, 17.8, height);

            // assert
            Assert.AreEqual(3, index);
        }

        [Test]
        [TestCase(1.2, 2.3, 1)]
        [TestCase(10.5, 4.5, 2)]
        [TestCase(0.1, 18.2, 3)]
        [TestCase(17.3, 15.9, 4)]
        public void GetCellIndexFor2DSigmaModelForVariousXYTest(double x, double y, int expectedIndex)
        {
            // setup
            // Grid:
            // O----O----O
            // | 3  | 4  |
            // |    |    |
            // O----O----O
            // |  1 | 2  |
            // |    |    |
            // O----O----O

            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 10, 10);
            var relativeThicknesses = new[]
            {
                1.0
            };
            var mapper = new PointToGridCellMapper {Grid = grid};
            mapper.SetSigmaLayers(relativeThicknesses);

            // call
            int index = mapper.GetWaqSegmentIndex(x, y, 0.5);

            // assert
            Assert.AreEqual(expectedIndex, index);
        }

        [Test]
        [TestCase(0.0, 3)]
        [TestCase(0.12, 3)]
        [TestCase(0.3333 - 1e-6, 3)]
        [TestCase(0.3333 + 1e-6, 7)]
        [TestCase(0.57, 7)]
        [TestCase(0.6666 - 1e-6, 7)]
        [TestCase(0.6666 + 1e-6, 11)]
        [TestCase(0.78, 11)]
        [TestCase(1.0, 11)]
        public void GetCellIndexFor3DSigmaModelForVariousZTest(double height, int expectedIndex)
        {
            // setup
            // Grid:
            // O----O----O
            // | 3  | 4  |
            // |    |    |
            // O----O----O
            // |  1 | 2  |
            // |    |    |
            // O----O----O
            // 3 Layers, each 1/3 of the height.

            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 10, 10);
            var relativeThicknesses = new[]
            {
                0.3333,
                0.3333,
                0.3333
            };
            var mapper = new PointToGridCellMapper {Grid = grid};
            mapper.SetSigmaLayers(relativeThicknesses);

            // call
            int index = mapper.GetWaqSegmentIndex(4.5, 17.8, height);

            // assert
            Assert.AreEqual(expectedIndex, index);
        }

        [Test]
        [TestCase(1.1, 2.2, 0.2, 1)]
        [TestCase(10.2, 9.9, 0.5, 6)]
        [TestCase(19.2, 18.3, 1.0, 12)]
        public void GetCellIndexFor3DSigmaModelForVariousZTest(double x, double y, double z, int expectedIndex)
        {
            // setup
            // Grid:
            // O----O----O
            // | 3  | 4  |
            // |    |    |
            // O----O----O
            // |  1 | 2  |
            // |    |    |
            // O----O----O
            // 3 Layers, each 1/3 of the height.

            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 10, 10);
            var relativeThicknesses = new[]
            {
                0.3333,
                0.3333,
                0.3333
            };
            var mapper = new PointToGridCellMapper {Grid = grid};
            mapper.SetSigmaLayers(relativeThicknesses);

            // call
            int index = mapper.GetWaqSegmentIndex(x, y, z);

            // assert
            Assert.AreEqual(expectedIndex, index);
        }

        [Test]
        [TestCase(1.0)]
        [TestCase(3.5)]
        [TestCase(7.7)]
        public void GetCellIndexFor2DZLayerModelForVariousZTest(double height)
        {
            // setup
            // Grid:
            // O----O----O
            // | 3  | 4  |
            // |    |    |
            // O----O----O
            // |  1 | 2  |
            // |    |    |
            // O----O----O

            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 10, 10);
            var relativeThicknesses = new[]
            {
                1.0
            };
            var mapper = new PointToGridCellMapper {Grid = grid};
            mapper.SetZLayers(relativeThicknesses, 1.0, 7.7);

            // call
            int index = mapper.GetWaqSegmentIndex(4.5, 17.8, height);

            // assert
            Assert.AreEqual(3, index);
        }

        [Test]
        [TestCase(1.2, 2.3, 1)]
        [TestCase(10.5, 4.5, 2)]
        [TestCase(0.1, 18.2, 3)]
        [TestCase(17.3, 15.9, 4)]
        public void GetCellIndexFor2DZLayerModelForVariousXYTest(double x, double y, int expectedIndex)
        {
            // setup
            // Grid:
            // O----O----O
            // | 3  | 4  |
            // |    |    |
            // O----O----O
            // |  1 | 2  |
            // |    |    |
            // O----O----O

            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 10, 10);
            var relativeThicknesses = new[]
            {
                1.0
            };
            var mapper = new PointToGridCellMapper {Grid = grid};
            mapper.SetZLayers(relativeThicknesses, 1.0, 7.7);

            // call
            int index = mapper.GetWaqSegmentIndex(x, y, 5.7);

            // assert
            Assert.AreEqual(expectedIndex, index);
        }

        [Test]
        [TestCase(1.0, 3)]
        [TestCase(1.7, 3)]
        [TestCase((1.0 + (0.3333 * (7.7 - 1.0))) - 1e-6, 3)]
        [TestCase(1.0 + (0.3333 * (7.7 - 1.0)) + 1e-6, 7)]
        [TestCase(4.1, 7)]
        [TestCase((1.0 + (0.6666 * (7.7 - 1.0))) - 1e-6, 7)]
        [TestCase(1.0 + (0.6666 * (7.7 - 1.0)) + 1e-6, 11)]
        [TestCase(6.8, 11)]
        [TestCase(7.7, 11)]
        public void GetCellIndexFor3DZLayerModelForVariousZTest(double height, int expectedIndex)
        {
            // setup
            // Grid:
            // O----O----O
            // | 3  | 4  |
            // |    |    |
            // O----O----O
            // |  1 | 2  |
            // |    |    |
            // O----O----O
            // 3 Layers, each 1/3 of the height.

            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 10, 10);
            var relativeThicknesses = new[]
            {
                0.3333,
                0.3333,
                0.3333
            };
            var mapper = new PointToGridCellMapper {Grid = grid};
            mapper.SetZLayers(relativeThicknesses, 1.0, 7.7);

            // call
            int index = mapper.GetWaqSegmentIndex(4.5, 17.8, height);

            // assert
            Assert.AreEqual(expectedIndex, index);
        }

        [Test]
        [TestCase(1.1, 2.2, 1.99, 1)]
        [TestCase(10.2, 9.9, 3.8, 6)]
        [TestCase(19.2, 18.3, 7.1, 12)]
        public void GetCellIndexFor3DZLayerModelForVariousZTest(double x, double y, double z, int expectedIndex)
        {
            // setup
            // Grid:
            // O----O----O
            // | 3  | 4  |
            // |    |    |
            // O----O----O
            // |  1 | 2  |
            // |    |    |
            // O----O----O
            // 3 Layers, each 1/3 of the height.

            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 10, 10);
            var relativeThicknesses = new[]
            {
                0.3333,
                0.3333,
                0.3333
            };
            var mapper = new PointToGridCellMapper {Grid = grid};
            mapper.SetZLayers(relativeThicknesses, 1.0, 7.7);

            // call
            int index = mapper.GetWaqSegmentIndex(x, y, z);

            // assert
            Assert.AreEqual(expectedIndex, index);
        }
    }
}