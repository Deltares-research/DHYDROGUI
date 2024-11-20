using DelftTools.Hydro;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Utils.Collections.Generic;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// Provides logic for creating <see cref="SharpMap.Api.Layers.ILayer"/> objects for collections
    /// of <see cref="LandBoundary2D"/> objects.
    /// </summary>
    internal sealed class LandBoundariesLayerProvider : FeaturesLayerProvider<LandBoundary2D>
    {
        /// <inheritdoc/>
        protected override string GetLayerName() =>
            HydroAreaLayerNames.LandBoundariesPluralName;

        /// <inheritdoc/>
        protected override VectorStyle GetVectorStyle() =>
            HydroAreaLayerStyles.LandBoundaryStyle;

        /// <inheritdoc/>
        protected override string GetFeatureTypeName() => "LandBoundary";

        /// <inheritdoc/>
        protected override IEventedList<LandBoundary2D> GetLayerFeatures(HydroArea hydroArea) =>
            hydroArea.LandBoundaries;
    }
}