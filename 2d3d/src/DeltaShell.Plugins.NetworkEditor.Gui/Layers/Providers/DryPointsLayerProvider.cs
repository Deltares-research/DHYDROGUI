using DelftTools.Hydro;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Utils.Collections.Generic;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// Provides logic for creating <see cref="SharpMap.Api.Layers.ILayer"/> objects for collections
    /// of dry points.
    /// </summary>
    internal sealed class DryPointsLayerProvider : FeaturesLayerProvider<GroupablePointFeature>
    {
        /// <inheritdoc/>
        protected override string GetLayerName() =>
            HydroAreaLayerNames.DryPointsPluralName;

        /// <inheritdoc/>
        protected override VectorStyle GetVectorStyle() =>
            HydroAreaLayerStyles.DryPointStyle;

        /// <inheritdoc/>
        protected override string GetFeatureTypeName() =>
            "DryPoint";

        protected override IEventedList<GroupablePointFeature> GetLayerFeatures(HydroArea hydroArea) =>
            hydroArea.DryPoints;
    }
}