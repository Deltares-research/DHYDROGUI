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
    internal class EstimatedSnappedFeatureGroupLayerSubProviderTest : LayerSubProviderBaseFixture<
        EstimatedSnappedFeatureGroupLayerSubProvider,
        EstimatedSnappedFeatureGroupLayerSubProviderTest.CanCreateLayerForParams,
        EstimatedSnappedFeatureGroupLayerSubProviderTest.CreateLayerParams,
        EstimatedSnappedFeatureGroupLayerSubProviderTest.GenerateChildLayerObjectsParams
    >

    {
        public class CanCreateLayerForParams : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                var model = Substitute.For<IWaterFlowFMModel>();
                var groupData = new EstimatedSnappedFeatureGroupData(model);
                yield return new TestCaseData(null, null, false);
                yield return new TestCaseData(new object(), null, false);
                yield return new TestCaseData(groupData, null, true);
                yield return new TestCaseData(groupData, new object(), true);
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public class CreateLayerParams : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                var model = Substitute.For<IWaterFlowFMModel>();
                var groupData = new EstimatedSnappedFeatureGroupData(model);

                var layer = Substitute.For<ILayer>();

                void ConfigureMock(IFlowFMLayerInstanceCreator instanceCreator) =>
                    instanceCreator.CreateEstimatedSnappedFeatureGroupLayer().Returns(layer);

                Action<IFlowFMLayerInstanceCreator, ILayer> assertValid =
                    CommonAsserts.CreatedLayer(layer, creator => creator.CreateEstimatedSnappedFeatureGroupLayer());



                yield return new TestCaseData(groupData,
                                              null,
                                              (Action<IFlowFMLayerInstanceCreator>)ConfigureMock,
                                              assertValid);
                yield return new TestCaseData(groupData,
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
                var groupData = new EstimatedSnappedFeatureGroupData(model);

                object[] expectedChildren =
                {
                    new EstimatedSnappedFeatureData(
                        model,
                        EstimatedSnappedFeatureType.ObservationPoints),
                    new EstimatedSnappedFeatureData(
                        model,
                        EstimatedSnappedFeatureType.ThinDams),
                    new EstimatedSnappedFeatureData(
                        model,
                        EstimatedSnappedFeatureType.FixedWeirs),
                    new EstimatedSnappedFeatureData(
                        model,
                        EstimatedSnappedFeatureType.LeveeBreaches),
                    new EstimatedSnappedFeatureData(
                        model,
                        EstimatedSnappedFeatureType.RoofAreas),
                    new EstimatedSnappedFeatureData(
                        model,
                        EstimatedSnappedFeatureType.DryPoints),
                    new EstimatedSnappedFeatureData(
                        model,
                        EstimatedSnappedFeatureType.DryAreas),
                    new EstimatedSnappedFeatureData(
                        model,
                        EstimatedSnappedFeatureType.Enclosures),
                    new EstimatedSnappedFeatureData(
                        model,
                        EstimatedSnappedFeatureType.Pumps),
                    new EstimatedSnappedFeatureData(
                        model,
                        EstimatedSnappedFeatureType.Weirs),
                    new EstimatedSnappedFeatureData(
                        model,
                        EstimatedSnappedFeatureType.Gates),
                    new EstimatedSnappedFeatureData(
                        model,
                        EstimatedSnappedFeatureType.ObservationCrossSections),
                    new EstimatedSnappedFeatureData(
                        model,
                        EstimatedSnappedFeatureType.Embankments),
                    new EstimatedSnappedFeatureData(
                        model,
                        EstimatedSnappedFeatureType.SourcesAndSinks),
                    new EstimatedSnappedFeatureData(
                        model,
                        EstimatedSnappedFeatureType.Boundaries),
                    new EstimatedSnappedFeatureData(
                        model,
                        EstimatedSnappedFeatureType.BoundariesWaterLevel),
                    new EstimatedSnappedFeatureData(
                        model,
                        EstimatedSnappedFeatureType.BoundariesVelocity),
                };

                yield return new TestCaseData(groupData, CommonAsserts.ChildrenEqualTo(expectedChildren));
                yield return new TestCaseData(null, CommonAsserts.NoChildren());
                yield return new TestCaseData(new object(), CommonAsserts.NoChildren());
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        protected override EstimatedSnappedFeatureGroupLayerSubProvider CreateDefault(IFlowFMLayerInstanceCreator instanceCreator) =>
            new EstimatedSnappedFeatureGroupLayerSubProvider(instanceCreator);
    }
}