using System;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers
{
    /// <summary>
    /// <see cref="IWaveLayerFactory"/> defines the methods to construct the
    /// different layers of the wave model.
    /// </summary>
    public interface IWaveLayerFactory
    {
        /// <summary>
        /// Creates a new model layer from the given <paramref name="waveModel"/>.
        /// </summary>
        /// <param name="waveModel">The wave model.</param>
        /// <returns>A new <see cref="ILayer"/> containing teh model.</returns>
        /// <exception cref="ArgumentNullException">
        /// Throw when <paramref name="waveModel"/> is <c>null</c>.
        /// </exception>
        ILayer CreateModelGroupLayer(WaveModel waveModel);

        /// <summary>
        /// Creates a new <see cref="WaveDomainData"/> layer.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <returns>
        /// A new <see cref="ILayer"/> of the <see cref="WaveDomainData"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="domain"/> is <c>null</c>.
        /// </exception>
        ILayer CreateWaveDomainDataLayer(WaveDomainData domain);
    }
}