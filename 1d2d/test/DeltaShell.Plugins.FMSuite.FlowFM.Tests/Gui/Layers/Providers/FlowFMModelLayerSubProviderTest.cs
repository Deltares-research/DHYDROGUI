using System;
using System.Collections;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers.Providers
{
    [TestFixture]
    internal class FlowFMModelLayerSubProviderTest : LayerSubProviderBaseFixture<
        FlowFMModelLayerSubProvider,
        FlowFMModelLayerSubProviderTest.CanCreateLayerForParams,
        FlowFMModelLayerSubProviderTest.CreateLayerParams,
        FlowFMModelLayerSubProviderTest.GenerateChildLayerObjectsParams
    >
    {
        public class CanCreateLayerForParams : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                var model = Substitute.For<IWaterFlowFMModel>();
                yield return new TestCaseData(new object(), null, false);
                yield return new TestCaseData(null, null, false);
                yield return new TestCaseData(model, null, true);
                yield return new TestCaseData(model, new object(), true);
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public class CreateLayerParams : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                var model = Substitute.For<IWaterFlowFMModel>();
                var layer = Substitute.For<ILayer>();

                void ConfigureMock(IFlowFMLayerInstanceCreator instanceCreator) =>
                    instanceCreator.CreateModelGroupLayer(model).Returns(layer);

                Action<IFlowFMLayerInstanceCreator, ILayer> assertValid =
                    CommonAsserts.CreatedLayer(layer, creator => creator.CreateModelGroupLayer(model));

                yield return new TestCaseData(model,
                                              null,
                                              (Action<IFlowFMLayerInstanceCreator>)ConfigureMock,
                                              assertValid);
                yield return new TestCaseData(model,
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
                var modelWithoutOutput = Substitute.For<IWaterFlowFMModel>();
                modelWithoutOutput.OutputIsEmpty.Returns(true);

                object[] expectedWithoutOutputObjects =
                {
                    new InputLayerData(modelWithoutOutput, LayerDataDimension.Data1D),
                    new InputLayerData(modelWithoutOutput, LayerDataDimension.Data2D),
                };

                yield return new TestCaseData(modelWithoutOutput, CommonAsserts.ChildrenEqualTo(expectedWithoutOutputObjects));

                var modelWithOutput = Substitute.For<IWaterFlowFMModel>();
                modelWithOutput.OutputIsEmpty.Returns(false);

                object[] expectedWithOutputObjects =
                {
                    new InputLayerData(modelWithOutput, LayerDataDimension.Data1D),
                    new InputLayerData(modelWithOutput, LayerDataDimension.Data2D),
                    new OutputLayerData(modelWithOutput),
                };

                yield return new TestCaseData(modelWithOutput, CommonAsserts.ChildrenEqualTo(expectedWithOutputObjects));

                yield return new TestCaseData(null, CommonAsserts.NoChildren());
                yield return new TestCaseData(new object(), CommonAsserts.NoChildren());
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        protected override FlowFMModelLayerSubProvider CreateDefault(IFlowFMLayerInstanceCreator instanceCreator) =>
            new FlowFMModelLayerSubProvider(instanceCreator);
    }
}