using System;
using System.Collections;
using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput1D;
using GeoAPI.Extensions.Coverages;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers.Providers.ProvidersInput1D
{
    [TestFixture]
    internal class Input1DGroupLayerSubProviderTest : LayerSubProviderBaseFixture<
        Input1DGroupLayerSubProvider,
        Input1DGroupLayerSubProviderTest.CanCreateLayerForParams,
        Input1DGroupLayerSubProviderTest.CreateLayerParams,
        Input1DGroupLayerSubProviderTest.GenerateChildLayerObjectsParams
    >
    {
        public class CanCreateLayerForParams : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            { 
                var model = Substitute.For<IWaterFlowFMModel>();
                var otherModel = Substitute.For<IWaterFlowFMModel>();

                var inputLayerData1D = new InputLayerData(model, LayerDataDimension.Data1D);
                var inputLayerData2D = new InputLayerData(model, LayerDataDimension.Data2D);

                yield return new TestCaseData(new object(), null, false);
                yield return new TestCaseData(null, null, false);

                yield return new TestCaseData(inputLayerData1D, null, false);
                yield return new TestCaseData(inputLayerData1D, new object(), false);
                yield return new TestCaseData(inputLayerData1D, otherModel, false);
                
                yield return new TestCaseData(inputLayerData2D, null, false);
                yield return new TestCaseData(inputLayerData2D, new object(), false);
                yield return new TestCaseData(inputLayerData2D, model, false);
                yield return new TestCaseData(inputLayerData2D, otherModel, false);

                yield return new TestCaseData(inputLayerData1D, model, true);
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public class CreateLayerParams : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                var model = Substitute.For<IWaterFlowFMModel>();
                var otherModel = Substitute.For<IWaterFlowFMModel>();

                var inputLayerData1D = new InputLayerData(model, LayerDataDimension.Data1D);
                var inputLayerData2D = new InputLayerData(model, LayerDataDimension.Data2D);

                var layer = Substitute.For<ILayer>();

                void ConfigureMock(IFlowFMLayerInstanceCreator instanceCreator) =>
                    instanceCreator.Create1DGroupLayer().Returns(layer);

                Action<IFlowFMLayerInstanceCreator, ILayer> assertValid =
                    CommonAsserts.CreatedLayer(layer, creator => creator.Create1DGroupLayer());

                yield return new TestCaseData(inputLayerData1D,
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
                yield return new TestCaseData(inputLayerData1D,
                                              new object(),
                                              (Action<IFlowFMLayerInstanceCreator>)ConfigureMock,
                                              CommonAsserts.NoLayerCreated());
                yield return new TestCaseData(inputLayerData1D,
                                              otherModel,
                                              (Action<IFlowFMLayerInstanceCreator>)ConfigureMock,
                                              CommonAsserts.NoLayerCreated());
                yield return new TestCaseData(inputLayerData2D,
                                              model,
                                              (Action<IFlowFMLayerInstanceCreator>)ConfigureMock,
                                              CommonAsserts.NoLayerCreated());
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        
        public class GenerateChildLayerObjectsParams : IEnumerable<TestCaseData>
        {
            private static IWaterFlowFMModel CreateMockedModel()
            {
                var model = Substitute.For<IWaterFlowFMModel>();
                model.Network.Returns(Substitute.For<IHydroNetwork>());
                model.NetworkDiscretization.Returns(Substitute.For<IDiscretization>());
                model.BoundaryConditions1D.Returns(new EventedList<Model1DBoundaryNodeData>());
                model.LateralSourcesData.Returns(new EventedList<Model1DLateralSourceData>());

                return model;
            }

            public IEnumerator<TestCaseData> GetEnumerator()
            {
                IWaterFlowFMModel modelWithNetwork = CreateMockedModel();
                var networkDataItem = Substitute.For<IDataItem>();
                networkDataItem.LinkedTo.Returns((ILinkable<IDataItem>)null);
                modelWithNetwork.GetDataItemByTag(WaterFlowFMModelDataSet.NetworkTag)
                                .Returns(networkDataItem);

                object[] childrenWithNetwork =
                {
                    modelWithNetwork.Network,
                    modelWithNetwork.NetworkDiscretization,
                    modelWithNetwork.BoundaryConditions1D,
                    modelWithNetwork.LateralSourcesData,
                    new InputFeatureGroupLayerData(modelWithNetwork, FeatureType.InitialConditions),
                    new InputFeatureGroupLayerData(modelWithNetwork, FeatureType.Friction),
                };

                yield return new TestCaseData(
                    new InputLayerData(modelWithNetwork, LayerDataDimension.Data1D),
                    CommonAsserts.ChildrenEqualTo(childrenWithNetwork));


                yield return new TestCaseData(
                    new InputLayerData(modelWithNetwork, LayerDataDimension.Data2D), 
                    CommonAsserts.NoChildren());

                yield return new TestCaseData(null, CommonAsserts.NoChildren());
                yield return new TestCaseData(new object(), CommonAsserts.NoChildren());
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        protected override Input1DGroupLayerSubProvider CreateDefault(IFlowFMLayerInstanceCreator instanceCreator) =>
            new Input1DGroupLayerSubProvider(instanceCreator);
    }
}