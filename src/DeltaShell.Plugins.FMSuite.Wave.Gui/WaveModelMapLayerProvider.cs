using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.Layers;
using DeltaShell.Plugins.FMSuite.Wave.IO;
using DeltaShell.Plugins.FMSuite.Wave.Layers;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using SharpMap.Api.Layers;
using SharpMap.Data.Providers;
using SharpMap.Editors.Interactors;
using SharpMap.Layers;
using SharpMap.Styles;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui
{
    public class WaveModelMapLayerProvider : IMapLayerProvider
    {
        public const string ObstacleLayerName = "Obstacles";
        public const string ObstacleDataLayerName = "Obstacle Data";
        public const string BoundaryLayerName = "Boundaries";
        public const string BoundaryConditionLayerName = "Boundary Conditions";
        public const string ObservationPointLayerName = "Observation Points";
        public const string ObservationCrossSectionLayerName = "Observation Cross-Sections";

        private static readonly string ModelName = typeof (WaveModel).Name;

        private static readonly Bitmap coordinateBasedBoundaryIcon = Common.Gui.Properties.Resources.boundary;
        private static readonly Bitmap obstacleDataIcon = Properties.Resources.wall_brick;
        
        public ILayer CreateLayer(object data, object parent)
        {
            var waveModel = data as WaveModel;
            if (waveModel != null)
            {
                return new ModelGroupLayer { Name = waveModel.Name, Model = waveModel };
            }

            var domain = data as WaveDomainData;
            if (domain != null)
            {
                return new GroupLayer("Domain (" + domain.Name + ")");
            }

            var model = parent as WaveModel;
            var discreteGrid = data as IDiscreteGridPointCoverage;
            if (discreteGrid != null)
            {
                ICoordinateSystem coordinateSystem;

                var store = parent as WavmFileFunctionStore;
                if (model != null)
                {
                    coordinateSystem = model.CoordinateSystem;
                }
                else if (store != null)
                {
                    coordinateSystem = null;
                }
                else
                {
                    var ownerWaveModel = WaveModels.FirstOrDefault(w => w.GetAllItemsRecursive().Contains(discreteGrid));
                    coordinateSystem = ownerWaveModel == null ? null : ownerWaveModel.CoordinateSystem;
                }

                if (discreteGrid is CurvilinearGrid)
                {
                    return new CurvilinearGridLayer
                    {
                        Name = discreteGrid.Name,
                        CurviLinearGrid = discreteGrid,
                        OptimizeRendering = discreteGrid.X.Values.Count > 50000,
                        DataSource = new WaveGridBasedDataSource(discreteGrid)
                        {
                            CoordinateSystem = coordinateSystem
                        },
                        ReadOnly = true // to exclude from spatial editor
                    };
                }
                return new CurvilinearVertexCoverageLayer
                {
                    Name = discreteGrid.Name,
                    Coverage = discreteGrid,
                    Visible = false,
                    OptimizeRendering = discreteGrid.X.Values.Count > 30000,
                    DataSource = new WaveGridBasedDataSource(discreteGrid)
                    {
                        CoordinateSystem = coordinateSystem
                    }
                };
            }

            var features = data as IEnumerable<Feature2D>;
            if (features != null && model != null)
            {
                if (Equals(features, model.Boundaries))
                {
                    return new VectorLayer(BoundaryLayerName)
                        {
                            DataSource =
                                new Feature2DCollection().Init(model.Boundaries, "Boundary", ModelName, model.CoordinateSystem, model.GetGridSnappedBoundary),
                            FeatureEditor = new Feature2DEditor(model),
                            Style = WaveModelLayerStyles.BoundaryStyle,
                            NameIsReadOnly = true,
                            Selectable = !model.BoundaryIsDefinedBySpecFile
                        };
                }
                if (Equals(features, model.Obstacles))
                {
                    return new VectorLayer(ObstacleLayerName)
                    {
                        DataSource = new Feature2DCollection().Init(model.Obstacles, "Obstacle", ModelName, model.CoordinateSystem),
                        FeatureEditor = new Feature2DEditor(model),
                        Style = new VectorStyle { Line = new Pen(Color.Red, 3f), GeometryType = typeof(ILineString) },
                        NameIsReadOnly = true
                    };
                }
                if (Equals(features, model.Sp2Boundaries))
                {
                    return new VectorLayer("Boundary from sp2")
                    {
                        DataSource = new Feature2DCollection().Init(model.Sp2Boundaries, "Sp2Boundary", ModelName, model.CoordinateSystem),
                        Style = new VectorStyle { Line = new Pen(Color.DarkOrange, 3f), GeometryType = typeof(ILineString) },
                        NameIsReadOnly = true,
                        Selectable = false
                    };
                }
                if (Equals(features, model.ObservationCrossSections))
                {
                    return new VectorLayer(ObservationCrossSectionLayerName)
                        {
                            DataSource =
                                new Feature2DCollection().Init(model.ObservationCrossSections, "CrS",
                                                               ModelName, model.CoordinateSystem),
                            FeatureEditor = new Feature2DEditor(model),
                            Style =
                                new VectorStyle
                                    {
                                        Line = new Pen(Color.LightSteelBlue, 3f),
                                        GeometryType = typeof (ILineString)
                                    },
                            NameIsReadOnly = true
                        };
                }
                if (Equals(features, model.ObservationPoints))
                {
                    return new VectorLayer(ObservationPointLayerName)
                        {
                            NameIsReadOnly = true,
                            FeatureEditor = new Feature2DEditor(model),
                            Style = new VectorStyle
                                {
                                    GeometryType = typeof (IPoint),
                                    Symbol = Common.Gui.Properties.Resources.Observation
                                },
                            DataSource =
                                new Feature2DCollection().Init(model.ObservationPoints, "ObservationPoints", ModelName,
                                                               model.CoordinateSystem)
                        };
                }
            }

            var boundaryConditions = data as IEventedList<WaveBoundaryCondition>;
            if (boundaryConditions != null && model != null)
            {
                return new VectorLayer(BoundaryConditionLayerName)
                    {
                        DataSource = new Feature2DCollection().Init(boundaryConditions, "BoundaryCondition", ModelName, model.CoordinateSystem),
                        Style = new VectorStyle { Symbol = coordinateBasedBoundaryIcon, GeometryType = typeof(IPoint) },
                        NameIsReadOnly = true
                    };
            }

            var obstacleData = data as IEventedList<WaveObstacle>;
            if (obstacleData != null && model != null)
            {
                return new VectorLayer(ObstacleDataLayerName)
                {
                    DataSource = new Feature2DCollection().Init(obstacleData, "WaveObstacleData", ModelName, model.CoordinateSystem),
                    Style = new VectorStyle{Symbol = obstacleDataIcon, GeometryType = typeof(IPoint)},
                    NameIsReadOnly = true
                };
            }

            var snappedGroupLayerData = data as WaveSnappedFeaturesGroupLayerData;
            if (snappedGroupLayerData != null)
            {
                var groupLayer = new GroupLayer("Grid-snapped features");
                foreach (var snappedFeatures in snappedGroupLayerData.ChildData)
                {
                    var vectorLayer = new VectorLayer("Boundaries")
                        {
                            DataSource = snappedFeatures,
                            NameIsReadOnly = true,
                            Selectable = false,
                            Style = new VectorStyle { Fill = Brushes.Gray, GeometryType = typeof(IPoint) }
                        };
                    groupLayer.Layers.Add(vectorLayer);
                }
                return groupLayer;
            }

            var wavmFileFunctionStore = data as WavmFileFunctionStore;
            if (wavmFileFunctionStore != null && wavmFileFunctionStore.Functions.Count != 0)
            {
                if (model != null)
                {
                    var dataItem = model.GetDataItemByValue(wavmFileFunctionStore);
                    if (dataItem.Tag.StartsWith(WaveModel.WavmStoreDataItemTag))
                    {
                        var domainName = string.Join(" ",
                            new string(dataItem.Tag.Skip(WaveModel.WavmStoreDataItemTag.Count()).ToArray()), "WAVM");
                        return new GroupLayer("Output (" + domainName + ")")
                        {
                            LayersReadOnly = true,
                        };
                    }
                }
                return new GroupLayer(wavmFileFunctionStore.Path)
                {
                    LayersReadOnly = true
                };
            }

            return null;
        }

        public bool CanCreateLayerFor(object data, object parentData)
        {
            return data is WaveModel 
                || data is WaveDomainData
                || data is IDiscreteGridPointCoverage
                || data is IEventedList<WaveObstacle>
                || data is IEventedList<Feature2D>
                || data is IEventedList<Feature2DPoint>
                || data is IEventedList<WaveBoundaryCondition>
                || data is WaveSnappedFeaturesGroupLayerData
                || data is WavmFileFunctionStore
                || data is CurvilinearCoverage;
        }

        public IEnumerable<object> ChildLayerObjects(object data)
        {
            var model = data as WaveModel;
            if (model != null)
            {
                WaveSnappedFeaturesGroupLayerData snappedData;
                if (!modelToSnappedFeatureData.TryGetValue(model, out snappedData))
                {
                    snappedData = new WaveSnappedFeaturesGroupLayerData(model);
                    modelToSnappedFeatureData.Add(model, snappedData);
                }
                yield return snappedData;
                yield return model.BoundaryConditions;
                yield return model.Boundaries;
                yield return model.Sp2Boundaries;
                yield return model.Obstacles;
                yield return model.ObservationPoints;
                yield return model.ObservationCrossSections;
                foreach (var waveDomain in WaveDomainHelper.GetAllDomains(model.OuterDomain))
                {
                    yield return waveDomain;
                }
                foreach (
                    var wavmFunctionStore in
                        model.WavmFunctionStores.Where(fs => fs.Functions.Any() && !string.IsNullOrEmpty(fs.Path)))
                {
                    yield return wavmFunctionStore;
                }
            }

            var domain = data as WaveDomainData;
            if (domain != null)
            {
                yield return domain.Grid;
                yield return domain.Bathymetry;
            }

            var store = data as WavmFileFunctionStore;
            if (store != null)
            {
                var waveModel = WaveModels.FirstOrDefault(m => m.WavmFunctionStores.Contains(store));
                if (waveModel == null)
                {
                    yield return store.Grid;
                }
                foreach (var coverage in store.Functions)
                    yield return coverage;
            }
        }

        private IEnumerable<WaveModel> WaveModels
        {
            get { return modelToSnappedFeatureData.Keys; }
        }

        private static readonly EnumerableConditionalWeakTable<WaveModel, WaveSnappedFeaturesGroupLayerData> modelToSnappedFeatureData =
            new EnumerableConditionalWeakTable<WaveModel, WaveSnappedFeaturesGroupLayerData>();
    }
}
