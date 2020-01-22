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
    /// <see cref="FixedWeirsLayerProvider"/> implements the
    /// <see cref="ILayerSubProvider"/> for data of type <see cref="IEventedList{FixedWeir}"/>.
    /// </summary>
    /// <seealso cref="ILayerSubProvider" />
    public class FixedWeirsLayerProvider : Feature2DLayerProvider<FixedWeir>
    {
        protected override ILayer CreateLayer(HydroArea hydroArea)
        {
            return new VectorLayer(HydroArea.FixedWeirsPluralName)
            {
                FeatureEditor = new Feature2DEditor(hydroArea),
                Style = AreaLayerStyles.FixedWeirStyle,
                DataSource = new HydroAreaFeature2DCollection(hydroArea).Init(hydroArea.FixedWeirs, "FixedWeir", "NetworkEditorModelName", hydroArea.CoordinateSystem),
                NameIsReadOnly = true
            };
        }
    }
}