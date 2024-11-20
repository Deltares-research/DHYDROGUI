using System.Collections.Generic;
using System.Linq;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Common.FunctionStores;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers.OutputData
{
    /// <summary>
    /// <see cref="WaveFileFunctionStoreGroupLayerSubProviderBase{T}"/> implements the
    /// <see cref="ILayerSubProvider"/> for function stores.
    /// </summary>
    /// <typeparam name="T">The type of wave function store.</typeparam>
    /// <seealso cref="ILayerSubProvider" />
    public abstract class WaveFileFunctionStoreGroupLayerSubProviderBase<T> : ILayerSubProvider 
        where T : IFMNetCdfFileFunctionStore 
    {
        private readonly IWaveLayerInstanceCreator instanceCreator;

        /// <summary>
        /// Gets the name of the layer.
        /// </summary>
        protected abstract string LayerName { get; }

        /// <summary>
        /// Creates a new <see cref="WaveFileFunctionStoreGroupLayerSubProviderBase{T}"/>.
        /// </summary>
        /// <param name="instanceCreator">The instance creator.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="instanceCreator"/> is <c>null</c>.
        /// </exception>
        protected WaveFileFunctionStoreGroupLayerSubProviderBase(IWaveLayerInstanceCreator instanceCreator)
        {
            Ensure.NotNull(instanceCreator, nameof(instanceCreator));
            this.instanceCreator = instanceCreator;
        }

        public bool CanCreateLayerFor(object sourceData, object parentData) =>
            parentData is IWaveOutputData &&
            sourceData is IEnumerable<T> mapFunctionStores &&
            mapFunctionStores.Any();

        public ILayer CreateLayer(object sourceData, object parentData) =>
            instanceCreator.CreateWaveOutputGroupLayer(LayerName);

        public IEnumerable<object> GenerateChildLayerObjects(object data)
        {
            return (IEnumerable<object>) (data is IEnumerable<T> functionStores 
                                              ? functionStores.Where(x => x.Functions.Any())
                                              : Enumerable.Empty<T>());
        }
    }
}