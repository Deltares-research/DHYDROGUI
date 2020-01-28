using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Gui.Layers.Providers
{
    [TestFixture]
    public class DryAreasLayerProviderTest : FeaturesLayerProviderTest<GroupableFeature2DPolygon>
    {
        [Test]
        public void CreateLayer_AddNewFeatureFromPolygonGeometry_AddsNewFeatureToLayerDataSourceAndConnectedHydroArea()
        {
            // Arrange
            ILayerSubProvider provider = GetLayerSubProvider();
            var hydroArea = new HydroArea();
            var polygon = Substitute.For<IPolygon>();
            ILayer layer = provider.CreateLayer(Substitute.For<object>(), hydroArea);

            // Precondition
            Assert.IsNotNull(layer);

            // Act
            layer.DataSource.Add(polygon);

            // Assert
            var groupableFeature = layer.DataSource.Features[0] as GroupableFeature2DPolygon;
            Assert.IsNotNull(groupableFeature);
            Assert.That(groupableFeature.Geometry, Is.SameAs(polygon));

            Assert.That(hydroArea.DryAreas.Single(), Is.SameAs(groupableFeature));
        }

        [Test]
        [TestCaseSource(nameof(GetCoordinateCollectionsWithMin4CoordinatesAndClosedGeometry))]
        public void CreateLayer_AddNewFeatureFromNonPolygonGeometryWithMin4CoordinatesAndClosedGeometry_AddsNewFeatureToLayerDataSourceAndConnectedHydroArea(Coordinate[] coordinates)
        {
            // Arrange
            ILayerSubProvider provider = GetLayerSubProvider();
            var hydroArea = new HydroArea();

            var geometry = Substitute.For<IGeometry>();
            geometry.Coordinates.Returns(coordinates);

            ILayer layer = provider.CreateLayer(Substitute.For<object>(), hydroArea);

            // Precondition
            Assert.IsNotNull(layer);

            // Act
            layer.DataSource.Add(geometry);

            // Assert
            var groupableFeature = layer.DataSource.Features[0] as GroupableFeature2DPolygon;
            Assert.IsNotNull(groupableFeature);
            Assert.That(groupableFeature.Geometry, Is.TypeOf<Polygon>());
            Assert.That(groupableFeature.Geometry.Coordinates, Is.EqualTo(coordinates));

            Assert.That(hydroArea.DryAreas.Single(), Is.SameAs(groupableFeature));
        }

        [Test]
        [TestCaseSource(nameof(GetCoordinateCollectionsWithMax3Coordinates))]
        public void CreateLayer_AddNewFeatureFromNonPolygonGeometryWithMaximum3Coordinates_DoesNotAddNewFeatureToLayerDataSourceAndConnectedHydroArea(Coordinate[] coordinates)
        {
            // Arrange
            ILayerSubProvider provider = GetLayerSubProvider();
            var hydroArea = new HydroArea();

            var geometry = Substitute.For<IGeometry>();
            geometry.Coordinates.Returns(coordinates);

            ILayer layer = provider.CreateLayer(Substitute.For<object>(), hydroArea);

            // Precondition
            Assert.IsNotNull(layer);

            // Act
            layer.DataSource.Add(geometry);

            // Assert
            Assert.IsEmpty(layer.DataSource.Features);
            Assert.IsEmpty(hydroArea.DryAreas);
        }

        [Test]
        public void CreateLayer_AddNewFeatureFromNonPolygonGeometryWithMin4CoordinatesAndNonClosedGeometry_ThenThrowsException()
        {
            // Arrange
            ILayerSubProvider provider = GetLayerSubProvider();
            var hydroArea = new HydroArea();

            var geometry = Substitute.For<IGeometry>();
            geometry.Coordinates.Returns(new[]
            {
                new Coordinate(0,0),
                new Coordinate(1,0),
                new Coordinate(1,1),
                new Coordinate(0,1)
            });

            ILayer layer = provider.CreateLayer(Substitute.For<object>(), hydroArea);

            // Precondition
            Assert.IsNotNull(layer);

            // Act
            void Call() => layer.DataSource.Add(geometry);

            // Assert
            var exception = Assert.Throws<ArgumentException>(Call);
            Assert.That(exception.Message, Is.EqualTo("points must form a closed linestring"));
        }

        private IEnumerable<Coordinate[]> GetCoordinateCollectionsWithMax3Coordinates()
        {
            yield return new Coordinate[0];
            yield return new[]
            {
                new Coordinate(0, 0)
            };
            yield return new[]
            {
                new Coordinate(0, 0),
                new Coordinate(1, 1)
            };
            yield return new[]
            {
                new Coordinate(0, 0),
                new Coordinate(1, 1),
                new Coordinate(2, 2)
            };
        }

        private IEnumerable<Coordinate[]> GetCoordinateCollectionsWithMin4CoordinatesAndClosedGeometry()
        {
            yield return new[]
            {
                new Coordinate(0, 0),
                new Coordinate(1, 0),
                new Coordinate(1, 1),
                new Coordinate(0, 0)
            };
            yield return new[]
            {
                new Coordinate(0, 0),
                new Coordinate(1, 0),
                new Coordinate(1, 1),
                new Coordinate(0, 1),
                new Coordinate(0, 0)
            };
        }

        protected override ILayerSubProvider GetLayerSubProvider()
        {
            return new DryAreasLayerProvider();
        }

        protected override HydroArea CreateHydroArea()
        {
            var hydroArea = new HydroArea();
            hydroArea.DryAreas.Add(new GroupableFeature2DPolygon());
            hydroArea.DryAreas.Add(new GroupableFeature2DPolygon());

            return hydroArea;
        }

        protected override IEventedList<GroupableFeature2DPolygon> GetStructureCollection(HydroArea hydroArea)
        {
            return hydroArea.DryAreas;
        }

        protected override Color ExpectedVectorStyleLineColor()
        {
            return Color.FromArgb(255, 138, 43,226);
        }

        protected override float ExpectedVectorStyleLineWidth()
        {
            return 1f;
        }

        protected override Type ExpectedVectorStyleGeometryType()
        {
            return typeof(IPolygon);
        }
    }
}