using System;
using System.Collections;
using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersOutput;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersOutput.SnappedFeatures;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers.Providers.ProvidersOutput
{
    [TestFixture]
    internal class OutputGroupLayerSubProviderTest : LayerSubProviderBaseFixture<
        OutputGroupLayerSubProvider,
        OutputGroupLayerSubProviderTest.CanCreateLayerForParams,
        OutputGroupLayerSubProviderTest.CreateLayerParams,
        OutputGroupLayerSubProviderTest.GenerateChildLayerObjectsParams
    >
    {
        public class CanCreateLayerForParams : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            { 
                var model = Substitute.For<IWaterFlowFMModel>();
                var otherModel = Substitute.For<IWaterFlowFMModel>();
                var outputLayerData = new OutputLayerData(model);

                yield return new TestCaseData(new object(), null, false);
                yield return new TestCaseData(null, null, false);

                yield return new TestCaseData(outputLayerData, null, false);
                yield return new TestCaseData(outputLayerData, new object(), false);
                yield return new TestCaseData(outputLayerData, otherModel, false);
                
                yield return new TestCaseData(outputLayerData, model, true);
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public class CreateLayerParams : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                var model = Substitute.For<IWaterFlowFMModel>();
                var outputLayerData = new OutputLayerData(model);

                var layer = Substitute.For<ILayer>();

                void ConfigureMock(IFlowFMLayerInstanceCreator instanceCreator) =>
                    instanceCreator.CreateOutputGroupLayer().Returns(layer);

                Action<IFlowFMLayerInstanceCreator, ILayer> assertValid =
                    CommonAsserts.CreatedLayer(layer, creator => creator.CreateOutputGroupLayer());

                yield return new TestCaseData(outputLayerData,
                                              model,
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
                yield return new TestCaseData(outputLayerData,
                                              new object(),
                                              (Action<IFlowFMLayerInstanceCreator>)ConfigureMock,
                                              CommonAsserts.NoLayerCreated());

                var otherModel = Substitute.For<IWaterFlowFMModel>();

                yield return new TestCaseData(outputLayerData,
                                              otherModel,
                                              (Action<IFlowFMLayerInstanceCreator>)ConfigureMock,
                                              CommonAsserts.NoLayerCreated());
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public class GenerateChildLayerObjectsParams : IEnumerable<TestCaseData>
        {
            private static TestCaseData TestCase(bool withSnappedOutput = false, 
                                                 bool with1D = false,
                                                 bool withMap = false, 
                                                 bool withHis = false, 
                                                 bool withClass = false, 
                                                 bool withFou = false)
            {
                var model = Substitute.For<IWaterFlowFMModel>();
                var children = new List<object>();

                if (withSnappedOutput)
                {
                    model.OutputSnappedFeaturesPath.Returns(".");
                    model.WriteSnappedFeatures.Returns(true);
                    children.Add(new OutputSnappedFeatureGroupData(model));
                }

                if (with1D)
                { 
                    var network = Substitute.For<IHydroNetwork>();
                    model.Network.Returns(network);
                    var store = new FM1DFileFunctionStore(network); 
                    model.Output1DFileStore.Returns(store);
                    children.Add(store);
                }

                if (withMap)
                {
                    var store = new FMMapFileFunctionStore();
                    model.OutputMapFileStore.Returns(store);
                    children.Add(store);
                }

                if (withHis)
                {
                    var network = Substitute.For<IHydroNetwork>();
                    var area = new HydroArea();
                    var store = new FMHisFileFunctionStore(network, area);
                    model.OutputHisFileStore.Returns(store);
                    children.Add(store);
                }

                if (withClass)
                {
                    var store = new FMClassMapFileFunctionStore("");
                    model.OutputClassMapFileStore.Returns(store);
                    children.Add(store);
                }

                if (withFou)
                {
                    var store = new FouFileFunctionStore();
                    model.OutputFouFileStore.Returns(store);
                    children.Add(store);
                }

                return new TestCaseData(new OutputLayerData(model), 
                                        CommonAsserts.ChildrenEqualTo(children));
            }

            public IEnumerator<TestCaseData> GetEnumerator()
            {
                yield return TestCase();
                yield return TestCase(withSnappedOutput: true);
                yield return TestCase(with1D: true);
                yield return TestCase(withMap: true);
                yield return TestCase(withHis: true);
                yield return TestCase(withClass: true);
                yield return TestCase(withFou: true);
                yield return TestCase(withSnappedOutput: true, with1D: true, withMap: true, withHis: true, withClass: true, withFou: true);

                var model = Substitute.For<IWaterFlowFMModel>();
                yield return new TestCaseData(
                    new InputLayerData(model, LayerDataDimension.Data1D), 
                    CommonAsserts.NoChildren());

                yield return new TestCaseData(null, CommonAsserts.NoChildren());
                yield return new TestCaseData(new object(), CommonAsserts.NoChildren());
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        protected override OutputGroupLayerSubProvider CreateDefault(IFlowFMLayerInstanceCreator instanceCreator) =>
            new OutputGroupLayerSubProvider(instanceCreator);
    }
}