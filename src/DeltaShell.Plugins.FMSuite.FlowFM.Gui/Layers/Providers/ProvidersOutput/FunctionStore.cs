using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.Common;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersOutput
{
    /// <summary>
    /// <see cref="FunctionStore"/> defines the 2D <see cref="BaseFunctionStoreDescription{TStore}"/>
    /// implementations.
    /// </summary>
    internal static class FunctionStore
    {
        /// <summary>
        /// <see cref="FM1DFileFunctionStoreDescription"/> defines the
        /// <see cref="BaseFunctionStoreDescription{T}"/> for the <see cref="FM1DFileFunctionStore"/>.
        /// </summary>
        internal sealed class FM1DFileFunctionStoreDescription :
            BaseFunctionStoreDescription<FM1DFileFunctionStore>
        {
            public override ILayer CreateLayer(IFlowFMLayerInstanceCreator creator) =>
                creator.CreateMapFileFunctionStore1DLayer();

            public override IEnumerable<object> GenerateChildren(FM1DFileFunctionStore store)
            {
                yield return store.OutputNetwork;
                yield return store.OutputDiscretization;

                foreach (IFunction f in store.Functions) 
                { 
                    yield return f;
                }
            }
        }

        /// <summary>
        /// <see cref="ClassMapFileFunctionStoreDescription"/> defines the
        /// <see cref="BaseFunctionStoreDescription{T}"/> for the <see cref="FMClassMapFileFunctionStore"/>.
        /// </summary>
        internal sealed class ClassMapFileFunctionStoreDescription :
            BaseFunctionStoreDescription<FMClassMapFileFunctionStore>
        {
            public override ILayer CreateLayer(IFlowFMLayerInstanceCreator creator) =>
                creator.CreateClassMapFileFunctionStoreLayer();

            public override IEnumerable<object> GenerateChildren(FMClassMapFileFunctionStore store)
            {
                yield return store.Network;
                yield return store.Grid;
                yield return store.Discretization;
                yield return store.Links;

                foreach (IFunction output in store.Functions)
                {
                    yield return output;
                }
            }
        }

        /// <summary>
        /// <see cref="FouFileFunctionStoreDescription"/> defines the
        /// <see cref="BaseFunctionStoreDescription{T}"/> for the <see cref="FouFileFunctionStore"/>.
        /// </summary>
        internal sealed class FouFileFunctionStoreDescription :
            BaseFunctionStoreDescription<FouFileFunctionStore>
        {
            public override ILayer CreateLayer(IFlowFMLayerInstanceCreator creator) =>
                creator.CreateFouFileFunctionStoreLayer();
        }

        /// <summary>
        /// <see cref="HisFileFunctionStoreDescription"/> defines the
        /// <see cref="BaseFunctionStoreDescription{T}"/> for the <see cref="FMHisFileFunctionStore"/>.
        /// </summary>
        internal sealed class HisFileFunctionStoreDescription :
            BaseFunctionStoreDescription<FMHisFileFunctionStore>
        {
            public override ILayer CreateLayer(IFlowFMLayerInstanceCreator creator) =>
                creator.CreateHisFileFunctionStoreLayer();
        }

        /// <summary>
        /// <see cref="MapFileFunctionStoreDescription"/> defines the
        /// <see cref="BaseFunctionStoreDescription{T}"/> for the <see cref="FMMapFileFunctionStore"/>.
        /// </summary>
        internal sealed class MapFileFunctionStoreDescription :
            BaseFunctionStoreDescription<FMMapFileFunctionStore>
        {
            public override ILayer CreateLayer(IFlowFMLayerInstanceCreator creator) =>
                creator.CreateMapFileFunctionStore2DLayer();

            public override IEnumerable<object> GenerateChildren(FMMapFileFunctionStore store)
            {
                yield return store.Grid;
                yield return store.Links;

                foreach (IGrouping<string, IFunction> group in store.GetFunctionGrouping())
                {
                    if (group.Count() == 1)
                    {
                        yield return group.ElementAt(0);
                    }
                    else
                    {

                        yield return group;
                    }
                }

                if (store.CustomVelocityCoverage != null)
                {
                    yield return store.CustomVelocityCoverage;
                }
            }
        }
    }
}