using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using SharpMap.Api.Layers;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// Provides logic for creating <see cref="ILayer"/> objects for collections
    /// of dry areas.
    /// </summary>
    internal sealed class DryAreasLayerProvider : GroupableFeature2DPolygonsLayerProvider
    {
        /// <inheritdoc/>
        protected override string GetLayerName()
        {
            return HydroArea.DryAreasPluralName;
        }

        /// <inheritdoc/>
        protected override VectorStyle GetVectorStyle()
        {
            return HydroAreaLayerStyles.DryAreaStyle;
        }

        /// <inheritdoc/>
        protected override string GetFeatureTypeName()
        {
            return "DryArea";
        }

        /// <inheritdoc/>
        protected override IEventedList<GroupableFeature2DPolygon> GetLayerFeatures(HydroArea hydroArea)
        {
            return hydroArea.DryAreas;
        }
    }
}