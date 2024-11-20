using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.Common.Gui.Layers;
using GeoAPI.Extensions.Feature;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// Provides logic for creating <see cref="ILayer"/> objects for <see cref="IHydroRegion"/> objects.
    /// </summary>
    internal sealed class HydroRegionLayerProvider : ILayerSubProvider
    {
        /// <inheritdoc/>
        public bool CanCreateLayerFor(object sourceData, object parentData) => sourceData is IHydroRegion;

        /// <inheritdoc/>
        public ILayer CreateLayer(object sourceData, object parentData) =>
            sourceData is IHydroRegion hydroRegion
                ? new HydroRegionMapLayer()
                {
                    Name = hydroRegion.Name,
                    Region = hydroRegion,
                    LayersReadOnly = true
                }
                : null;

        /// <inheritdoc/>
        public IEnumerable<object> GenerateChildLayerObjects(object data)
        {
            // Note that due to the current design, this will also generate data for
            // more specific layers, such as the HydroAreaLayerProvider.
            if (!(data is IHydroRegion hydroRegion))
            {
                return Enumerable.Empty<IRegion>();
            }

            return hydroRegion.SubRegions;
        }
    }
}