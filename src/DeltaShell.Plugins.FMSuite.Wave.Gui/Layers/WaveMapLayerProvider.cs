using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Gui;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers
{
    /// <summary>
    /// <see cref="WaveMapLayerProvider"/> provides the layers of the Wave plugin, it does so through
    /// its sub providers.
    /// </summary>
    /// <seealso cref="IMapLayerProvider" />
    public class WaveMapLayerProvider : IMapLayerProvider
    {
        private readonly IList<IWaveLayerSubProvider> subProviders = new List<IWaveLayerSubProvider>();

        public ILayer CreateLayer(object data, object parentData)
        {
            IWaveLayerSubProvider validSubProvider = subProviders.FirstOrDefault(lp => lp.CanCreateLayerFor(data, parentData));
            return validSubProvider?.CreateLayer(data, parentData);
        }

        public bool CanCreateLayerFor(object data, object parentData)
        {
            return subProviders.Any(lp => lp.CanCreateLayerFor(data, parentData));
        }

        public IEnumerable<object> ChildLayerObjects(object data)
        {
            return subProviders.SelectMany(lp => lp.GenerateChildLayerObjects(data));
        }

        /// <summary>
        /// Register the provided <paramref name="provider"/> within this <see cref="WaveMapLayerProvider"/>.
        /// </summary>
        /// <param name="provider"> The provider to be registered. </param>
        public void RegisterSubProvider(IWaveLayerSubProvider provider)
        {
            Ensure.NotNull(provider, nameof(provider));
            subProviders.Add(provider);
        }
    }
}