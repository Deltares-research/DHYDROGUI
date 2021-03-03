using DelftTools.Hydro;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers;
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
        protected override string GetLayerName()
        {
            return HydroAreaLayerNames.EnclosureName;
        }

        /// <inheritdoc/>
        protected override VectorStyle GetVectorStyle()
        {
            return HydroAreaLayerStyles.EnclosureStyle;
        }

        /// <inheritdoc/>
        protected override string GetFeatureTypeName()
        {
            return "Enclosure";
        }

        /// <inheritdoc/>
        protected override IEventedList<GroupableFeature2DPolygon> GetLayerFeatures(HydroArea hydroArea)
        {
            return hydroArea.Enclosures;
        }
    }
}