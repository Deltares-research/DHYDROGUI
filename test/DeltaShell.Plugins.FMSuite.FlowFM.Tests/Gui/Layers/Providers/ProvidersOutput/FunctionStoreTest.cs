using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersOutput;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers.Providers.Common;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers.Providers.ProvidersOutput
{
    [TestFixture]
    public class FunctionStoreTest
    {
        [TestFixture]
        internal class FM1DFileFunctionStoreDescriptionTest :
            BaseFunctionStoreDescriptionTestFixture<
                FunctionStore.FM1DFileFunctionStoreDescription,
                FM1DFileFunctionStore>
        {
            public override ILayer GetLayer(IFlowFMLayerInstanceCreator creator) =>
                creator.CreateMapFileFunctionStore1DLayer();

            public override FM1DFileFunctionStore GetStore() =>
                new FM1DFileFunctionStore(Substitute.For<IHydroNetwork>());

            public override IEnumerable<object> GetChildren(FM1DFileFunctionStore store) =>
                (new object[]
                    {
                        store.OutputNetwork,
                        store.OutputDiscretization
                    }).Concat(store.Functions);
        }

        [TestFixture]
        internal class ClassMapFileFunctionStoreDescription :
            BaseFunctionStoreDescriptionTestFixture<
                FunctionStore.ClassMapFileFunctionStoreDescription,
                FMClassMapFileFunctionStore>
        {
            public override ILayer GetLayer(IFlowFMLayerInstanceCreator creator) =>
                creator.CreateClassMapFileFunctionStoreLayer();

            public override FMClassMapFileFunctionStore GetStore() =>
                new FMClassMapFileFunctionStore("");

            public override IEnumerable<object> GetChildren(FMClassMapFileFunctionStore store) =>
                (new object[]
                    {
                        store.Network,
                        store.Grid,
                        store.Discretization,
                        store.Links,
                    }).Concat(store.Functions);
        }

        [TestFixture]
        internal class FouFileFunctionStoreDescriptionTest :
            BaseFunctionStoreDescriptionTestFixture<
                FunctionStore.FouFileFunctionStoreDescription,
                FouFileFunctionStore>
        {
            public override ILayer GetLayer(IFlowFMLayerInstanceCreator creator) =>
                creator.CreateFouFileFunctionStoreLayer();

            public override FouFileFunctionStore GetStore() =>
                new FouFileFunctionStore();

            public override IEnumerable<object> GetChildren(FouFileFunctionStore store) =>
                store.Functions;
        }

        [TestFixture]
        internal class HisFileFunctionStoreDescriptionTest :
            BaseFunctionStoreDescriptionTestFixture<
                FunctionStore.HisFileFunctionStoreDescription,
                FMHisFileFunctionStore>
        {
            public override ILayer GetLayer(IFlowFMLayerInstanceCreator creator) =>
                creator.CreateHisFileFunctionStoreLayer();


            public override FMHisFileFunctionStore GetStore()
            {
                var network = Substitute.For<IHydroNetwork>();
                var area = new HydroArea();
                return new FMHisFileFunctionStore(network, area);
            }

            public override IEnumerable<object> GetChildren(FMHisFileFunctionStore store) =>
                store.Functions;
        }

        [TestFixture]
        internal class MapFileFunctionStoreDescriptionTest :
            BaseFunctionStoreDescriptionTestFixture<
                FunctionStore.MapFileFunctionStoreDescription,
                FMMapFileFunctionStore>
        {
            public override ILayer GetLayer(IFlowFMLayerInstanceCreator creator) =>
                creator.CreateMapFileFunctionStore2DLayer();

            public override FMMapFileFunctionStore GetStore() =>
                new FMMapFileFunctionStore();

            public override IEnumerable<object> GetChildren(FMMapFileFunctionStore store)
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