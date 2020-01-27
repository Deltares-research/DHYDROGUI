using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using SharpMap.Api.Layers;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// Provides logic for creating <see cref="ILayer"/> objects for collections
    /// of dry points.
    /// </summary>
    public class DryPointsLayerProvider : GroupableFeaturesLayerProvider<GroupablePointFeature>
    {
        /// <inheritdoc/>
        protected override string GetLayerName()
        {
            return HydroArea.DryPointsPluralName;
        }

        /// <inheritdoc/>
        protected override VectorStyle GetVectorStyle()
        {
            return HydroAreaLayerStyles.DryPointStyle;
        }

        /// <inheritdoc/>
        protected override string GetFeatureTypeName()
        {
            return "DryPoint";
        }

        protected override IEventedList<GroupablePointFeature> GetLayerFeatures(HydroArea hydroArea)
        {
            return hydroArea.DryPoints;
        }
    }
}