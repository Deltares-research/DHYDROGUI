using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers.OutputData
{
    /// <summary>
    /// <see cref="WavhFileFunctionStoreLayerSubProvider"/> implements the layer provider
    /// for <see cref="WavhFileFunctionStore"/> objects.
    /// </summary>
    /// <seealso cref="WaveFileFunctionStoreLayerSubProviderBase{WavhFileFunctionStore}"/>
    public class WavhFileFunctionStoreLayerSubProvider :
        WaveFileFunctionStoreLayerSubProviderBase<IWavhFileFunctionStore>
    {
        /// <summary>
        /// Creates a new <see cref="WavhFileFunctionStoreLayerSubProvider"/>.
        /// </summary>
        /// <param name="instanceCreator">The factory to build the layers with.</param>
        /// <param name="getWaveModelsFunc"> Function to retrieve the WaveModels. </param>
        /// <exception cref="ArgumentNullException">
        /// Throw when <paramref name="instanceCreator"/> or
        /// <paramref name="getWaveModelsFunc"/> is <c>null</c>.
        /// </exception>
        public WavhFileFunctionStoreLayerSubProvider(IWaveLayerInstanceCreator instanceCreator,
                                                     Func<IEnumerable<IWaveModel>> getWaveModelsFunc) :
            base(instanceCreator, getWaveModelsFunc) {}

        public override IEnumerable<object> GenerateChildLayerObjects(object data)
        {
            if (!(data is IWavhFileFunctionStore store))
            {
                yield break;
            }

            foreach (IFunction coverage in store.Functions)
            {
                yield return coverage;
            }
        }

        protected override bool IsContainedInModel(IWavhFileFunctionStore store, IWaveModel model) =>
            model.WaveOutputData.WavhFileFunctionStores.Contains(store);
    }
}