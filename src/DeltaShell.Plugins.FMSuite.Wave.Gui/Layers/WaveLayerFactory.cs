using System;
using DeltaShell.Plugins.FMSuite.Common.Layers;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers
{
    /// <summary>
    /// <see cref="WaveLayerFactory"/> provides the methods to construct the
    /// different layers of the wave model.
    /// </summary>
    public static class WaveLayerFactory
    {
        /// <summary>
        /// Create a new model layer from the given <paramref name="waveModel"/>.
        /// </summary>
        /// <param name="waveModel">The wave model.</param>
        /// <returns>A new <see cref="ILayer"/> containing teh model.</returns>
        /// <exception cref="ArgumentNullException">
        /// Throw when <paramref name="waveModel"/> is <c>null</c>.
        /// </exception>
        public static ILayer CreateModelGroupLayer(WaveModel waveModel)
        {
            if (waveModel == null)
            {
                throw new ArgumentNullException(nameof(waveModel));
            }

            return new ModelGroupLayer
            {
                Name = waveModel.Name,
                Model = waveModel,
            };
        }
    }
}