using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers.OutputData;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Layers.Providers.OutputData
{
    [TestFixture]
    public class WavmFileFunctionStoreLayerSubProviderTest
    {
        [Test]
        public void Constructor_ReturnsExpectedResult()
        {
            // Setup
            var instanceCreator = Substitute.For<IWaveLayerInstanceCreator>();
            IEnumerable<WaveModel> GetModels() => Enumerable.Empty<WaveModel>();

            // Call
            var subProvider = new WavmFileFunctionStoreLayerSubProvider(instanceCreator, GetModels);

            // Assert
            Assert.That(subProvider, Is.InstanceOf<ILayerSubProvider>());
        }

        [Test]
        public void Constructor_FactoryNull_ThrowsArgumentNullException()
        {
            // Setup
            IEnumerable<WaveModel> GetModels() => Enumerable.Empty<WaveModel>();

            // Call | Assert
            void Call() => new WavmFileFunctionStoreLayerSubProvider(null, GetModels);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception.ParamName, Is.EqualTo("instanceCreator"));
        }

        [Test]
        public void Constructor_GetModelsNull_ThrowsArgumentNullException()
        {
            // Setup
            var instanceCreator = Substitute.For<IWaveLayerInstanceCreator>();

            // Call | Assert
            void Call() => new WavmFileFunctionStoreLayerSubProvider(instanceCreator, null);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception.ParamName, Is.EqualTo("getWaveModelsFunc"));
        }

        [Test]
        public void CreateLayer_InvalidSourceAndParentData_ReturnsNull()
        {
            // Setup
            var instanceCreator = Substitute.For<IWaveLayerInstanceCreator>();
            IEnumerable<WaveModel> GetModels() => Enumerable.Empty<WaveModel>();

            var subProvider = new WavmFileFunctionStoreLayerSubProvider(instanceCreator, GetModels);

            // Call
            ILayer result = subProvider.CreateLayer(new object(), new object());

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GenerateChildLayerObjects_InvalidData_ReturnsEmptyEnumerable()
        {
            // Setup
            var instanceCreator = Substitute.For<IWaveLayerInstanceCreator>();
            IEnumerable<WaveModel> GetModels() => Enumerable.Empty<WaveModel>();

            var subProvider = new WavmFileFunctionStoreLayerSubProvider(instanceCreator, GetModels);

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
            var instanceCreator = Substitute.For<IWaveLayerInstanceCreator>();
            IEnumerable<WaveModel> GetModels() => Enumerable.Empty<WaveModel>();

            var subProvider = new WavmFileFunctionStoreLayerSubProvider(instanceCreator, GetModels);

            // Call
            bool result = subProvider.CanCreateLayerFor(new object(), new object());

            // Assert
            Assert.That(result, Is.False);
        }
    }
}