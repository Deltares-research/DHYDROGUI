using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Providers;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;
using SharpMap.Editors.Interactors;
using SharpMap.Layers;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Gui.Layers.Providers
{
    [TestFixture]
    public abstract class GroupableFeaturesLayerProviderTest<T> where T : IGroupableFeature, new()
    {
        [Test]
        public void CanCreateLayerFor_SourceDataOfTypeEventedList_ParentDataOfTypeHydroArea_ReturnsTrue()
        {
            // Arrange
            ILayerSubProvider provider = GetLayerSubProvider();

            // Act
            bool canCreateLayer = provider.CanCreateLayerFor(Substitute.For<EventedList<T>>(), Substitute.For<HydroArea>());

            // Assert
            Assert.IsTrue(canCreateLayer);
        }

        [Test]
        public void CreateLayer_ParentDataNotOfTypeHydroArea_ReturnsNull()
        {
            // Arrange
            ILayerSubProvider provider = GetLayerSubProvider();

            // Act
            ILayer layer = provider.CreateLayer(Substitute.For<object>(), Substitute.For<object>());

            // Assert
            Assert.IsNull(layer);
        }

        [Test]
        public void CreateLayer_ParentDataNull_ReturnsVectorLayer()
        {
            // Arrange
            ILayerSubProvider provider = GetLayerSubProvider();

            // Act
            ILayer layer = provider.CreateLayer(Substitute.For<object>(), null);

            // Assert
            Assert.IsNull(layer);
        }

        [Test]
        public void GenerateChildLayerObjects_Always_ReturnsEmptyEnumerable()
        {
            // Arrange
            ILayerSubProvider provider = GetLayerSubProvider();

            // Act
            IEnumerable<object> childLayerObjects = provider.GenerateChildLayerObjects(Substitute.For<object>());

            // Assert
            Assert.IsEmpty(childLayerObjects);
        }

        [Test]
        public void CreateLayer_ParentDataOfTypeHydroArea_ReturnsVectorLayer()
        {
            // Arrange
            ILayerSubProvider provider = GetLayerSubProvider();
            HydroArea hydroArea = CreateHydroArea();

            // Act
            var vectorLayer = provider.CreateLayer(Substitute.For<object>(), hydroArea) as VectorLayer;

            // Assert
            Assert.IsNotNull(vectorLayer);
            Assert.That(vectorLayer.FeatureEditor is Feature2DEditor);

            AssertVectorStyle(vectorLayer.Style, ExpectedVectorStyleColor(), ExpectedVectorStyleLineWidth(), ExpectedVectorStyleGeometryType());

            var hydroAreaFeature2DCollection = vectorLayer.DataSource as HydroAreaFeature2DCollection;
            Assert.IsNotNull(hydroAreaFeature2DCollection);
            Assert.That(hydroAreaFeature2DCollection.FeatureType, Is.EqualTo(typeof(T)));
            Assert.That(hydroAreaFeature2DCollection.Features, Is.EqualTo(GetStructureCollection(hydroArea)));
            Assert.That(hydroAreaFeature2DCollection.ModelName, Is.EqualTo("NetworkEditorModelName"));
            Assert.That(hydroAreaFeature2DCollection.CoordinateSystem, Is.SameAs(hydroArea.CoordinateSystem));

            Assert.IsTrue(vectorLayer.NameIsReadOnly);

            AssertLayerProviderSpecificSettings(vectorLayer);
        }

        private static void AssertVectorStyle(VectorStyle style, Color expectedLineColor, float expectedLineWidth, Type expectedGeometryType)
        {
            Assert.That(style.Line.Color, Is.EqualTo(expectedLineColor));
            Assert.That(style.Line.Width, Is.EqualTo(expectedLineWidth));
            Assert.That(style.GeometryType, Is.EqualTo(expectedGeometryType));
        }

        protected virtual void AssertLayerProviderSpecificSettings(VectorLayer vectorLayer)
        {
            // This method may be overridden in case there are specific settings to assert.
        }

        protected abstract ILayerSubProvider GetLayerSubProvider();

        protected abstract HydroArea CreateHydroArea();

        protected abstract IEventedList<T> GetStructureCollection(HydroArea hydroArea);

        protected abstract Color ExpectedVectorStyleColor();

        protected abstract float ExpectedVectorStyleLineWidth();

        protected abstract Type ExpectedVectorStyleGeometryType();
    }
}