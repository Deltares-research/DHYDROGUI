using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersOutput;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers.Providers.ProvidersOutput
{
    [TestFixture]
    internal class FunctionGroupingLayerSubProviderTest : LayerSubProviderBaseFixture<
        FunctionGroupingLayerSubProvider,
        FunctionGroupingLayerSubProviderTest.CanCreateLayerForParams,
        FunctionGroupingLayerSubProviderTest.CreateLayerParams,
        FunctionGroupingLayerSubProviderTest.GenerateChildLayerObjectsParams
    >
    {
        public class CanCreateLayerForParams : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                IGrouping<string, IFunction> functionsGrouping =
                    Enumerable.Range(0, 10).Select(_ => Substitute.For<IFunction>())
                              .GroupBy(x => "key")
                              .First();

                yield return new TestCaseData(new object(), null, false);
                yield return new TestCaseData(null, null, false);

                yield return new TestCaseData(functionsGrouping, null, true);
                yield return new TestCaseData(functionsGrouping, new object(), true);
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public class CreateLayerParams : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                IGrouping<string, IFunction> functionsGrouping =
                    Enumerable.Range(0, 10).Select(_ => Substitute.For<IFunction>())
                              .GroupBy(x => "key")
                              .First();

                var layer = Substitute.For<ILayer>();

                void ConfigureMock(IFlowFMLayerInstanceCreator instanceCreator) =>
                    instanceCreator.CreateFunctionGroupingLayer(functionsGrouping).Returns(layer);

                Action<IFlowFMLayerInstanceCreator, ILayer> assertValid =
                    CommonAsserts.CreatedLayer(layer, creator => creator.CreateFunctionGroupingLayer(functionsGrouping));

                yield return new TestCaseData(functionsGrouping,
                                              null,
                                              (Action<IFlowFMLayerInstanceCreator>)ConfigureMock,
                                              assertValid);
                yield return new TestCaseData(functionsGrouping,
                                              new object(),
                                              (Action<IFlowFMLayerInstanceCreator>)ConfigureMock,
                                              assertValid);

                yield return new TestCaseData(null,
                                              null,
                                              (Action<IFlowFMLayerInstanceCreator>)ConfigureMock,
                                              CommonAsserts.NoLayerCreated());
                yield return new TestCaseData(new object(),
                                              new object(),
                                              (Action<IFlowFMLayerInstanceCreator>)ConfigureMock,
                                              CommonAsserts.NoLayerCreated());
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        
        public class GenerateChildLayerObjectsParams : IEnumerable<TestCaseData>
        {

            public IEnumerator<TestCaseData> GetEnumerator()
            {

                IGrouping<string, IFunction> functionsGrouping =
                    Enumerable.Range(0, 10).Select(_ => Substitute.For<IFunction>())
                              .GroupBy(x => "key")
                              .First();

                yield return new TestCaseData(functionsGrouping,
                                              CommonAsserts.ChildrenEqualTo(functionsGrouping.Select(v => (object) v).ToList()));
                yield return new TestCaseData(null, CommonAsserts.NoChildren());
                yield return new TestCaseData(new object(), CommonAsserts.NoChildren());
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        protected override FunctionGroupingLayerSubProvider CreateDefault(IFlowFMLayerInstanceCreator instanceCreator) =>
            new FunctionGroupingLayerSubProvider(instanceCreator);
    }
}