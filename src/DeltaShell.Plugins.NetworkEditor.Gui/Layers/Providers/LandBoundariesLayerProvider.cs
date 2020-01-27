using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using SharpMap.Api.Layers;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// Provides logic for creating <see cref="ILayer"/> objects for collections
    /// of <see cref="LandBoundary2D"/> objects.
    /// </summary>
    public class LandBoundariesLayerProvider : GroupableFeaturesLayerProvider<LandBoundary2D>
    {
        /// <inheritdoc/>
        protected override string GetLayerName()
        {
            return HydroArea.LandBoundariesPluralName;
        }

        /// <inheritdoc/>
        protected override VectorStyle GetVectorStyle()
        {
            return HydroAreaLayerStyles.LandBoundaryStyle;
        }

        /// <inheritdoc/>
        protected override string GetFeatureTypeName()
        {
            return "LandBoundary";
        }

        /// <inheritdoc/>
        protected override IEventedList<LandBoundary2D> GetLayerFeatures(HydroArea hydroArea)
        {
            return hydroArea.LandBoundaries;
        }
    }
}