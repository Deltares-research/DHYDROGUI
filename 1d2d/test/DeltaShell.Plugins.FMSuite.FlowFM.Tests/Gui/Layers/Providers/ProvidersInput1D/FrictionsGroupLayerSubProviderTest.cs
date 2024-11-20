using System;
using System.Collections;
using System.Collections.Generic;
using DelftTools.Hydro.Roughness;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput1D;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers.Providers.ProvidersInput1D
{
    [TestFixture]
    internal class FrictionsGroupLayerSubProviderTest : LayerSubProviderBaseFixture<
        FrictionsGroupLayerSubProvider,
        FrictionsGroupLayerSubProviderTest.CanCreateLayerForParams,
        FrictionsGroupLayerSubProviderTest.CreateLayerParams,
        FrictionsGroupLayerSubProviderTest.GenerateChildLayerObjectsParams
    >
    {
        public class CanCreateLayerForParams : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            { 
                var model = Substitute.For<IWaterFlowFMModel>();
                var validSourceData = new InputFeatureGroupLayerData(model, FeatureType.Friction);
                var invalidSourceData = new InputFeatureGroupLayerData(model, FeatureType.InitialConditions);

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
                var validSourceData = new InputFeatureGroupLayerData(model, FeatureType.Friction);
                var invalidSourceData = new InputFeatureGroupLayerData(model, FeatureType.InitialConditions);

                var layer = Substitute.For<ILayer>();

                void ConfigureMock(IFlowFMLayerInstanceCreator instanceCreator) =>
                    instanceCreator.CreateFrictionGroupLayer().Returns(layer);

                Action<IFlowFMLayerInstanceCreator, ILayer> assertValid =
                    CommonAsserts.CreatedLayer(layer, creator => creator.CreateFrictionGroupLayer());

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
                var channelFrictionDefinitions = new EventedList<ChannelFrictionDefinition>();
                var pipeFrictionDefinitions = new EventedList<PipeFrictionDefinition>();
                var roughnessSections = new EventedList<RoughnessSection>();

                model.ChannelFrictionDefinitions.Returns(channelFrictionDefinitions);
                model.PipeFrictionDefinitions.Returns(pipeFrictionDefinitions);
                model.RoughnessSections.Returns(roughnessSections);

                var validSourceData = new InputFeatureGroupLayerData(model, FeatureType.Friction);
                var invalidSourceData = new InputFeatureGroupLayerData(model, FeatureType.InitialConditions);

                object[] expectedChildren =
                {
                    channelFrictionDefinitions,
                    pipeFrictionDefinitions,
                    roughnessSections,
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

        protected override FrictionsGroupLayerSubProvider CreateDefault(IFlowFMLayerInstanceCreator instanceCreator) =>
            new FrictionsGroupLayerSubProvider(instanceCreator);
    }
}