using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.CustomRenderers;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.FeatureEditing;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.ProjectExplorer;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Grids;
using SharpMap.Api;
using SharpMap.Api.Layers;
using SharpMap.Editors;
using SharpMap.Layers;
using SharpMap.Rendering;
using SharpMap.Styles;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui
{
    public class WaterQualityModelMapLayerProvider : IMapLayerProvider
    {
        public ILayer CreateLayer(object data, object parentData)
        {
            ILayer layer = CreateLayerForWaterQualityModel(data, parentData);
            if (layer != null)
            {
                return layer;
            }

            var unstructuredGrid = data as UnstructuredGrid;
            if (unstructuredGrid != null)
            {
                var group = new GroupLayer("Unstructured Grid");
                group.Layers.Add(new UnstructuredGridLayer
                {
                    Grid = unstructuredGrid,
                    Name = "Blocked Flow Links",
                    NameIsReadOnly = true,
                    Selectable = false,
                    Renderer = new GridEdgeRenderer(Color.DarkRed) {GridEdgeRenderMode = GridEdgeRenderMode.EdgesWithBlockedFlowLinks}
                });
                group.Layers.Add(new UnstructuredGridLayer
                {
                    Grid = unstructuredGrid,
                    NameIsReadOnly = true,
                    FeatureEditor = new FeatureEditor(),
                    UnstructuredGridSelectionType = UnstructuredGridSelectionType.Cells
                });
                return group;
            }

            return null;
        }

        public bool CanCreateLayerFor(object data, object parentData)
        {
            return CanCreateLayerForWaterQualityModel(data, parentData) ||
                   data is WaterQualityFunctionWrapper && ((WaterQualityFunctionWrapper) data).Function is ICoverage;
        }

        public IEnumerable<object> ChildLayerObjects(object data)
        {
            foreach (object childLayerObject in GetChildLayerObjectsForWaterQualityModel(data))
            {
                yield return childLayerObject;
            }

            var functionList = data as IEventedList<IFunction>;
            if (functionList == null)
            {
                yield break;
            }

            foreach (IFunction function in functionList)
            {
                yield return function;
            }
        }

        public void AfterCreate(ILayer layer, object layerObject, object parentObject, IDictionary<ILayer, object> objectsLookup)
        {
            // Nothing needs to be done after creation
        }

        private static ILayer CreateLayerForWaterQualityModel(object data, object parentData)
        {
            var waterQualityModel = data as WaterQualityModel;
            if (waterQualityModel != null)
            {
                return new GroupLayer(waterQualityModel.Name)
                {
                    LayersReadOnly = true,
                    NameIsReadOnly = true
                };
            }

            var modelfolder = data as ModelFolder;
            if (modelfolder != null && modelfolder.Model is WaterQualityModel)
            {
                return new GroupLayer(modelfolder.Role == DataItemRole.Input ? "Input" : "Output")
                {
                    LayersReadOnly = true,
                    NameIsReadOnly = true
                };
            }

            var parentModel = GetParentModel<WaterQualityModel>(parentData);
            if (parentModel == null)
            {
                return null;
            }

            ICoordinateSystem coordinateSystem = parentModel.Grid != null ? parentModel.CoordinateSystem : null;

            if (Equals(data, parentModel.InitialConditions))
            {
                return new GroupLayer("Initial Conditions")
                {
                    LayersReadOnly = true,
                    NameIsReadOnly = true
                };
            }

            if (Equals(data, parentModel.ProcessCoefficients))
            {
                return new GroupLayer("Process Coefficients")
                {
                    LayersReadOnly = true,
                    NameIsReadOnly = true
                };
            }

            if (Equals(data, parentModel.Dispersion))
            {
                return new GroupLayer("Dispersion")
                {
                    LayersReadOnly = true,
                    NameIsReadOnly = true
                };
            }

            if (Equals(data, parentModel.Boundaries))
            {
                return new VectorLayer("Boundaries")
                {
                    DataSource =
                        new WaqModelFeatureCollection(parentModel).Init(parentModel.Boundaries, "Boundary",
                                                                        parentModel.Name, coordinateSystem),
                    ReadOnly = true,
                    CustomRenderers = new List<IFeatureRenderer> {new BoundaryRenderer()},
                    Style = new VectorStyle
                    {
                        Line = new Pen(Color.DarkBlue, 3f),
                        GeometryType = typeof(ILineString)
                    },
                    NameIsReadOnly = true
                };
            }

            if (Equals(data, parentModel.Loads))
            {
                return new VectorLayer("Loads")
                {
                    DataSource = CreateFeatureCollection(parentModel.Loads, "Load", parentModel, coordinateSystem),
                    ReadOnly = false,
                    Style = new VectorStyle
                    {
                        GeometryType = typeof(IPoint),
                        Symbol = Resources.weight
                    },
                    FeatureEditor = new WaterQualityFeatureEditor(),
                    NameIsReadOnly = true
                };
            }

            if (Equals(data, parentModel.ObservationPoints))
            {
                return new VectorLayer("Observation Points")
                {
                    DataSource =
                        CreateFeatureCollection(parentModel.ObservationPoints, "Observation Point", parentModel,
                                                coordinateSystem),
                    FeatureEditor = new WaterQualityFeatureEditor(),
                    ReadOnly = false,
                    Style = new VectorStyle
                    {
                        GeometryType = typeof(IPoint),
                        Symbol = Resources.Observation
                    },
                    NameIsReadOnly = true
                };
            }

            if (Equals(data, parentModel.OutputSubstancesDataItemSet))
            {
                return new GroupLayer("Substances")
                {
                    LayersReadOnly = true,
                    NameIsReadOnly = true
                };
            }

            if (Equals(data, parentModel.OutputParametersDataItemSet))
            {
                return new GroupLayer("Output Parameters")
                {
                    LayersReadOnly = true,
                    NameIsReadOnly = true
                };
            }

            return null;
        }

        private static IFeatureProvider CreateFeatureCollection<T>(IEventedList<T> features, string featureName,
                                                                   WaterQualityModel model,
                                                                   ICoordinateSystem coordinateSystem)
            where T : NameablePointFeature, new()
        {
            var featureCollection = new WaqModelFeatureCollection(model);
            featureCollection.AddNewFeatureFromGeometryDelegate = (provider, geometry) =>
            {
                var feature = new T {Geometry = new Point(geometry.Coordinate.X, geometry.Coordinate.Y, model.GetDefaultZ())};
                featureCollection.Features.Add(feature);
                return feature;
            };

            return featureCollection.Init(features, featureName, model.Name, coordinateSystem);
        }

        private static bool CanCreateLayerForWaterQualityModel(object data, object parentData)
        {
            return CanCreateLayerForModelCore(data, parentData,
                                              new Func<WaterQualityModel, object>[]
                                              {
                                                  m => m.InitialConditions,
                                                  m => m.ProcessCoefficients,
                                                  m => m.Dispersion,
                                                  m => m.Boundaries,
                                                  m => m.Loads,
                                                  m => m.ObservationPoints,
                                                  m => m.Grid,
                                                  m => m.OutputSubstancesDataItemSet,
                                                  m => m.OutputParametersDataItemSet
                                              });
        }

        private static bool CanCreateLayerForModelCore<T>(object data, object parentData,
                                                          IEnumerable<Func<T, object>> modelLayerData)
            where T : class, ITimeDependentModel
        {
            if (data is T)
            {
                return true;
            }

            var modelFolder = data as ModelFolder;
            if (modelFolder != null && modelFolder.Model is T)
            {
                return true;
            }

            var parentModel = GetParentModel<T>(parentData);
            if (parentModel != null)
            {
                return modelLayerData.Any(getter => Equals(data, getter(parentModel)));
            }

            return false;
        }

        private static IEnumerable<object> GetChildLayerObjectsForWaterQualityModel(object data)
        {
            var waterQualityModel = data as WaterQualityModel;
            if (waterQualityModel != null)
            {
                yield return new ModelFolder
                {
                    Model = waterQualityModel,
                    Role = DataItemRole.Input
                };
                yield return new ModelFolder
                {
                    Model = waterQualityModel,
                    Role = DataItemRole.Output
                };
            }

            var modelfolder = data as ModelFolder;
            if (modelfolder != null && modelfolder.Model is WaterQualityModel)
            {
                var wqModel = (WaterQualityModel) modelfolder.Model;

                if (modelfolder.Role == DataItemRole.Input)
                {
                    yield return wqModel.Boundaries;
                    yield return wqModel.Loads;
                    yield return wqModel.ObservationPoints;
                    yield return wqModel.ObservationAreas;

                    yield return wqModel.Grid;

                    yield return wqModel.InitialConditions;
                    yield return wqModel.ProcessCoefficients;
                    yield return wqModel.Dispersion;

                    yield return wqModel.Bathymetry;
                }

                if (modelfolder.Role == DataItemRole.Output)
                {
                    yield return wqModel.OutputSubstancesDataItemSet;
                    yield return wqModel.OutputParametersDataItemSet;

                    IEnumerable<ICoverage> networkCoverages = wqModel
                                                              .DataItems.Where(di => di.Role == DataItemRole.Output)
                                                              .Select(d => d.Value).OfType<ICoverage>();
                    foreach (ICoverage coverage in networkCoverages)
                    {
                        yield return coverage;
                    }
                }
            }

            var dataItemSet = data as IDataItemSet;
            if (dataItemSet != null)
            {
                IEnumerable<ICoverage> coverages = dataItemSet
                                                   .DataItems.Where(di => di.Role == DataItemRole.Output)
                                                   .Select(d => d.Value).OfType<ICoverage>();
                foreach (ICoverage coverage in coverages)
                {
                    yield return coverage;
                }
            }
        }

        private static T GetParentModel<T>(object parentData) where T : class
        {
            return parentData is ModelFolder
                       ? ((ModelFolder) parentData).Model as T
                       : parentData as T;
        }
    }
}