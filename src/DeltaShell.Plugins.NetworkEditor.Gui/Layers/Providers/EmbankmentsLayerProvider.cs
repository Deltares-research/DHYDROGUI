using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers.Editors;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers.Renderers;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// Provides logic for creating <see cref="ILayer"/> objects for collections
    /// of <see cref="Embankment"/> objects.
    /// </summary>
    internal sealed class EmbankmentsLayerProvider : FeaturesLayerProvider<Embankment>
    {
        /// <inheritdoc/>
        protected override ILayer CreateLayer(HydroArea hydroArea)
        {
            ILayer layer = base.CreateLayer(hydroArea);
            layer.CustomRenderers.Add(new EmbankmentRenderer());

            return layer;
        }

        /// <inheritdoc/>
        protected override IFeatureEditor GetLayerFeatureEditor(HydroArea hydroArea) =>
            new HydroAreaFeatureEditor(hydroArea) { CreateNewFeature = l => new Embankment { Region = hydroArea } };

        /// <inheritdoc/>
        protected override string GetLayerName() =>
            HydroAreaLayerNames.EmbankmentsPluralName;

        /// <inheritdoc/>
        protected override VectorStyle GetVectorStyle() =>
            HydroAreaLayerStyles.EmbankmentStyle;

        /// <inheritdoc/>
        protected override string GetFeatureTypeName() => nameof(Embankment);

        /// <inheritdoc/>
        protected override IEventedList<Embankment> GetLayerFeatures(HydroArea hydroArea) =>
            hydroArea.Embankments;
    }
}