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
    /// of <see cref="ThinDam2D"/> objects.
    /// </summary>
    public class ThinDamsLayerProvider : GroupableFeaturesLayerProvider<ThinDam2D>
    {
        /// <inheritdoc/>
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