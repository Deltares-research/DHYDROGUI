using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// Provides logic for creating <see cref="SharpMap.Api.Layers.ILayer"/> objects for collections
    /// of <see cref="BridgePillar"/> objects.
    /// </summary>
    internal sealed class BridgePillarsLayerProvider : FeaturesLayerProvider<BridgePillar>
    {
        /// <inheritdoc/>
        protected override string GetLayerName()
        {
            return HydroAreaLayerNames.BridgePillarsPluralName;
        }

        /// <inheritdoc/>
        protected override VectorStyle GetVectorStyle()
        {
            return HydroAreaLayerStyles.BridgePillarStyle;
        }

        /// <inheritdoc/>
        protected override string GetFeatureTypeName()
        {
            return "BridgePillar";
        }

        /// <inheritdoc/>
        protected override IEventedList<BridgePillar> GetLayerFeatures(HydroArea hydroArea)
        {
            return hydroArea.BridgePillars;
        }
    }
}