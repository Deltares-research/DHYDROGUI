using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers;
using DeltaShell.Plugins.FMSuite.Wave.Layers;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Layers.Providers
{
    [TestFixture]
    public class WaveModelLayerSubProviderTest : WaveLayerSubProviderTestFixture
    {
        private readonly WaveModel waveModel = new WaveModel {OuterDomain = new WaveDomainData("Domain")};

        protected override Func<IWaveLayerFactory, ILayerSubProvider> ConstructorCall { get; } =
            factory => new WaveModelLayerSubProvider(factory);

        protected override object GetValidSourceData() => waveModel;

        protected override object GetValidParentData() => null;

        protected override object GetInvalidSourceData() => new object();

        protected override object GetInvalidParentData() => null;

        protected override ILayer ExpectedCall(IWaveLayerFactory FactoryMock) =>
            FactoryMock.CreateModelGroupLayer(waveModel);

        [Test]
        public void GenerateChildLayerObjects_NotModelAsData_ReturnsEmptyEnumerable()
        {
            // Setup
            var factory = Substitute.For<IWaveLayerFactory>();
            var subProvider = new WaveModelLayerSubProvider(factory);

            var obj = new object();

            // Call
            IEnumerable<object> result = subProvider.GenerateChildLayerObjects(obj);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GenerateChildLayerObjects_ModelAsData_ReturnsExpectedItems()
        {
            // Setup
            ILayerSubProvider subProvider = ConstructSubProvider();

            // Call
            IList<object> result = subProvider.GenerateChildLayerObjects(waveModel).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(9));

            Assert.That(result, Has.Member(waveModel.BoundaryConditions));
            Assert.That(result, Has.Member(waveModel.Boundaries));
            Assert.That(result, Has.Member(waveModel.Sp2Boundaries));
            Assert.That(result, Has.Member(waveModel.Obstacles));
            Assert.That(result, Has.Member(waveModel.ObservationPoints));
            Assert.That(result, Has.Member(waveModel.ObservationCrossSections));
            Assert.That(result, Has.Member(waveModel.OuterDomain));

            Assert.That(result.Count(x => x is WaveSnappedFeaturesGroupLayerData), Is.EqualTo(1));
            Assert.That(result.Count(x => x is BoundaryMapFeaturesContainer), Is.EqualTo(1));
        }
    }
}