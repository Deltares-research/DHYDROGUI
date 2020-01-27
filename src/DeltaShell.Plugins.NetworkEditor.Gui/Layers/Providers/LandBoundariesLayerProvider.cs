using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Providers;
using SharpMap.Api.Layers;
using SharpMap.Editors.Interactors;
using SharpMap.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// Provides logic for creating <see cref="ILayer"/> objects for collections
    /// of <see cref="LandBoundary2D"/> objects.
    /// </summary>
    public class LandBoundariesLayerProvider : GroupableFeaturesLayerProvider<LandBoundary2D>
    {
        /// <inheritdoc/>
        protected override ILayer CreateLayer(HydroArea hydroArea)
        {
            return new VectorLayer(HydroArea.LandBoundariesPluralName)
            {
                FeatureEditor = new Feature2DEditor(hydroArea),
                Style = HydroAreaLayerStyles.LandBoundaryStyle,
                DataSource = new HydroAreaFeature2DCollection(hydroArea).Init(hydroArea.LandBoundaries, "LandBoundary", "NetworkEditorModelName", hydroArea.CoordinateSystem),
                NameIsReadOnly = true
            };
        }
    }
}