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
    public class NetworkEditorMapLayerProviderCreator
    {
        /// <summary>
        /// Constructs the <see cref="IMapLayerProvider"/> for the NetworkEditor plugin.
        /// </summary>
        /// <returns> A configured <see cref="IMapLayerProvider"/> for the NetworkEditor plugin. </returns>
        public IMapLayerProvider CreateMapLayerProvider()
        {
            var layerProvider = new MapLayerProvider();
            ILayerSubProvider[] subLayerProviders = GetSubLayerProviders().ToArray();

            layerProvider.RegisterSubProviders(subLayerProviders);

            return layerProvider;
        }

        internal IEnumerable<ILayerSubProvider> GetSubLayerProviders()
        {
            yield return new HydroAreaLayerProvider();
            yield return new ThinDamsLayerProvider();
            yield return new FixedWeirsLayerProvider();
            yield return new ObservationPointsLayerProvider();
            yield return new ObservationCrossSectionsLayerProvider();
            yield return new PumpsLayerProvider();
            yield return new WeirsLayerProvider();
            yield return new LandBoundariesLayerProvider();
        }
    }
}