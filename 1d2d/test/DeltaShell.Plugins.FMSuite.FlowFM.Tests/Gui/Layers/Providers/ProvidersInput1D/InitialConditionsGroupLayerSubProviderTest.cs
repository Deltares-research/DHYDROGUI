using System;
using System.Collections;
using System.Collections.Generic;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput1D;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers.Providers.ProvidersInput1D
{
    [TestFixture]
    internal class InitialConditionsGroupLayerSubProviderTest : LayerSubProviderBaseFixture<
        InitialConditionsGroupLayerSubProvider,
        InitialConditionsGroupLayerSubProviderTest.CanCreateLayerForParams,
        InitialConditionsGroupLayerSubProviderTest.CreateLayerParams,
        InitialConditionsGroupLayerSubProviderTest.GenerateChildLayerObjectsParams
    >
    {
        public class CanCreateLayerForParams : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            { 
                var model = Substitute.For<IWaterFlowFMModel>();
                var validSourceData = new InputFeatureGroupLayerData(model, FeatureType.InitialConditions);
                var invalidSourceData = new InputFeatureGroupLayerData(model, FeatureType.Friction);

                yield return new TestCaseData(new object(), null, false);
                yield return new TestCaseData(null, null, false);
                yield return new TestCaseData(invalidSourceData, null, false);
                yield return new TestCaseData(invalidSourceData, new object(), false);
                yield return new TestCaseData(validSourceData, null, true);
                yield return new TestCaseData(validSourceData, new object(), true);
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public class CreateLayerParams : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                var model = Substitute.For<IWaterFlowFMModel>();
                var validSourceData = new InputFeatureGroupLayerData(model, FeatureType.InitialConditions);
                var invalidSourceData = new InputFeatureGroupLayerData(model, FeatureType.Friction);

                var layer = Substitute.For<ILayer>();

                void ConfigureMock(IFlowFMLayerInstanceCreator instanceCreator) =>
                    instanceCreator.CreateInitialConditionsGroupLayer().Returns(layer);

                Action<IFlowFMLayerInstanceCreator, ILayer> assertValid =
                    CommonAsserts.CreatedLayer(layer, creator => creator.CreateInitialConditionsGroupLayer());

                yield return new TestCaseData(validSourceData,
                                              null,
                                              (Action<IFlowFMLayerInstanceCreator>)ConfigureMock,
                                              assertValid);
                yield return new TestCaseData(validSourceData,
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
                yield return new TestCaseData(invalidSourceData,
                                              new object(),
                                              (Action<IFlowFMLayerInstanceCreator>)ConfigureMock,
                                              CommonAsserts.NoLayerCreated());
                yield return new TestCaseData(invalidSourceData,
                                              null,
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
                var initialConditions = new EventedList<ChannelInitialConditionDefinition>();

                model.ChannelInitialConditionDefinitions.Returns(initialConditions);

                var validSourceData = new InputFeatureGroupLayerData(model, FeatureType.InitialConditions);
                var invalidSourceData = new InputFeatureGroupLayerData(model, FeatureType.Friction);

                object[] expectedChildren =
                {
                    initialConditions,
                };

                yield return new TestCaseData(
                    validSourceData,
                    CommonAsserts.ChildrenEqualTo(expectedChildren));

                yield return new TestCaseData(null, CommonAsserts.NoChildren());
                yield return new TestCaseData(new object(), CommonAsserts.NoChildren());
                yield return new TestCaseData(invalidSourceData, CommonAsserts.NoChildren());
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        protected override InitialConditionsGroupLayerSubProvider CreateDefault(IFlowFMLayerInstanceCreator instanceCreator) =>
            new InitialConditionsGroupLayerSubProvider(instanceCreator);
    }
}