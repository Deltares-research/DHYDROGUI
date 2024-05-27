using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using DelftTools.Functions;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.FunctionStores;
using DeltaShell.Plugins.FMSuite.Common.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using SharpMap.Api;
using SharpMap.Api.Layers;
using SharpMap.Data.Providers;
using SharpMap.Editors;
using SharpMap.Editors.Interactors;
using SharpMap.Layers;
using SharpMap.Rendering;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui
{
    public class FlowFMMapLayerProvider : IMapLayerProvider
    {
        public const string BoundariesLayerName = "Boundaries";
        public const string BoundaryConditionsLayerName = "Boundary Conditions";
        public const string SourcesAndSinksLayerName = "Sources and Sinks";
        public const string OutputSnappedFeaturesLayerName = "Output Snapped features";
        public const string GridSnappedFeaturesLayerName = "Estimated Grid-snapped features";

        private static readonly ConditionalWeakTable<WaterFlowFMModel, FMSnappedFeaturesGroupLayerData>
            snappedGroupLayerDataMapping =
                new ConditionalWeakTable<WaterFlowFMModel, FMSnappedFeaturesGroupLayerData>();

        private static readonly ConditionalWeakTable<WaterFlowFMModel, FMOutputSnappedFeaturesGroupLayerData>
            outputSnappedGroupLayerDataMapping =
                new ConditionalWeakTable<WaterFlowFMModel, FMOutputSnappedFeaturesGroupLayerData>();

        private static readonly ILog log = LogManager.GetLogger(typeof(FlowFMMapLayerProvider));

        private static readonly string modelName = nameof(WaterFlowFMModel);

        /// <summary>
        /// Creates a maplayer.
        /// </summary>
        /// <param name="data"> The data object for which the layer is created. </param>
        /// <param name="parentData"> The parent object. </param>
        /// <returns> The layer that is created for the data object. </returns>
        public ILayer CreateLayer(object data, object parentData)
        {
            var waterFlowFmModel = data as WaterFlowFMModel;
            if (waterFlowFmModel != null)
            {
                return new ModelGroupLayer
                {
                    Name = waterFlowFmModel.Name,
                    Model = waterFlowFmModel,
                    NameIsReadOnly = true
                };
            }

            var importedGridFile = data as ImportedFMNetFile;
            if (importedGridFile != null)
            {
                return new UnstructuredGridLayer
                {
                    Grid = importedGridFile.Grid,
                    NameIsReadOnly = true,
                    FeatureEditor = new FeatureEditor()
                };
            }

            if (data is IEventedList<Feature2D> feature2Ds && parentData is WaterFlowFMModel parentWaterFlowFmModel1)
            {
                if (Equals(feature2Ds, parentWaterFlowFmModel1.Boundaries))
                {
                    return new VectorLayer(BoundariesLayerName)
                    {
                        DataSource =
                            new Feature2DCollection().Init(feature2Ds, "Boundary", modelName, parentWaterFlowFmModel1.CoordinateSystem),
                        FeatureEditor =
                            new Boundary2DEditor(parentWaterFlowFmModel1) { AllowRemovePoint = new RemoveBoundaryPointDialog(parentWaterFlowFmModel1).ShowDialogForFeature },
                        Style = HydroAreaLayerStyles.BoundariesStyle,
                        NameIsReadOnly = true,
                        ShowInLegend = false
                    };
                }

                if (Equals(feature2Ds, parentWaterFlowFmModel1.Pipes))
                {
                    return new VectorLayer(SourcesAndSinksLayerName)
                    {
                        DataSource =
                            new Feature2DCollection().Init(feature2Ds, "SourceSink", modelName,
                                                           parentWaterFlowFmModel1.CoordinateSystem),
                        FeatureEditor =
                            new Feature2DEditor(parentWaterFlowFmModel1),
                        Style = HydroAreaLayerStyles.SourcesAndSinksStyle,
                        NameIsReadOnly = true,
                        CustomRenderers =
                            new IFeatureRenderer[]
                            {
                                new ArrowLineStringAdornerRenderer
                                {
                                    Orientation = Orientation.Forward,
                                    Opacity = 1
                                }
                            },
                        ShowInLegend = false
                    };
                }
                
                if (Equals(feature2Ds, parentWaterFlowFmModel1.LateralFeatures))
                {
                    return LateralMapLayerProvider.Create(feature2Ds, parentWaterFlowFmModel1);
                }
            }

            if (data is IEventedList<BoundaryConditionSet> allBoundaryConditionSets && parentData is WaterFlowFMModel parentWaterFlowModel2)
            {
                CategorialTheme theme = CreateBoundaryConditionsTheme();
                return new VectorLayer(BoundaryConditionsLayerName)
                {
                    DataSource =
                        new Feature2DCollection().Init(allBoundaryConditionSets, "BoundaryCondition", modelName,
                                                       parentWaterFlowModel2.CoordinateSystem),
                    Theme = theme,
                    Style = (VectorStyle)theme.DefaultStyle,
                    NameIsReadOnly = true,
                    ShowInTreeView = true,
                    ShowInLegend = false,
                    Selectable = false
                };
            }

            if (data is IFMMapFileFunctionStore)
            {
                var groupLayer = new GroupLayer("Output (map)")
                {
                    LayersReadOnly = true,
                    NameIsReadOnly = true
                };
                groupLayer.Layers.CollectionChanged += MapGroupLayerLayersCollectionChanged;
                return groupLayer;
            }

            if (data is IFMHisFileFunctionStore)
            {
                return new GroupLayer("Output (his)")
                {
                    LayersReadOnly = true,
                    NameIsReadOnly = true
                };
            }

            if (data is IFMClassMapFileFunctionStore)
            {
                var groupLayer = new GroupLayer("Output (class)")
                {
                    LayersReadOnly = true,
                    NameIsReadOnly = true
                };
                return groupLayer;
            }

            if (data is FMOutputSnappedFeaturesGroupLayerData outputSnappedGroupLayerData)
            {
                var groupLayer = new GroupLayer(OutputSnappedFeaturesLayerName)
                {
                    Visible = false,
                    NameIsReadOnly = true
                };

                groupLayer.Layers.AddRange(outputSnappedGroupLayerData.CreateLayers());
                groupLayer.LayersReadOnly = true;
                return groupLayer;
            }

            if (data is FMSnappedFeaturesGroupLayerData snappedGroupLayerData)
            {
                var groupLayer = new GroupLayer(GridSnappedFeaturesLayerName)
                {
                    Visible = false,
                    NameIsReadOnly = true
                };
                foreach (SnappedFeatureCollection snappedFeatures in snappedGroupLayerData.ChildData)
                {
                    var layer = new VectorLayer(snappedFeatures.LayerName)
                    {
                        Style = snappedFeatures.SnappedLayerStyle,
                        DataSource = snappedFeatures,
                        Selectable = false,
                        NameIsReadOnly = true
                    };
                    groupLayer.Layers.Add(layer);
                    snappedFeatures.Layer = layer;
                }

                groupLayer.LayersReadOnly = true;
                return groupLayer;
            }

            if (data is IGrouping<string, IFunction> grouping)
            {
                List<IFunction> functions = grouping.ToList();
                if (functions.Any())
                {
                    string groupLayerName = GetCommonFunctionName(functions);
                    return new GroupLayer(string.IsNullOrEmpty(groupLayerName) ? grouping.Key : groupLayerName);
                }
            }

            if (data is Samples samples)
            {
                return SamplesMapLayerProvider.Create(samples);
            }

            return null;
        }

        /// <summary>
        /// Determines whether this instance can create a layer for the specified data object.
        /// </summary>
        /// <param name="data"> The data object for which will be determined whether a layer can be created. </param>
        /// <param name="parentData"> The parent object. </param>
        /// <returns>
        /// <c> true </c> if this instance [can create layer for] the specified data; otherwise, <c> false </c>.
        /// </returns>
        public bool CanCreateLayerFor(object data, object parentData)
        {
            return data is WaterFlowFMModel
                   || data is IGrouping<string, IFunction>
                   || data is IFMMapFileFunctionStore
                   || data is IFMHisFileFunctionStore
                   || data is IFMClassMapFileFunctionStore
                   || data is ImportedFMNetFile
                   || data is IEventedList<BoundaryConditionSet> && parentData is WaterFlowFMModel
                   || data is FMSnappedFeaturesGroupLayerData
                   || data is FMOutputSnappedFeaturesGroupLayerData
                   || data is IEventedList<Feature2D> // Boundaries and sources&sinks
                   || data is Samples;
        }

        /// <summary>
        /// Child objects for <paramref name="data"/>. Objects will be used to create child layers
        /// for the group layer (<paramref name="data"/>)
        /// </summary>
        /// <param name="data"> Group layer data </param>
        /// <returns>
        /// Child objects for <paramref name="data"/>
        /// </returns>
        public IEnumerable<object> ChildLayerObjects(object data)
        {
            var model = data as WaterFlowFMModel;
            if (model != null)
            {
                IModel rootModel = GetRootModel(model);
                if (rootModel == null || rootModel is WaterFlowFMModel ||
                    model.GetDataItemByValue(model.Area).LinkedTo == null)
                {
                    if (model.Area.Enclosures.Count > 0)
                    {
                        foreach (GroupableFeature2DPolygon enclosure in model.Area.Enclosures)
                        {
                            var geoAsPol = enclosure.Geometry as Polygon;
                            if (geoAsPol == null || !geoAsPol.IsValid)
                            {
                                log.WarnFormat(
                                    Resources.WaterFlowFMEnclosureValidator_Validate_Drawn_polygon_not__0__not_valid,
                                    enclosure.Name);
                            }
                        }
                    }

                    yield return model.Area;
                }

                yield return model.BoundaryConditionSets;
                yield return model.Boundaries;
                yield return model.Pipes;
                yield return model.LateralFeatures;

                if (!snappedGroupLayerDataMapping.TryGetValue(model, out FMSnappedFeaturesGroupLayerData layerData))
                {
                    layerData = new FMSnappedFeaturesGroupLayerData(model);
                    snappedGroupLayerDataMapping.Add(model, layerData);
                }

                yield return layerData;

                if (model.WriteSnappedFeatures && Directory.Exists(model.OutputSnappedFeaturesPath))
                {
                    if (!outputSnappedGroupLayerDataMapping.TryGetValue(model, out FMOutputSnappedFeaturesGroupLayerData outputLayerData)
                        || model.Status == ActivityStatus.Finished)
                    {
                        outputLayerData = new FMOutputSnappedFeaturesGroupLayerData(model);
                        //Clear model
                        outputSnappedGroupLayerDataMapping.Remove(model);
                        outputSnappedGroupLayerDataMapping.Add(model, outputLayerData);
                    }

                    outputLayerData.CoordinateSystem = model.CoordinateSystem;

                    yield return outputLayerData;
                }

                yield return model.Grid;

                yield return model.SpatialData.InitialWaterLevel;
                yield return model.SpatialData.Roughness;
                yield return model.SpatialData.Viscosity;
                yield return model.SpatialData.Diffusivity;
                yield return model.ModelDefinition.InitialVelocityX;
                yield return model.ModelDefinition.InitialVelocityY;

                if (model.HeatFluxModelType != HeatFluxModelType.None)
                {
                    yield return model.SpatialData.InitialTemperature;
                }

                if (model.UseSalinity)
                {
                    yield return model.SpatialData.InitialSalinity;
                }

                foreach (UnstructuredGridCellCoverage tracer in model.SpatialData.InitialTracers)
                {
                    yield return tracer;
                }

                if (model.UseMorSed)
                {
                    foreach (UnstructuredGridCellCoverage fraction in model.SpatialData.InitialFractions)
                    {
                        yield return fraction;
                    }
                }

                yield return model.SpatialData.Bathymetry;

                if (model.OutputMapFileStore != null)
                {
                    yield return model.OutputMapFileStore;
                }

                if (model.OutputHisFileStore != null)
                {
                    yield return model.OutputHisFileStore;
                }

                if (model.OutputClassMapFileStore != null)
                {
                    yield return model.OutputClassMapFileStore;
                }
            }

            if (data is IFMNetCdfFileFunctionStore outputStore)
            {
                if (outputStore is IFMMapFileFunctionStore fmMapFileFunctionStore)
                {
                    foreach (object output in GetMapOutputFunctions(fmMapFileFunctionStore))
                    {
                        yield return output;
                    }
                }
                else if (outputStore is IFMClassMapFileFunctionStore fmClassMapFileFunctionStore)
                {
                    yield return fmClassMapFileFunctionStore.Grid;

                    foreach (IFunction output in outputStore.Functions)
                    {
                        yield return output;
                    }
                }
                else
                {
                    foreach (IFunction output in outputStore.Functions)
                    {
                        yield return output;
                    }
                }
            }

            // groupings currently used by IFMMapFileFunctionStore (for sedimentation outputs)
            if (data is IGrouping<string, IFunction> grouping)
            {
                foreach (IFunction function in grouping)
                {
                    yield return function;
                }
            }
        }

        public void AfterCreate(ILayer layer, object layerObject, object parentObject, IDictionary<ILayer, object> objectsLookup)
        {
            // Nothing needs to be done after creation
        }

        private static string GetCommonFunctionName(IList<IFunction> functions)
        {
            if (!functions.Any())
            {
                return string.Empty;
            }

            char[] commonFunctionName = functions[0].Name.ToCharArray();

            for (var i = 1; i < functions.Count; i++)
            {
                char[] functionName = functions[i].Name.ToCharArray();
                var commonCharacters = new List<char>();
                for (var j = 0; j < Math.Min(commonFunctionName.Length, functionName.Length); j++)
                {
                    if (commonFunctionName[j] == functionName[j])
                    {
                        commonCharacters.Add(commonFunctionName[j]);
                    }
                }

                commonFunctionName = new string(commonCharacters.ToArray()).Replace("()", string.Empty).ToCharArray();
            }

            return new string(commonFunctionName).Trim();
        }

        private void MapGroupLayerLayersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var layer = e.GetRemovedOrAddedItem() as UnstructuredGridLayer;
            if (layer == null || e.Action != NotifyCollectionChangedAction.Add)
            {
                return;
            }

            layer.GridColor = Color.Gray;
        }

        private static IEnumerable<object> GetMapOutputFunctions(IFMMapFileFunctionStore mapStore)
        {
            yield return mapStore.Grid;

            IEnumerable<IGrouping<string, IFunction>> functionGrouping = mapStore.GetFunctionGrouping();
            foreach (IGrouping<string, IFunction> group in functionGrouping)
            {
                if (group.Count() == 1)
                {
                    yield return group.ElementAt(0);
                    continue;
                }

                yield return group;
            }

            // Needs to be handled separately since it would be grouped with EastwardSeaWaterVelocityStandardName
            if (mapStore.CustomVelocityCoverage != null)
            {
                yield return mapStore.CustomVelocityCoverage;
            }
        }

        private static CategorialTheme CreateBoundaryConditionsTheme()
        {
            var theme = new CategorialTheme
            {
                AttributeName = nameof(BoundaryConditionSet.VariableDescription),
                DefaultStyle = null,
                NoDataValues = new List<string> { "" }
            };

            foreach (BoundaryConditionDataType dataType in new FlowBoundaryConditionEditorController()
                .AllSupportedDataTypes)
            {
                foreach (FlowBoundaryQuantityType qt in Enum.GetValues(typeof(FlowBoundaryQuantityType)))
                {
                    var style = new VectorStyle
                    {
                        GeometryType = typeof(IPoint),
                        Symbol = BoundaryDataMapSymbols.GetSymbol(qt, dataType)
                    };

                    string boundaryConditionName = FlowBoundaryCondition.GetDescription(qt, dataType);

                    var themeItem = new CategorialThemeItem(boundaryConditionName, style, null,
                                                            boundaryConditionName);
                    theme.AddThemeItem(themeItem);
                }
            }

            theme.DefaultStyle = new VectorStyle
            {
                GeometryType = typeof(IPoint),
                Symbol = Properties.Resources.empty
            };
            return theme;
        }

        private static IModel GetRootModel(IModel model)
        {
            IModel rootModelForModel = GetRootModelRecursive(model);
            return rootModelForModel == model ? null : rootModelForModel;
        }

        private static IModel GetRootModelRecursive(IModel model)
        {
            return model.Owner is IModel ownerModel
                       ? GetRootModelRecursive(ownerModel)
                       : model;
        }
    }
}