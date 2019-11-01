using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.Gui.Properties;
using DeltaShell.Plugins.FMSuite.Common.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
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
        private static readonly string ModelName = typeof(WaveModel).Name;


        public ILayer CreateLayer(object data, object parent)
        {
            var waveModel = data as WaveModel;
            if (waveModel != null)
            {
                return new ModelGroupLayer
                {
                    Name = waveModel.Name,
                    Model = waveModel
                };
            }

            var domain = data as WaveDomainData;
            if (domain != null)
            {
                return new GroupLayer("Domain (" + domain.Name + ")");
            }

            var model = parent as IWaveModel;
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
                    WaveModel ownerWaveModel =
                        GetWaveModels?.Invoke().FirstOrDefault(w => w.GetAllItemsRecursive()
                                                                     .Contains(discreteGrid));

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
                    },
                    ReadOnly = !discreteGrid.IsEditable // Exclude output from spatial editor
                };
            }

            var features = data as IEnumerable<Feature2D>;
            if (features != null && model != null)
            {
                if (Equals(features, model.Boundaries))
                {
                    return new VectorLayer(WaveLayerNames.BoundaryLayerName)
                    {
                        DataSource =
                            new Feature2DCollection().Init(model.Boundaries, "Boundary", ModelName,
                                                           model.CoordinateSystem, model.GetGridSnappedBoundary),
                        FeatureEditor = new Feature2DEditor(model),
                        Style = WaveModelLayerStyles.BoundaryStyle,
                        NameIsReadOnly = true,
                        Selectable = !model.BoundaryIsDefinedBySpecFile
                    };
                }

                if (Equals(features, model.Obstacles))
                {
                    return new VectorLayer(WaveLayerNames.ObstacleLayerName)
                    {
                        DataSource =
                            new Feature2DCollection().Init(model.Obstacles, "Obstacle", ModelName,
                                                           model.CoordinateSystem),
                        FeatureEditor = new Feature2DEditor(model),
                        Style = new VectorStyle
                        {
                            Line = new Pen(Color.Red, 3f),
                            GeometryType = typeof(ILineString)
                        },
                        NameIsReadOnly = true
                    };
                }

                if (Equals(features, model.Sp2Boundaries))
                {
                    return new VectorLayer("Boundary from sp2")
                    {
                        DataSource =
                            new Feature2DCollection().Init(model.Sp2Boundaries, "Sp2Boundary", ModelName,
                                                           model.CoordinateSystem),
                        Style = new VectorStyle
                        {
                            Line = new Pen(Color.DarkOrange, 3f),
                            GeometryType = typeof(ILineString)
                        },
                        NameIsReadOnly = true,
                        Selectable = false
                    };
                }

                if (Equals(features, model.ObservationCrossSections))
                {
                    return new VectorLayer(WaveLayerNames.ObservationCrossSectionLayerName)
                    {
                        DataSource =
                            new Feature2DCollection().Init(model.ObservationCrossSections, "CrS",
                                                           ModelName, model.CoordinateSystem),
                        FeatureEditor = new Feature2DEditor(model),
                        Style =
                            new VectorStyle
                            {
                                Line = new Pen(Color.LightSteelBlue, 3f),
                                GeometryType = typeof(ILineString)
                            },
                        NameIsReadOnly = true
                    };
                }

                if (Equals(features, model.ObservationPoints))
                {
                    return new VectorLayer(WaveLayerNames.ObservationPointLayerName)
                    {
                        NameIsReadOnly = true,
                        FeatureEditor = new Feature2DEditor(model),
                        Style = new VectorStyle
                        {
                            GeometryType = typeof(IPoint),
                            Symbol = Resources.Observation
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
                return new VectorLayer(WaveLayerNames.BoundaryConditionLayerName)
                {
                    DataSource =
                        new Feature2DCollection().Init(boundaryConditions, "BoundaryCondition", ModelName,
                                                       model.CoordinateSystem),
                    Style = new VectorStyle
                    {
                        Symbol = WaveLayerIcons.CoordinateBasedBoundary,
                        GeometryType = typeof(IPoint)
                    },
                    NameIsReadOnly = true
                };
            }

            var obstacleData = data as IEventedList<WaveObstacle>;
            if (obstacleData != null && model != null)
            {
                return new VectorLayer(WaveLayerNames.ObstacleDataLayerName)
                {
                    DataSource =
                        new Feature2DCollection().Init(obstacleData, "WaveObstacleData", ModelName,
                                                       model.CoordinateSystem),
                    Style = new VectorStyle
                    {
                        Symbol = WaveLayerIcons.ObstacleData,
                        GeometryType = typeof(IPoint)
                    },
                    NameIsReadOnly = true
                };
            }

            var snappedGroupLayerData = data as WaveSnappedFeaturesGroupLayerData;
            if (snappedGroupLayerData != null)
            {
                var groupLayer = new GroupLayer(WaveLayerNames.GridSnappedFeaturesLayerName);
                foreach (FeatureCollection snappedFeatures in snappedGroupLayerData.ChildData)
                {
                    var vectorLayer = new VectorLayer("Boundaries")
                    {
                        DataSource = snappedFeatures,
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

            var wavmFileFunctionStore = data as WavmFileFunctionStore;
            if (wavmFileFunctionStore != null && wavmFileFunctionStore.Functions.Count != 0)
            {
                if (model != null)
                {
                    IDataItem dataItem = model.GetDataItemByValue(wavmFileFunctionStore);
                    if (dataItem.Tag.StartsWith(WaveModel.WavmStoreDataItemTag))
                    {
                        string domainName = string.Join(" ",
                                                        new string(
                                                            dataItem.Tag.Skip(WaveModel.WavmStoreDataItemTag.Count())
                                                                    .ToArray()), "WAVM");
                        return new GroupLayer("Output (" + domainName + ")")
                        {
                            LayersReadOnly = true,
                        };
                    }
                }

                return new GroupLayer(wavmFileFunctionStore.Path) {LayersReadOnly = true};
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
                yield return new WaveSnappedFeaturesGroupLayerData(model);
                yield return model.BoundaryConditions;
                yield return model.Boundaries;
                yield return model.Sp2Boundaries;
                yield return model.Obstacles;
                yield return model.ObservationPoints;
                yield return model.ObservationCrossSections;
                foreach (WaveDomainData waveDomain in WaveDomainHelper.GetAllDomains(model.OuterDomain))
                {
                    yield return waveDomain;
                }

                foreach (
                    WavmFileFunctionStore wavmFunctionStore in
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
                WaveModel waveModel = GetWaveModels?.Invoke().FirstOrDefault(m => m.WavmFunctionStores.Contains(store));
                if (waveModel == null)
                {
                    yield return store.Grid;
                }

                foreach (IFunction coverage in store.Functions)
                {
                    yield return coverage;
                }
            }
        }

        public Func<IEnumerable<WaveModel>> GetWaveModels { get; set; }
    }
}