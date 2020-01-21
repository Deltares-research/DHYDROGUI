using System;
using System.Collections.Generic;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers;
using DeltaShell.Plugins.FMSuite.Wave.Layers;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Layers.Providers
{
    [TestFixture]
    public class WaveSnappedFeaturesGroupLayerDataLayerSubProviderTest : WaveLayerSubProviderTestFixture
    {
        private readonly WaveSnappedFeaturesGroupLayerData snappedFeaturesData = 
            new WaveSnappedFeaturesGroupLayerData(new WaveModel());

        protected override Func<IWaveLayerFactory, ILayerSubProvider> ConstructorCall { get; } =
            (factory) => new WaveSnappedFeaturesGroupLayerDataLayerSubProvider(factory);

        protected override object GetValidSourceData() =>
            snappedFeaturesData;

        protected override object GetValidParentData() => null;

        protected override object GetInvalidSourceData() => new object();

        protected override object GetInvalidParentData() => null;

        protected override ILayer ExpectedCall(IWaveLayerFactory FactoryMock) =>
            FactoryMock.CreateSnappedFeaturesLayer(snappedFeaturesData);

        [Test]
        public void GenerateChildLayerObjects_ReturnsEmptyEnumerable()
        {
            // Setup
            ILayerSubProvider subProvider = ConstructSubProvider();

            // Call
            IEnumerable<object> result = subProvider.GenerateChildLayerObjects(GetValidSourceData());

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }
    }
}