using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DelftTools.Hydro.Link1d2d;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersOutput;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using GeoAPI.Extensions.CoordinateSystems;
using NetTopologySuite.Extensions.Grids;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers.Providers.ProvidersOutput
{
    [TestFixture]
    internal class Links1D2DOutputLayerSubProviderTest : LayerSubProviderBaseFixture<
        Links1D2DOutputLayerSubProvider,
        Links1D2DOutputLayerSubProviderTest.CanCreateLayerForParams,
        Links1D2DOutputLayerSubProviderTest.CreateLayerParams,
        Links1D2DOutputLayerSubProviderTest.GenerateChildLayerObjectsParams
    >
    {
        public class CanCreateLayerForParams : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                List<ILink1D2D> sourceData = 
                    Enumerable.Range(0, 5)
                              .Select(_ => Substitute.For<ILink1D2D>())
                              .ToList();
                var parentDataMap = new FMMapFileFunctionStore();
                var parentDataClass = new FMClassMapFileFunctionStore("");

                yield return new TestCaseData(new object(), null, false);
                yield return new TestCaseData(null, null, false);
                yield return new TestCaseData(sourceData, null, false);
                yield return new TestCaseData(sourceData, new object(), false);
                yield return new TestCaseData(new object(), parentDataMap, false);
                yield return new TestCaseData(null, parentDataMap, false);

                yield return new TestCaseData(new List<ILink1D2D>(), parentDataMap, false);
                yield return new TestCaseData(new List<ILink1D2D>(), parentDataClass, false);
                
                yield return new TestCaseData(sourceData, parentDataMap, true);
                yield return new TestCaseData(sourceData, parentDataClass, true);
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public class CreateLayerParams : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                var sourceData = new List<ILink1D2D>();
                var parentDataMap = new FMMapFileFunctionStore();
                var parentDataClass = new FMClassMapFileFunctionStore("");
                
                var coordinateSystem = Substitute.For<ICoordinateSystem>();

                // HACK: Grid is null for default and cannot be set without loading data.
                var gridClass = new UnstructuredGrid()
                {
                    CoordinateSystem = coordinateSystem,
                };

                Type classStoreType = typeof(FMClassMapFileFunctionStore);
                FieldInfo gridBackingField = 
                    classStoreType.GetField("grid", BindingFlags.NonPublic | BindingFlags.Instance);
                gridBackingField.SetValue(parentDataClass, gridClass);

                var gridMap = new UnstructuredGrid()
                {
                    CoordinateSystem = coordinateSystem,
                };

                Type mapStoreType = typeof(FMMapFileFunctionStore);
                PropertyInfo gridProperty = mapStoreType.GetProperty("Grid");
                gridProperty.SetValue(parentDataMap, gridMap);

                var layer = Substitute.For<ILayer>();

                void ConfigureMock(IFlowFMLayerInstanceCreator instanceCreator) =>
                    instanceCreator.CreateLinks1D2DLayer(sourceData, coordinateSystem).Returns(layer);

                Action<IFlowFMLayerInstanceCreator, ILayer> assertValid =
                    CommonAsserts.CreatedLayer(layer, creator => creator.CreateLinks1D2DLayer(sourceData, coordinateSystem));

                yield return new TestCaseData(sourceData,
                                              parentDataClass,
                                              (Action<IFlowFMLayerInstanceCreator>)ConfigureMock,
                                              assertValid);
                yield return new TestCaseData(sourceData,
                                              parentDataMap,
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
                yield return new TestCaseData(sourceData,
                                              null,
                                              (Action<IFlowFMLayerInstanceCreator>)ConfigureMock,
                                              CommonAsserts.NoLayerCreated());
                yield return new TestCaseData(null,
                                              parentDataClass,
                                              (Action<IFlowFMLayerInstanceCreator>)ConfigureMock,
                                              CommonAsserts.NoLayerCreated());
                yield return new TestCaseData(null,
                                              parentDataMap,
                                              (Action<IFlowFMLayerInstanceCreator>)ConfigureMock,
                                              CommonAsserts.NoLayerCreated());
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        
        public class GenerateChildLayerObjectsParams : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            { 
                var sourceData = new List<ILink1D2D>();

                yield return new TestCaseData(sourceData, CommonAsserts.NoChildren());
                yield return new TestCaseData(null, CommonAsserts.NoChildren());
                yield return new TestCaseData(new object(), CommonAsserts.NoChildren());
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        protected override Links1D2DOutputLayerSubProvider CreateDefault(IFlowFMLayerInstanceCreator instanceCreator) =>
            new Links1D2DOutputLayerSubProvider(instanceCreator);
    }
}