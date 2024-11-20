using DelftTools.Hydro;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Utils.Collections.Generic;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// Provides logic for creating <see cref="SharpMap.Api.Layers.ILayer"/> objects for collections
    /// of dry areas.
    /// </summary>
    internal sealed class DryAreasLayerProvider : GroupableFeature2DPolygonsLayerProvider
    {
        /// <inheritdoc/>
        protected override string GetLayerName() =>
            HydroAreaLayerNames.DryAreasPluralName;

        /// <inheritdoc/>
        protected override VectorStyle GetVectorStyle() =>
            HydroAreaLayerStyles.DryAreaStyle;

        /// <inheritdoc/>
        protected override string GetFeatureTypeName() => "DryArea";

        /// <inheritdoc/>
        protected override IEventedList<GroupableFeature2DPolygon> GetLayerFeatures(HydroArea hydroArea) =>
            hydroArea.DryAreas;
    }
}