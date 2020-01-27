using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Gui.Layers;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// Provides logic for creating <see cref="ILayer"/> objects for collections
    /// of <see cref="IGroupableFeature"/> objects.
    /// </summary>
    /// <typeparam name="T"> The type of <see cref="IGroupableFeature"/>. </typeparam>
    public abstract class GroupableFeaturesLayerProvider<T> : ILayerSubProvider where T : IGroupableFeature
    {
        /// <inheritdoc/>
        public bool CanCreateLayerFor(object sourceData, object parentData)
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
        protected abstract ILayer CreateLayer(HydroArea hydroArea);
    }
}