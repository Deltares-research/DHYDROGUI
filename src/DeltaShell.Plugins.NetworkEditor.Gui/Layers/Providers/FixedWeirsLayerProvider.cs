using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Providers;
using SharpMap.Api.Layers;
using SharpMap.Editors.Interactors;
using SharpMap.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// Provides logic for creating <see cref="ILayer"/> objects for collections
    /// of <see cref="FixedWeir"/> objects.
    /// </summary>
    public class FixedWeirsLayerProvider : Feature2DLayerProvider<FixedWeir>
    {
        /// <inheritdoc/>
        protected override ILayer CreateLayer(HydroArea hydroArea)
        {
            return new VectorLayer(HydroArea.FixedWeirsPluralName)
            {
                FeatureEditor = new Feature2DEditor(hydroArea),
                Style = HydroAreaLayerStyles.FixedWeirStyle,
                DataSource = new HydroAreaFeature2DCollection(hydroArea).Init(hydroArea.FixedWeirs, "FixedWeir", "NetworkEditorModelName", hydroArea.CoordinateSystem),
                NameIsReadOnly = true
            };
        }
    }
}