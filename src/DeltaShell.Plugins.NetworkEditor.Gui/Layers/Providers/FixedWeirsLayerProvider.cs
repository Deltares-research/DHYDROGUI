using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using SharpMap.Api.Layers;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// Provides logic for creating <see cref="ILayer"/> objects for collections
    /// of <see cref="FixedWeir"/> objects.
    /// </summary>
    public class FixedWeirsLayerProvider : GroupableFeaturesLayerProvider<FixedWeir>
    {
        /// <inheritdoc/>
        protected override string GetLayerName()
        {
            return HydroArea.FixedWeirsPluralName;
        }

        /// <inheritdoc/>
        protected override VectorStyle GetVectorStyle()
        {
            return HydroAreaLayerStyles.FixedWeirStyle;
        }

        /// <inheritdoc/>
        protected override string GetFeatureTypeName()
        {
            return "FixedWeir";
        }

        /// <inheritdoc/>
        protected override IEventedList<FixedWeir> GetLayerFeatures(HydroArea hydroArea)
        {
            return hydroArea.FixedWeirs;
        }
    }
}