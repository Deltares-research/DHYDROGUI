using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IWaveLayerFactory factory;
        private readonly Func<IEnumerable<WaveModel>> getWaveModelsFunc;

        /// <summary>
        /// Creates a new <see cref="WavmFileFunctionStoreLayerSubProvider"/>.
        /// </summary>
        /// <param name="factory">The factory to build the layers with.</param>
        /// <param name="getWaveModelsFunc"> Function to retrieve the WaveModels. </param>
        /// <exception cref="ArgumentNullException">
        /// Throw when <paramref name="factory"/> or
        /// <paramref name="getWaveModelsFunc"/> is <c>null</c>.
        /// </exception>
        public WavmFileFunctionStoreLayerSubProvider(IWaveLayerFactory factory,
                                                     Func<IEnumerable<WaveModel>> getWaveModelsFunc)
        {
            Ensure.NotNull(factory, nameof(factory));
            Ensure.NotNull(getWaveModelsFunc, nameof(getWaveModelsFunc));

            this.factory = factory;
            this.getWaveModelsFunc = getWaveModelsFunc;
        }

        public bool CanCreateLayerFor(object sourceData, object parentData)
        {
            return sourceData is WavmFileFunctionStore store &&
                   store.Functions.Any();
        }

        public ILayer CreateLayer(object sourceData, object parentData)
        {
            // TODO: D3DFMIQ-2283
            return null;
            //if (!(sourceData is WavmFileFunctionStore store &&
            //      store.Functions.Any()))
            //{
            //    return null;
            //}

            //string domainName = store.Path;
            //var overrideDomainName = true;

            //if (parentData is IWaveModel model)
            //{
            //    IDataItem dataItem = model.GetDataItemByValue(store);
            //    string dataItemTag = dataItem.Tag;

            //    if (dataItemTag.StartsWith(WaveModel.WavmStoreDataItemTag))
            //    {
            //        var paramsValue = new string(dataItemTag.Skip(WaveModel.WavmStoreDataItemTag.Length).ToArray());
            //        domainName = string.Join(" ", paramsValue, "WAVM");
            //        overrideDomainName = false;
            //    }
            //}

            //return factory.CreateOutputLayer(domainName, overrideDomainName);
        }

        public IEnumerable<object> GenerateChildLayerObjects(object data)
        {
            // TODO: D3DFMIQ-2283
            yield break;

            //if (!(data is WavmFileFunctionStore store))
            //{
            //    yield break;
            //}

            //WaveModel waveModel = getWaveModelsFunc?.Invoke().FirstOrDefault(m => m.WavmFunctionStores.Contains(store));
            //if (waveModel == null)
            //{
            //    yield return store.Grid;
            //}

            //foreach (IFunction coverage in store.Functions)
            //{
            //    yield return coverage;
            //}
        }
    }
}