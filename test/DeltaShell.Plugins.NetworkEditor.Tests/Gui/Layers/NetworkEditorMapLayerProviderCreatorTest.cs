using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Gui.Layers
{
    [TestFixture]
    public class NetworkEditorMapLayerProviderCreatorTest
    {
        [Test]
        public void GetSubLayerProviders_ReturnsExpectedLayerProviders()
        {
            // Act
            ILayerSubProvider[] providers = NetworkEditorMapLayerProviderCreator.GetSubLayerProviders()
                                                                                .ToArray();

            // Assert
            Assert.That(providers.Length, Is.EqualTo(14));

            Assert.That(providers.Any(p => p is HydroAreaLayerProvider));
            Assert.That(providers.Any(p => p is HydroRegionLayerProvider));
            Assert.That(providers.Any(p => p is ThinDamsLayerProvider));
            Assert.That(providers.Any(p => p is FixedWeirsLayerProvider));
            Assert.That(providers.Any(p => p is ObservationPointsLayerProvider));
            Assert.That(providers.Any(p => p is ObservationCrossSectionsLayerProvider));
            Assert.That(providers.Any(p => p is PumpsLayerProvider));
            Assert.That(providers.Any(p => p is WeirsLayerProvider));
            Assert.That(providers.Any(p => p is LandBoundariesLayerProvider));
            Assert.That(providers.Any(p => p is DryPointsLayerProvider));
            Assert.That(providers.Any(p => p is DryAreasLayerProvider));
            Assert.That(providers.Any(p => p is EmbankmentsLayerProvider));
            Assert.That(providers.Any(p => p is EnclosuresLayerProvider));
            Assert.That(providers.Any(p => p is BridgePillarsLayerProvider));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenNetworkEditorMapLayerProvider_WhenCreatingLayersRecursively_ThenExpectedLayersAreCreated()
        {
            // Arrange
            string hydroAreaName = Guid.NewGuid().ToString();
            var hydroArea = new HydroArea
            {
                Name = hydroAreaName
            };

            IMapLayerProvider mapLayerProvider = NetworkEditorMapLayerProviderCreator.CreateMapLayerProvider();

            // Act
            var hydroAreaLayer = (HydroAreaLayer) MapLayerProviderHelper.CreateLayersRecursive(hydroArea, null, new[]
            {
                mapLayerProvider
            });

            // Assert
            Assert.That(hydroAreaLayer.Name, Is.EqualTo(hydroAreaName));

            IEventedList<ILayer> subLayers = hydroAreaLayer.Layers;
            Assert.That(subLayers.Count, Is.EqualTo(12));

            Assert.That(subLayers.Any(l => l.Name == HydroAreaLayerNames.ThinDamsPluralName));
            Assert.That(subLayers.Any(l => l.Name == HydroAreaLayerNames.FixedWeirsPluralName));
            Assert.That(subLayers.Any(l => l.Name == HydroAreaLayerNames.ObservationPointsPluralName));
            Assert.That(subLayers.Any(l => l.Name == HydroAreaLayerNames.ObservationCrossSectionsPluralName));
            Assert.That(subLayers.Any(l => l.Name == HydroAreaLayerNames.PumpsPluralName));
            Assert.That(subLayers.Any(l => l.Name == HydroAreaLayerNames.WeirsPluralName));
            Assert.That(subLayers.Any(l => l.Name == HydroAreaLayerNames.LandBoundariesPluralName));
            Assert.That(subLayers.Any(l => l.Name == HydroAreaLayerNames.DryPointsPluralName));
            Assert.That(subLayers.Any(l => l.Name == HydroAreaLayerNames.DryAreasPluralName));
            Assert.That(subLayers.Any(l => l.Name == HydroAreaLayerNames.EmbankmentsPluralName));
            Assert.That(subLayers.Any(l => l.Name == HydroAreaLayerNames.EnclosureName));
            Assert.That(subLayers.Any(l => l.Name == HydroAreaLayerNames.BridgePillarsPluralName));
        }
    }
}