using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Collections.Extensions;
using DeltaShell.NGHS.Common;
using DeltaShell.NGHS.Common.Gui;
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
        private readonly IList<ILayerSubProvider> subProviders = new List<ILayerSubProvider>();

        public ILayer CreateLayer(object data, object parentData)
        {
            ILayerSubProvider validSubProvider = subProviders.FirstOrDefault(lp => lp.CanCreateLayerFor(data, parentData));
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
        /// Register the provided <paramref name="providers"/> within this <see cref="WaveMapLayerProvider"/>.
        /// </summary>
        /// <param name="providers"> The providers to be registered. </param>
        public void RegisterSubProviders(IList<ILayerSubProvider> providers)
        {
            Ensure.DoesNotContainNullObjects(providers, nameof(providers));
            subProviders.AddRange(providers);
        }
    }
}