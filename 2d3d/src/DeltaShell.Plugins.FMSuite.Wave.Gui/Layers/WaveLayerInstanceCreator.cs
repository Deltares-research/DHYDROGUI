using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.FMSuite.Common.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Containers;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Grids;
using SharpMap.Api;
using SharpMap.Api.Layers;
using SharpMap.Data.Providers;
using SharpMap.Editors.Interactors;
using SharpMap.Layers;
using SharpMap.Styles;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers
{
    /// <summary>
    /// <see cref="WaveLayerInstanceCreator"/> provides the methods to construct the
    /// different layers of the wave model.
    /// </summary>
    public class WaveLayerInstanceCreator : IWaveLayerInstanceCreator
    {
        /// <summary> The wave model name. </summary>
        private static readonly string waveModelName = nameof(WaveModel);

        public ILayer CreateModelGroupLayer(WaveModel waveModel)
        {
            Ensure.NotNull(waveModel, nameof(waveModel));

            return new ModelGroupLayer
            {
                Name = waveModel.Name,
                Model = waveModel
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
                DataSource = new Feature2DCollection().Init(waveModel.FeatureContainer.Obstacles,
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
                DataSource = new Feature2DCollection().Init(waveModel.FeatureContainer.ObservationPoints,
                                                            "ObservationPoints",
                                                            waveModelName,
                                                            waveModel.CoordinateSystem)
            };
        }

        public ILayer CreateObservationCrossSectionLayer(IWaveModel waveModel)
        {
            Ensure.NotNull(waveModel, nameof(waveModel));

            return new VectorLayer(WaveLayerNames.ObservationCrossSectionLayerName)
            {
                DataSource = new Feature2DCollection().Init(waveModel.FeatureContainer.ObservationCrossSections,
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

        public ILayer CreateGridLayer(IDiscreteGridPointCoverage discreteGrid,
                                      ICoordinateSystem coordinateSystem)
        {
            Ensure.NotNull(discreteGrid, nameof(discreteGrid));

            return discreteGrid is CurvilinearGrid
                       ? CreateCurvilinearGridLayer(discreteGrid, coordinateSystem)
                       : CreateCurvilinearVertexCoverageLayer(discreteGrid, coordinateSystem);
        }

        public ILayer CreateBoundaryLayer(IBoundaryMapFeaturesContainer featuresProviderContainer)
        {
            Ensure.NotNull(featuresProviderContainer, nameof(featuresProviderContainer));

            var groupLayer = new GroupLayer(WaveLayerNames.BoundaryLayerName) {LayersReadOnly = false};

            groupLayer.Layers.AddRange(CreateBoundaryLayers(featuresProviderContainer));

            return groupLayer;
        }

        public ILayer CreateSupportPointsLayer(IFeatureProvider featureProvider)
        {
            Ensure.NotNull(featureProvider, nameof(featureProvider));
            return new VectorLayer(WaveLayerNames.BoundarySupportPointsLayerName)
            {
                ShowInTreeView = false,
                DataSource = featureProvider,
                ReadOnly = true,
                Selectable = false,
                NameIsReadOnly = true,
                ShowInLegend = false,
                SmoothingMode = SmoothingMode.AntiAlias,
                Style = new VectorStyle
                {
                    Fill = new SolidBrush(DeltaresColor.LightBlue),
                    GeometryType = typeof(IPoint)
                }
            };
        }

        public ILayer CreateBoundaryLineLayer(IFeatureProvider featureProvider)
        {
            Ensure.NotNull(featureProvider, nameof(featureProvider));
            return new VectorLayer(WaveLayerNames.BoundaryLineLayerName)
            {
                ShowInTreeView = false,
                DataSource = featureProvider,
                NameIsReadOnly = true,
                ReadOnly = true,
                ShowInLegend = false,
                SmoothingMode = SmoothingMode.AntiAlias,
                Style = new VectorStyle
                {
                    Line = new Pen(DeltaresColor.Blue, 3f),
                    GeometryType = typeof(ILineString)
                }
            };
        }

        public ILayer CreateBoundaryStartPointLayer(IFeatureProvider featureProvider)
        {
            Ensure.NotNull(featureProvider, nameof(featureProvider));

            var style = new VectorStyle
            {
                Fill = new SolidBrush(Color.LightGreen),
                GeometryType = typeof(IPoint)
            };

            return CreateReadOnlyLayer(WaveLayerNames.BoundaryStartPointsLayerName, featureProvider, style);
        }

        public ILayer CreateBoundaryEndPointLayer(IFeatureProvider featureProvider)
        {
            Ensure.NotNull(featureProvider, nameof(featureProvider));

            var style = new VectorStyle
            {
                Fill = new SolidBrush(Color.LightCoral),
                GeometryType = typeof(IPoint)
            };

            return CreateReadOnlyLayer(WaveLayerNames.BoundaryEndPointsLayerName, featureProvider, style);
        }

        public ILayer CreateInactiveSupportPointsLayer(IFeatureProvider featureProvider)
        {
            Ensure.NotNull(featureProvider, nameof(featureProvider));

            var style = new VectorStyle
            {
                Fill = new SolidBrush(Color.LightGray),
                Outline = new Pen(Color.DimGray),
                GeometryType = typeof(IPoint),
                ShapeSize = 26
            };

            return CreateReadOnlyLayer(WaveLayerNames.InactiveSupportPointsLayerName, featureProvider, style);
        }

        public ILayer CreateActiveSupportPointsLayer(IFeatureProvider featureProvider)
        {
            Ensure.NotNull(featureProvider, nameof(featureProvider));

            var style = new VectorStyle
            {
                Fill = new SolidBrush(Color.Gold),
                GeometryType = typeof(IPoint),
                ShapeSize = 26
            };

            return CreateReadOnlyLayer(WaveLayerNames.ActiveSupportPointsLayerName, featureProvider, style);
        }

        public ILayer CreateWaveOutputDataLayer(IWaveOutputData outputData)
        {
            Ensure.NotNull(outputData, nameof(outputData));

            return CreateWaveOutputGroupLayer(WaveLayerNames.WaveOutputDataLayerName);
        }

        public ILayer CreateWaveOutputGroupLayer(string layerName)
        {
            Ensure.NotNull(layerName, nameof(layerName));
            return new GroupLayer(layerName) {LayersReadOnly = true};
        }

        public ILayer CreateSelectedSupportPointLayer(IFeatureProvider featureProvider)
        {
            Ensure.NotNull(featureProvider, nameof(featureProvider));

            var style = new VectorStyle
            {
                Fill = new SolidBrush(Color.PaleVioletRed),
                GeometryType = typeof(IPoint),
                ShapeSize = 32,
                EnableOutline = false
            };

            return CreateReadOnlyLayer(WaveLayerNames.SelectedSupportPointLayerName, featureProvider, style);
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

        private IEnumerable<ILayer> CreateBoundaryLayers(IBoundaryMapFeaturesContainer featuresProviderContainer)
        {
            yield return CreateBoundaryStartPointLayer(featuresProviderContainer.BoundaryStartPointMapFeatureProvider);
            yield return CreateBoundaryEndPointLayer(featuresProviderContainer.BoundaryEndPointMapFeatureProvider);
            yield return CreateSupportPointsLayer(featuresProviderContainer.SupportPointMapFeatureProvider);
            yield return CreateBoundaryLineLayer(featuresProviderContainer.BoundaryLineMapFeatureProvider);
        }

        private static VectorLayer CreateReadOnlyLayer(string layerName,
                                                       IFeatureProvider dataSource,
                                                       VectorStyle style) =>
            new VectorLayer(layerName)
            {
                ShowInTreeView = false,
                DataSource = dataSource,
                ReadOnly = true,
                Selectable = false,
                NameIsReadOnly = true,
                ShowInLegend = false,
                SmoothingMode = SmoothingMode.AntiAlias,
                Style = style
            };
    }
}