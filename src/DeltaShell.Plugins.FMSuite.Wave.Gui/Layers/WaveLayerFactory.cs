using System.Collections.Generic;
using System.Drawing;
using DeltaShell.NGHS.Common;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.FMSuite.Common.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Layers;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Grids;
using SharpMap.Api.Layers;
using SharpMap.Data.Providers;
using SharpMap.Editors.Interactors;
using SharpMap.Layers;
using SharpMap.Styles;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers
{
    /// <summary>
    /// <see cref="WaveLayerFactory"/> provides the methods to construct the
    /// different layers of the wave model.
    /// </summary>
    public class WaveLayerFactory : IWaveLayerFactory
    {
        /// <summary> The wave model name. </summary>
        private static readonly string waveModelName = typeof(WaveModel).Name;

        public ILayer CreateModelGroupLayer(WaveModel waveModel)
        {
            Ensure.NotNull(waveModel, nameof(waveModel));

            return new ModelGroupLayer
            {
                Name = waveModel.Name,
                Model = waveModel,
            };
        }

        public ILayer CreateWaveDomainDataLayer(WaveDomainData domain)
        {
            Ensure.NotNull(domain, nameof(domain));
            return new GroupLayer(WaveLayerNames.GetDomainLayerName(domain.Name));
        }

        public ILayer CreateObstacleLayer(IWaveModel waveModel)
        {
            Ensure.NotNull(waveModel, nameof(waveModel));

            return new VectorLayer(WaveLayerNames.ObstacleLayerName)
            {
                DataSource = new Feature2DCollection().Init(waveModel.Obstacles,
                                                            "Obstacle", 
                                                            waveModelName,
                                                            waveModel.CoordinateSystem),
                FeatureEditor = new Feature2DEditor(waveModel),
                Style = new VectorStyle
                {
                    Line = new Pen(Color.Red, 3f),
                    GeometryType = typeof(ILineString)
                },
                NameIsReadOnly = true
            };
        }

        public ILayer CreateObservationPointsLayer(IWaveModel waveModel)
        {
            Ensure.NotNull(waveModel, nameof(waveModel));

            return new VectorLayer(WaveLayerNames.ObservationPointLayerName)
            {
                NameIsReadOnly = true,
                FeatureEditor = new Feature2DEditor(waveModel),
                Style = new VectorStyle
                {
                    GeometryType = typeof(IPoint),
                    Symbol = WaveLayerIcons.ObservationPoint
                },
                DataSource = new Feature2DCollection().Init(waveModel.ObservationPoints, 
                                                            "ObservationPoints",
                                                            waveModelName,
                                                            waveModel.CoordinateSystem),
            };
        }

        public ILayer CreateObservationCrossSectionLayer(IWaveModel waveModel)
        {
            Ensure.NotNull(waveModel, nameof(waveModel));

            return new VectorLayer(WaveLayerNames.ObservationCrossSectionLayerName)
            {
                DataSource = new Feature2DCollection().Init(waveModel.ObservationCrossSections, 
                                                            "CrS",
                                                            waveModelName,
                                                            waveModel.CoordinateSystem),
                FeatureEditor = new Feature2DEditor(waveModel),
                Style = new VectorStyle
                {
                    Line = new Pen(Color.LightSteelBlue, 3f),
                    GeometryType = typeof(ILineString)
                },
                NameIsReadOnly = true
            };
        }

        public ILayer CreateSnappedFeaturesLayer(WaveSnappedFeaturesGroupLayerData snappedFeatures)
        {
            Ensure.NotNull(snappedFeatures, nameof(snappedFeatures));

            var groupLayer = new GroupLayer(WaveLayerNames.GridSnappedFeaturesLayerName);
            foreach (FeatureCollection snappedFeaturesData in snappedFeatures.ChildData)
            {
                var vectorLayer = new VectorLayer("Boundaries")
                {
                    DataSource = snappedFeaturesData,
                    NameIsReadOnly = true,
                    Selectable = false,
                    Style = new VectorStyle
                    {
                        Fill = Brushes.Gray,
                        GeometryType = typeof(IPoint)
                    }
                };

                groupLayer.Layers.Add(vectorLayer);
            }

            return groupLayer;
        }

        public ILayer CreateOutputLayer(string domainName, bool overrideLayerName = false)
        {
            Ensure.NotNull(domainName, nameof(domainName));

            string layerName = overrideLayerName ? domainName 
                                   : WaveLayerNames.GetOutputLayerName(domainName);

            return new GroupLayer(layerName)
            {
                LayersReadOnly = true,
            };
        }

        public ILayer CreateGridLayer(IDiscreteGridPointCoverage discreteGrid,
                                             ICoordinateSystem coordinateSystem)
        {
            Ensure.NotNull(discreteGrid, nameof(discreteGrid));

            return discreteGrid is CurvilinearGrid 
                       ? CreateCurvilinearGridLayer(discreteGrid, coordinateSystem) 
                       : CreateCurvilinearVertexCoverageLayer(discreteGrid, coordinateSystem);
        }

        private static ILayer CreateCurvilinearGridLayer(IDiscreteGridPointCoverage discreteGrid,
                                                         ICoordinateSystem coordinateSystem)
        {
            return new CurvilinearGridLayer
            {
                Name = discreteGrid.Name,
                CurviLinearGrid = discreteGrid,
                OptimizeRendering = discreteGrid.X.Values.Count > 50000,
                DataSource = new WaveGridBasedDataSource(discreteGrid) {CoordinateSystem = coordinateSystem},
                ReadOnly = true // to exclude from spatial editor
            };
        }

        private static ILayer CreateCurvilinearVertexCoverageLayer(IDiscreteGridPointCoverage discreteGrid,
                                                                   ICoordinateSystem coordinateSystem)
        {
            return new CurvilinearVertexCoverageLayer
            {
                Name = discreteGrid.Name,
                Coverage = discreteGrid,
                Visible = false,
                OptimizeRendering = discreteGrid.X.Values.Count > 30000,
                DataSource = new WaveGridBasedDataSource(discreteGrid) {CoordinateSystem = coordinateSystem},
                ReadOnly = !discreteGrid.IsEditable // Exclude output from spatial editor
            };
        }

        public ILayer CreateBoundaryLayer(BoundaryMapFeaturesContainer featuresProviderContainer,
                                          IWaveModel model)
        {
            Ensure.NotNull(featuresProviderContainer, nameof(featuresProviderContainer));
            Ensure.NotNull(model, nameof(model));

            var groupLayer = new GroupLayer(WaveLayerNames.SpatiallyVaryingBoundaryLayerName)
            {
                LayersReadOnly = false,
            };

            groupLayer.Layers.AddRange(CreateBoundaryLayers(featuresProviderContainer, model));

            return groupLayer;
        }

        private static IEnumerable<ILayer> CreateBoundaryLayers(BoundaryMapFeaturesContainer featuresProviderContainer, IWaveModel model)
        {
            yield return CreateSupportPointsLayer(featuresProviderContainer.SupportPointMapFeatureProvider,
                                                  model);
            yield return CreateBoundaryEndPointLayer(featuresProviderContainer.BoundaryEndPointMapFeatureProvider);
            yield return CreateBoundaryLineLayer(featuresProviderContainer.BoundaryLineMapFeatureProvider,
                                                 model);
        }

        private static ILayer CreateSupportPointsLayer(BoundarySupportPointMapFeatureProvider featureProvider, IWaveModel model)
        {
            return new VectorLayer(WaveLayerNames.BoundarySupportPointsLayerName)
            {
                ShowInTreeView = false,
                DataSource = featureProvider,
                ReadOnly = true,
                Selectable = false,
                NameIsReadOnly = true,
                FeatureEditor = new Feature2DEditor(model),
                ShowInLegend = false,
                Style = new VectorStyle
                {
                    Fill = new SolidBrush(DeltaresColor.LightBlue),
                    GeometryType = typeof(IPoint)
                }
            };
        }

        private static ILayer CreateBoundaryLineLayer(BoundaryLineMapFeatureProvider featureProvider,
                                                      IWaveModel model)
        {
            var lineDataLayer = new VectorLayer(WaveLayerNames.BoundaryLineLayerName)
            {
                ShowInTreeView = false,
                DataSource = featureProvider,
                NameIsReadOnly = true,
                FeatureEditor = new Feature2DEditor(model),
                ReadOnly = true,
                ShowInLegend = false,
                
                Style = new VectorStyle
                {
                    // TODO: Figure out whether we want to make these configurable, or whether we want to define a set of predefined values
                    Line = new Pen(Color.Blue, 3f),
                    GeometryType = typeof(ILineString)
                },
            };

            return lineDataLayer;
        }

        private static ILayer CreateBoundaryEndPointLayer(BoundaryEndPointMapFeatureProvider featureProvider)
        {
            var endPointsLayer = new VectorLayer(WaveLayerNames.BoundaryEndPointsLayerName)
            {
                ShowInTreeView = false,
                ShowInLegend = false,
                DataSource = featureProvider,
                Selectable = false,
                ReadOnly = true,
                NameIsReadOnly = true,
                Style = new VectorStyle
                {
                    Fill = Brushes.Gray,
                    GeometryType = typeof(IPoint),
                    SymbolScale = 0.5F,
                },
            };

            return endPointsLayer;
        }
    }
}