using DeltaShell.Plugins.FMSuite.Wave.OutputData;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers.OutputData
{
    /// <summary>
    /// <see cref="WavhFileFunctionStoreGroupLayerSubProvider"/> implements the
    /// <see cref="WaveFileFunctionStoreGroupLayerSubProviderBase{T}"/> to create layers
    /// for <see cref="WavhFileFunctionStore"/> objects.
    /// </summary>
    /// <seealso cref="WaveFileFunctionStoreGroupLayerSubProviderBase{WavhFileFunctionStore}" />
    public class WavhFileFunctionStoreGroupLayerSubProvider :
        WaveFileFunctionStoreGroupLayerSubProviderBase<IWavhFileFunctionStore>
    {
        /// <summary>
        /// Creates a new <see cref="WavhFileFunctionStoreGroupLayerSubProvider"/>.
        /// </summary>
        /// <param name="instanceCreator">The instance creator.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="instanceCreator"/> is <c>null</c>.
        /// </exception>
        public WavhFileFunctionStoreGroupLayerSubProvider(IWaveLayerInstanceCreator instanceCreator) : 
            base(instanceCreator) {}

        protected override string LayerName => WaveLayerNames.WavhFunctionGroupLayerName;
    }
}