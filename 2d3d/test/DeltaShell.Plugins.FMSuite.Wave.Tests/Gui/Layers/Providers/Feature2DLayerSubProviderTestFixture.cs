using System.Collections.Generic;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Gui.Layers;
using NetTopologySuite.Extensions.Features;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Layers.Providers
{
    [TestFixture]
    public abstract class Feature2DLayerSubProviderTestFixture : WaveLayerSubProviderTestFixture
    {
        protected abstract IWaveModel Model { get; }
        protected abstract IEnumerable<Feature2D> RelevantFeature { get; }

        [Test]
        public void GenerateChildLayerObjects_ReturnsEmptyEnumerable()
        {
            // Setup
            ILayerSubProvider subProvider = ConstructSubProvider();

            // Call
            IEnumerable<object> result = subProvider.GenerateChildLayerObjects(RelevantFeature);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        protected override object GetValidSourceData() => RelevantFeature;

        protected override object GetValidParentData() => Model;

        protected override object GetInvalidSourceData() => Substitute.For<IEventedList<Feature2D>>();

        protected override object GetInvalidParentData() => Model;
    }
}