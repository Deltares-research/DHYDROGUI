using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors;
using SharpMap.Api.Editors;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// Provides logic for creating <see cref="SharpMap.Api.Layers.ILayer"/> objects for collections
    /// of <see cref="Embankment"/> objects.
    /// </summary>
    public class EmbankmentsLayerProvider : FeaturesLayerProvider<Embankment>
    {
        /// <inheritdoc/>
        protected override IFeatureEditor GetLayerFeatureEditor(HydroArea hydroArea)
        {
            return new HydroAreaFeatureEditor(hydroArea)
            {
                CreateNewFeature = l => new Embankment
                {
                    Region = hydroArea
                }
            };
        }

        /// <inheritdoc/>
        protected override string GetLayerName()
        {
            return HydroArea.EmbankmentsPluralName;
        }

        /// <inheritdoc/>
        protected override VectorStyle GetVectorStyle()
        {
            return HydroAreaLayerStyles.EmbankmentStyle;
        }

        /// <inheritdoc/>
        protected override string GetFeatureTypeName()
        {
            return "Embankment";
        }

        /// <inheritdoc/>
        protected override IEventedList<Embankment> GetLayerFeatures(HydroArea hydroArea)
        {
            return hydroArea.Embankments;
        }
    }
}