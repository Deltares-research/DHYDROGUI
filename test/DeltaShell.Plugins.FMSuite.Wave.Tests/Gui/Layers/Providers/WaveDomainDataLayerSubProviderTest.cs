using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Layers.Providers
{
    [TestFixture]
    public class WaveDomainDataLayerSubProviderTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var factory = Substitute.For<IWaveLayerFactory>();

            // Call
            var subProvider = new WaveDomainDataLayerSubProvider(factory);

            // Assert
            Assert.That(subProvider, Is.InstanceOf<IWaveLayerSubProvider>());
        }

        [Test]
        public void Constructor_FactoryNull_ThrowsArgumentNullException()
        {
            // Call | Assert
            void Call() => new WaveDomainDataLayerSubProvider(null);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception.ParamName, Is.EqualTo("factory"));
        }

        [Test]
        public void CanCreateLayerFor_ValidWaveDomainDataAndAnyParentData_ReturnsTrue()
        {
            // Setup
            var factory = Substitute.For<IWaveLayerFactory>();
            var subProvider = new WaveDomainDataLayerSubProvider(factory);

            var domainData = new WaveDomainData("Domain");

            // Call
            bool result = subProvider.CanCreateLayerFor(domainData, null);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CanCreateLayerFor_AnySourceDataNotWaveDomainData_ReturnsFalse()
        {
            // Setup
            var factory = Substitute.For<IWaveLayerFactory>();
            var subProvider = new WaveDomainDataLayerSubProvider(factory);

            var obj = new object();

            // Call
            bool result = subProvider.CanCreateLayerFor(obj, null);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void CreateLayer_ValidDomainDataAndAnyParentData_ReturnsExpectedLayer()
        {
            // Setup
            var factory = Substitute.For<IWaveLayerFactory>();
            var subProvider = new WaveDomainDataLayerSubProvider(factory);

            var domainData = new WaveDomainData("Domain");
            var layer = Substitute.For<ILayer>();

            factory.CreateWaveDomainDataLayer(domainData).Returns(layer);

            // Call
            ILayer result = subProvider.CreateLayer(domainData, null);

            // Assert
            Assert.That(result, Is.SameAs(layer));
            factory.Received(1).CreateWaveDomainDataLayer(domainData);
        }

        [Test]
        public void CreateLayer_AnyDataNotWaveDomainData_ReturnsNull()
        {
            // Setup
            var factory = Substitute.For<IWaveLayerFactory>();
            var subProvider = new WaveDomainDataLayerSubProvider(factory);

            var obj = new object();

            // Call
            ILayer result = subProvider.CreateLayer(obj, null);

            // Assert
            Assert.That(result, Is.Null);
            factory.DidNotReceiveWithAnyArgs().CreateWaveDomainDataLayer(null);
        }

        [Test]
        public void GenerateChildLayerObjects_AnyDataNotDomainData_ReturnsEmptyEnumerable()
        {
            // Setup
            var factory = Substitute.For<IWaveLayerFactory>();
            var subProvider = new WaveDomainDataLayerSubProvider(factory);

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
            var factory = Substitute.For<IWaveLayerFactory>();
            var subProvider = new WaveDomainDataLayerSubProvider(factory);

            var domainData = new WaveDomainData("Domain");

            // Call
            IEnumerable<object> result = subProvider.GenerateChildLayerObjects(domainData);

            // Assert
            var expectedResults = new List<object>
            {
                domainData.Grid,
                domainData.Bathymetry,
            };

            Assert.That(result, Is.EquivalentTo(expectedResults));
        }
    }
}