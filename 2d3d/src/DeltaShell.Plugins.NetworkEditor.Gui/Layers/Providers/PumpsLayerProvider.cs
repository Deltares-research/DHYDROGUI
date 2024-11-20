using DelftTools.Hydro;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Utils.Collections.Generic;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Editors.Interactors;
using SharpMap.Rendering;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// Provides logic for creating <see cref="ILayer"/> objects for collections
    /// of <see cref="Pump"/> objects.
    /// </summary>
    internal sealed class PumpsLayerProvider : FeaturesLayerProvider<Pump>
    {
        /// <inheritdoc/>
        protected override IFeatureEditor GetLayerFeatureEditor(HydroArea hydroArea) =>
            new Feature2DEditor(hydroArea) { CreateNewFeature = l => new Pump() };

        /// <inheritdoc/>
        protected override ILayer CreateLayer(HydroArea hydroArea)
        {
            ILayer layer = base.CreateLayer(hydroArea);
            layer.CustomRenderers.Add(new ArrowLineStringAdornerRenderer());

            return layer;
        }

        /// <inheritdoc/>
        protected override string GetLayerName() =>
            HydroAreaLayerNames.PumpsPluralName;

        /// <inheritdoc/>
        protected override VectorStyle GetVectorStyle() =>
            HydroAreaLayerStyles.PumpStyle;

        /// <inheritdoc/>
        protected override string GetFeatureTypeName() => nameof(Pump);

        /// <inheritdoc/>
        protected override IEventedList<Pump> GetLayerFeatures(HydroArea hydroArea) =>
            hydroArea.Pumps;
    }
}