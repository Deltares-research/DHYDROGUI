using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Gui.Layers;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// <see cref="Feature2DLayerProvider{T}"/> implements the
    /// <see cref="ILayerSubProvider"/> for data of type <see cref="T"/>.
    /// </summary>
    /// <seealso cref="ILayerSubProvider" />
    public abstract class Feature2DLayerProvider<T> : ILayerSubProvider where T : Feature2D
    {
        public bool CanCreateLayerFor(object sourceData, object parentData)
        {
            return sourceData is IEventedList<T> && parentData is HydroArea;
        }

        public ILayer CreateLayer(object sourceData, object parentData)
        {
            return parentData is HydroArea hydroArea
                       ? CreateLayer(hydroArea)
                       : null;
        }

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