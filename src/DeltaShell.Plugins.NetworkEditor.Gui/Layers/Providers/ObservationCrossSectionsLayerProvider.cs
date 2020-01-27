using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Providers;
using SharpMap.Api;
using SharpMap.Api.Layers;
using SharpMap.Editors.Interactors;
using SharpMap.Layers;
using SharpMap.Rendering;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// Provides logic for creating <see cref="ILayer"/> objects for collections
    /// of <see cref="ObservationCrossSection2D"/> objects.
    /// </summary>
    public class ObservationCrossSectionsLayerProvider : Feature2DLayerProvider<ObservationCrossSection2D>
    {
        /// <inheritdoc/>
        protected override ILayer CreateLayer(HydroArea hydroArea)
        {
            return new VectorLayer(HydroArea.ObservationCrossSectionsPluralName)
            {
                FeatureEditor = new Feature2DEditor(hydroArea),
                Style = HydroAreaLayerStyles.ObsCrossSectionStyle,
                DataSource = new HydroAreaFeature2DCollection(hydroArea).Init(hydroArea.ObservationCrossSections, "ObservationCrossSection", "NetworkEditorModelName", hydroArea.CoordinateSystem),
                NameIsReadOnly = true,
                CustomRenderers = new IFeatureRenderer[]
                {
                    new ArrowLineStringAdornerRenderer()
                }
            };
        }
    }
}
