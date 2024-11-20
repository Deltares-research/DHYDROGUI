using System;
using System.Collections;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers;
using NetTopologySuite.Extensions.Coverages;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers.Providers
{
    [TestFixture]
    internal class LeveeBreachWidthCoverageLayerSubProviderTest: LayerSubProviderBaseFixture<
        LeveeBreachWidthCoverageLayerSubProvider,
        LeveeBreachWidthCoverageLayerSubProviderTest.CanCreateLayerForParams,
        LeveeBreachWidthCoverageLayerSubProviderTest.CreateLayerParams,
        LeveeBreachWidthCoverageLayerSubProviderTest.GenerateChildLayerObjectsParams
    >
    {
        internal static FeatureCoverage GetValidCoverage() =>
            new FeatureCoverage("dambreak breach width (dambreak_breach_width)");
        internal static FeatureCoverage GetInvalidCoverage() =>
            new FeatureCoverage("Some other name");

        public class CanCreateLayerForParams : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                yield return new TestCaseData(null, null, false);
                yield return new TestCaseData(new object(), null, false);

                FeatureCoverage invalidCoverage = GetInvalidCoverage();
                yield return new TestCaseData(invalidCoverage, null, false);
                yield return new TestCaseData(invalidCoverage, new object(), false);

                FeatureCoverage validCoverage = GetValidCoverage();
                yield return new TestCaseData(validCoverage, null, true);
                yield return new TestCaseData(validCoverage, new object(), true);
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public class CreateLayerParams : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                var layer = Substitute.For<ILayer>();

                void ConfigureMock(IFlowFMLayerInstanceCreator instanceCreator) =>
                    instanceCreator.CreateLeveeBreachWidthCoverageLayer(null)
                                   .ReturnsForAnyArgs(layer);

                FeatureCoverage validCoverage = GetValidCoverage();
                Action<IFlowFMLayerInstanceCreator, ILayer> assertValid =
                    CommonAsserts.CreatedLayer(layer, creator => creator.CreateLeveeBreachWidthCoverageLayer(validCoverage));

                yield return new TestCaseData(validCoverage,
                                              null,
                                              (Action<IFlowFMLayerInstanceCreator>)ConfigureMock,
                                              assertValid);
                yield return new TestCaseData(validCoverage,
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

                FeatureCoverage invalidCoverage = GetInvalidCoverage();
                yield return new TestCaseData(invalidCoverage,
                                              null,
                                              (Action<IFlowFMLayerInstanceCreator>)ConfigureMock,
                                              CommonAsserts.NoLayerCreated());
                yield return new TestCaseData(invalidCoverage,
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

                FeatureCoverage invalidCoverage = GetInvalidCoverage();
                yield return new TestCaseData(invalidCoverage, CommonAsserts.NoChildren());
                FeatureCoverage validCoverage = GetValidCoverage();
                yield return new TestCaseData(validCoverage, CommonAsserts.NoChildren());
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        protected override LeveeBreachWidthCoverageLayerSubProvider CreateDefault(IFlowFMLayerInstanceCreator instanceCreator) =>
            new LeveeBreachWidthCoverageLayerSubProvider(instanceCreator);
    }
}