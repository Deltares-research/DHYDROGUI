using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers;
using GeoAPI.Extensions.Feature;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;
using SharpMap.Editors.Interactors;
using SharpMap.Layers;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Gui.Layers.Providers
{
    [TestFixture]
    public abstract class FeaturesLayerProviderTest<T> where T : IFeature, new()
    {
        [Test]
        public virtual void CanCreateLayerFor_SourceDataOfTypeEventedList_ParentDataOfTypeHydroArea_ReturnsTrue()
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

            AssertVectorStyle(vectorLayer.Style);

            var hydroAreaFeature2DCollection = vectorLayer.DataSource as HydroAreaFeature2DCollection;
            Assert.IsNotNull(hydroAreaFeature2DCollection);
            Assert.That(hydroAreaFeature2DCollection.FeatureType, Is.EqualTo(typeof(T)));
            Assert.That(hydroAreaFeature2DCollection.Features, Is.EqualTo(GetStructureCollection(hydroArea)));
            Assert.That(hydroAreaFeature2DCollection.ModelName, Is.EqualTo("NetworkEditorModelName"));
            Assert.That(hydroAreaFeature2DCollection.CoordinateSystem, Is.SameAs(hydroArea.CoordinateSystem));

            Assert.IsTrue(vectorLayer.NameIsReadOnly);

            AssertLayerProviderSpecificSettings(vectorLayer);
        }

        private void AssertVectorStyle(VectorStyle style)
        {
            Assert.That(style.Line.Color, Is.EqualTo(ExpectedVectorStyleLineColor()));
            Assert.That(style.Line.Width, Is.EqualTo(ExpectedVectorStyleLineWidth()));
            Assert.That(style.GeometryType, Is.EqualTo(ExpectedVectorStyleGeometryType()));
        }

        protected virtual void AssertLayerProviderSpecificSettings(VectorLayer vectorLayer)
        {
            // This method may be overridden in case there are specific settings to assert.
        }

        protected abstract ILayerSubProvider GetLayerSubProvider();

        protected abstract HydroArea CreateHydroArea();

        protected abstract IEventedList<T> GetStructureCollection(HydroArea hydroArea);

        protected abstract Color ExpectedVectorStyleLineColor();

        protected abstract float ExpectedVectorStyleLineWidth();

        protected abstract Type ExpectedVectorStyleGeometryType();
    }
}