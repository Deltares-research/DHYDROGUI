using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Common.FunctionStores;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers.OutputData
{
    /// <summary>
    /// <see cref="WaveFileFunctionStoreLayerSubProviderBase{T}" /> defines the common logic
    /// for sub layer providers of Wave file function stores.
    /// </summary>
    /// <typeparam name="T">The type of wave file function store.</typeparam>
    /// <seealso cref="ILayerSubProvider" />
    public abstract class WaveFileFunctionStoreLayerSubProviderBase<T> : ILayerSubProvider 
        where T : IFMNetCdfFileFunctionStore
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
        protected WaveFileFunctionStoreLayerSubProviderBase(
            IWaveLayerInstanceCreator instanceCreator,
            Func<IEnumerable<IWaveModel>> getWaveModelsFunc)
        {
            Ensure.NotNull(instanceCreator, nameof(instanceCreator));
            Ensure.NotNull(getWaveModelsFunc, nameof(getWaveModelsFunc));

            this.instanceCreator = instanceCreator;
            this.getWaveModelsFunc = getWaveModelsFunc;
        }

        public bool CanCreateLayerFor(object sourceData, object parentData) => 
            sourceData is T store && store.Functions.Any();

        public ILayer CreateLayer(object sourceData, object parentData) =>
            sourceData is T funcStore ? instanceCreator.CreateWaveOutputGroupLayer(Path.GetFileName(funcStore.Path)) : null;

        public abstract IEnumerable<object> GenerateChildLayerObjects(object data);

        /// <summary>
        /// Determines whether <paramref name="store"/> is contained in <paramref name="model"/>.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="model">The model.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="store"/> is contained in <paramref name="model"/>; otherwise, <c>false</c>.
        /// </returns>
        protected abstract bool IsContainedInModel(T store, IWaveModel model);

        /// <summary>
        /// Determines whether <paramref name="store"/> is a stand alone model.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <returns>
        /// <c>true</c> if [is stand alone function store] [the specified store]; otherwise, <c>false</c>.
        /// </returns>
        protected bool IsStandAloneFunctionStore(T store) =>
            !getWaveModelsFunc.Invoke().Any(m => IsContainedInModel(store, m));
    }
}