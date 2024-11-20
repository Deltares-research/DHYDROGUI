using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers;
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
            Assert.That(providers.Length, Is.EqualTo(13));

            IEnumerable<Type> providerTypes = providers.Select(p => p.GetType());
            Assert.That(providerTypes, Is.EqualTo(new[]
            {
                typeof(HydroAreaLayerProvider),
                typeof(HydroRegionLayerProvider),
                typeof(ThinDamsLayerProvider),
                typeof(FixedWeirsLayerProvider),
                typeof(ObservationPointsLayerProvider),
                typeof(ObservationCrossSectionsLayerProvider),
                typeof(PumpsLayerProvider),
                typeof(StructuresLayerProvider),
                typeof(LandBoundariesLayerProvider),
                typeof(DryPointsLayerProvider),
                typeof(DryAreasLayerProvider),
                typeof(EnclosuresLayerProvider),
                typeof(BridgePillarsLayerProvider)
            }));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenNetworkEditorMapLayerProvider_WhenCreatingLayersRecursively_ThenExpectedLayersAreCreated()
        {
            // Arrange
            var hydroAreaName = Guid.NewGuid().ToString();
            var hydroArea = new HydroArea { Name = hydroAreaName };

            IMapLayerProvider mapLayerProvider = NetworkEditorMapLayerProviderCreator.CreateMapLayerProvider();

            // Act
            var hydroAreaLayer = (HydroAreaLayer)MapLayerProviderHelper.CreateLayersRecursive(hydroArea, null, new[]
            {
                mapLayerProvider
            });

            // Assert
            Assert.That(hydroAreaLayer.Name, Is.EqualTo(hydroAreaName));

            IEventedList<ILayer> subLayers = hydroAreaLayer.Layers;
            Assert.That(subLayers.Count, Is.EqualTo(11));

            Assert.That(subLayers.Any(l => l.Name == HydroAreaLayerNames.ThinDamsPluralName));
            Assert.That(subLayers.Any(l => l.Name == HydroAreaLayerNames.FixedWeirsPluralName));
            Assert.That(subLayers.Any(l => l.Name == HydroAreaLayerNames.ObservationPointsPluralName));
            Assert.That(subLayers.Any(l => l.Name == HydroAreaLayerNames.ObservationCrossSectionsPluralName));
            Assert.That(subLayers.Any(l => l.Name == HydroAreaLayerNames.PumpsPluralName));
            Assert.That(subLayers.Any(l => l.Name == HydroAreaLayerNames.StructuresPluralName));
            Assert.That(subLayers.Any(l => l.Name == HydroAreaLayerNames.LandBoundariesPluralName));
            Assert.That(subLayers.Any(l => l.Name == HydroAreaLayerNames.DryPointsPluralName));
            Assert.That(subLayers.Any(l => l.Name == HydroAreaLayerNames.DryAreasPluralName));
            Assert.That(subLayers.Any(l => l.Name == HydroAreaLayerNames.EnclosureName));
            Assert.That(subLayers.Any(l => l.Name == HydroAreaLayerNames.BridgePillarsPluralName));
        }
    }
}