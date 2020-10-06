using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.FMSuite.Common.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Containers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api;
using SharpMap.Api.Layers;
using SharpMap.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Layers
{
    [TestFixture]
    public class WaveLayerFactoryTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var factory = new WaveLayerFactory();

            // Assert
            Assert.That(factory, Is.InstanceOf<IWaveLayerFactory>());
        }

        [Test]
        public void CreateModelGroupLayer_ValidModel_ReturnsCorrectResults()
        {
            // Setup
            var waveModel = new WaveModel();
            var factory = new WaveLayerFactory();

            // Call
            ILayer layer = factory.CreateModelGroupLayer(waveModel);

            // Assert
            Assert.That(layer, Is.InstanceOf<ModelGroupLayer>(),
                        $"Expected the result to be an instance of {nameof(ModelGroupLayer)}");

            var modelGroupLayer = (ModelGroupLayer) layer;
            Assert.That(modelGroupLayer.Model, Is.SameAs(waveModel),
                        "Expected the model of the layer to the same as the provided model.");
            Assert.That(modelGroupLayer.Name, Is.EqualTo(waveModel.Name),
                        "Expected the name of the layer to the equal as the provided model name.");
        }

        [Test]
        public void CreateModelGroupLayer_WaveModelNull_ThrowsArgumentNullException()
        {
            // Setup
            var factory = new WaveLayerFactory();

            // Call
            void Call() => factory.CreateModelGroupLayer(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("waveModel"));
        }

        [Test]
        public void CreateWaveDomainDataLayer_ValidDomain_ReturnsCorrectResults()
        {
            // Setup
            var expectedDomainName = "DomainName";
            var domain = new WaveDomainData(expectedDomainName);
            var factory = new WaveLayerFactory();

            // Call
            ILayer layer = factory.CreateWaveDomainDataLayer(domain);

            // Assert
            Assert.That(layer, Is.InstanceOf<GroupLayer>(),
                        $"Expected the result to be an instance of {nameof(GroupLayer)}");
            Assert.That(layer.Name, Is.EqualTo($"Domain ({expectedDomainName})"),
                        "Expected the layer to have a different name.");
        }

        [Test]
        public void CreateWaveDomainDataLayer_DomainNull_ThrowsArgumentNullException()
        {
            // Setup
            var factory = new WaveLayerFactory();

            // Call
            void Call() => factory.CreateWaveDomainDataLayer(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("domain"));
        }

        [Test]
        public void CreateObstacleLayer_ValidWaveModel_ReturnsCorrectResults()
        {
            // Setup
            var model = new WaveModel();
            var factory = new WaveLayerFactory();

            // Call
            ILayer layer = factory.CreateObstacleLayer(model);

            // Assert
            Assert.That(layer, Is.InstanceOf<VectorLayer>(),
                        $"Expected the result to be an instance of {nameof(VectorLayer)}");
            Assert.That(layer.Name, Is.EqualTo("Obstacles"),
                        "Expected the layer to have a different name.");
        }

        [Test]
        public void CreateObstacleLayer_WaveModelNull_ThrowsArgumentNullException()
        {
            // Setup
            var factory = new WaveLayerFactory();

            // Call
            void Call() => factory.CreateObstacleLayer(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("waveModel"));
        }

        [Test]
        public void CreateObservationPointsLayer_ValidWaveModel_ReturnsCorrectResults()
        {
            // Setup
            var model = new WaveModel();
            var factory = new WaveLayerFactory();

            // Call
            ILayer layer = factory.CreateObservationPointsLayer(model);

            // Assert
            Assert.That(layer, Is.InstanceOf<VectorLayer>(),
                        $"Expected the result to be an instance of {nameof(VectorLayer)}");
            Assert.That(layer.Name, Is.EqualTo("Observation Points"),
                        "Expected the layer to have a different name.");
        }

        [Test]
        public void CreateObservationPointsLayer_WaveModelNull_ThrowsArgumentNullException()
        {
            // Setup
            var factory = new WaveLayerFactory();

            // Call
            void Call() => factory.CreateObservationPointsLayer(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("waveModel"));
        }

        [Test]
        public void CreateObservationCrossSectionsLayer_ValidWaveModel_ReturnsCorrectResults()
        {
            // Setup
            var model = new WaveModel();
            var factory = new WaveLayerFactory();

            // Call
            ILayer layer = factory.CreateObservationCrossSectionLayer(model);

            // Assert
            Assert.That(layer, Is.InstanceOf<VectorLayer>(),
                        $"Expected the result to be an instance of {nameof(VectorLayer)}");
            Assert.That(layer.Name, Is.EqualTo("Observation Cross-Sections"),
                        "Expected the layer to have a different name.");
        }

        [Test]
        public void CreateObservationCrosSectionsLayer_WaveModelNull_ThrowsArgumentNullException()
        {
            // Setup
            var factory = new WaveLayerFactory();

            // Call
            void Call() => factory.CreateObservationCrossSectionLayer(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("waveModel"));
        }

        [Test]
        [TestCase(true, "domainName", "domainName")]
        [TestCase(false, "domainName", "Output (domainName)")]
        public void CreateOutputLayer_ValidDomainName_ReturnsCorrectResults(bool overrideLayerName,
                                                                            string domainName,
                                                                            string expectedName)
        {
            // Setup
            var factory = new WaveLayerFactory();

            // Call
            ILayer layer = factory.CreateOutputLayer(domainName, overrideLayerName);

            // Assert
            Assert.That(layer, Is.InstanceOf<GroupLayer>());
            Assert.That(layer.Name, Is.EqualTo(expectedName));

            var groupLayer = (GroupLayer) layer;
            Assert.That(groupLayer.LayersReadOnly, Is.True);
        }

        [Test]
        public void CreateOutputLayer_DomainNameNull_ThrowsArgumentNullException()
        {
            // Setup
            var factory = new WaveLayerFactory();

            // Call
            void Call() => factory.CreateOutputLayer(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("domainName"));
        }

        [Test]
        public void CreateGridLayer_ValidCurvilinearGrid_ReturnsCorrectResults()
        {
            // Setup
            const string gridName = "gridName";

            IList<double> xCoordinates = new List<double> {0.0};
            IList<double> yCoordinates = new List<double> {0.0};

            var coordinateSystem = Substitute.For<ICoordinateSystem>();
            var grid = new CurvilinearGrid(1, 1,
                                           xCoordinates, yCoordinates,
                                           string.Empty) {Name = gridName};

            var factory = new WaveLayerFactory();

            // Call
            ILayer layer = factory.CreateGridLayer(grid, coordinateSystem);

            // Assert
            Assert.That(layer, Is.InstanceOf<CurvilinearGridLayer>(),
                        $"Expected the result to be an instance of {nameof(CurvilinearGridLayer)}");
            Assert.That(layer.Name, Is.EqualTo(gridName),
                        "Expected the layer to have a different name.");
        }

        [Test]
        public void CreateGridLayer_ValidIDiscreteGridPointCoverage_ReturnsCorrectResults()
        {
            // Setup
            const string gridName = "gridName";
            IList<double> xCoordinates = new List<double> {0.0};
            IList<double> yCoordinates = new List<double> {0.0};

            var grid = new DiscreteGridPointCoverage(1, 1, xCoordinates, yCoordinates) {Name = gridName};

            var coordinateSystem = Substitute.For<ICoordinateSystem>();

            var factory = new WaveLayerFactory();

            // Call
            ILayer layer = factory.CreateGridLayer(grid, coordinateSystem);

            // Assert
            Assert.That(layer, Is.InstanceOf<CurvilinearVertexCoverageLayer>(),
                        $"Expected the result to be an instance of {nameof(CurvilinearVertexCoverageLayer)}");
            Assert.That(layer.Name, Is.EqualTo(gridName),
                        "Expected the layer to have a different name.");
        }

        [Test]
        public void CreateGridLayer_DiscreteGridNull_ThrowsArgumentNullException()
        {
            // Setup
            var coordinateSystem = Substitute.For<ICoordinateSystem>();
            var factory = new WaveLayerFactory();

            // Call
            void Call() => factory.CreateGridLayer(null, coordinateSystem);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("discreteGrid"));
        }

        [Test]
        public void CreateBoundaryLayer_ValidParameters_ReturnsCorrectResults()
        {
            // Setup
            var featureProviderContainer = Substitute.For<IBoundaryMapFeaturesContainer>();
            var factory = new WaveLayerFactory();

            // Call
            ILayer layer = factory.CreateBoundaryLayer(featureProviderContainer);

            Assert.That(layer, Is.InstanceOf<GroupLayer>(),
                        $"Expected the result to be an instance of {nameof(GroupLayer)}");
            Assert.That(layer.Name, Is.EqualTo(WaveLayerNames.BoundaryLayerName),
                        "Expected the layer to have a different name.");

            var groupLayer = (GroupLayer) layer;
            Assert.That(groupLayer.Layers.Count, Is.EqualTo(4),
                        "Expected a different number of layers:");

            ILayer lineLayer = groupLayer.Layers.FirstOrDefault(x => x.Name == WaveLayerNames.BoundaryLineLayerName);
            Assert.That(lineLayer, Is.Not.Null,
                        $"Expected the layer with name '{WaveLayerNames.BoundaryLineLayerName}' to exist.");
            Assert.That(lineLayer, Is.InstanceOf(typeof(VectorLayer)),
                        $"Expected the layer with name '{WaveLayerNames.BoundaryLineLayerName}' to be of type {typeof(VectorLayer)}");
            AssertCorrectMapTreeBehaviourSubBoundaryLayer(lineLayer);

            ILayer startPointsLayer =
                groupLayer.Layers.FirstOrDefault(x => x.Name == WaveLayerNames.BoundaryStartPointsLayerName);
            Assert.That(startPointsLayer, Is.Not.Null,
                        $"Expected the layer with name '{WaveLayerNames.BoundaryStartPointsLayerName}' to exist.");
            AssertCorrectPointBoundaryLayer(startPointsLayer, featureProviderContainer.BoundaryStartPointMapFeatureProvider, WaveLayerNames.BoundaryStartPointsLayerName, Color.LightGreen);

            ILayer endPointsLayer =
                groupLayer.Layers.FirstOrDefault(x => x.Name == WaveLayerNames.BoundaryEndPointsLayerName);
            Assert.That(endPointsLayer, Is.Not.Null,
                        $"Expected the layer with name '{WaveLayerNames.BoundaryEndPointsLayerName}' to exist.");
            AssertCorrectPointBoundaryLayer(endPointsLayer, featureProviderContainer.BoundaryEndPointMapFeatureProvider, WaveLayerNames.BoundaryEndPointsLayerName, Color.LightCoral);

            ILayer supportPointsLayer =
                groupLayer.Layers.FirstOrDefault(x => x.Name == WaveLayerNames.BoundarySupportPointsLayerName);
            Assert.That(supportPointsLayer, Is.Not.Null,
                        $"Expected the layer with name '{WaveLayerNames.BoundarySupportPointsLayerName}' to exist.");
            AssertCorrectPointBoundaryLayer(supportPointsLayer, featureProviderContainer.SupportPointMapFeatureProvider, WaveLayerNames.BoundarySupportPointsLayerName, DeltaresColor.LightBlue);
        }

        [Test]
        public void CreateBoundaryLayer_FeaturesProviderContainerNull_ThrowsArgumentNullException()
        {
            // Setup
            var model = Substitute.For<IWaveModel>();
            var factory = new WaveLayerFactory();

            // Call
            void Call() => factory.CreateBoundaryLayer(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("featuresProviderContainer"));
        }

        [Test]
        public void CreateBoundaryLineLayer_ValidParameters_ExpectedResults()
        {
            // Setup
            var featureProvider = Substitute.For<IFeatureProvider>();
            var factory = new WaveLayerFactory();

            // Call
            ILayer layer = factory.CreateBoundaryLineLayer(featureProvider);

            // Assert
            Assert.That(layer, Is.InstanceOf(typeof(VectorLayer)),
                        $"Expected the layer with name '{WaveLayerNames.BoundaryLineLayerName}' to be of type {typeof(VectorLayer)}");
            AssertCorrectMapTreeBehaviourSubBoundaryLayer(layer);
        }

        [Test]
        public void CreateBoundaryLineLayer_FeatureProviderNull_ThrowsArgumentNullException()
        {
            var factory = new WaveLayerFactory();

            void Call() => factory.CreateBoundaryLineLayer(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("featureProvider"));
        }

        [Test]
        public void CreateBoundaryStartPointsLayer_ValidParameters_ExpectedResults()
        {
            // Setup
            var featureProvider = Substitute.For<IFeatureProvider>();
            var factory = new WaveLayerFactory();

            // Call
            ILayer layer = factory.CreateBoundaryStartPointLayer(featureProvider);

            // Assert
            AssertCorrectPointBoundaryLayer(layer, featureProvider, WaveLayerNames.BoundaryStartPointsLayerName, Color.LightGreen);
        }

        [Test]
        public void CreateBoundaryStartPointsLayer_FeatureProviderNull_ThrowsArgumentNullException()
        {
            var factory = new WaveLayerFactory();

            void Call() => factory.CreateBoundaryStartPointLayer(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("featureProvider"));
        }

        [Test]
        public void CreateBoundaryEndPointsLayer_ValidParameters_ExpectedResults()
        {
            // Setup
            var featureProvider = Substitute.For<IFeatureProvider>();
            var factory = new WaveLayerFactory();

            // Call
            ILayer layer = factory.CreateBoundaryEndPointLayer(featureProvider);

            // Assert
            AssertCorrectPointBoundaryLayer(layer, featureProvider, WaveLayerNames.BoundaryEndPointsLayerName, Color.LightCoral);
        }

        [Test]
        public void CreateBoundaryEndPointsLayer_FeatureProviderNull_ThrowsArgumentNullException()
        {
            var factory = new WaveLayerFactory();

            void Call() => factory.CreateBoundaryEndPointLayer(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("featureProvider"));
        }

        [Test]
        public void CreateSupportPointsLayer_ValidParameters_ExpectedResults()
        {
            // Setup
            var featureProvider = Substitute.For<IFeatureProvider>();
            var factory = new WaveLayerFactory();

            // Call
            ILayer supportPointsLayer = factory.CreateSupportPointsLayer(featureProvider);

            // Assert
            AssertCorrectPointBoundaryLayer(supportPointsLayer, featureProvider, WaveLayerNames.BoundarySupportPointsLayerName, DeltaresColor.LightBlue);
        }

        [Test]
        public void CreateSupportPointsLayer_FeatureProviderNull_ThrowsArgumentNullException()
        {
            var factory = new WaveLayerFactory();

            void Call() => factory.CreateSupportPointsLayer(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("featureProvider"));
        }

        [Test]
        public void CreateSelectedSupportPointLayer_ValidParameters_ExpectedResults()
        {
            // Setup
            var featureProvider = Substitute.For<IFeatureProvider>();
            var factory = new WaveLayerFactory();

            // Call
            ILayer layer = factory.CreateSelectedSupportPointLayer(featureProvider);

            // Assert
            AssertCorrectPointBoundaryLayer(layer, featureProvider, WaveLayerNames.SelectedSupportPointLayerName, Color.PaleVioletRed);
        }

        [Test]
        public void CreateSelectedSupportPointLayer_FeatureProviderNull_ThrowsArgumentNullException()
        {
            var factory = new WaveLayerFactory();

            void Call() => factory.CreateSelectedSupportPointLayer(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("featureProvider"));
        }

        [Test]
        public void CreateActiveSupportPointsLayer_ValidParameters_ExpectedResults()
        {
            // Setup
            var featureProvider = Substitute.For<IFeatureProvider>();
            var factory = new WaveLayerFactory();

            // Call
            ILayer layer = factory.CreateActiveSupportPointsLayer(featureProvider);

            // Assert
            AssertCorrectPointBoundaryLayer(layer, featureProvider, WaveLayerNames.ActiveSupportPointsLayerName, Color.Gold);
        }

        [Test]
        public void CreateActiveSupportPointsLayer_FeatureProviderNull_ThrowsArgumentNullException()
        {
            var factory = new WaveLayerFactory();

            void Call() => factory.CreateActiveSupportPointsLayer(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("featureProvider"));
        }

        [Test]
        public void CreateInactiveSupportPointsLayer_ValidParameters_ExpectedResults()
        {
            // Setup
            var featureProvider = Substitute.For<IFeatureProvider>();
            var factory = new WaveLayerFactory();

            // Call
            ILayer layer = factory.CreateInactiveSupportPointsLayer(featureProvider);

            // Assert
            AssertCorrectPointBoundaryLayer(layer, featureProvider, WaveLayerNames.InactiveSupportPointsLayerName, Color.LightGray);
        }

        [Test]
        public void CreateInactiveSupportPointsLayer_FeatureProviderNull_ThrowsArgumentNullException()
        {
            var factory = new WaveLayerFactory();

            void Call() => factory.CreateInactiveSupportPointsLayer(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("featureProvider"));
        }

        [Test]
        public void CreateWaveOutputDataLayer_OutputDataNull_ThrowsArgumentNullException()
        {
            var factory = new WaveLayerFactory();

            void Call() => factory.CreateWaveOutputDataLayer(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("outputData"));
        }

        [Test]
        public void CreateWaveOutputDataLayer_ValidParameters_ExpectedResults()
        {
            // Setup
            var outputData = Substitute.For<IWaveOutputData>();
            var factory = new WaveLayerFactory();

            // Call
            ILayer layer = factory.CreateWaveOutputDataLayer(outputData);

            // Assert
            Assert.That(layer, Is.InstanceOf<GroupLayer>());
            Assert.That(layer.Name, Is.EqualTo(WaveLayerNames.WaveOutputDataLayerName));
            Assert.That(((GroupLayer) layer).LayersReadOnly, Is.True);
        }


        private static void AssertCorrectPointBoundaryLayer(ILayer layer,
                                                            IFeatureProvider featureProvider,
                                                            string expectedLayerName,
                                                            Color expectedColor)
        {
            Assert.That(layer.Name, Is.EqualTo(expectedLayerName));
            Assert.That(layer, Is.InstanceOf(typeof(VectorLayer)),
                        $"Expected the layer with name '{expectedLayerName}' to be of type {typeof(VectorLayer)}");
            AssertCorrectMapTreeBehaviourSubBoundaryLayer(layer);

            Assert.That(layer.DataSource,
                        Is.SameAs(featureProvider));
            Assert.That(layer.Selectable, Is.False);

            var vectorLayer = (VectorLayer) layer;
            Assert.That(vectorLayer.Style.GeometryType, Is.EqualTo(typeof(IPoint)));

            var solidBrush = vectorLayer.Style.Fill as SolidBrush;
            Assert.That(solidBrush, Is.Not.Null);
            Assert.That(solidBrush.Color.Equals(expectedColor));
        }

        private static void AssertCorrectMapTreeBehaviourSubBoundaryLayer(ILayer layer)
        {
            Assert.That(layer.ReadOnly, Is.True);
            Assert.That(layer.ShowInTreeView, Is.False);
            Assert.That(layer.ShowInLegend, Is.False);
            Assert.That(layer.NameIsReadOnly, Is.True);
            Assert.That(layer.FeatureEditor, Is.Not.Null);
        }
    }
}