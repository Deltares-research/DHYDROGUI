using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Api
{
    [TestFixture]
    public class UnstrucGridOperationApiTest
    {
        [Test]
        public void SnapsToGrid_WithGeometryNull_ThenReturnFalse()
        {
            // Setup
            var meshApi = MockRepository.GenerateStub<IFlexibleMeshModelApi>();
            var api = new UnstrucGridOperationApi(meshApi);

            // Call
            bool snapsToGrid = api.SnapsToGrid(null);

            // Assert
            Assert.That(snapsToGrid, Is.False);
        }

        [Test]
        public void SnapsToGrid_WithFlexibleMeshModelApiReturningGeometryValues_ThenReturnTrue()
        {
            // Setup
            var doubleValues = new double[0];
            var intValues = new int[0];
            var meshApi = MockRepository.GenerateMock<IFlexibleMeshModelApi>();
            meshApi.Expect(a => a.GetSnappedFeature(string.Empty, null, null, ref doubleValues, ref doubleValues, ref intValues))
                   .IgnoreArguments()
                   .OutRef(
                       new[]
                       {
                           0.0,
                           1.0,
                           1.0,
                           -999.0
                       },
                       new[]
                       {
                           0.0,
                           0.0,
                           1.0,
                           -999.0
                       },
                       new[]
                       {
                           1,
                           1,
                           1,
                           0
                       }
                   )
                   .Return(true);

            var coordinates = new[]
            {
                new Coordinate(0.0, 0.0),
                new Coordinate(1.0, 1.0)
            };
            var geometry = MockRepository.GenerateMock<IGeometry>();
            geometry.Expect(g => g.Coordinates)
                    .Return(coordinates);

            var api = new UnstrucGridOperationApi(meshApi);

            // Call
            bool snapsToGrid = api.SnapsToGrid(geometry);

            // Assert
            Assert.That(snapsToGrid, Is.True);
        }

        [Test]
        public void GetGridSnappedGeometry_WithFlexibleMeshModelApiReturningFalse_ThenThrowInvalidOperationException()
        {
            // Setup
            var doubleValues = new double[0];
            var intValues = new int[0];
            var meshApi = MockRepository.GenerateMock<IFlexibleMeshModelApi>();
            meshApi.Expect(a => a.GetSnappedFeature(string.Empty, null, null, ref doubleValues, ref doubleValues, ref intValues))
                   .IgnoreArguments()
                   .Return(false);

            var coordinates = new[]
            {
                new Coordinate(0.0, 0.0)
            };
            var geometry = MockRepository.GenerateMock<IGeometry>();
            geometry.Expect(g => g.Coordinates)
                    .Return(coordinates);

            var api = new UnstrucGridOperationApi(meshApi);

            // Call
            IGeometry gridSnappedGeometry = api.GetGridSnappedGeometry("featureType", geometry);

            // Assert
            Assert.That(gridSnappedGeometry, Is.SameAs(geometry));
        }

        [Test]
        public void GetGridSnappedGeometry_WithFlexibleMeshApiReturningPointValues_ThenReturnMultiplePoints()
        {
            // Setup
            var doubleValues = new double[0];
            var intValues = new int[0];
            var meshApi = MockRepository.GenerateMock<IFlexibleMeshModelApi>();
            meshApi.Expect(a => a.GetSnappedFeature(string.Empty, null, null, ref doubleValues, ref doubleValues, ref intValues))
                   .IgnoreArguments()
                   .OutRef(
                       new[]
                       {
                           0.0,
                           -999.0,
                           2.0,
                           -999.0
                       },
                       new[]
                       {
                           0.0,
                           -999.0,
                           1.0,
                           -999.0
                       },
                       new[]
                       {
                           1,
                           0,
                           2,
                           0
                       }
                   )
                   .Return(true);

            var geometry1 = new LineString(new[]
            {
                new Coordinate(0.0, 0.0),
                new Coordinate(1.0, 1.0)
            });

            var geometry2 = new LineString(new[]
            {
                new Coordinate(0.0, 0.0),
                new Coordinate(1.0, 1.0)
            });
            var geometries = new List<IGeometry>
            {
                geometry1,
                geometry2
            };

            var api = new UnstrucGridOperationApi(meshApi);

            // Call
            IGeometry[] gridSnappedGeometries = api.GetGridSnappedGeometry("featureType", geometries).ToArray();

            // Assert
            Assert.That(gridSnappedGeometries[0], Is.EqualTo(new Point(0.0, 0.0)));
            Assert.That(gridSnappedGeometries[1], Is.EqualTo(new Point(2.0, 1.0)));
        }

        [Test]
        public void GetGridSnappedGeometry_WithFlexibleMeshApiReturningLineString_ThenReturnMultipleLineStrings()
        {
            // Setup
            var doubleValues = new double[0];
            var intValues = new int[0];
            var meshApi = MockRepository.GenerateMock<IFlexibleMeshModelApi>();
            meshApi.Expect(a => a.GetSnappedFeature(string.Empty, null, null, ref doubleValues, ref doubleValues, ref intValues))
                   .IgnoreArguments()
                   .OutRef(
                       new[]
                       {
                           0.0,
                           1.0,
                           -999.0,
                           2.0,
                           3.0,
                           -999.0
                       },
                       new[]
                       {
                           0.0,
                           1.0,
                           -999.0,
                           1.0,
                           3.0,
                           -999.0
                       },
                       new[]
                       {
                           1,
                           1,
                           0,
                           2,
                           2,
                           0
                       }
                   )
                   .Return(true);

            var geometry1 = new LineString(new[]
            {
                new Coordinate(0.0, 0.0),
                new Coordinate(1.0, 1.0)
            });

            var geometry2 = new LineString(new[]
            {
                new Coordinate(2.0, 1.0),
                new Coordinate(3.0, 3.0)
            });
            var geometries = new List<IGeometry>
            {
                geometry1,
                geometry2
            };

            var api = new UnstrucGridOperationApi(meshApi);

            // Call
            IGeometry[] gridSnappedGeometries = api.GetGridSnappedGeometry("featureType", geometries).ToArray();

            // Assert
            Assert.That(gridSnappedGeometries[0], Is.EqualTo(geometry1));
            Assert.That(gridSnappedGeometries[1], Is.EqualTo(geometry2));
        }

        [Test]
        public void GetGridSnappedGeometry_WithFlexibleMeshModelApiNull_ThenReturnGeometries()
        {
            // Setup
            var geometry = MockRepository.GenerateMock<IGeometry>();
            var api = new UnstrucGridOperationApi(null);

            // Call
            IGeometry returnedGeometry = api.GetGridSnappedGeometry(string.Empty, geometry);

            // Assert
            Assert.That(returnedGeometry, Is.SameAs(geometry));
        }

        [Test]
        public void GetGridSnappedGeometry_WithFeatureTypeEqualToNoSnapping_ThenReturnGeometries()
        {
            // Setup
            var meshApi = MockRepository.GenerateMock<IFlexibleMeshModelApi>();
            var geometry = MockRepository.GenerateMock<IGeometry>();
            var api = new UnstrucGridOperationApi(meshApi);

            // Call
            IGeometry returnedGeometry = api.GetGridSnappedGeometry("no_snap", geometry);

            // Assert
            Assert.That(returnedGeometry, Is.SameAs(geometry));
        }

        [Test]
        public void GetGridSnappedGeometry_WithEmptyGeometryCollection_ThenReturnGeometries()
        {
            // Setup
            var meshApi = MockRepository.GenerateMock<IFlexibleMeshModelApi>();
            var geometry = new List<IGeometry>();
            var api = new UnstrucGridOperationApi(meshApi);

            // Call
            IEnumerable<IGeometry> returnedGeometry = api.GetGridSnappedGeometry(string.Empty, geometry);

            // Assert
            Assert.That(returnedGeometry, Is.SameAs(geometry));
        }
    }
}