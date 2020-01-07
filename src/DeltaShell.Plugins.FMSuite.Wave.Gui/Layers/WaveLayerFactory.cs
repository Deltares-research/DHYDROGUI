using System;
using System.Drawing;
using DelftTools.Utils.Collections.Generic;
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
    public static class WaveLayerFactory
    {
        /// <summary> The wave model name. </summary>
        private static readonly string waveModelName = typeof(WaveModel).Name;

        /// <summary>
        /// Creates a new model layer from the given <paramref name="waveModel"/>.
        /// </summary>
        /// <param name="waveModel">The wave model.</param>
        /// <returns>A new <see cref="ILayer"/> containing teh model.</returns>
        /// <exception cref="ArgumentNullException">
        /// Throw when <paramref name="waveModel"/> is <c>null</c>.
        /// </exception>
        public static ILayer CreateModelGroupLayer(WaveModel waveModel)
        {
            if (waveModel == null)
            {
                throw new ArgumentNullException(nameof(waveModel));
            }

            return new ModelGroupLayer
            {
                Name = waveModel.Name,
                Model = waveModel,
            };
        }

        /// <summary>
        /// Creates a new <see cref="WaveDomainData"/> layer.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <returns>
        /// A new <see cref="ILayer"/> of the <see cref="WaveDomainData"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="domain"/> is <c>null</c>.
        /// </exception>
        public static ILayer CreateWaveDomainDataLayer(WaveDomainData domain)
        {
            if (domain == null)
            {
                throw new ArgumentNullException(nameof(domain));
            }

            return new GroupLayer(WaveLayerNames.GetDomainLayerName(domain.Name));
        }

        /// <summary>
        /// Creates a new obstacle layer from the obstacles within
        /// <paramref name="waveModel"/>.
        /// </summary>
        /// <param name="waveModel">The wave model.</param>
        /// <returns>
        /// A new <see cref="ILayer"/> visualising the obstacle features.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="waveModel"/> is <c>null</c>.
        /// </exception>
        public static ILayer CreateObstacleLayer(IWaveModel waveModel)
        {
            if (waveModel == null)
            {
                throw new ArgumentNullException(nameof(waveModel));
            }

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

        /// <summary>
        /// Creates a new obstacle data layer.
        /// </summary>
        /// <param name="obstacleData">The obstacle data.</param>
        /// <param name="coordinateSystem">The coordinate system.</param>
        /// <returns>
        /// A new <see cref="ILayer"/> visualising the obstacle data features.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the parameters is <c>null</c>.
        /// </exception>
        public static ILayer CreateObstacleDataLayer(IEventedList<WaveObstacle> obstacleData, 
                                                     ICoordinateSystem coordinateSystem)
        {
            if (obstacleData == null)
            {
                throw new ArgumentNullException(nameof(obstacleData));
            }

            return new VectorLayer(WaveLayerNames.ObstacleDataLayerName)
            {
                DataSource = new Feature2DCollection().Init(obstacleData, 
                                                            "WaveObstacleData", 
                                                            waveModelName,
                                                            coordinateSystem),
                Style = new VectorStyle
                {
                    Symbol = WaveLayerIcons.ObstacleData,
                    GeometryType = typeof(IPoint)
                },
                NameIsReadOnly = true
            };
        }

        /// <summary>
        /// Creates a new observation points layer from the observation points
        /// within <paramref name="waveModel"/>.
        /// </summary>
        /// <param name="waveModel">The wave model.</param>
        /// <returns>
        /// A new <see cref="ILayer"/> visualising the observation points features.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="waveModel"/> is <c>null</c>.
        /// </exception>
        public static ILayer CreateObservationPointsLayer(IWaveModel waveModel)
        {
            if (waveModel == null)
            {
                throw new ArgumentNullException(nameof(waveModel));
            }

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

        /// <summary>
        /// Creates a new observation cross-section layer from the observation cross sections
        /// within <paramref name="waveModel"/>.
        /// </summary>
        /// <param name="waveModel">The wave model.</param>
        /// <returns>
        /// A new <see cref="ILayer"/> visualising the observation cross-sections features.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="waveModel"/> is <c>null</c>.
        /// </exception>
        public static ILayer CreateObservationCrossSectionLayer(IWaveModel waveModel)
        {
            if (waveModel == null)
            {
                throw new ArgumentNullException(nameof(waveModel));
            }

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

        /// <summary>
        /// Creates a new snapped features layer from the <paramref name="snappedFeatures"/>.
        /// </summary>
        /// <param name="snappedFeatures">The snapped features.</param>
        /// <returns>
        /// A new <see cref="ILayer"/> visualising the snapped features.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="snappedFeatures"/> is <c>null</c>.
        /// </exception>
        public static ILayer CreateSnappedFeaturesLayer(WaveSnappedFeaturesGroupLayerData snappedFeatures)
        {
            if (snappedFeatures == null)
            {
                throw new ArgumentNullException(nameof(snappedFeatures));
            }

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

        /// <summary>
        /// Creates a new output layer with the given <paramref name="domainName"/>.
        /// </summary>
        /// <param name="domainName">Name of the domain.</param>
        /// <param name="overrideLayerName">if set to <c>true</c> use the <paramref name="domainName"/> verbatim.</param>
        /// <returns>
        /// A new output <see cref="ILayer"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="domainName"/> is <c>null</c>.
        /// </exception>
        public static ILayer CreateOutputLayer(string domainName, bool overrideLayerName = false)
        {
            if (domainName == null)
            {
                throw new ArgumentNullException(nameof(domainName));
            }

            string layerName = overrideLayerName ? domainName 
                                   : WaveLayerNames.GetOutputLayerName(domainName);

            return new GroupLayer(layerName)
            {
                LayersReadOnly = true,
            };
        }

        /// <summary>
        /// Creates a new grid layer from the given <paramref name="discreteGrid"/>
        /// and <paramref name="coordinateSystem"/>.
        /// </summary>
        /// <param name="discreteGrid">The discrete grid.</param>
        /// <param name="coordinateSystem">The coordinate system.</param>
        /// <returns>
        /// A new grid <see cref="ILayer"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="discreteGrid"/> is <c>null</c>.
        /// </exception>
        public static ILayer CreateGridLayer(IDiscreteGridPointCoverage discreteGrid,
                                             ICoordinateSystem coordinateSystem)
        {
            if (discreteGrid == null)
            {
                throw new ArgumentNullException(nameof(discreteGrid));
            }

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

        /// <summary>
        /// Creates the boundary group layer.
        /// </summary>
        /// <param name="featuresProviderContainer">The features container.</param>
        /// <param name="model">The model.</param>
        /// <returns>
        /// A new boundary group layer.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public static ILayer CreateBoundaryLayer(BoundaryMapFeaturesContainer featuresProviderContainer,
                                                 IWaveModel model)
        {
            if (featuresProviderContainer == null)
            {
                throw new ArgumentNullException(nameof(featuresProviderContainer));
            }

            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var groupLayer = new GroupLayer(WaveLayerNames.SpatiallyVaryingBoundaryLayerName)
            {
                LayersReadOnly = false,
            };

            ILayer endPointsDataLayer =
                CreateBoundaryEndPointLayer(featuresProviderContainer.BoundaryEndPointMapFeatureProvider,
                                            model);
            groupLayer.Layers.Add(endPointsDataLayer);
            
            ILayer lineDataLayer = 
                CreateBoundaryLineLayer(featuresProviderContainer.BoundaryLineMapFeatureProvider,
                                        model);
            groupLayer.Layers.Add(lineDataLayer);

            ILayer supportPointsLayer =
                CreateSupportPointsLayer(featuresProviderContainer.SupportPointMapFeatureProvider, model);

            groupLayer.Layers.Add(supportPointsLayer);

            return groupLayer;
        }

        private static ILayer CreateSupportPointsLayer(BoundarySupportPointMapFeatureProvider featureProvider, IWaveModel model)
        {
            return new VectorLayer(WaveLayerNames.BoundarySupportPointsLayerName)
            {
                DataSource = featureProvider,
                ReadOnly = true,
                Selectable = false,
                NameIsReadOnly = true,
                FeatureEditor = new Feature2DEditor(model),
                Style = new VectorStyle
                {
                    Fill = new SolidBrush(Color.FromArgb(14, 187, 240)),
                    GeometryType = typeof(IPoint)
                }
            };
        }

        private static ILayer CreateBoundaryLineLayer(BoundaryLineMapFeatureProvider featureProvider,
                                                      IWaveModel model)
        {
            var lineDataLayer = new VectorLayer(WaveLayerNames.BoundaryLineLayerName)
            {
                DataSource = featureProvider,
                NameIsReadOnly = true,
                FeatureEditor = new Feature2DEditor(model),
                Style = new VectorStyle
                {
                    Line = new Pen(Color.Blue, 3f),
                    GeometryType = typeof(ILineString)
                },
            };

            return lineDataLayer;
        }

        private static ILayer CreateBoundaryEndPointLayer(BoundaryEndPointMapFeatureProvider featureProvider,
                                                          IWaveModel model)
        {
            var endPointsLayer = new VectorLayer(WaveLayerNames.BoundaryEndPointsLayerName)
            {
                DataSource = featureProvider,
                Selectable = false,
                ReadOnly = true,
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