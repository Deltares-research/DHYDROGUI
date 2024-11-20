using System;
using System.Collections;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersOutput.SnappedFeatures;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers.Providers.ProvidersOutput.SnappedFeatures
{
    internal class OutputSnappedFeatureLayerSubProviderTest : LayerSubProviderBaseFixture<
        OutputSnappedFeatureLayerSubProvider,
        OutputSnappedFeatureLayerSubProviderTest.CanCreateLayerForParams,
        OutputSnappedFeatureLayerSubProviderTest.CreateLayerParams,
        OutputSnappedFeatureLayerSubProviderTest.GenerateChildLayerObjectsParams
    >
    {
        public class CanCreateLayerForParams : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                var model = Substitute.For<IWaterFlowFMModel>();
                var data = new OutputSnappedFeatureData(
                    model, 
                    "layerName", 
                    "dataPath");

                yield return new TestCaseData(new object(), null, false);
                yield return new TestCaseData(null, null, false);

                yield return new TestCaseData(data, null, true);
                yield return new TestCaseData(data, new object(), true);
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public class CreateLayerParams : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                const string layerName = "layerName";
                const string dataPath = "dataPath";

                var model = Substitute.For<IWaterFlowFMModel>();
                var data = new OutputSnappedFeatureData(model, layerName, dataPath);

                var layer = Substitute.For<ILayer>();

                void ConfigureMock(IFlowFMLayerInstanceCreator instanceCreator) =>
                    instanceCreator.CreateOutputSnappedFeatureLayer(layerName, dataPath, model).Returns(layer);

                Action<IFlowFMLayerInstanceCreator, ILayer> assertValid =
                    CommonAsserts.CreatedLayer(layer, creator => creator.CreateOutputSnappedFeatureLayer(layerName, dataPath, model));

                yield return new TestCaseData(data,
                                              null,
                                              (Action<IFlowFMLayerInstanceCreator>)ConfigureMock,
                                              assertValid);
                yield return new TestCaseData(data,
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
                var model = Substitute.For<IWaterFlowFMModel>();
                var data = new OutputSnappedFeatureData(model, "layer", "path");

                yield return new TestCaseData(data, CommonAsserts.NoChildren());
                yield return new TestCaseData(null, CommonAsserts.NoChildren());
                yield return new TestCaseData(new object(), CommonAsserts.NoChildren());
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        protected override OutputSnappedFeatureLayerSubProvider CreateDefault(IFlowFMLayerInstanceCreator instanceCreator) =>
            new OutputSnappedFeatureLayerSubProvider(instanceCreator);
    }
}