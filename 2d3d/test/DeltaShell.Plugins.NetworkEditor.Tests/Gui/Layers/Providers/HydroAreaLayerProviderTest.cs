using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Gui.Layers.Providers
{
    [TestFixture]
    public class HydroAreaLayerProviderTest
    {
        [Test]
        public void CanCreateLayerFor_SourceDataOfTypeHydroArea_ReturnsTrue()
        {
            // Arrange
            var provider = new HydroAreaLayerProvider();

            // Act
            bool canCreateLayer = provider.CanCreateLayerFor(Substitute.For<HydroArea>(), Substitute.For<object>());

            // Assert
            Assert.IsTrue(canCreateLayer);
        }

        [Test]
        public void CreateLayer_SourceDataOfTypeHydroArea_ReturnsAreaLayer()
        {
            // Arrange
            var provider = new HydroAreaLayerProvider();

            // Act
            ILayer layer = provider.CreateLayer(Substitute.For<HydroArea>(), Substitute.For<object>());

            // Assert
            Assert.That(layer, Is.TypeOf<HydroAreaLayer>());
        }

        [Test]
        public void CreateLayer_SourceDataNotOfTypeHydroArea_ReturnsNull()
        {
            // Arrange
            var provider = new HydroAreaLayerProvider();

            // Act
            ILayer layer = provider.CreateLayer(Substitute.For<object>(), Substitute.For<object>());

            // Assert
            Assert.IsNull(layer);
        }

        [Test]
        public void CreateAreaLayer_ValidHydroArea_ReturnsExpectedHydroAreaLayer()
        {
            // Arrange
            var provider = new HydroAreaLayerProvider();
            var hydroArea = new HydroArea();

            // Act
            var areaLayer = provider.CreateLayer(hydroArea, Substitute.For<object>()) as HydroAreaLayer;

            // Assert
            Assert.IsNotNull(areaLayer);
            Assert.That(areaLayer.HydroArea, Is.SameAs(hydroArea));
            Assert.IsTrue(areaLayer.NameIsReadOnly);
        }

        [Test]
        [TestCaseSource(nameof(GetTypes))]
        public void GenerateChildLayerObjects_DataOfTypeHydroArea_ReturnsExpectedCollections(Type structureCollectionType)
        {
            // Arrange
            var provider = new HydroAreaLayerProvider();

            // Act
            object[] childLayerObjects = provider.GenerateChildLayerObjects(new HydroArea()).ToArray();

            // Assert
            Assert.IsTrue(childLayerObjects.Any(o => o.GetType() == structureCollectionType));
        }

        [Test]
        public void GenerateChildLayerObjects_DataNotOfTypeHydroArea_ReturnsEmptyEnumerable()
        {
            // Arrange
            var provider = new HydroAreaLayerProvider();

            // Act
            IEnumerable<object> childLayerObjects = provider.GenerateChildLayerObjects(Substitute.For<object>());

            // Assert
            Assert.IsEmpty(childLayerObjects);
        }

        private static IEnumerable<Type> GetTypes()
        {
            yield return typeof(EventedList<ThinDam2D>);
            yield return typeof(EventedList<FixedWeir>);
            yield return typeof(EventedList<GroupableFeature2DPoint>);
            yield return typeof(EventedList<ObservationCrossSection2D>);
            yield return typeof(EventedList<Pump>);
            yield return typeof(EventedList<Structure>);
            yield return typeof(EventedList<LandBoundary2D>);
            yield return typeof(EventedList<GroupablePointFeature>);
            yield return typeof(EventedList<GroupableFeature2DPolygon>);
            yield return typeof(EventedList<BridgePillar>);
        }
    }
}