using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers;
using NSubstitute;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Layers
{
    [TestFixture]
    public class WaveMapLayerProviderTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var mapLayerProvider = new WaveMapLayerProvider();
            
            // Assert
            Assert.That(mapLayerProvider, Is.InstanceOf<WaveMapLayerProvider>());
        }

        [Test]
        public void RegisterSubProvider_ProviderNull_ThrowsArgumentNullException()
        {
            // Setup
            var mapLayerProvider = new WaveMapLayerProvider();

            // Call
            void Call() => mapLayerProvider.RegisterSubProvider(null);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            // Assert
            Assert.That(exception.ParamName, Is.EqualTo("provider"));
        }

        private static WaveMapLayerProvider GetMapLayerProviderWithSubProviders(IEnumerable<IWaveLayerSubProvider> subProviders)
        {
            var mapLayerProvider = new WaveMapLayerProvider();

            foreach (IWaveLayerSubProvider waveLayerSubProvider in subProviders)
            {
                mapLayerProvider.RegisterSubProvider(waveLayerSubProvider);
            }

            return mapLayerProvider;
        }

        private static IEnumerable<TestCaseData> GetSubProviders()
        {
            var prov0 = Substitute.For<IWaveLayerSubProvider>();
            var prov1 = Substitute.For<IWaveLayerSubProvider>();
            var prov2 = Substitute.For<IWaveLayerSubProvider>();
            var prov3 = Substitute.For<IWaveLayerSubProvider>();

            yield return new TestCaseData(new List<IWaveLayerSubProvider>());
            yield return new TestCaseData(new List<IWaveLayerSubProvider> { prov0 });
            yield return new TestCaseData(new List<IWaveLayerSubProvider>
            {
                prov0,
                prov1,
            });
            yield return new TestCaseData(new List<IWaveLayerSubProvider>
            {
                prov0,
                prov1,
                prov2,
            });
            yield return new TestCaseData(new List<IWaveLayerSubProvider>
            {
                prov0,
                prov1,
                prov3,
            });
        }

        private static void ConfigureAsCannotCreateLayerFor(IEnumerable<IWaveLayerSubProvider> subProviders, object sourceData, object parentData)
        {
            foreach (IWaveLayerSubProvider subProv in subProviders)
            {
                subProv.CanCreateLayerFor(sourceData, parentData).Returns(false);
            }
        }

        private IWaveLayerSubProvider CreateCorrectSubProviderForData(object sourceData, object parentData, out ILayer layer)
        {
            var correctProvider = Substitute.For<IWaveLayerSubProvider>();
            correctProvider.CanCreateLayerFor(sourceData, parentData).Returns(true);

            layer = Substitute.For<ILayer>();
            correctProvider.CreateLayer(sourceData, parentData).Returns(layer);

            return correctProvider;
        }

        [Test]
        [TestCaseSource(nameof(GetSubProviders))]
        public void CanCreateLayerFor_NoValidSubProvider_ReturnsFalse(IList<IWaveLayerSubProvider> subProviders)
        {
            // Setup
            var sourceData = new object();
            var parentData = new object();

            ConfigureAsCannotCreateLayerFor(subProviders, sourceData, parentData);
            WaveMapLayerProvider provider = GetMapLayerProviderWithSubProviders(subProviders);

            // Call
            bool result = provider.CanCreateLayerFor(sourceData, parentData);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        [TestCaseSource(nameof(GetSubProviders))]
        public void CanCreateLayerFor_ValidSubProvider_ReturnsTrue(IList<IWaveLayerSubProvider> subProviders)
        {
            // Setup
            var sourceData = Substitute.For<IWaveModel>();
            var parentData = Substitute.For<IWaveModel>();

            ConfigureAsCannotCreateLayerFor(subProviders, sourceData, parentData);
            IWaveLayerSubProvider correctProvider = CreateCorrectSubProviderForData(sourceData, parentData, out ILayer _);

            subProviders.Add(correctProvider);
            
            WaveMapLayerProvider mapLayerProvider = GetMapLayerProviderWithSubProviders(subProviders);
            
            // Call
            bool result = mapLayerProvider.CanCreateLayerFor(sourceData, parentData);

            // Assert
            Assert.That(result, Is.True);
            correctProvider.Received(1).CanCreateLayerFor(sourceData, parentData);
        }

        [Test]
        [TestCaseSource(nameof(GetSubProviders))]
        public void CreateLayer_NoValidSubProvider_ReturnsNull(IList<IWaveLayerSubProvider> subProviders)
        {
            // Setup
            var sourceData = Substitute.For<IWaveModel>();
            var parentData = Substitute.For<IWaveModel>();

            ConfigureAsCannotCreateLayerFor(subProviders, sourceData, parentData);

            WaveMapLayerProvider mapLayerProvider = GetMapLayerProviderWithSubProviders(subProviders);
            
            // Call
            ILayer result = mapLayerProvider.CreateLayer(sourceData, parentData);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        [TestCaseSource(nameof(GetSubProviders))]
        public void CreateLayer_ValidSubProvider_ReturnsCorrectLayer(IList<IWaveLayerSubProvider> subProviders)
        {
            // Setup
            var sourceData = Substitute.For<IWaveModel>();
            var parentData = Substitute.For<IWaveModel>();

            ConfigureAsCannotCreateLayerFor(subProviders, sourceData, parentData);

            IWaveLayerSubProvider correctProvider = CreateCorrectSubProviderForData(sourceData, parentData, out ILayer layer);
            subProviders.Add(correctProvider);
            
            WaveMapLayerProvider mapLayerProvider = GetMapLayerProviderWithSubProviders(subProviders);
            
            // Call
            ILayer result = mapLayerProvider.CreateLayer(sourceData, parentData);

            // Assert
            Assert.That(result, Is.SameAs(layer));
            correctProvider.Received(1).CreateLayer(sourceData, parentData);
        }

        private static void ConfigureChildDataItems(IEnumerable<IWaveLayerSubProvider> subProviders, 
                                                    object data,
                                                    out IEnumerable<object> childObjects)
        {
            var childObjectsGenerated = new List<object>();

            foreach (IWaveLayerSubProvider prov in subProviders)
            {
                IList<IWaveModel> objs = Enumerable.Range(0, 3).Select(_ => Substitute.For<IWaveModel>()).ToList();
                prov.GenerateChildLayerObjects(data).Returns(objs);
                childObjectsGenerated.AddRange(objs);
            }

            childObjects = childObjectsGenerated;
        }

        [Test]
        [TestCaseSource(nameof(GetSubProviders))]
        public void ChildLayerObjects_ReturnsCorrectLayer(IList<IWaveLayerSubProvider> subProviders)
        {
            // Setup
            var data = Substitute.For<IWaveModel>();

            ConfigureChildDataItems(subProviders, data, out IEnumerable<object> childObjects);
            WaveMapLayerProvider mapLayerProvider = GetMapLayerProviderWithSubProviders(subProviders);
            
            // Call
            IEnumerable<object> result = mapLayerProvider.ChildLayerObjects(data);

            // Assert
            Assert.That(result, Is.EquivalentTo(childObjects));

            foreach (IWaveLayerSubProvider subProvider in subProviders)
            {
                subProvider.Received(1).GenerateChildLayerObjects(data);
            }
        }
    }
}