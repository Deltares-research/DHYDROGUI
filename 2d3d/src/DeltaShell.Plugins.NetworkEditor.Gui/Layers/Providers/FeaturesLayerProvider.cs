using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Gui.Layers;
using GeoAPI.Extensions.Feature;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Editors.Interactors;
using SharpMap.Layers;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// Provides logic for creating <see cref="ILayer"/> objects for collections
    /// of <see cref="IFeature"/> objects.
    /// </summary>
    /// <typeparam name="T"> The type of <see cref="IFeature"/>. </typeparam>
    internal abstract class FeaturesLayerProvider<T> : ILayerSubProvider where T : IFeature
    {
        /// <inheritdoc/>
        public virtual bool CanCreateLayerFor(object sourceData, object parentData)
        {
            return sourceData is IEventedList<T> && parentData is HydroArea;
        }

        /// <inheritdoc/>
        public ILayer CreateLayer(object sourceData, object parentData)
        {
            return parentData is HydroArea hydroArea
                       ? CreateLayer(hydroArea)
                       : null;
        }

        /// <inheritdoc/>
        public IEnumerable<object> GenerateChildLayerObjects(object data)
        {
            yield break;
        }

        /// <summary>
        /// Creates and returns the appropriate layer for data of type <see cref="T"/>.
        /// </summary>
        /// <param name="hydroArea"> The hydro area that owns the data. </param>
        /// <returns> A layer for showing the data. </returns>
        protected virtual ILayer CreateLayer(HydroArea hydroArea)
        {
            return new VectorLayer(GetLayerName())
            {
                FeatureEditor = GetLayerFeatureEditor(hydroArea),
                Style = GetVectorStyle(),
                DataSource = new HydroAreaFeature2DCollection(hydroArea).Init(GetLayerFeatures(hydroArea), GetFeatureTypeName(), "NetworkEditorModelName", hydroArea.CoordinateSystem),
                NameIsReadOnly = true
            };
        }

        protected virtual IFeatureEditor GetLayerFeatureEditor(HydroArea hydroArea)
        {
            return new Feature2DEditor(hydroArea);
        }

        /// <summary>
        /// Gets the layer name.
        /// </summary>
        /// <returns> The layer name. </returns>
        protected abstract string GetLayerName();

        /// <summary>
        /// Gets the <see cref="VectorStyle"/> for returned layers.
        /// </summary>
        /// <returns> The vector style. </returns>
        protected abstract VectorStyle GetVectorStyle();

        /// <summary>
        /// Gets the base name for all features in this layer.
        /// </summary>
        /// <returns> The feature type name. </returns>
        protected abstract string GetFeatureTypeName();

        /// <summary>
        /// Gets the <see cref="DelftTools.Hydro.GroupableFeatures.IGroupableFeature"/>
        /// objects that are shown in the provided layer.
        /// </summary>
        /// <param name="hydroArea"> The hydro area that owns the features. </param>
        /// <returns> The requested features. </returns>
        protected abstract IEventedList<T> GetLayerFeatures(HydroArea hydroArea);
    }
}