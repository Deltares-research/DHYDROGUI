using DelftTools.Hydro;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Utils.Collections.Generic;
using SharpMap.Api.Layers;
using SharpMap.Rendering;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// Provides logic for creating <see cref="ILayer"/> objects for collections
    /// of <see cref="ObservationCrossSection2D"/> objects.
    /// </summary>
    internal sealed class ObservationCrossSectionsLayerProvider : FeaturesLayerProvider<ObservationCrossSection2D>
    {
        /// <inheritdoc/>
        protected override ILayer CreateLayer(HydroArea hydroArea)
        {
            ILayer layer = base.CreateLayer(hydroArea);
            layer.CustomRenderers.Add(new ArrowLineStringAdornerRenderer());

            return layer;
        }

        /// <inheritdoc/>
        protected override string GetLayerName() =>
            HydroAreaLayerNames.ObservationCrossSectionsPluralName;

        /// <inheritdoc/>
        protected override VectorStyle GetVectorStyle() =>
            HydroAreaLayerStyles.ObsCrossSectionStyle;

        /// <inheritdoc/>
        protected override string GetFeatureTypeName() =>
            "ObservationCrossSection";

        /// <inheritdoc/>
        protected override IEventedList<ObservationCrossSection2D> GetLayerFeatures(HydroArea hydroArea) =>
            hydroArea.ObservationCrossSections;
    }
}