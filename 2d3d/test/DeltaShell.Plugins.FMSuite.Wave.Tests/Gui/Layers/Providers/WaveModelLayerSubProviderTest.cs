using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Containers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Layers.Providers
{
    [TestFixture]
    public class WaveModelLayerSubProviderTest : WaveLayerSubProviderTestFixture
    {
        private readonly WaveModel waveModel = new WaveModel {OuterDomain = new WaveDomainData("Domain")};

        protected override Func<IWaveLayerInstanceCreator, ILayerSubProvider> ConstructorCall { get; } =
            factory => new WaveModelLayerSubProvider(factory);

        [Test]
        public void GenerateChildLayerObjects_NotModelAsData_ReturnsEmptyEnumerable()
        {
            // Setup
            var instanceCreator = Substitute.For<IWaveLayerInstanceCreator>();
            var subProvider = new WaveModelLayerSubProvider(instanceCreator);

            var obj = new object();

            // Call
            IEnumerable<object> result = subProvider.GenerateChildLayerObjects(obj);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GenerateChildLayerObjects_DisconnectedModelAsData_ReturnsExpectedItems()
        {
            // Setup
            ILayerSubProvider subProvider = ConstructSubProvider();

            // Call
            IList<object> result = subProvider.GenerateChildLayerObjects(waveModel).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(5));

            Assert.That(result, Has.Member(waveModel.FeatureContainer.Obstacles));
            Assert.That(result, Has.Member(waveModel.FeatureContainer.ObservationPoints));
            Assert.That(result, Has.Member(waveModel.FeatureContainer.ObservationCrossSections));
            Assert.That(result, Has.Member(waveModel.OuterDomain));

            Assert.That(result.Count(x => x is BoundaryMapFeaturesContainer), Is.EqualTo(1));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GenerateChildLayerObjects_ConnectedModelAsData_ReturnsExpectedItems()
        {
            // Setup

            using (var model = new WaveModel { OuterDomain = new WaveDomainData("Domain") })
            using (var tempDir = new TemporaryDirectory())
            {
                model.WaveOutputData.ConnectTo(tempDir.Path, true);
                ILayerSubProvider subProvider = ConstructSubProvider();

                // Call
                IList<object> result = subProvider.GenerateChildLayerObjects(model).ToList();

                // Assert
                Assert.That(result.Count, Is.EqualTo(6));

                Assert.That(result, Has.Member(model.FeatureContainer.Obstacles));
                Assert.That(result, Has.Member(model.FeatureContainer.ObservationPoints));
                Assert.That(result, Has.Member(model.FeatureContainer.ObservationCrossSections));
                Assert.That(result, Has.Member(model.OuterDomain));
                Assert.That(result, Has.Member(model.WaveOutputData));
                Assert.That(result.Count(x => x is BoundaryMapFeaturesContainer), Is.EqualTo(1));
            }
        }

        protected override object GetValidSourceData() => waveModel;

        protected override object GetValidParentData() => null;

        protected override object GetInvalidSourceData() => new object();

        protected override object GetInvalidParentData() => null;

        protected override ILayer ExpectedCall(IWaveLayerInstanceCreator instanceCreatorMock) =>
            instanceCreatorMock.CreateModelGroupLayer(waveModel);

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            waveModel.Dispose();
        }
    }
}