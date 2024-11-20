using System;
using System.Collections;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput2D.SnappedFeatures;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers.Providers.ProvidersInput2D.SnappedFeatures
{
    [TestFixture]
    internal class EstimatedSnappedFeatureLayerSubProviderTest : LayerSubProviderBaseFixture<
        EstimatedSnappedFeatureLayerSubProvider,
        EstimatedSnappedFeatureLayerSubProviderTest.CanCreateLayerForParams,
        EstimatedSnappedFeatureLayerSubProviderTest.CreateLayerParams,
        EstimatedSnappedFeatureLayerSubProviderTest.GenerateChildLayerObjectsParams
    >
    {
        public class CanCreateLayerForParams : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                var model = Substitute.For<IWaterFlowFMModel>();
                var data = new EstimatedSnappedFeatureData(
                    model, 
                    EstimatedSnappedFeatureType.BoundariesVelocity);

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
                const EstimatedSnappedFeatureType featureType = EstimatedSnappedFeatureType.BoundariesVelocity;

                var model = Substitute.For<IWaterFlowFMModel>();
                var data = new EstimatedSnappedFeatureData(model, featureType);

                var layer = Substitute.For<ILayer>();

                void ConfigureMock(IFlowFMLayerInstanceCreator instanceCreator) =>
                    instanceCreator.CreateEstimatedSnappedFeatureLayer(model, featureType).Returns(layer);

                Action<IFlowFMLayerInstanceCreator, ILayer> assertValid =
                    CommonAsserts.CreatedLayer(layer, creator => creator.CreateEstimatedSnappedFeatureLayer(model, featureType));

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
                const EstimatedSnappedFeatureType featureType = EstimatedSnappedFeatureType.BoundariesVelocity;

                var model = Substitute.For<IWaterFlowFMModel>();
                var data = new EstimatedSnappedFeatureData(model, featureType);

                yield return new TestCaseData(data,
                                              CommonAsserts.NoChildren());
                yield return new TestCaseData(null, CommonAsserts.NoChildren());
                yield return new TestCaseData(new object(), CommonAsserts.NoChildren());
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        protected override EstimatedSnappedFeatureLayerSubProvider CreateDefault(IFlowFMLayerInstanceCreator instanceCreator) =>
            new EstimatedSnappedFeatureLayerSubProvider(instanceCreator);
    }
}