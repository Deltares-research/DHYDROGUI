using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Providers;
using SharpMap.Api.Layers;
using SharpMap.Editors.Interactors;
using SharpMap.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// <see cref="ThinDamsLayerProvider"/> implements the
    /// <see cref="ILayerSubProvider"/> for data of type <see cref="IEventedList{ThinDam2D}"/>.
    /// </summary>
    /// <seealso cref="ILayerSubProvider" />
    public class ThinDamsLayerProvider : Feature2DLayerProvider<ThinDam2D>
    {
        protected override ILayer CreateLayer(HydroArea hydroArea)
        {
            return new VectorLayer(HydroArea.ThinDamsPluralName)
            {
                FeatureEditor = new Feature2DEditor(hydroArea),
                Style = HydroAreaLayerStyles.ThinDamStyle,
                DataSource = new HydroAreaFeature2DCollection(hydroArea).Init(hydroArea.ThinDams, "ThinDam", "NetworkEditorModelName", hydroArea.CoordinateSystem),
                NameIsReadOnly = true
            };
        }
    }
}