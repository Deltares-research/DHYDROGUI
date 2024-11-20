using DelftTools.Hydro;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers.Renderers;
using SharpMap.Api.Layers;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// Provides logic for creating <see cref="ILayer"/> objects for collections
    /// of enclosures.
    /// </summary>
    internal sealed class EnclosuresLayerProvider : GroupableFeature2DPolygonsLayerProvider
    {
        /// <inheritdoc/>
        protected override ILayer CreateLayer(HydroArea hydroArea)
        {
            ILayer layer = base.CreateLayer(hydroArea);
            layer.Opacity = 0.25f;
            layer.CustomRenderers.Add(new EnclosureRenderer());

            return layer;
        }

        /// <inheritdoc/>
        protected override string GetLayerName() =>
            HydroAreaLayerNames.EnclosureName;

        /// <inheritdoc/>
        protected override VectorStyle GetVectorStyle() =>
            HydroAreaLayerStyles.EnclosureStyle;

        /// <inheritdoc/>
        protected override string GetFeatureTypeName() =>
            "Enclosure";

        /// <inheritdoc/>
        protected override IEventedList<GroupableFeature2DPolygon> GetLayerFeatures(HydroArea hydroArea) =>
            hydroArea.Enclosures;
    }
}