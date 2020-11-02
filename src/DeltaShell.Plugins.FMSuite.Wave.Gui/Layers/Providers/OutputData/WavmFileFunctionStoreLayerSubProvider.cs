using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers.OutputData
{
    /// <summary>
    /// <see cref="WavmFileFunctionStoreLayerSubProvider"/> implements the
    /// <see cref="ILayerSubProvider"/> for data of type <see cref="WavmFileFunctionStore"/>.
    /// </summary>
    /// <seealso cref="ILayerSubProvider"/>
    public class WavmFileFunctionStoreLayerSubProvider : ILayerSubProvider
    {
        private readonly IWaveLayerInstanceCreator instanceCreator;
        private readonly Func<IEnumerable<IWaveModel>> getWaveModelsFunc;

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
                                                     Func<IEnumerable<IWaveModel>> getWaveModelsFunc)
        {
            Ensure.NotNull(instanceCreator, nameof(instanceCreator));
            Ensure.NotNull(getWaveModelsFunc, nameof(getWaveModelsFunc));

            this.instanceCreator = instanceCreator;
            this.getWaveModelsFunc = getWaveModelsFunc;
        }

        public bool CanCreateLayerFor(object sourceData, object parentData)
        {
            return sourceData is WavmFileFunctionStore store &&
                   store.Functions.Any();
        }

        public ILayer CreateLayer(object sourceData, object parentData) =>
            sourceData is WavmFileFunctionStore funcStore ? instanceCreator.CreateWaveOutputGroupLayer(Path.GetFileName(funcStore.Path)) : null;

        protected bool IsStandAloneFunctionStore(WavmFileFunctionStore store) =>
            !getWaveModelsFunc.Invoke().Any(m => m.WaveOutputData.WavmFileFunctionStores.Contains(store));

        public IEnumerable<object> GenerateChildLayerObjects(object data)
        {
            if (!(data is WavmFileFunctionStore store))
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
    }
}