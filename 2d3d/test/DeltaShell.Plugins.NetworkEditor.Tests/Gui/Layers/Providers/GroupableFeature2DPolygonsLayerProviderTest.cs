using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.GroupableFeatures;
using DeltaShell.NGHS.Common.Gui.Layers;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Gui.Layers.Providers
{
    [TestFixture]
    public abstract class GroupableFeature2DPolygonsLayerProviderTest : FeaturesLayerProviderTest<GroupableFeature2DPolygon>
    {
        [Test]
        public override void CanCreateLayerFor_SourceDataOfTypeEventedList_ParentDataOfTypeHydroArea_ReturnsTrue()
        {
            // Arrange
            ILayerSubProvider provider = GetLayerSubProvider();
            HydroArea hydroArea = CreateHydroArea();

            // Act
            bool canCreateLayer = provider.CanCreateLayerFor(GetStructureCollection(hydroArea), hydroArea);

            // Assert
            Assert.IsTrue(canCreateLayer);
        }

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

            Assert.That(GetStructureCollection(hydroArea).Single(), Is.SameAs(groupableFeature));
        }

        [Test]
        public void CreateLayer_AddNewFeatureFromNonPolygonGeometryWithMin4CoordinatesAndClosedGeometry_AddsNewFeatureToLayerDataSourceAndConnectedHydroArea()
        {
            // Arrange
            ILayerSubProvider provider = GetLayerSubProvider();
            var hydroArea = new HydroArea();

            var geometry = Substitute.For<IGeometry>();
            var coordinates = new[]
            {
                new Coordinate(0, 0),
                new Coordinate(1, 0),
                new Coordinate(1, 1),
                new Coordinate(0, 0)
            };
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

            Assert.That(GetStructureCollection(hydroArea).Single(), Is.SameAs(groupableFeature));
        }

        [Test]
        public void CreateLayer_AddNewFeatureFromNonPolygonGeometryWithMaximum3Coordinates_DoesNotAddNewFeatureToLayerDataSourceAndConnectedHydroArea()
        {
            // Arrange
            ILayerSubProvider provider = GetLayerSubProvider();
            var hydroArea = new HydroArea();

            var geometry = Substitute.For<IGeometry>();
            geometry.Coordinates.Returns(new[]
            {
                new Coordinate(0, 0),
                new Coordinate(1, 1),
                new Coordinate(2, 2)
            });

            ILayer layer = provider.CreateLayer(Substitute.For<object>(), hydroArea);

            // Precondition
            Assert.IsNotNull(layer);

            // Act
            layer.DataSource.Add(geometry);

            // Assert
            Assert.IsEmpty(layer.DataSource.Features);
            Assert.IsEmpty(GetStructureCollection(hydroArea));
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
                new Coordinate(0, 0),
                new Coordinate(1, 0),
                new Coordinate(1, 1),
                new Coordinate(0, 1)
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
    }
}