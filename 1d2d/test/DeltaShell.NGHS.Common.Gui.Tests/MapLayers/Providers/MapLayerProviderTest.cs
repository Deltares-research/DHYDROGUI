using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Gui;
using DeltaShell.NGHS.Common.Gui.MapLayers.Providers;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.NGHS.Common.Gui.Tests.MapLayers.Providers
{
    [TestFixture]
    public class MapLayerProviderTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Act
            var mapLayerProvider = new MapLayerProvider();

            // Assert
            Assert.That(mapLayerProvider, Is.InstanceOf<IMapLayerProvider>());
        }

        [Test]
        [TestCaseSource(nameof(GetSubProviders))]
        public void CreateLayer_NoValidSubProvider_ReturnsNull(IList<ILayerSubProvider> subProviders)
        {
            // Setup
            var sourceData = Substitute.For<object>();
            var parentData = Substitute.For<object>();

            ConfigureAsCannotCreateLayerFor(subProviders, sourceData, parentData);

            MapLayerProvider mapLayerProvider = GetMapLayerProviderWithSubProviders(subProviders);

            // Call
            ILayer result = mapLayerProvider.CreateLayer(sourceData, parentData);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        [TestCaseSource(nameof(GetSubProviders))]
        public void CreateLayer_ValidSubProvider_ReturnsCorrectLayer(IList<ILayerSubProvider> subProviders)
        {
            // Setup
            var sourceData = Substitute.For<object>();
            var parentData = Substitute.For<object>();

            ConfigureAsCannotCreateLayerFor(subProviders, sourceData, parentData);

            ILayerSubProvider correctProvider = CreateCorrectSubProviderForData(sourceData, parentData, out ILayer layer);
            subProviders.Add(correctProvider);

            MapLayerProvider mapLayerProvider = GetMapLayerProviderWithSubProviders(subProviders);

            // Call
            ILayer result = mapLayerProvider.CreateLayer(sourceData, parentData);

            // Assert
            Assert.That(result, Is.SameAs(layer));
            correctProvider.Received(1).CreateLayer(sourceData, parentData);
        }

        [Test]
        [TestCaseSource(nameof(GetSubProviders))]
        public void CanCreateLayerFor_NoValidSubProvider_ReturnsFalse(IList<ILayerSubProvider> subProviders)
        {
            // Arrange
            var sourceData = new object();
            var parentData = new object();

            ConfigureAsCannotCreateLayerFor(subProviders, sourceData, parentData);
            MapLayerProvider provider = GetMapLayerProviderWithSubProviders(subProviders);

            // Act
            bool result = provider.CanCreateLayerFor(sourceData, parentData);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void CanCreateLayerFor_OneValidSubProvider_ReturnsTrue()
        {
            // Arrange
            var layerSubProvider1 = Substitute.For<ILayerSubProvider>();
            layerSubProvider1.CanCreateLayerFor(Arg.Any<object>(), Arg.Any<object>())
                             .Returns(true);

            var layerSubProvider2 = Substitute.For<ILayerSubProvider>();
            layerSubProvider2.CanCreateLayerFor(Arg.Any<object>(), Arg.Any<object>())
                             .Returns(false);

            var mapLayerProvider = new MapLayerProvider();
            ILayerSubProvider[] layerSubProviders =
            {
                layerSubProvider1,
                layerSubProvider2
            };
            mapLayerProvider.RegisterSubProviders(layerSubProviders);

            // Act
            bool result = mapLayerProvider.CanCreateLayerFor(new object(), new object());

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void ChildLayerObjects_ReturnsAllChildLayerObjects()
        {
            // Arrange
            var data = new object();

            var layerSubProvider1 = Substitute.For<ILayerSubProvider>();
            var childLayerObject1 = new object();
            object[] childLayerObjects1 =
            {
                childLayerObject1
            };
            layerSubProvider1.GenerateChildLayerObjects(data).Returns(childLayerObjects1);

            var layerSubProvider2 = Substitute.For<ILayerSubProvider>();
            var childLayerObject2 = new object();
            object[] childLayerObjects2 =
            {
                childLayerObject2
            };
            layerSubProvider2.GenerateChildLayerObjects(data).Returns(childLayerObjects2);

            ILayerSubProvider[] layerSubProviders =
            {
                layerSubProvider1,
                layerSubProvider2
            };

            var mapLayerProvider = new MapLayerProvider();
            mapLayerProvider.RegisterSubProviders(layerSubProviders);

            // Act
            object[] mapChildLayerObjects = mapLayerProvider.ChildLayerObjects(data).ToArray();

            // Assert
            Assert.That(mapChildLayerObjects.Length, Is.EqualTo(2));
            Assert.Contains(childLayerObject1, mapChildLayerObjects);
            Assert.Contains(childLayerObject2, mapChildLayerObjects);
        }

        [Test]
        public void RegisterSubProviders_SubLayerProvidersNull_ThrowsArgumentNullException()
        {
            // Arrange
            var mapLayerProvider = new MapLayerProvider();

            // Act
            void Call() => mapLayerProvider.RegisterSubProviders(null);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            // Assert
            Assert.That(exception.ParamName, Is.EqualTo("layerSubProviders"));
        }

        private ILayerSubProvider CreateCorrectSubProviderForData(object sourceData, object parentData, out ILayer layer)
        {
            var correctProvider = Substitute.For<ILayerSubProvider>();
            correctProvider.CanCreateLayerFor(sourceData, parentData).Returns(true);

            layer = Substitute.For<ILayer>();
            correctProvider.CreateLayer(sourceData, parentData).Returns(layer);

            return correctProvider;
        }

        private static MapLayerProvider GetMapLayerProviderWithSubProviders(IList<ILayerSubProvider> subProviders)
        {
            var mapLayerProvider = new MapLayerProvider();
            mapLayerProvider.RegisterSubProviders(subProviders);

            return mapLayerProvider;
        }

        private static IEnumerable<TestCaseData> GetSubProviders()
        {
            var prov0 = Substitute.For<ILayerSubProvider>();
            var prov1 = Substitute.For<ILayerSubProvider>();
            var prov2 = Substitute.For<ILayerSubProvider>();
            var prov3 = Substitute.For<ILayerSubProvider>();

            yield return new TestCaseData(new List<ILayerSubProvider>());
            yield return new TestCaseData(new List<ILayerSubProvider> {prov0});
            yield return new TestCaseData(new List<ILayerSubProvider>
            {
                prov0,
                prov1
            });
            yield return new TestCaseData(new List<ILayerSubProvider>
            {
                prov0,
                prov1,
                prov2
            });
            yield return new TestCaseData(new List<ILayerSubProvider>
            {
                prov0,
                prov1,
                prov3
            });
        }

        private static void ConfigureAsCannotCreateLayerFor(IEnumerable<ILayerSubProvider> subProviders, object sourceData, object parentData)
        {
            foreach (ILayerSubProvider subProv in subProviders)
            {
                subProv.CanCreateLayerFor(sourceData, parentData).Returns(false);
            }
        }
    }
}