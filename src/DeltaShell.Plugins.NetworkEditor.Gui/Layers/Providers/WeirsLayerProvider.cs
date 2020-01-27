using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
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
    /// of <see cref="Weir2D"/> objects.
    /// </summary>
    public class WeirsLayerProvider : GroupableFeaturesLayerProvider<Weir2D>
    {
        /// <inheritdoc/>
        protected override ILayer CreateLayer(HydroArea hydroArea)
        {
            return new VectorLayer(HydroArea.WeirsPluralName)
            {
                FeatureEditor = new Feature2DEditor(hydroArea),
                Style = HydroAreaLayerStyles.WeirStyle,
                DataSource = new HydroAreaFeature2DCollection(hydroArea).Init(hydroArea.Weirs, "structure", "NetworkEditorModelName", hydroArea.CoordinateSystem),
                NameIsReadOnly = true,
                CustomRenderers = new IFeatureRenderer[]
                {
                    new ArrowLineStringAdornerRenderer()
                }
            };
        }
    }
}