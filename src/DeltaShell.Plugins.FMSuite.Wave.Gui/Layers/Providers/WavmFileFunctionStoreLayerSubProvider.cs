using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.FMSuite.Wave.IO;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers
{
    // TODO: add corresponding test
    /// <summary>
    /// <see cref="WavmFileFunctionStoreLayerSubProvider"/> implements the
    /// <see cref="IWaveLayerSubProvider"/> for data of type <see cref="WavmFileFunctionStore"/>.
    /// </summary>
    /// <seealso cref="IWaveLayerSubProvider" />
    public class WavmFileFunctionStoreLayerSubProvider : IWaveLayerSubProvider
    {
        private readonly Func<IEnumerable<WaveModel>> getWaveModelsFunc;
        private readonly IWaveLayerFactory factory;

        /// <summary>
        /// Creates a new <see cref="WaveSnappedFeaturesGroupLayerDataLayerSubProvider"/>.
        /// </summary>
        /// <param name="factory">The factory to build the layers with.</param>
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

            if (!(sourceData is WavmFileFunctionStore store &&
                  store.Functions.Any()))
            {
                return null;
            }

            string domainName = store.Path;
            var overrideDomainName = true;

            if (parentData is IWaveModel model)
            {
                IDataItem dataItem = model.GetDataItemByValue(store);
                string dataItemTag = dataItem.Tag;

                if (dataItemTag.StartsWith(WaveModel.WavmStoreDataItemTag))
                {
                    var paramsValue = new string(dataItemTag.Skip(WaveModel.WavmStoreDataItemTag.Length).ToArray());
                    domainName = string.Join(" ", paramsValue, "WAVM");
                    overrideDomainName = false;
                }
            }

            return factory.CreateOutputLayer(domainName, overrideDomainName);
        }

        public IEnumerable<object> GenerateChildLayerObjects(object data)
        {
            if (!(data is WavmFileFunctionStore store))
            {
                yield break;
            }
            
            WaveModel waveModel = getWaveModelsFunc?.Invoke().FirstOrDefault(m => m.WavmFunctionStores.Contains(store));
            if (waveModel == null)
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