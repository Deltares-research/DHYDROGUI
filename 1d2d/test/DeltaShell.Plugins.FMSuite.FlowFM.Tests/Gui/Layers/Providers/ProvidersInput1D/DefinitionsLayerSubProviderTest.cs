using System;
using System.Collections;
using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput1D;
using GeoAPI.Extensions.Feature;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers.Providers.ProvidersInput1D
{
    [TestFixture(typeof(ChannelInitialConditionDefinition))]
    [TestFixture(typeof(ChannelFrictionDefinition))]
    [TestFixture(typeof(PipeFrictionDefinition))]
    internal class DefinitionsLayerSubProviderTest<TDefinition> : LayerSubProviderBaseFixture<
        DefinitionsLayerSubProvider<TDefinition>,
        DefinitionsLayerSubProviderTest<TDefinition>.CanCreateLayerForParams,
        DefinitionsLayerSubProviderTest<TDefinition>.CreateLayerParams,
        DefinitionsLayerSubProviderTest<TDefinition>.GenerateChildLayerObjectsParams
    >
        where TDefinition : IFeature
    {
        public class CanCreateLayerForParams : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                var sourceData = new EventedList<TDefinition>();
                var parentGroupData = new InputFeatureGroupLayerData(
                    Substitute.For<IWaterFlowFMModel>(),
                    FeatureType);

                var invalidParentGroupData = new InputFeatureGroupLayerData(
                    Substitute.For<IWaterFlowFMModel>(),
                    InvalidFeatureType);

                yield return new TestCaseData(null, null, false);
                yield return new TestCaseData(new object(), null, false);
                yield return new TestCaseData(new EventedList<IFeature>(), parentGroupData, false);
                yield return new TestCaseData(sourceData, invalidParentGroupData, false);
                yield return new TestCaseData(sourceData, null, false);
                yield return new TestCaseData(sourceData, parentGroupData, true);
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public class CreateLayerParams : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                var model = Substitute.For<IWaterFlowFMModel>();
                var network = Substitute.For<IHydroNetwork>();
                model.Network.Returns(network);

                var sourceData = new EventedList<TDefinition>();
                var parentGroupData = new InputFeatureGroupLayerData(
                    model,
                    FeatureType);

                var layer = Substitute.For<ILayer>();

                void ConfigureMock(IFlowFMLayerInstanceCreator instanceCreator) =>
                    instanceCreator.CreateDefinitionsLayer(Name, sourceData, network).Returns(layer);

                Action<IFlowFMLayerInstanceCreator, ILayer> assertValid =
                    CommonAsserts.CreatedLayer(layer, creator => creator.CreateDefinitionsLayer(Name, sourceData, network));

                yield return new TestCaseData(sourceData,
                                              parentGroupData,
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

                var invalidParentGroupData = new InputFeatureGroupLayerData(
                    model,
                    InvalidFeatureType);
                yield return new TestCaseData(sourceData,
                                              invalidParentGroupData,
                                              (Action<IFlowFMLayerInstanceCreator>)ConfigureMock,
                                              CommonAsserts.NoLayerCreated());
                yield return new TestCaseData(sourceData,
                                              null,
                                              (Action<IFlowFMLayerInstanceCreator>)ConfigureMock,
                                              CommonAsserts.NoLayerCreated());
                yield return new TestCaseData(new EventedList<IFeature>(),
                                              parentGroupData,
                                              (Action<IFlowFMLayerInstanceCreator>)ConfigureMock,
                                              CommonAsserts.NoLayerCreated());
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        
        public class GenerateChildLayerObjectsParams : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            { 
                var sourceData = new EventedList<TDefinition>();

                yield return new TestCaseData(sourceData, CommonAsserts.NoChildren());
                yield return new TestCaseData(null, CommonAsserts.NoChildren());
                yield return new TestCaseData(new object(), CommonAsserts.NoChildren());
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        protected override DefinitionsLayerSubProvider<TDefinition> CreateDefault(IFlowFMLayerInstanceCreator instanceCreator) =>
            new DefinitionsLayerSubProvider<TDefinition>(Name, FeatureType, instanceCreator);

        private static string Name =>
            typeof(TDefinition) == typeof(ChannelInitialConditionDefinition) ? FlowFMLayerNames.ChannelInitialConditionDefinitionsLayerName :
            typeof(TDefinition) == typeof(ChannelFrictionDefinition) ? FlowFMLayerNames.ChannelFrictionDefinitionsLayerName:
            typeof(TDefinition) == typeof(PipeFrictionDefinition) ? FlowFMLayerNames.PipeFrictionDefinitionsLayerName:
            throw new ArgumentOutOfRangeException();
        
        private static FeatureType FeatureType =>
            typeof(TDefinition) == typeof(ChannelInitialConditionDefinition) ? FeatureType.InitialConditions :
            typeof(TDefinition) == typeof(ChannelFrictionDefinition) ? FeatureType.Friction :
            typeof(TDefinition) == typeof(PipeFrictionDefinition) ? FeatureType.Friction :
            throw new ArgumentOutOfRangeException();

        private static FeatureType InvalidFeatureType =>
            typeof(TDefinition) == typeof(ChannelInitialConditionDefinition) ? FeatureType.Friction :
            typeof(TDefinition) == typeof(ChannelFrictionDefinition) ? FeatureType.InitialConditions :
            typeof(TDefinition) == typeof(PipeFrictionDefinition) ? FeatureType.InitialConditions :
            throw new ArgumentOutOfRangeException();
    }
}