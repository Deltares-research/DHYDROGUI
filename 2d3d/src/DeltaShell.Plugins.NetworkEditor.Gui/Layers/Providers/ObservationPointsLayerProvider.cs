using DelftTools.Hydro;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Utils.Collections.Generic;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// Provides logic for creating <see cref="SharpMap.Api.Layers.ILayer"/> objects for collections
    /// of observation points.
    /// </summary>
    internal sealed class ObservationPointsLayerProvider : FeaturesLayerProvider<GroupableFeature2DPoint>
    {
        /// <inheritdoc/>
        protected override string GetLayerName() =>
            HydroAreaLayerNames.ObservationPointsPluralName;

        /// <inheritdoc/>
        protected override VectorStyle GetVectorStyle() =>
            HydroAreaLayerStyles.ObservationPointStyle;

        /// <inheritdoc/>
        protected override string GetFeatureTypeName() =>
            "ObservationPoint";

        /// <inheritdoc/>
        protected override IEventedList<GroupableFeature2DPoint> GetLayerFeatures(HydroArea hydroArea) =>
            hydroArea.ObservationPoints;
    }
}