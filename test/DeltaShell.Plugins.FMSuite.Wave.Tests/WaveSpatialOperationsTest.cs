using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro2D.Features;
using DelftTools.Hydro2D.Operations;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common;
using DeltaShell.Plugins.FMSuite.Common.Grid;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests
{
    [TestFixture]
    public class WaveSpatialOperationsTest
    {
        [Test]
        public void MergeOperationsTest()
        {
            const int nxCells = 10;
            const int nyCells = 10;
            var grid = GetGrid(nxCells, nyCells);
            var coverageDefinition = new WaveCoverageDefinition("test", new WaveDomainData("test") {Grid = grid}, d => null);
            
            var operations = new List<SpatialOperationInfo>();
            
            var geometry = new Polygon(
                new LinearRing(new ICoordinate[]
                    {
                        new Coordinate(-1, -1), new Coordinate(-1, nyCells), new Coordinate(nxCells, nyCells),
                        new Coordinate(nxCells, -1), new Coordinate(-1, -1)
                    }));
            
            var sampleInfo = new SamplesOperationInfo();
            sampleInfo.AddPointValue(0.0,0.0,1.0);
            sampleInfo.AddPointValue(0.0, nyCells-1.0, 1.0);
            sampleInfo.AddPointValue(nxCells-1.0, 0.0, 1.0);
            sampleInfo.AddPointValue(nxCells-1.0, nyCells-1.0, 1.0);
            operations.Add(sampleInfo);

            operations.Add(new PolygonOperationInfo
                {
                    Operator = Operator.Add,
                    Value = 1.5,
                    Polygons = new EventedList<Feature2D>(new[] {new Feature2D {Geometry = geometry}})
                });

            operations.Add(new PolygonOperationInfo
                {
                    Operator = Operator.Multiply,
                    Value = 10,
                    Polygons = new EventedList<Feature2D>(new[] {new Feature2D {Geometry = geometry}})
                });
           
            var result = WaveSpatialOperations.Merge(operations, coverageDefinition);

            Assert.IsNotNull(result);
            Assert.AreEqual(100, result.Points.Count);
            Assert.AreEqual(25.0, result.Points[0].Value);


        }

        private static Delft3DGrid GetGrid(int nxCells, int nyCells)
        {
            var xCoords = new List<double>();
            var yCoords = new List<double>();
            for (int i = 0; i < nxCells; ++i)
            {
                for (int j = 0; j < nyCells; ++j)
                {
                    xCoords.Add(1.0 * j);
                    yCoords.Add(1.0 * i);
                }
            }
            var grid = new Delft3DGrid(nyCells, nxCells, xCoords, yCoords, "Cartesian");
            return grid;
        }

        [Test]
        public void SetSamplesToLocations()
        {
            var operation = new SamplesOperationInfo();
            operation.AddPointValue(0.0, 0.0, 1.0);
            operation.AddPointValue(1.0, 0.0, 2.0);
            operation.AddPointValue(0.0, 1.0, 3.0);
            operation.AddPointValue(1.0, 1.0, 4.0);

            const double delta = 0.01;
            var locations = new List<ICoordinate>();
            locations.AddRange(operation.Points.Select(p => new Coordinate(p.X + delta, p.Y + delta)));

            var result = WaveSpatialOperations.SetSamplesToLocations(operation, locations, -999.0);

            Assert.AreEqual(4, result.Count);
            Assert.AreEqual(1.0, result[0].Value);
            Assert.AreEqual(2.0, result[1].Value);
            Assert.AreEqual(3.0, result[2].Value);
            Assert.AreEqual(4.0, result[3].Value);
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void SetManySamplesToManyLocationsFast()
        {
            const int sizeN = 1000;
            const int sizeM = 1000;
            const int nSamples = sizeN * sizeM;
            const double dx = 1.0;
            const double dy = 1.0;

            var gen = new Random(0);
            var locations = new List<ICoordinate>();
            var operation = new SamplesOperationInfo();
            for (var i = 0; i < sizeN; ++i)
            {
                for (int j = 0; j < sizeM; ++j)
                {
                    locations.Add(new Coordinate(j * dx, i * dy));
                }
            }

            for (int i = 0; i < nSamples; ++i)
            {
                operation.AddPointValue(gen.NextDouble() * sizeM * dx, gen.NextDouble() * sizeN * dy, 10.0);
            }

            IList<PointValue> result = null;
            TestHelper.AssertIsFasterThan(8000, () =>
            {
                result = WaveSpatialOperations.SetSamplesToLocations(operation, locations, -999);
            });

            Assert.IsNotNull(result);
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void SetSparseSamplesToManyLocationsFast()
        {
            const int sizeN = 1000;
            const int sizeM = 1000;
            const int nSamples = sizeN;
            const double dx = 1.0;
            const double dy = 1.0;

            var gen = new Random(0);
            var locations = new List<ICoordinate>();
            var operation = new SamplesOperationInfo();
            for (int i = 0; i < sizeN; ++i)
            {
                for (int j = 0; j < sizeM; ++j)
                {
                    locations.Add(new Coordinate(j * dx, i * dy));
                }
            }

            for (int i = 0; i < nSamples; ++i)
            {
                operation.AddPointValue(gen.NextDouble() * sizeM * dx, gen.NextDouble() * sizeN * dy, 10.0);
            }

            IList<PointValue> result = null;
            TestHelper.AssertIsFasterThan(3000, () =>
            {
                result = WaveSpatialOperations.SetSamplesToLocations(operation, locations, -999);
            });

            Assert.IsNotNull(result);
        }

        [Test]
        public void SetSamplesToLocationsWithDryPoints()
        {
            var operation = new SamplesOperationInfo();
            operation.AddPointValue(0.0, 0.0, 1.0);
            operation.AddPointValue(1.0, 0.0, 2.0);
            operation.AddPointValue(0.0, 1.0, 3.0);
            operation.AddPointValue(1.0, 1.0, 4.0);

            const double delta = 0;
            var locations = new List<ICoordinate>();
            locations.AddRange(operation.Points.Select(p => new Coordinate(p.X + delta, p.Y + delta)));
            locations[3].X = locations[3].Y = double.NaN;

            var result = WaveSpatialOperations.SetSamplesToLocations(operation, locations, -999.0);

            Assert.AreEqual(4, result.Count);
            Assert.AreEqual(1.0, result[0].Value);
            Assert.AreEqual(2.0, result[1].Value);
            Assert.AreEqual(3.0, result[2].Value);
            Assert.AreEqual(-999, result[3].Value);
        }


        [Test]
        [Category(TestCategory.DataAccess)]
        public void TriangulateLargeSampleSet()
        {
            var mdwPath = TestHelper.GetTestFilePath(@"wave_timespacevarbnd/tst.mdw");
            Assert.IsTrue(File.Exists(mdwPath));
            var waveModel = new WaveModel(mdwPath);

            var locations = waveModel.OuterDomain.SubDomains[0].BathymetryDefinition.DataLocations.ToList();
            var result =
                SpatialOperations.Triangulate(
                    waveModel.OuterDomain.SubDomains[0].BathymetryDefinition.Operations[0] as SamplesOperationInfo,
                    locations);

            Assert.AreEqual(locations.Count, result.Points.Count);
        }
    }
}
