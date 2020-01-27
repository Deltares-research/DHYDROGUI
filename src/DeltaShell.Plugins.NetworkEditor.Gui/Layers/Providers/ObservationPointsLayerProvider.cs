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
    /// of observation points.
    /// </summary>
    public class ObservationPointsLayerProvider : Feature2DLayerProvider<GroupableFeature2DPoint>
    {
        /// <inheritdoc/>
        protected override ILayer CreateLayer(HydroArea hydroArea)
        {
            return new VectorLayer(HydroArea.ObservationPointsPluralName)
            {
                FeatureEditor = new Feature2DEditor(hydroArea),
                Style = HydroAreaLayerStyles.ObservationPointStyle,
                DataSource = new HydroAreaFeature2DCollection(hydroArea).Init(hydroArea.ObservationPoints, "ObservationPoint", "NetworkEditorModelName", hydroArea.CoordinateSystem),
                NameIsReadOnly = true,
            };
        }
    }
}