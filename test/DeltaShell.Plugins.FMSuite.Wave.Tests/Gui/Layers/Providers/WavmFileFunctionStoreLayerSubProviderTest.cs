using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Layers.Providers
{
    [TestFixture]
    public class WavmFileFunctionStoreLayerSubProviderTest
    {
        [Test]
        public void Constructor_ReturnsExpectedResult()
        {
            // Setup
            var factory = Substitute.For<IWaveLayerFactory>();
            IEnumerable<WaveModel> GetModels() => Enumerable.Empty<WaveModel>();

            // Call
            var subProvider = new WavmFileFunctionStoreLayerSubProvider(factory, GetModels);

            // Assert
            Assert.That(subProvider, Is.InstanceOf<IWaveLayerSubProvider>());
        }

        [Test]
        public void Constructor_FactoryNull_ThrowsArgumentNullException()
        {
            // Setup
            IEnumerable<WaveModel> GetModels() => Enumerable.Empty<WaveModel>();

            // Call | Assert
            void Call() => new WavmFileFunctionStoreLayerSubProvider(null, GetModels);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception.ParamName, Is.EqualTo("factory"));
        }

        [Test]
        public void Constructor_GetModelsNull_ThrowsArgumentNullException()
        {
            // Setup
            var factory = Substitute.For<IWaveLayerFactory>();

            // Call | Assert
            void Call() => new WavmFileFunctionStoreLayerSubProvider(factory, null);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception.ParamName, Is.EqualTo("getWaveModelsFunc"));
        }

        [Test]
        public void CreateLayer_InvalidSourceAndParentData_ReturnsNull()
        {
            // Setup
            var factory = Substitute.For<IWaveLayerFactory>();
            IEnumerable<WaveModel> GetModels() => Enumerable.Empty<WaveModel>();

            var subProvider = new WavmFileFunctionStoreLayerSubProvider(factory, GetModels);

            // Call
            ILayer result = subProvider.CreateLayer(new object(), new object());

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GenerateChildLayerObjects_InvalidData_ReturnsEmptyEnumerable()
        {
            // Setup
            var factory = Substitute.For<IWaveLayerFactory>();
            IEnumerable<WaveModel> GetModels() => Enumerable.Empty<WaveModel>();

            var subProvider = new WavmFileFunctionStoreLayerSubProvider(factory, GetModels);

            // Call
            IEnumerable<object> result = subProvider.GenerateChildLayerObjects(new object());

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void CanCreateLayerFor_InvalidSourceAndParentData_ReturnsFalse()
        {
            // Setup
            var factory = Substitute.For<IWaveLayerFactory>();
            IEnumerable<WaveModel> GetModels() => Enumerable.Empty<WaveModel>();

            var subProvider = new WavmFileFunctionStoreLayerSubProvider(factory, GetModels);

            // Call
            bool result = subProvider.CanCreateLayerFor(new object(), new object());

            // Assert
            Assert.That(result, Is.False);
            
        }
    }
}