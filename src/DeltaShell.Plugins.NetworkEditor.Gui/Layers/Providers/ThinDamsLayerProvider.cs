using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// Provides logic for creating <see cref="SharpMap.Api.Layers.ILayer"/> objects for collections
    /// of <see cref="ThinDam2D"/> objects.
    /// </summary>
    internal sealed class ThinDamsLayerProvider : FeaturesLayerProvider<ThinDam2D>
    {
        /// <inheritdoc/>
        protected override string GetLayerName()
        {
            return HydroArea.ThinDamsPluralName;
        }

        /// <inheritdoc/>
        protected override VectorStyle GetVectorStyle()
        {
            return HydroAreaLayerStyles.ThinDamStyle;
        }

        /// <inheritdoc/>
        protected override string GetFeatureTypeName()
        {
            return "ThinDam";
        }

        /// <inheritdoc/>
        protected override IEventedList<ThinDam2D> GetLayerFeatures(HydroArea hydroArea)
        {
            return hydroArea.ThinDams;
        }
    }
}