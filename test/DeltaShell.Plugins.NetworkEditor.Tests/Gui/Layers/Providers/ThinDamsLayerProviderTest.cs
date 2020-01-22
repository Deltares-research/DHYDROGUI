using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;
using SharpMap.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Gui.Layers.Providers
{
    [TestFixture]
    public class ThinDamsLayerProviderTest
    {
        [Test]
        public void CanCreateLayerFor_SourceDataOfTypeThinDamEventedList_ParentDataOfTypeHydroArea_ReturnsTrue()
        {
            // Arrange
            var provider = new ThinDamsLayerProvider();

            // Act
            bool canCreateLayer = provider.CanCreateLayerFor(Substitute.For<EventedList<ThinDam2D>>(), Substitute.For<HydroArea>());

            // Assert
            Assert.IsTrue(canCreateLayer);
        }

        [Test]
        public void CreateLayer_ParentDataOfTypeHydroArea_ReturnsVectorLayer()
        {
            // Arrange
            var provider = new ThinDamsLayerProvider();

            // Act
            ILayer layer = provider.CreateLayer(Substitute.For<object>(), new HydroArea());

            // Assert
            Assert.That(layer, Is.TypeOf<VectorLayer>());
        }

        [Test]
        public void CreateLayer_ParentDataNotOfTypeHydroArea_ReturnsVectorLayer()
        {
            // Arrange
            var provider = new ThinDamsLayerProvider();

            // Act
            ILayer layer = provider.CreateLayer(Substitute.For<object>(), Substitute.For<object>());

            // Assert
            Assert.IsNull(layer);
        }

        [Test]
        public void GenerateChildLayerObjects_Always_ReturnsEmptyEnumerable()
        {
            // Arrange
            var provider = new ThinDamsLayerProvider();

            // Act
            IEnumerable<object> childLayerObjects = provider.GenerateChildLayerObjects(Substitute.For<object>());

            // Assert
            Assert.IsEmpty(childLayerObjects);
        }
    }
}