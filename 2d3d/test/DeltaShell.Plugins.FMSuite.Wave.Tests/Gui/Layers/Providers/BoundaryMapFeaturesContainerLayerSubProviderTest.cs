using System;
using System.Collections.Generic;
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
    public class BoundaryMapFeaturesContainerLayerSubProviderTest : WaveLayerSubProviderTestFixture
    {
        private readonly IBoundaryMapFeaturesContainer container = Substitute.For<IBoundaryMapFeaturesContainer>();
        private readonly IWaveModel model = Substitute.For<IWaveModel>();

        protected override Func<IWaveLayerInstanceCreator, ILayerSubProvider> ConstructorCall { get; } =
            (factory) => new BoundaryMapFeaturesContainerLayerSubProvider(factory);

        [Test]
        public void GenerateChildLayerObjects_ReturnsEmptyEnumerable()
        {
            // Setup
            ILayerSubProvider subProvider = ConstructSubProvider();

            // Call
            IEnumerable<object> result = subProvider.GenerateChildLayerObjects(container);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        protected override object GetValidSourceData() => container;

        protected override object GetValidParentData() => model;

        protected override object GetInvalidSourceData() => container;

        protected override object GetInvalidParentData() => null;

        protected override ILayer ExpectedCall(IWaveLayerInstanceCreator instanceCreatorMock) =>
            instanceCreatorMock.CreateBoundaryLayer(container);
    }
}