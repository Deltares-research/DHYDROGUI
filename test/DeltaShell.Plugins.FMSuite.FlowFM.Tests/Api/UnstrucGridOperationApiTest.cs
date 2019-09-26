using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using GeoAPI.Geometries;
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
                       new[] { 0.0, 1.0, 1.0, -999.0 },
                       new[] { 0.0, 0.0, 1.0, -999.0 },
                       new[] { 1, 1, 1, 0 }
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