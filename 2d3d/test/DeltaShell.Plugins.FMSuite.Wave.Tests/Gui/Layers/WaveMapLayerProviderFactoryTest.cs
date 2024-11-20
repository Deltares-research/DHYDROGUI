using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers.OutputData;
using GeoAPI.Extensions.CoordinateSystems;
using NetTopologySuite.Extensions.Coverages;
using NSubstitute;
using NUnit.Framework;
using SharpMap;
using SharpMap.Api.Layers;
using SharpMap.Layers;
using SharpMap.UI.Forms;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Layers
{
    [TestFixture]
    public class WaveMapLayerProviderFactoryTest
    {
        [Test]
        public void GetSubProviders_ReturnsExpectedResults()
        {
            // Call
            IList<ILayerSubProvider> result = WaveMapLayerProviderFactory.GetSubProviders(Enumerable.Empty<WaveModel>)?.ToList();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(12));

            Assert.That(result.Any(x => x is BoundaryMapFeaturesContainerLayerSubProvider),
                        $"Expected one {nameof(BoundaryMapFeaturesContainerLayerSubProvider)}");
            Assert.That(result.Any(x => x is DiscreteGridPointCoverageLayerSubProvider),
                        $"Expected one {nameof(DiscreteGridPointCoverageLayerSubProvider)}");
            Assert.That(result.Any(x => x is ObservationCrossSectionLayerSubProvider),
                        $"Expected one {nameof(ObservationCrossSectionLayerSubProvider)}");
            Assert.That(result.Any(x => x is ObservationPointLayerSubProvider),
                        $"Expected one {nameof(ObservationPointLayerSubProvider)}");
            Assert.That(result.Any(x => x is ObstacleLayerSubProvider),
                        $"Expected one {nameof(ObstacleLayerSubProvider)}");
            Assert.That(result.Any(x => x is WaveDomainDataLayerSubProvider),
                        $"Expected one {nameof(WaveDomainDataLayerSubProvider)}");
            Assert.That(result.Any(x => x is WaveModelLayerSubProvider),
                        $"Expected one {nameof(WaveModelLayerSubProvider)}");
            Assert.That(result.Any(x => x is WavmFileFunctionStoreLayerSubProvider),
                        $"Expected one {nameof(WavmFileFunctionStoreLayerSubProvider)}");
            Assert.That(result.Any(x => x is WaveOutputDataLayerSubProvider),
                        $"Expected one {nameof(WaveOutputDataLayerSubProvider)}");
            Assert.That(result.Any(x => x is WavmFileFunctionStoreGroupLayerSubProvider),
                        $"Expected one {nameof(WavmFileFunctionStoreGroupLayerSubProvider)}");
            Assert.That(result.Any(x => x is WavhFileFunctionStoreGroupLayerSubProvider),
                        $"Expected one {nameof(WavhFileFunctionStoreGroupLayerSubProvider)}");
            Assert.That(result.Any(x => x is WavhFileFunctionStoreLayerSubProvider),
                        $"Expected one {nameof(WavhFileFunctionStoreLayerSubProvider)}");
        }

        [Test]
        public void GetSubProviders_GetWaveModelsNull_ThrowsArgumentNullException()
        {
            void Call() => WaveMapLayerProviderFactory.GetSubProviders(null).ToList();

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("getWaveModelsFunc"));
        }

        [Test]
        public void ConstructMapLayerProvider_ReturnsExpectedResults()
        {
            // Call
            IMapLayerProvider result = WaveMapLayerProviderFactory.ConstructMapLayerProvider(Enumerable.Empty<WaveModel>);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void ConstructMapLayerProvider_GetWaveModelsNull_ThrowsArgumentNullException()
        {
            void Call() => WaveMapLayerProviderFactory.ConstructMapLayerProvider(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("getWaveModelsFunc"));
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
                WaveMapLayerProviderFactory.ConstructMapLayerProvider(() => new[]
                {
                    model
                });

            var layer = (IGroupLayer) MapLayerProviderHelper.CreateLayersRecursive(model, null, new[]
            {
                provider
            });

            layer.Layers.ForEach(l => l.Visible = true);

            var map = new Map
            {
                Layers = {layer},
                Size = new Size
                {
                    Width = 800,
                    Height = 800
                }
            };
            map.ZoomToExtents();

            var mapControl = new MapControl
            {
                Map = map,
                Dock = DockStyle.Fill
            };

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