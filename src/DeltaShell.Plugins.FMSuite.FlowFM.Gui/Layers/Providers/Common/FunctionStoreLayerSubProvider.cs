using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Gui.MapLayers.Providers;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.Common
{
    /// <summary>
    /// <see cref="FunctionStoreLayerSubProvider{TDescription,TStore}"/> implements the
    /// <see cref="ILayerSubProvider"/> for a <typeparamref name="TStore"/> with the help
    /// of a <typeparamref name="TDescription"/>.
    /// </summary>
    /// <typeparam name="TDescription">The description used to generate the children and layers.</typeparam>
    /// <typeparam name="TStore">The actual store type for which this provider provides.</typeparam>
    internal sealed class FunctionStoreLayerSubProvider<TDescription, TStore> : ILayerSubProvider
        where TDescription : BaseFunctionStoreDescription<TStore>, new()
        where TStore : IFunctionStore
    {
        private readonly IFlowFMLayerInstanceCreator instanceCreator;
        private readonly TDescription storeDescription;

        /// <summary>
        /// Creates a new <see cref="FunctionStoreLayerSubProvider{TDescription,TStore}"/> with the
        /// provided <paramref name="instanceCreator"/>.
        /// </summary>
        /// <param name="instanceCreator">The instance creator used to construct the layers.</param>
        /// <param name="storeDescription">The object used to generate the children and layers.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="instanceCreator"/> is <c>null</c>.
        /// </exception>
        public FunctionStoreLayerSubProvider(IFlowFMLayerInstanceCreator instanceCreator,
                                            TDescription storeDescription)
        {
            Ensure.NotNull(instanceCreator, nameof(instanceCreator));
            this.instanceCreator = instanceCreator;
            this.storeDescription = storeDescription;
        }

        /// <summary>
        /// Creates a new <see cref="FunctionStoreLayerSubProvider{TDescription,TStore}"/> with the
        /// provided <paramref name="instanceCreator"/>.
        /// </summary>
        /// <param name="instanceCreator">The instance creator used to construct the layers.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="instanceCreator"/> is <c>null</c>.
        /// </exception>
        public FunctionStoreLayerSubProvider(IFlowFMLayerInstanceCreator instanceCreator) :
            this(instanceCreator, new TDescription()) { }

        public bool CanCreateLayerFor(object sourceData, object parentData) =>
            sourceData is TStore;

        public ILayer CreateLayer(object sourceData, object parentData) =>
            CanCreateLayerFor(sourceData, parentData)
                ? storeDescription.CreateLayer(instanceCreator)
                : null;

        public IEnumerable<object> GenerateChildLayerObjects(object data) =>
            data is TStore store
                ? storeDescription.GenerateChildren(store)
                : Enumerable.Empty<object>();
    }
}