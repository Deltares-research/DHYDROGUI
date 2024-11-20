using System;
using System.Collections.Generic;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Layers.Providers
{
    [TestFixture]
    public class WaveDomainDataLayerSubProviderTest : WaveLayerSubProviderTestFixture
    {
        private readonly WaveDomainData domainData = new WaveDomainData("Domain");

        protected override Func<IWaveLayerInstanceCreator, ILayerSubProvider> ConstructorCall { get; } =
            (factory) => new WaveDomainDataLayerSubProvider(factory);

        [Test]
        public void GenerateChildLayerObjects_AnyDataNotDomainData_ReturnsEmptyEnumerable()
        {
            // Setup
            var instanceCreator = Substitute.For<IWaveLayerInstanceCreator>();
            var subProvider = new WaveDomainDataLayerSubProvider(instanceCreator);

            var obj = new object();

            // Call
            IEnumerable<object> result = subProvider.GenerateChildLayerObjects(obj);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GenerateChildLayerObjects_DomainData_ReturnsExpectedElements()
        {
            // Setup
            var instanceCreator = Substitute.For<IWaveLayerInstanceCreator>();
            var subProvider = new WaveDomainDataLayerSubProvider(instanceCreator);

            var domainData = new WaveDomainData("Domain");

            // Call
            IEnumerable<object> result = subProvider.GenerateChildLayerObjects(domainData);

            // Assert
            var expectedResults = new List<object>
            {
                domainData.Grid,
                domainData.Bathymetry
            };

            Assert.That(result, Is.EquivalentTo(expectedResults));
        }

        protected override object GetValidSourceData() => domainData;

        protected override object GetValidParentData() => null;

        protected override object GetInvalidSourceData() => new object();

        protected override object GetInvalidParentData() => null;

        protected override ILayer ExpectedCall(IWaveLayerInstanceCreator instanceCreatorMock) =>
            instanceCreatorMock.CreateWaveDomainDataLayer(domainData);
    }
}