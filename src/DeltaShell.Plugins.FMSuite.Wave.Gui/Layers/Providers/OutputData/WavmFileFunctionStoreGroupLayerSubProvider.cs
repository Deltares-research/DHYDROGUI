using DeltaShell.Plugins.FMSuite.Wave.OutputData;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers.OutputData
{
    /// <summary>
    /// <see cref="WavmFileFunctionStoreGroupLayerSubProvider"/> implements the
    /// <see cref="WaveFileFunctionStoreGroupLayerSubProviderBase{T}"/> to create layers
    /// for <see cref="WavmFileFunctionStore"/> objects.
    /// </summary>
    /// <seealso cref="WaveFileFunctionStoreGroupLayerSubProviderBase{WavmFileFunctionStore}" />
    public class WavmFileFunctionStoreGroupLayerSubProvider : 
        WaveFileFunctionStoreGroupLayerSubProviderBase<IWavmFileFunctionStore>
    {
        /// <summary>
        /// Creates a new <see cref="WavmFileFunctionStoreGroupLayerSubProvider"/>.
        /// </summary>
        /// <param name="instanceCreator">The instance creator.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="instanceCreator"/> is <c>null</c>.
        /// </exception>
        public WavmFileFunctionStoreGroupLayerSubProvider(IWaveLayerInstanceCreator instanceCreator) : 
            base(instanceCreator) {}

        protected override string LayerName => WaveLayerNames.WavmFunctionGroupLayerName;
    }
}