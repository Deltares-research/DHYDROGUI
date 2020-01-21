using DelftTools.Shell.Gui;

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
            var provider = new NetworkEditorMapLayerProvider();

            return provider;
        }
    }
}