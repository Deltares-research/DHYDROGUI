using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Extensions;
using Deltares.Infrastructure.API.Guards;
using SharpMap.Api.Layers;

namespace DeltaShell.NGHS.Common.Gui.MapLayers.Providers
{
    /// <summary>
    /// Provider of logic for creating the layers, typically for a specific plugin.
    /// It creates the layers through its sub layer providers.
    /// </summary>
    public sealed class MapLayerProvider : IMapLayerProvider
    {
        private readonly IList<ILayerSubProvider> subProviders = new List<ILayerSubProvider>();

        /// <summary>
        /// Register the provided <paramref name="layerSubProviders"/> to this layer provider.
        /// </summary>
        /// <param name="layerSubProviders"> The layer sub providers to be registered. </param>
        public void RegisterSubProviders(IList<ILayerSubProvider> layerSubProviders)
        {
            Ensure.NotNull(layerSubProviders, nameof(layerSubProviders));
            layerSubProviders.ForEach(e => Ensure.NotNull(e, nameof(layerSubProviders)));
            subProviders.AddRange(layerSubProviders);
        }

        public ILayer CreateLayer(object data, object parentData)
        {
            ILayerSubProvider validSubProvider = subProviders.FirstOrDefault(lp => lp.CanCreateLayerFor(data, parentData));
            return validSubProvider?.CreateLayer(data, parentData);
        }

        public bool CanCreateLayerFor(object data, object parentData) => 
            subProviders.Any(lp => lp.CanCreateLayerFor(data, parentData));

        public IEnumerable<object> ChildLayerObjects(object data) => 
            subProviders.SelectMany(lp => lp.GenerateChildLayerObjects(data));

        public void AfterCreate(ILayer layer, object layerObject, object parentObject, IDictionary<ILayer, object> objectsLookup)
        {
            // Nothing needs to be done after creation
        }
    }
}