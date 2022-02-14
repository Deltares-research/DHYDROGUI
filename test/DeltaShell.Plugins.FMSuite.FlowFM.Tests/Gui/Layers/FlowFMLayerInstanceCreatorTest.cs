using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput2D.SnappedFeatures;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using GeoAPI.Extensions.CoordinateSystems;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api;
using SharpMap.Api.Layers;
using SharpMap.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers
{
    [TestFixture]
    public class FlowFMLayerInstanceCreatorTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            var creator = new FlowFMLayerInstanceCreator();
            Assert.That(creator, Is.InstanceOf<IFlowFMLayerInstanceCreator>());
        }

        private static IEnumerable<TestCaseData> CreateLayerData()
        {
            TestCaseData ToCase(Func<IFlowFMLayerInstanceCreator, ILayer> func, string name) =>
                new TestCaseData(func, name) { TestName = name };

            ILayer Create1DGroupLayer(IFlowFMLayerInstanceCreator creator) => creator.Create1DGroupLayer();
            yield return ToCase(Create1DGroupLayer, FlowFMLayerNames.GroupLayer1DName);

            ILayer Create2DGroupLayer(IFlowFMLayerInstanceCreator creator) => creator.Create2DGroupLayer();
            yield return ToCase(Create2DGroupLayer, FlowFMLayerNames.GroupLayer2DName);

            ILayer CreateInputGroupLayer(IFlowFMLayerInstanceCreator creator) => creator.CreateInputGroupLayer();
            yield return ToCase(CreateInputGroupLayer, FlowFMLayerNames.InputGroupLayerName);

            ILayer CreateOutputGroupLayer(IFlowFMLayerInstanceCreator creator) => creator.CreateOutputGroupLayer();
            yield return ToCase(CreateOutputGroupLayer, FlowFMLayerNames.OutputGroupLayerName);

            ILayer CreateFrictionGroupLayer(IFlowFMLayerInstanceCreator creator) => creator.CreateFrictionGroupLayer();
            yield return ToCase(CreateFrictionGroupLayer, FlowFMLayerNames.Friction1DGroupLayerName);

            ILayer CreateInitialConditionsGroupLayer(IFlowFMLayerInstanceCreator creator) => creator.CreateInitialConditionsGroupLayer();
            yield return ToCase(CreateInitialConditionsGroupLayer, FlowFMLayerNames.InitialConditions1DGroupLayerName);

            ILayer CreateMapStore1DLayer(IFlowFMLayerInstanceCreator creator) => creator.CreateMapFileFunctionStore1DLayer();
            yield return ToCase(CreateMapStore1DLayer, FlowFMLayerNames.MapFile1DGroupLayerName);

            ILayer CreateMapStore2DLayer(IFlowFMLayerInstanceCreator creator) => creator.CreateMapFileFunctionStore2DLayer();
            yield return ToCase(CreateMapStore2DLayer, FlowFMLayerNames.MapFile2DGroupLayerName);

            ILayer CreateHisStoreLayer(IFlowFMLayerInstanceCreator creator) => creator.CreateHisFileFunctionStoreLayer();
            yield return ToCase(CreateHisStoreLayer, FlowFMLayerNames.HistoryFileGroupLayerName);

            ILayer CreateClassStoreLayer(IFlowFMLayerInstanceCreator creator) => creator.CreateClassMapFileFunctionStoreLayer();
            yield return ToCase(CreateClassStoreLayer, FlowFMLayerNames.ClassMapFileGroupLayerName);

            ILayer CreateFouStoreLayer(IFlowFMLayerInstanceCreator creator) => creator.CreateFouFileFunctionStoreLayer();
            yield return ToCase(CreateFouStoreLayer, FlowFMLayerNames.FouFileGroupLayerName);
            
            ILayer CreateEstimatedSnappedFeatureLayer(IFlowFMLayerInstanceCreator creator) => creator.CreateEstimatedSnappedFeatureGroupLayer();
            yield return ToCase(CreateEstimatedSnappedFeatureLayer, FlowFMLayerNames.EstimatedSnappedFeaturesLayerName);

        }

        [Test]
        [TestCaseSource(nameof(CreateLayerData))]
        public void CreateLayer_ReturnsGroupLayerWithName(Func<IFlowFMLayerInstanceCreator, ILayer> createLayer,
                                                          string expectedLayerName)
        {
            var creator = new FlowFMLayerInstanceCreator();

            ILayer layer = createLayer(creator);

            Assert.That(layer, Is.InstanceOf<IGroupLayer>());
            Assert.That(layer.Name, Is.EqualTo(expectedLayerName));
            Assert.That(layer.NameIsReadOnly, Is.True);
            Assert.That(((IGroupLayer)layer).LayersReadOnly, Is.True);
        }

        [Test]
        public void CreateModelGroupLayer_ExpectedResults()
        {
            const string expectedName = "🦄";
            var model = Substitute.For<IWaterFlowFMModel>();
            model.Name.Returns(expectedName);

            var creator = new FlowFMLayerInstanceCreator();

            ILayer layer = creator.CreateModelGroupLayer(model);

            Assert.That(layer, Is.InstanceOf<ModelGroupLayer>());
            var modelGroupLayer = (ModelGroupLayer)layer;
            Assert.That(modelGroupLayer.Name, Is.EqualTo(expectedName));
            Assert.That(modelGroupLayer.Model, Is.SameAs(model));
            Assert.That(modelGroupLayer.LayersReadOnly, Is.True);
            Assert.That(modelGroupLayer.NameIsReadOnly, Is.True);
        }

        [Test]
        public void CreateModelGroupLayer_ModelNull_ThrowsArgumentNullException()
        {
            var creator = new FlowFMLayerInstanceCreator();
            void CreateLayer() => creator.CreateModelGroupLayer(null);
            var exception = Assert.Throws<ArgumentNullException>(CreateLayer);
            Assert.That(exception.ParamName, Is.EqualTo("model"));
        }

        [Test]
        public void CreateBoundariesLayer_ExpectedResults()
        {
            var boundaries = new EventedList<Feature2D>(new[]
            {
                Substitute.ForPartsOf<Feature2D>(),
                Substitute.ForPartsOf<Feature2D>(),
                Substitute.ForPartsOf<Feature2D>(),
            });

            var model = Substitute.For<IWaterFlowFMModel>();
            model.Boundaries.Returns(boundaries);

            var creator = new FlowFMLayerInstanceCreator();

            ILayer layer = creator.CreateBoundariesLayer(model);
            Assert.That(layer.Name, Is.EqualTo(FlowFMLayerNames.BoundariesLayerName));

            Assert.That(layer, Is.InstanceOf<VectorLayer>());
            var vectorLayer = (VectorLayer)layer;
            Assert.That(vectorLayer.NameIsReadOnly, Is.True);

            IFeatureProvider dataSource = vectorLayer.DataSource;
            Assert.That(dataSource.Features, Is.EqualTo(boundaries));

            Assert.That(vectorLayer.FeatureEditor, Is.Not.Null);
        }

        [Test]
        public void CreateBoundariesLayer_ModelNull_ThrowsArgumentNullException()
        {
            var creator = new FlowFMLayerInstanceCreator();
            void CreateLayer() => creator.CreateBoundariesLayer(null);
            var exception = Assert.Throws<ArgumentNullException>(CreateLayer);
            Assert.That(exception.ParamName, Is.EqualTo("model"));
        }

        [Test]
        public void CreatePipesLayer_ExpectedResults()
        {
            var pipes = new EventedList<Feature2D>(new[]
            {
                Substitute.ForPartsOf<Feature2D>(),
                Substitute.ForPartsOf<Feature2D>(),
                Substitute.ForPartsOf<Feature2D>(),
            });

            var model = Substitute.For<IWaterFlowFMModel>();
            model.Pipes.Returns(pipes);

            var creator = new FlowFMLayerInstanceCreator();

            ILayer layer = creator.CreatePipesLayer(model);
            Assert.That(layer.Name, Is.EqualTo(FlowFMLayerNames.SourcesAndSinksLayerName));

            Assert.That(layer, Is.InstanceOf<VectorLayer>());
            var vectorLayer = (VectorLayer)layer;
            Assert.That(vectorLayer.NameIsReadOnly, Is.True);

            IFeatureProvider dataSource = vectorLayer.DataSource;
            Assert.That(dataSource.Features, Is.EqualTo(pipes));

            Assert.That(vectorLayer.FeatureEditor, Is.Not.Null);

            Assert.That(vectorLayer.CustomRenderers, Is.Not.Empty);
        }

        [Test]
        public void CreatePipesLayer_ModelNull_ThrowsArgumentNullException()
        {
            var creator = new FlowFMLayerInstanceCreator();
            void CreateLayer() => creator.CreatePipesLayer(null);
            var exception = Assert.Throws<ArgumentNullException>(CreateLayer);
            Assert.That(exception.ParamName, Is.EqualTo("model"));
        }

        [Test]
        public void CreateLinks1D2DLayer_ExpectedResults()
        {
            var links = new EventedList<ILink1D2D>(new[]
            {
                Substitute.For<ILink1D2D>(),
                Substitute.For<ILink1D2D>(),
                Substitute.For<ILink1D2D>(),
            });

            var model = Substitute.For<IWaterFlowFMModel>();
            model.Links.Returns(links);

            var creator = new FlowFMLayerInstanceCreator();

            ILayer layer = creator.CreateLinks1D2DLayer(model);
            Assert.That(layer.Name, Is.EqualTo(FlowFMLayerNames.Links1D2DLayerName));

            Assert.That(layer, Is.InstanceOf<VectorLayer>());
            var vectorLayer = (VectorLayer)layer;
            Assert.That(vectorLayer.NameIsReadOnly, Is.True);
            Assert.That(vectorLayer.Selectable, Is.True);

            IFeatureProvider dataSource = vectorLayer.DataSource;
            Assert.That(dataSource.Features, Is.EqualTo(links));

            Assert.That(vectorLayer.FeatureEditor, Is.Not.Null);
        }

        [Test]
        public void CreateLinksLayer_ModelNull_ThrowsArgumentNullException()
        {
            var creator = new FlowFMLayerInstanceCreator();
            void CreateLayer() => creator.CreateLinks1D2DLayer(null);
            var exception = Assert.Throws<ArgumentNullException>(CreateLayer);
            Assert.That(exception.ParamName, Is.EqualTo("model"));
        }

        [Test]
        public void CreateLinks1D2DLayer_Output_ExpectedResults()
        {
            var links = new List<ILink1D2D>(new[]
            {
                Substitute.For<ILink1D2D>(),
                Substitute.For<ILink1D2D>(),
                Substitute.For<ILink1D2D>(),
            });

            var creator = new FlowFMLayerInstanceCreator();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();

            ILayer layer = creator.CreateLinks1D2DLayer(links, coordinateSystem);
            Assert.That(layer.Name, Is.EqualTo(FlowFMLayerNames.Links1D2DLayerName));

            Assert.That(layer, Is.InstanceOf<VectorLayer>());
            var vectorLayer = (VectorLayer)layer;
            Assert.That(vectorLayer.NameIsReadOnly, Is.True);
            Assert.That(vectorLayer.Selectable, Is.True);

            IFeatureProvider dataSource = vectorLayer.DataSource;
            Assert.That(dataSource.Features, Is.EqualTo(links));
            Assert.That(dataSource.CoordinateSystem, Is.SameAs(coordinateSystem));

            Assert.That(vectorLayer.FeatureEditor, Is.Not.Null);
        }

        [Test]
        public void CreateLinksLayer_DataSourceNull_ThrowsArgumentNullException()
        {
            var coordinateSystem = Substitute.For<ICoordinateSystem>();
            var creator = new FlowFMLayerInstanceCreator();
            void CreateLayer() => creator.CreateLinks1D2DLayer(null, coordinateSystem);
            var exception = Assert.Throws<ArgumentNullException>(CreateLayer);
            Assert.That(exception.ParamName, Is.EqualTo("data"));
        }

        [Test]
        public void CreateBoundaryConditionSetsLayer_ExpectedResults()
        {
            var sets = new EventedList<BoundaryConditionSet>(new[]
            {
                new BoundaryConditionSet(),
                new BoundaryConditionSet(),
                new BoundaryConditionSet(),
            });

            var model = Substitute.For<IWaterFlowFMModel>();
            model.BoundaryConditionSets.Returns(sets);

            var creator = new FlowFMLayerInstanceCreator();

            ILayer layer = creator.CreateBoundaryConditionSetsLayer(model);
            Assert.That(layer.Name, Is.EqualTo(FlowFMLayerNames.BoundaryConditionsLayerName));

            Assert.That(layer, Is.InstanceOf<VectorLayer>());
            var vectorLayer = (VectorLayer)layer;
            Assert.That(vectorLayer.NameIsReadOnly, Is.True);
            Assert.That(vectorLayer.ShowInTreeView, Is.True);
            Assert.That(vectorLayer.Selectable, Is.False);
            Assert.That(vectorLayer.ShowInLegend, Is.False);

            IFeatureProvider dataSource = vectorLayer.DataSource;
            Assert.That(dataSource.Features, Is.EqualTo(sets));

            Assert.That(vectorLayer.FeatureEditor, Is.Not.Null);
        }

        [Test]
        public void CreateBoundaryConditionSetsLayer_ModelNull_ThrowsArgumentNullException()
        {
            var creator = new FlowFMLayerInstanceCreator();
            void CreateLayer() => creator.CreateBoundaryConditionSetsLayer(null);
            var exception = Assert.Throws<ArgumentNullException>(CreateLayer);
            Assert.That(exception.ParamName, Is.EqualTo("model"));
        }

        [Test]
        public void CreateBoundaryNodeDataLayer_ExpectedResults()
        {
            var nodeData = new EventedList<Model1DBoundaryNodeData>(new[]
            {
                new Model1DBoundaryNodeData(),
                new Model1DBoundaryNodeData(),
                new Model1DBoundaryNodeData(),
            });

            var model = Substitute.For<IWaterFlowFMModel>();
            model.BoundaryConditions1D.Returns(nodeData);

            var network = Substitute.For<IHydroNetwork, INotifyPropertyChanged>();
            model.Network.Returns(network);

            var creator = new FlowFMLayerInstanceCreator();

            ILayer layer = creator.CreateBoundaryNodeDataLayer(model);
            Assert.That(layer.Name, Is.EqualTo(FlowFMLayerNames.BoundaryData1DLayerName));

            Assert.That(layer, Is.InstanceOf<VectorLayer>());
            var vectorLayer = (VectorLayer)layer;
            Assert.That(vectorLayer.Visible, Is.False);
            Assert.That(vectorLayer.NameIsReadOnly, Is.True);
            Assert.That(vectorLayer.Selectable, Is.True);

            IFeatureProvider dataSource = vectorLayer.DataSource;
            Assert.That(dataSource.Features, Is.EqualTo(nodeData));

            Assert.That(vectorLayer.FeatureEditor, Is.Not.Null);
        }

        [Test]
        public void CreateBoundaryNodeDataLayer_ModelNull_ThrowsArgumentNullException()
        {
            var creator = new FlowFMLayerInstanceCreator();
            void CreateLayer() => creator.CreateBoundaryNodeDataLayer(null);
            var exception = Assert.Throws<ArgumentNullException>(CreateLayer);
            Assert.That(exception.ParamName, Is.EqualTo("model"));
        }

        [Test]
        public void CreateLateralDataLayer_ExpectedResults()
        {
            var lateralData = new EventedList<Model1DLateralSourceData>(new[]
            {
                new Model1DLateralSourceData(),
                new Model1DLateralSourceData(),
                new Model1DLateralSourceData(),
            });

            var model = Substitute.For<IWaterFlowFMModel>();
            model.LateralSourcesData.Returns(lateralData);

            var network = Substitute.For<IHydroNetwork, INotifyPropertyChanged>();
            model.Network.Returns(network);

            var creator = new FlowFMLayerInstanceCreator();

            ILayer layer = creator.CreateLateralDataLayer(model);
            Assert.That(layer.Name, Is.EqualTo(FlowFMLayerNames.LateralData1DLayerName));

            Assert.That(layer, Is.InstanceOf<VectorLayer>());
            var vectorLayer = (VectorLayer)layer;
            Assert.That(vectorLayer.Selectable, Is.True);
            Assert.That(vectorLayer.NameIsReadOnly, Is.True);

            IFeatureProvider dataSource = vectorLayer.DataSource;
            Assert.That(dataSource.Features, Is.EqualTo(lateralData));

            Assert.That(vectorLayer.FeatureEditor, Is.Not.Null);
        }

        [Test]
        public void CreateLateralNodeDataLayer_ModelNull_ThrowsArgumentNullException()
        {
            var creator = new FlowFMLayerInstanceCreator();
            void CreateLayer() => creator.CreateLateralDataLayer(null);
            var exception = Assert.Throws<ArgumentNullException>(CreateLayer);
            Assert.That(exception.ParamName, Is.EqualTo("model"));
        }

        [Test]
        public void CreateFunctionGroupingLayer_ExpectedResults()
        {
            const string groupName = "funky";
            IGrouping<string, IFunction> functionsGrouping =
                Enumerable.Range(0, 10).Select(_ => Substitute.For<IFunction>())
                          .GroupBy(x => groupName)
                          .First();

            foreach (IFunction function in functionsGrouping)
            {
                function.Name.Returns(groupName);
            }

            var creator = new FlowFMLayerInstanceCreator();

            ILayer layer = creator.CreateFunctionGroupingLayer(functionsGrouping);
            Assert.That(layer.Name, Is.EqualTo(groupName));

            Assert.That(layer, Is.InstanceOf<IGroupLayer>());
            Assert.That(layer.NameIsReadOnly, Is.True);
            Assert.That(((IGroupLayer)layer).LayersReadOnly, Is.True);
        }

        [Test]
        public void CreateImportedFMNetFileLayer_ExpectedResults()
        {
            var netFile = new ImportedFMNetFile("");
            var creator = new FlowFMLayerInstanceCreator();

            ILayer layer = creator.CreateImportedFMNetFileLayer(netFile);

            Assert.That(layer, Is.InstanceOf<UnstructuredGridLayer>());
            Assert.That(layer.NameIsReadOnly, Is.True);
            Assert.That(layer.FeatureEditor, Is.Not.Null);
        }

        [Test]
        public void CreateLeveeBreachWidthCoverageLayer_ExpectedResults()
        {
            const string name = "featureCoverage";
            var featureCoverage = new FeatureCoverage();
            featureCoverage.Name = name;
            featureCoverage.IsEditable = false;
            var creator = new FlowFMLayerInstanceCreator();

            ILayer layer = creator.CreateLeveeBreachWidthCoverageLayer(featureCoverage);
            Assert.That(layer.Name, Is.EqualTo(name));

            Assert.That(layer, Is.InstanceOf<FeatureCoverageLayer>());
            Assert.That(layer.NameIsReadOnly, Is.True);
            Assert.That(((FeatureCoverageLayer)layer).FeatureCoverage, Is.SameAs(featureCoverage));
        }

        [Test]
        public void CreateDefinitionsLayer_ExpectedResults()
        {
            const string name = "definitions";
            var definitions = new EventedList<ChannelFrictionDefinition>(
                Enumerable.Range(0, 3)
                          .Select(_ => new ChannelFrictionDefinition(Substitute.For<IChannel>())));
            var network = Substitute.For<IHydroNetwork, INotifyPropertyChanged>();

            var creator = new FlowFMLayerInstanceCreator();

            ILayer layer = creator.CreateDefinitionsLayer(name, definitions, network);
            Assert.That(layer.Name, Is.EqualTo(name));

            Assert.That(layer, Is.InstanceOf<VectorLayer>());
            Assert.That(layer.Visible, Is.False);
            Assert.That(layer.Selectable, Is.True);
            Assert.That(layer.NameIsReadOnly, Is.True);
            Assert.That(layer.CanBeRemovedByUser, Is.False);

            Assert.That(((VectorLayer)layer).DataSource.Features, Is.EqualTo(definitions));
        }

        [Test]
        public void CreateOutputSnappedFeatureGroupLayer_ExpectedResults()
        {
            var creator = new FlowFMLayerInstanceCreator();

            ILayer layer = creator.CreateOutputSnappedFeatureGroupLayer();

            Assert.That(layer, Is.InstanceOf<IGroupLayer>());
            Assert.That(layer.Name, Is.EqualTo(FlowFMLayerNames.OutputSnappedFeaturesLayerName));
            Assert.That(layer.Visible, Is.False);
            Assert.That(layer.NameIsReadOnly, Is.True);
        }

        [Test]
        public void CreateOutputSnappedFeatureLayer()
        {
            const string layerName = "layerName";
            const string featureDataPath = "someDataPath";
            var model = Substitute.For<IWaterFlowFMModel>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();
            model.CoordinateSystem.Returns(coordinateSystem);

            var creator = new FlowFMLayerInstanceCreator();

            ILayer layer = creator.CreateOutputSnappedFeatureLayer(
                layerName, featureDataPath, model);

            Assert.That(layer, Is.Not.Null);
            Assert.That(layer.DataSource, Is.Not.Null);
            Assert.That(layer.DataSource.CoordinateSystem, Is.SameAs(coordinateSystem));
        }

        private static IEnumerable<TestCaseData> EstimatedSnappedFeatureData()
        {
            TestCaseData ToCase(IWaterFlowFMModel model,
                                EstimatedSnappedFeatureType featureType,
                                string layerName) =>
                new TestCaseData(model, featureType, layerName)
                {
                    TestName = layerName,
                };

            IWaterFlowFMModel GetModel()
            {
                var model = Substitute.For<IWaterFlowFMModel>();
                var area = new HydroArea();
                model.Area.Returns(area);
                model.Boundaries.Returns(new EventedList<Feature2D>());
                model.SourcesAndSinks.Returns(new EventedList<SourceAndSink>());

                return model;
            }

            TestCaseData GetCase<T>(Action<IWaterFlowFMModel, IEnumerable<T>> setData,
                                    EstimatedSnappedFeatureType featureType,
                                    string layerName) where T : new()
            {
                IWaterFlowFMModel model = GetModel();
                IEnumerable<T> data = Enumerable.Range(0, 3).Select(_ => new T());
                setData(model, data);

                return ToCase(model, featureType, layerName);
            }

            yield return GetCase<GroupableFeature2DPoint>(
                    (model, data) => model.Area.ObservationPoints.AddRange(data),
                    EstimatedSnappedFeatureType.ObservationPoints, 
                    FlowFMLayerNames.EstimatedSnappedObservationPoints);

            yield return GetCase<ThinDam2D>(
                    (model, data) => model.Area.ThinDams.AddRange(data),
                    EstimatedSnappedFeatureType.ThinDams, 
                    FlowFMLayerNames.EstimatedSnappedThinDams);

            yield return GetCase<FixedWeir>(
                    (model, data) => model.Area.FixedWeirs.AddRange(data),
                    EstimatedSnappedFeatureType.FixedWeirs, 
                    FlowFMLayerNames.EstimatedSnappedFixedWeirs);

            TestCaseData LeveeBreach()
            {
                IWaterFlowFMModel model = GetModel();

                return ToCase(model, 
                              EstimatedSnappedFeatureType.LeveeBreaches, 
                              FlowFMLayerNames.EstimatedSnappedLeveeBreaches);
            }
            yield return LeveeBreach();

            yield return GetCase<GroupableFeature2DPolygon>(
                    (model, data) => model.Area.RoofAreas.AddRange(data), 
                    EstimatedSnappedFeatureType.RoofAreas, 
                    FlowFMLayerNames.EstimatedSnappedRoofAreas);
            
            yield return GetCase<GroupableFeature2DPolygon>(
                    (model, data) => model.Area.RoofAreas.AddRange(data),
                    EstimatedSnappedFeatureType.RoofAreas, 
                    FlowFMLayerNames.EstimatedSnappedRoofAreas);
            
            yield return GetCase<GroupablePointFeature>(
                    (model, data) => model.Area.DryPoints.AddRange(data),
                    EstimatedSnappedFeatureType.DryPoints, 
                    FlowFMLayerNames.EstimatedSnappedDryPoints);

            yield return GetCase<GroupableFeature2DPolygon>(
                    (model, data) => model.Area.DryAreas.AddRange(data),
                    EstimatedSnappedFeatureType.DryAreas, 
                    FlowFMLayerNames.EstimatedSnappedDryAreas);
            
            yield return GetCase<GroupableFeature2DPolygon>(
                    (model, data) => model.Area.Enclosures.AddRange(data),
                    EstimatedSnappedFeatureType.Enclosures, 
                    FlowFMLayerNames.EstimatedSnappedEnclosures);

            yield return GetCase<Pump2D>(
                    (model, data) => model.Area.Pumps.AddRange(data),
                    EstimatedSnappedFeatureType.Pumps, 
                    FlowFMLayerNames.EstimatedSnappedPumps);

            yield return GetCase<Weir2D>(
                    (model, data) => model.Area.Weirs.AddRange(data),
                    EstimatedSnappedFeatureType.Weirs, 
                    FlowFMLayerNames.EstimatedSnappedWeirs);

            yield return GetCase<Gate2D>(
                    (model, data) => model.Area.Gates.AddRange(data),
                    EstimatedSnappedFeatureType.Gates, 
                    FlowFMLayerNames.EstimatedSnappedGates);

            yield return GetCase<ObservationCrossSection2D>(
                    (model, data) => model.Area.ObservationCrossSections.AddRange(data),
                    EstimatedSnappedFeatureType.ObservationCrossSections, 
                    FlowFMLayerNames.EstimatedSnappedObservationCrossSections);

            yield return GetCase<Embankment>(
                (model, data) => model.Area.Embankments.AddRange(data),
                EstimatedSnappedFeatureType.Embankments,
                FlowFMLayerNames.EstimatedSnappedEmbankments);

            yield return GetCase<SourceAndSink>(
                (model, data) => model.SourcesAndSinks.AddRange(data),
                EstimatedSnappedFeatureType.SourcesAndSinks,
                FlowFMLayerNames.EstimatedSnappedSourcesAndSinks);

            yield return GetCase<Feature2D>(
                (model, data) => model.Boundaries.AddRange(data),
                EstimatedSnappedFeatureType.Boundaries,
                FlowFMLayerNames.EstimatedSnappedBoundaries);

            yield return GetCase<Feature2D>(
                (model, data) => model.Boundaries.AddRange(data),
                EstimatedSnappedFeatureType.BoundariesWaterLevel,
                FlowFMLayerNames.EstimatedSnappedBoundariesWaterLevel);

            yield return GetCase<Feature2D>(
                (model, data) => model.Boundaries.AddRange(data),
                EstimatedSnappedFeatureType.BoundariesVelocity,
                FlowFMLayerNames.EstimatedSnappedBoundariesVelocity);
        }

        [Test]
        [TestCaseSource(nameof(EstimatedSnappedFeatureData))]
        public void CreateEstimatedSnappedFeatureLayer_ExpectedResults(
            IWaterFlowFMModel model,
            EstimatedSnappedFeatureType featureType,
            string layerName)
        {
            var creator = new FlowFMLayerInstanceCreator();
            
            ILayer layer = creator.CreateEstimatedSnappedFeatureLayer(
                model, featureType);

            Assert.That(layer.Name, Is.EqualTo(layerName));
            Assert.That(layer, Is.InstanceOf<VectorLayer>());
            var vectorLayer = (VectorLayer)layer;

            Assert.That(vectorLayer.DataSource, Is.Not.Null);
            Assert.That(vectorLayer.DataSource, Is.InstanceOf<SnappedFeatureCollection>());
        }
    }
}