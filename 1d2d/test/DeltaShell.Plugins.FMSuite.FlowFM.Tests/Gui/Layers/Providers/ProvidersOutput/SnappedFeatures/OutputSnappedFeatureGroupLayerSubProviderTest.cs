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
    [TestFixture]
    internal class OutputSnappedFeatureGroupLayerSubProviderTest : LayerSubProviderBaseFixture<
        OutputSnappedFeatureGroupLayerSubProvider,
        OutputSnappedFeatureGroupLayerSubProviderTest.CanCreateLayerForParams,
        OutputSnappedFeatureGroupLayerSubProviderTest.CreateLayerParams,
        OutputSnappedFeatureGroupLayerSubProviderTest.GenerateChildLayerObjectsParams
    >

    {
        public class CanCreateLayerForParams : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                yield return new TestCaseData(null, null, false);
                yield return new TestCaseData(new object(), null, false);
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public class CreateLayerParams : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                var layer = Substitute.For<ILayer>();
                void ConfigureMock(IFlowFMLayerInstanceCreator instanceCreator) =>
                    instanceCreator.CreateOutputSnappedFeatureGroupLayer().Returns(layer);

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
                yield return new TestCaseData(null, CommonAsserts.NoChildren());
                yield return new TestCaseData(new object(), CommonAsserts.NoChildren());
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        protected override OutputSnappedFeatureGroupLayerSubProvider CreateDefault(IFlowFMLayerInstanceCreator instanceCreator) =>
            new OutputSnappedFeatureGroupLayerSubProvider(instanceCreator);
    }
}