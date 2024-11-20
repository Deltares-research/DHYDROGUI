using System;
using System.Collections;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers.Providers
{
    [TestFixture]
    internal class ImportedFMNetFileLayerSubProviderTest: LayerSubProviderBaseFixture<
        ImportedFMNetFileLayerSubProvider,
        ImportedFMNetFileLayerSubProviderTest.CanCreateLayerForParams,
        ImportedFMNetFileLayerSubProviderTest.CreateLayerParams,
        ImportedFMNetFileLayerSubProviderTest.GenerateChildLayerObjectsParams
    >

    {
        public class CanCreateLayerForParams : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                var netFile = new ImportedFMNetFile("does/not/matter.nc");
                yield return new TestCaseData(null, null, false);
                yield return new TestCaseData(new object(), null, false);
                yield return new TestCaseData(netFile, null, true);
                yield return new TestCaseData(netFile, new object(), true);
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public class CreateLayerParams : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                var netFile = new ImportedFMNetFile("does/not/matter.nc");
                var layer = Substitute.For<ILayer>();

                void ConfigureMock(IFlowFMLayerInstanceCreator instanceCreator) =>
                    instanceCreator.CreateImportedFMNetFileLayer(netFile).Returns(layer);

                Action<IFlowFMLayerInstanceCreator, ILayer> assertValid =
                    CommonAsserts.CreatedLayer(layer, creator => creator.CreateImportedFMNetFileLayer(netFile));

                yield return new TestCaseData(netFile,
                                              null,
                                              (Action<IFlowFMLayerInstanceCreator>)ConfigureMock,
                                              assertValid);
                yield return new TestCaseData(netFile,
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
                var netFile = new ImportedFMNetFile("does/not/matter.nc");

                yield return new TestCaseData(netFile, CommonAsserts.NoChildren());
                yield return new TestCaseData(null, CommonAsserts.NoChildren());
                yield return new TestCaseData(new object(), CommonAsserts.NoChildren());
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        protected override ImportedFMNetFileLayerSubProvider CreateDefault(IFlowFMLayerInstanceCreator instanceCreator) =>
            new ImportedFMNetFileLayerSubProvider(instanceCreator);
        
    }
}