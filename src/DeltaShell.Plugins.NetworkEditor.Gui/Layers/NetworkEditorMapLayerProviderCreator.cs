using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Gui;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers
{
    /// <summary>
    /// <see cref="NetworkEditorMapLayerProviderCreator"/> provides the methods to
    /// construct a configured <see cref="IMapLayerProvider"/> for the
    /// NetworkEditor plugin.
    /// </summary>
    internal static class NetworkEditorMapLayerProviderCreator
    {
        /// <summary>
        /// Constructs the <see cref="IMapLayerProvider"/> for the NetworkEditor plugin.
        /// </summary>
        /// <returns> A configured <see cref="IMapLayerProvider"/> for the NetworkEditor plugin. </returns>
        public static IMapLayerProvider CreateMapLayerProvider()
        {
            var layerProvider = new MapLayerProvider();
            ILayerSubProvider[] subLayerProviders = GetSubLayerProviders().ToArray();

            layerProvider.RegisterSubProviders(subLayerProviders);

            return layerProvider;
        }

        internal static IEnumerable<ILayerSubProvider> GetSubLayerProviders()
        {
            yield return new HydroAreaLayerProvider();
            yield return new HydroRegionLayerProvider();
            yield return new ThinDamsLayerProvider();
            yield return new FixedWeirsLayerProvider();
            yield return new ObservationPointsLayerProvider();
            yield return new ObservationCrossSectionsLayerProvider();
            yield return new PumpsLayerProvider();
            yield return new StructuresLayerProvider();
            yield return new LandBoundariesLayerProvider();
            yield return new DryPointsLayerProvider();
            yield return new DryAreasLayerProvider();
            yield return new EnclosuresLayerProvider();
            yield return new BridgePillarsLayerProvider();
        }
    }
}