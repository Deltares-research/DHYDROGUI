using DelftTools.Hydro;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Utils.Collections.Generic;
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
        protected override string GetLayerName() =>
            HydroAreaLayerNames.ThinDamsPluralName;

        /// <inheritdoc/>
        protected override VectorStyle GetVectorStyle() =>
            HydroAreaLayerStyles.ThinDamStyle;

        /// <inheritdoc/>
        protected override string GetFeatureTypeName() => "ThinDam";

        /// <inheritdoc/>
        protected override IEventedList<ThinDam2D> GetLayerFeatures(HydroArea hydroArea) =>
            hydroArea.ThinDams;
    }
}