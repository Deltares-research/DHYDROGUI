using System.Collections.Generic;
using DelftTools.Functions;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.Common
{
    /// <summary>
    /// <see cref="BaseFunctionStoreDescription{TStore}"/> defines the base implementation
    /// for function store descriptions.
    /// </summary>
    /// <typeparam name="TStore">The type of function store.</typeparam>
    internal abstract class BaseFunctionStoreDescription<TStore>
        where TStore : IFunctionStore
    {
        /// <summary>
        /// Creates the <see cref="ILayer"/> corresponding with the <typeparamref name="TStore"/>
        /// </summary>
        /// <param name="creator">The instance used to create the layers</param>
        /// <returns>The layer associated with the <typeparamref name="TStore"/>.</returns>
        public abstract ILayer CreateLayer(IFlowFMLayerInstanceCreator creator);

        /// <summary>
        /// Generate the children objects of the provided <paramref name="store"/>.
        /// </summary>
        /// <param name="store">The store to obtain the children from.</param>
        /// <returns>The children of the provided <paramref name="store"/>.</returns>
        public virtual IEnumerable<object> GenerateChildren(TStore store) => store.Functions;
    }
}