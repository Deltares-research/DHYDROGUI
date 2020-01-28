using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// Provides logic for creating <see cref="SharpMap.Api.Layers.ILayer"/> objects for collections
    /// of observation points.
    /// </summary>
    public class ObservationPointsLayerProvider : GroupableFeaturesLayerProvider<GroupableFeature2DPoint>
    {
        /// <inheritdoc/>
        protected override string GetLayerName()
        {
            return HydroArea.ObservationPointsPluralName;
        }

        /// <inheritdoc/>
        protected override VectorStyle GetVectorStyle()
        {
            return HydroAreaLayerStyles.ObservationPointStyle;
        }

        /// <inheritdoc/>
        protected override string GetFeatureTypeName()
        {
            return "ObservationPoint";
        }

        /// <inheritdoc/>
        protected override IEventedList<GroupableFeature2DPoint> GetLayerFeatures(HydroArea hydroArea)
        {
            return hydroArea.ObservationPoints;
        }
    }
}