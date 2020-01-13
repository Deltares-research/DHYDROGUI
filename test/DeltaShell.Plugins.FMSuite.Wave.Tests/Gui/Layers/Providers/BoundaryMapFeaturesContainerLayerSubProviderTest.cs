using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Layers.Providers
{
    [TestFixture]
    public class BoundaryMapFeaturesContainerLayerSubProviderTest : WaveLayerSubProviderTestFixture
    {
        private readonly BoundaryMapFeaturesContainer container = new BoundaryMapFeaturesContainer(Substitute.For<IBoundaryContainer>(),
                                                                                                   null);
        private readonly IWaveModel model = Substitute.For<IWaveModel>();

        protected override Func<IWaveLayerFactory, IWaveLayerSubProvider> ConstructorCall { get; } =
            (factory) => new BoundaryMapFeaturesContainerLayerSubProvider(factory);

        protected override object GetValidSourceData() => container;

        protected override object GetValidParentData() => model;

        protected override object GetInvalidSourceData() => container;

        protected override object GetInvalidParentData() => null;

        protected override ILayer ExpectedCall(IWaveLayerFactory FactoryMock) =>
            FactoryMock.CreateBoundaryLayer(container, model);

        [Test]
        public void GenerateChildLayerObjects_ReturnsEmptyEnumerable()
        {
            // Setup
            IWaveLayerSubProvider subProvider = ConstructSubProvider();

            // Call
            IEnumerable<object> result = subProvider.GenerateChildLayerObjects(container);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }
    }
}