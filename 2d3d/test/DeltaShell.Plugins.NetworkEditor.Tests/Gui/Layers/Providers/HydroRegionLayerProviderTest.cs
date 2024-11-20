using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers;
using GeoAPI.Extensions.Feature;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Gui.Layers.Providers
{
    [TestFixture]
    public class HydroRegionLayerProviderTest
    {
        [Test]
        [TestCaseSource(nameof(GetCanCreateLayerForData))]
        public void CanCreateLayerFor_ExpectedResults(object sourceData, object parentData, bool expectedResult)
        {
            // Setup
            var provider = new HydroRegionLayerProvider();

            // Call
            bool canCreateLayer = provider.CanCreateLayerFor(sourceData, parentData);

            // Assert
            Assert.That(canCreateLayer, Is.EqualTo(expectedResult));
        }

        [Test]
        public void CreateLayer_SourceDataHydroRegion_ReturnsHydroRegionMapLayer()
        {
            // Setup
            var provider = new HydroRegionLayerProvider();
            var sourceData = Substitute.For<IHydroRegion>();
            sourceData.Name = "aRegion";

            // Call
            ILayer layer = provider.CreateLayer(sourceData, null);

            // Assert
            Assert.That(layer, Is.InstanceOf<HydroRegionMapLayer>());
            var hydroRegionLayer = (HydroRegionMapLayer) layer;

            Assert.That(hydroRegionLayer.Name, Is.EqualTo(sourceData.Name));
            Assert.That(hydroRegionLayer.Region, Is.SameAs(sourceData));
            Assert.That(hydroRegionLayer.LayersReadOnly, Is.EqualTo(true));
        }

        [Test]
        public void CreateLayer_SourceDataNotOfTypeHydroRegion_ReturnsNull()
        {
            // Setup
            var provider = new HydroRegionLayerProvider();

            // Call
            ILayer layer = provider.CreateLayer(Substitute.For<object>(), Substitute.For<object>());

            // Assert
            Assert.IsNull(layer);
        }

        [Test]
        public void GenerateChildLayerObjects_ReturnsSubRegions()
        {
            // Setup
            var provider = new HydroRegionLayerProvider();
            var sourceData = Substitute.For<IHydroRegion>();
            sourceData.SubRegions = new EventedList<IRegion>()
            {
                Substitute.For<IRegion>(),
                Substitute.For<IRegion>(),
                Substitute.For<IRegion>()
            };

            // Call
            object[] childLayerObjects = provider.GenerateChildLayerObjects(sourceData).ToArray();

            // Assert
            Assert.That(childLayerObjects, Is.EquivalentTo(sourceData.SubRegions));
        }

        [Test]
        public void GenerateChildLayerObjects_DataNotOfTypeHydroArea_ReturnsEmptyEnumerable()
        {
            // Setup
            var provider = new HydroRegionLayerProvider();

            // Call
            IEnumerable<object> childLayerObjects = provider.GenerateChildLayerObjects(Substitute.For<object>());

            // Assert
            Assert.IsEmpty(childLayerObjects);
        }

        private static IEnumerable<TestCaseData> GetCanCreateLayerForData()
        {
            var sourceData = Substitute.For<IHydroRegion>();
            var parentData = new object();

            var incorrectSourceData = new object();

            yield return new TestCaseData(sourceData, parentData, true);
            yield return new TestCaseData(sourceData, null, true);
            yield return new TestCaseData(null, null, false);
            yield return new TestCaseData(new object(), null, false);
            yield return new TestCaseData(incorrectSourceData, parentData, false);
            yield return new TestCaseData(incorrectSourceData, null, false);
        }
    }
}