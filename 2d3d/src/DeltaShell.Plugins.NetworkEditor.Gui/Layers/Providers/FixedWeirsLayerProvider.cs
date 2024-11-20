using DelftTools.Hydro;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Utils.Collections.Generic;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// Provides logic for creating <see cref="SharpMap.Api.Layers.ILayer"/> objects for collections
    /// of <see cref="FixedWeir"/> objects.
    /// </summary>
    internal sealed class FixedWeirsLayerProvider : FeaturesLayerProvider<FixedWeir>
    {
        /// <inheritdoc/>
        protected override string GetLayerName() =>
            HydroAreaLayerNames.FixedWeirsPluralName;

        /// <inheritdoc/>
        protected override VectorStyle GetVectorStyle() =>
            HydroAreaLayerStyles.FixedWeirStyle;

        /// <inheritdoc/>
        protected override string GetFeatureTypeName() => nameof(FixedWeir);

        /// <inheritdoc/>
        protected override IEventedList<FixedWeir> GetLayerFeatures(HydroArea hydroArea) =>
            hydroArea.FixedWeirs;
    }
}