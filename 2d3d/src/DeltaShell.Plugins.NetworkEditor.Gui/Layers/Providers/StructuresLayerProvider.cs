using DelftTools.Hydro;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Utils.Collections.Generic;
using SharpMap.Api.Layers;
using SharpMap.Rendering;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// Provides logic for creating <see cref="ILayer"/> objects for collections
    /// of <see cref="IStructure"/> objects.
    /// </summary>
    internal sealed class StructuresLayerProvider : FeaturesLayerProvider<Structure>
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
            HydroAreaLayerNames.StructuresPluralName;

        /// <inheritdoc/>
        protected override VectorStyle GetVectorStyle() =>
            HydroAreaLayerStyles.WeirStyle;

        /// <inheritdoc/>
        protected override string GetFeatureTypeName() => nameof(Structure);

        /// <inheritdoc/>
        protected override IEventedList<Structure> GetLayerFeatures(HydroArea hydroArea) =>
            hydroArea.Structures;
    }
}