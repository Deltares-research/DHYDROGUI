using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Providers;
using SharpMap.Api;
using SharpMap.Api.Layers;
using SharpMap.Data.Providers;
using SharpMap.Editors.Interactors;
using SharpMap.Layers;
using SharpMap.Rendering;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// Provides logic for creating <see cref="ILayer"/> objects for collections
    /// of <see cref="Pump2D"/> objects.
    /// </summary>
    public class PumpsLayerProvider : GroupableFeaturesLayerProvider<Pump2D>
    {
        protected override ILayer CreateLayer(HydroArea hydroArea)
        {
            Feature2DCollection areaFeature2DCollection = new HydroAreaFeature2DCollection(hydroArea).Init(hydroArea.Pumps, "pump", "NetworkEditorModelName", hydroArea.CoordinateSystem);
            areaFeature2DCollection.FeatureType = typeof(Pump2D); // Override so we can use FeatureAttributes!

            return new VectorLayer(HydroArea.PumpsPluralName)
            {
                FeatureEditor = new Feature2DEditor(hydroArea)
                {
                    CreateNewFeature = layer => new Pump2D(true)
                },
                Style = HydroAreaLayerStyles.PumpStyle,
                DataSource = areaFeature2DCollection,
                NameIsReadOnly = true,
                CustomRenderers = new IFeatureRenderer[]
                {
                    new ArrowLineStringAdornerRenderer()
                }
            };
        }
    }
}