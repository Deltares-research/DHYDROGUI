using System.Collections.Generic;
using SharpMap.Api.Layers;

namespace DeltaShell.NGHS.Common.Gui.MapLayers.Providers
{
    /// <summary>
    /// <see cref="ILayerSubProvider"/> provides an interface for
    /// layer sub provider. Each <see cref="ILayerSubProvider"/> should be
    /// responsible for creating one type of layer. These sub-providers are
    /// then presented as one <see cref="MapLayerProvider"/>.
    /// </summary>
    public interface ILayerSubProvider
    {
        /// <summary>
        /// Determines whether this <see cref="ILayerSubProvider"/>
        /// can create a layer for the specified <paramref name="sourceData"/>
        /// and <paramref name="parentData"/>.
        /// </summary>
        /// <param name="sourceData">The source data.</param>
        /// <param name="parentData">The parent data.</param>
        /// <returns>
        /// <c>true</c> if this <see cref="ILayerSubProvider"/> can create
        /// a layer for the specified data; <c>false</c> otherwise;
        /// </returns>
        bool CanCreateLayerFor(object sourceData, object parentData);

        /// <summary>
        /// Creates the layer for the specified <paramref name="sourceData"/>.
        /// </summary>
        /// <param name="sourceData">The source data.</param>
        /// <param name="parentData">The parent data.</param>
        /// <returns> The layer constructed from the provided data. </returns>
        ILayer CreateLayer(object sourceData, object parentData);

        /// <summary>
        /// Generates the child layer objects associated with this
        /// <see cref="ILayerSubProvider"/>.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>
        /// The set of objects required to create the layers associated with
        /// this <see cref="ILayerSubProvider"/>.
        /// </returns>
        IEnumerable<object> GenerateChildLayerObjects(object data);
    }
}