using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Editors.Interactors;
using SharpMap.Rendering;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// Provides logic for creating <see cref="ILayer"/> objects for collections
    /// of <see cref="Pump2D"/> objects.
    /// </summary>
    public class PumpsLayerProvider : FeaturesLayerProvider<Pump2D>
    {
        /// <inheritdoc/>
        protected override IFeatureEditor GetLayerFeatureEditor(HydroArea hydroArea)
        {
            return new Feature2DEditor(hydroArea)
            {
                CreateNewFeature = l => new Pump2D(true)
            };
        }

        /// <inheritdoc/>
        protected override ILayer CreateLayer(HydroArea hydroArea)
        {
            ILayer layer = base.CreateLayer(hydroArea);
            layer.CustomRenderers.Add(new ArrowLineStringAdornerRenderer());

            return layer;
        }

        /// <inheritdoc/>
        protected override string GetLayerName()
        {
            return HydroArea.PumpsPluralName;
        }

        /// <inheritdoc/>
        protected override VectorStyle GetVectorStyle()
        {
            return HydroAreaLayerStyles.PumpStyle;
        }

        /// <inheritdoc/>
        protected override string GetFeatureTypeName()
        {
            return "pump";
        }

        /// <inheritdoc/>
        protected override IEventedList<Pump2D> GetLayerFeatures(HydroArea hydroArea)
        {
            return hydroArea.Pumps;
        }
    }
}