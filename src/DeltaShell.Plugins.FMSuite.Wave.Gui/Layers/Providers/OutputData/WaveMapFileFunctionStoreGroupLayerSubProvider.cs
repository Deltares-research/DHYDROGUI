using DeltaShell.Plugins.FMSuite.Wave.OutputData;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers.OutputData
{
    /// <summary>
    /// <see cref="WaveMapFileFunctionStoreGroupLayerSubProvider"/> implements the
    /// <see cref="WaveFileFunctionStoreGroupLayerSubProvider{T}"/> to create layers
    /// for <see cref="WavmFileFunctionStore"/> objects.
    /// </summary>
    /// <seealso cref="WaveFileFunctionStoreGroupLayerSubProvider{WavmFileFunctionStore}" />
    public class WaveMapFileFunctionStoreGroupLayerSubProvider : 
        WaveFileFunctionStoreGroupLayerSubProvider<WavmFileFunctionStore>
    {
        /// <summary>
        /// Creates a new <see cref="WaveMapFileFunctionStoreGroupLayerSubProvider"/>.
        /// </summary>
        /// <param name="instanceCreator">The instance creator.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="instanceCreator"/> is <c>null</c>.
        /// </exception>
        public WaveMapFileFunctionStoreGroupLayerSubProvider(IWaveLayerInstanceCreator instanceCreator) : 
            base(instanceCreator) {}

        protected override string LayerName => WaveLayerNames.WavmFunctionGroupLayerName;
    }
}