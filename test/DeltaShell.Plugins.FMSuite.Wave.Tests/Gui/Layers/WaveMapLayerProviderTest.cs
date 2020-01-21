using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.FMSuite.Wave.Gui;
using GeoAPI.Extensions.CoordinateSystems;
using NetTopologySuite.Extensions.Coverages;
using NSubstitute;
using SharpMap;
using SharpMap.Api.Layers;
using SharpMap.Layers;
using SharpMap.UI.Forms;

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
            void Call() => mapLayerProvider.RegisterSubProviders(null);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            // Assert
            Assert.That(exception.ParamName, Is.EqualTo("providers"));
        }

        private static WaveMapLayerProvider GetMapLayerProviderWithSubProviders(IList<ILayerSubProvider> subProviders)
        {
            var mapLayerProvider = new WaveMapLayerProvider();
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
            yield return new TestCaseData(new List<ILayerSubProvider> { prov0 });
            yield return new TestCaseData(new List<ILayerSubProvider>
            {
                prov0,
                prov1,
            });
            yield return new TestCaseData(new List<ILayerSubProvider>
            {
                prov0,
                prov1,
                prov2,
            });
            yield return new TestCaseData(new List<ILayerSubProvider>
            {
                prov0,
                prov1,
                prov3,
            });
        }

        private static void ConfigureAsCannotCreateLayerFor(IEnumerable<ILayerSubProvider> subProviders, object sourceData, object parentData)
        {
            foreach (ILayerSubProvider subProv in subProviders)
            {
                subProv.CanCreateLayerFor(sourceData, parentData).Returns(false);
            }
        }

        private ILayerSubProvider CreateCorrectSubProviderForData(object sourceData, object parentData, out ILayer layer)
        {
            var correctProvider = Substitute.For<ILayerSubProvider>();
            correctProvider.CanCreateLayerFor(sourceData, parentData).Returns(true);

            layer = Substitute.For<ILayer>();
            correctProvider.CreateLayer(sourceData, parentData).Returns(layer);

            return correctProvider;
        }

        [Test]
        [TestCaseSource(nameof(GetSubProviders))]
        public void CanCreateLayerFor_NoValidSubProvider_ReturnsFalse(IList<ILayerSubProvider> subProviders)
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
        public void CanCreateLayerFor_ValidSubProvider_ReturnsTrue(IList<ILayerSubProvider> subProviders)
        {
            // Setup
            var sourceData = Substitute.For<IWaveModel>();
            var parentData = Substitute.For<IWaveModel>();

            ConfigureAsCannotCreateLayerFor(subProviders, sourceData, parentData);
            ILayerSubProvider correctProvider = CreateCorrectSubProviderForData(sourceData, parentData, out ILayer _);

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
        public void CreateLayer_NoValidSubProvider_ReturnsNull(IList<ILayerSubProvider> subProviders)
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
        public void CreateLayer_ValidSubProvider_ReturnsCorrectLayer(IList<ILayerSubProvider> subProviders)
        {
            // Setup
            var sourceData = Substitute.For<IWaveModel>();
            var parentData = Substitute.For<IWaveModel>();

            ConfigureAsCannotCreateLayerFor(subProviders, sourceData, parentData);

            ILayerSubProvider correctProvider = CreateCorrectSubProviderForData(sourceData, parentData, out ILayer layer);
            subProviders.Add(correctProvider);
            
            WaveMapLayerProvider mapLayerProvider = GetMapLayerProviderWithSubProviders(subProviders);
            
            // Call
            ILayer result = mapLayerProvider.CreateLayer(sourceData, parentData);

            // Assert
            Assert.That(result, Is.SameAs(layer));
            correctProvider.Received(1).CreateLayer(sourceData, parentData);
        }

        private static void ConfigureChildDataItems(IEnumerable<ILayerSubProvider> subProviders, 
                                                    object data,
                                                    out IEnumerable<object> childObjects)
        {
            var childObjectsGenerated = new List<object>();

            foreach (ILayerSubProvider prov in subProviders)
            {
                IList<IWaveModel> objs = Enumerable.Range(0, 3).Select(_ => Substitute.For<IWaveModel>()).ToList();
                prov.GenerateChildLayerObjects(data).Returns(objs);
                childObjectsGenerated.AddRange(objs);
            }

            childObjects = childObjectsGenerated;
        }

        [Test]
        [TestCaseSource(nameof(GetSubProviders))]
        public void ChildLayerObjects_ReturnsCorrectLayer(IList<ILayerSubProvider> subProviders)
        {
            // Setup
            var data = Substitute.For<IWaveModel>();

            ConfigureChildDataItems(subProviders, data, out IEnumerable<object> childObjects);
            WaveMapLayerProvider mapLayerProvider = GetMapLayerProviderWithSubProviders(subProviders);
            
            // Call
            IEnumerable<object> result = mapLayerProvider.ChildLayerObjects(data);

            // Assert
            Assert.That(result, Is.EquivalentTo(childObjects));

            foreach (ILayerSubProvider subProvider in subProviders)
            {
                subProvider.Received(1).GenerateChildLayerObjects(data);
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowLayersForWaveModel()
        {
            string mdwPath = TestHelper.GetTestFilePath(@"coordinateBasedBoundary\obw.mdw");
            var model = new WaveModel(mdwPath);
            ShowModelLayers(model);
        }

        private static void ShowModelLayers(WaveModel model)
        {
            IMapLayerProvider provider = 
                WaveMapLayerProviderFactory.ConstructMapLayerProvider(() => new[] {model});

            var layer = (IGroupLayer)MapLayerProviderHelper.CreateLayersRecursive(model, null, new[] { provider });

            layer.Layers.ForEach(l => l.Visible = true);

            var map = new Map { Layers = { layer }, Size = new Size { Width = 800, Height = 800 } };
            map.ZoomToExtents();

            var mapControl = new MapControl { Map = map, Dock = DockStyle.Fill };

            WindowsFormsTestHelper.ShowModal(mapControl);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void CreateLayer_WithDiscreteGridPointCoverage_ThenCurvilinearVertexCoverageLayerWithExpectedValuesIsReturned(bool isEditable)
        {
            // Setup
            var waveModel = Substitute.For<IWaveModel>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();
            waveModel.CoordinateSystem = coordinateSystem;

            IMapLayerProvider mapLayerProvider = WaveMapLayerProviderFactory.ConstructMapLayerProvider(Enumerable.Empty<WaveModel>);

            var outputCoverage = new DiscreteGridPointCoverage
            {
                Name = "Name",
                IsEditable = isEditable
            };

            // Call
            var layer = (CurvilinearVertexCoverageLayer) mapLayerProvider.CreateLayer(outputCoverage, waveModel);

            // Assert
            Assert.AreEqual("Name", layer.Name, "The name of the coverage is not correctly set in the layer");
            Assert.AreSame(outputCoverage, layer.Coverage, "The coverage is not correctly set in the layer");
            Assert.IsFalse(layer.Visible, "The visibility of the layer should be false");
            Assert.IsFalse(layer.OptimizeRendering, "The optimize rendering setting of the layer should be false");
            Assert.AreEqual(!isEditable, layer.ReadOnly, "The read only setting is not correctly set in the layer");

            var dataSource = (WaveGridBasedDataSource) layer.DataSource;
            Assert.AreSame(waveModel.CoordinateSystem, dataSource.CoordinateSystem, "The coordinate system in the data source of the layer is not the same as the coordinate system of the coverage");
        }
    }
}