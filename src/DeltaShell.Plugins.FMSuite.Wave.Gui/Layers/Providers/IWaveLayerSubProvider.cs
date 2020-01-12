using System.Collections.Generic;
using DelftTools.Shell.Gui;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers
{
    /// <summary>
    /// <see cref="IWaveLayerSubProvider"/> provides an interface for wave
    /// layer sub provider. Each <see cref="IWaveLayerSubProvider"/> should be
    /// responsible for creating one type of layer. These sub-providers are
    /// then presented as one <see cref="IMapLayerProvider"/> through the
    /// <see cref="WaveMapLayerProvider"/>.
    /// </summary>
    public interface IWaveLayerSubProvider
    {
        /// <summary>
        /// Determines whether this <see cref="IWaveLayerSubProvider"/>
        /// can create a layer for the specified <paramref name="sourceData"/>
        /// and <paramref name="parentData"/>.
        /// </summary>
        /// <param name="sourceData">The source data.</param>
        /// <param name="parentData">The parent data.</param>
        /// <returns>
        /// <c>true</c> if this <see cref="IWaveLayerSubProvider"/> can create
        /// a layer for the specified data; false otherwise;
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
        /// <see cref="IWaveLayerSubProvider"/>.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>
        /// The set of objects required to create the layers associated with
        /// this <see cref="IWaveLayerSubProvider"/>.
        /// </returns>
        IEnumerable<object> GenerateChildLayerObjects(object data);
    }
}