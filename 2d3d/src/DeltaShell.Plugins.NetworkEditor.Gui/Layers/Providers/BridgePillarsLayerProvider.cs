using DelftTools.Hydro;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Utils.Collections.Generic;
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
        protected override string GetLayerName() =>
            HydroAreaLayerNames.BridgePillarsPluralName;

        /// <inheritdoc/>
        protected override VectorStyle GetVectorStyle() =>
            HydroAreaLayerStyles.BridgePillarStyle;

        /// <inheritdoc/>
        protected override string GetFeatureTypeName() => nameof(BridgePillar);

        /// <inheritdoc/>
        protected override IEventedList<BridgePillar> GetLayerFeatures(HydroArea hydroArea) =>
            hydroArea.BridgePillars;
    }
}