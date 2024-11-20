using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers.OutputData
{
    /// <summary>
    /// <see cref="WavmFileFunctionStoreLayerSubProvider"/> implements the layer provider
    /// for <see cref="WavmFileFunctionStore"/> objects.
    /// </summary>
    /// <seealso cref="WaveFileFunctionStoreLayerSubProviderBase{WavmFileFunctionStore}" />
    public sealed class WavmFileFunctionStoreLayerSubProvider : 
        WaveFileFunctionStoreLayerSubProviderBase<IWavmFileFunctionStore>
    {
        /// <summary>
        /// Creates a new <see cref="WavmFileFunctionStoreLayerSubProvider"/>.
        /// </summary>
        /// <param name="instanceCreator">The factory to build the layers with.</param>
        /// <param name="getWaveModelsFunc"> Function to retrieve the WaveModels. </param>
        /// <exception cref="ArgumentNullException">
        /// Throw when <paramref name="instanceCreator"/> or
        /// <paramref name="getWaveModelsFunc"/> is <c>null</c>.
        /// </exception>
        public WavmFileFunctionStoreLayerSubProvider(IWaveLayerInstanceCreator instanceCreator,
                                                     Func<IEnumerable<IWaveModel>> getWaveModelsFunc) : 
            base(instanceCreator, getWaveModelsFunc) { }

        public override IEnumerable<object> GenerateChildLayerObjects(object data)
        {
            if (!(data is IWavmFileFunctionStore store))
            {
                yield break;
            }

            if (IsStandAloneFunctionStore(store))
            {
                yield return store.Grid;
            }

            foreach (IFunction coverage in store.Functions)
            {
                yield return coverage;
            }
        }

        protected override bool IsContainedInModel(IWavmFileFunctionStore store, IWaveModel model) =>
            model.WaveOutputData.WavmFileFunctionStores.Contains(store);
    }
}