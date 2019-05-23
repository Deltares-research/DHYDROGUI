using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using DelftTools.Functions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.FunctionStores;
using DeltaShell.Plugins.FMSuite.Common.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
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
        private static readonly ConditionalWeakTable<WaterFlowFMModel, FMSnappedFeaturesGroupLayerData> snappedGroupLayerDataMapping =
            new ConditionalWeakTable<WaterFlowFMModel, FMSnappedFeaturesGroupLayerData>();

        private static readonly ConditionalWeakTable<WaterFlowFMModel, FMOutputSnappedFeaturesGroupLayerData> outputSnappedGroupLayerDataMapping =
            new ConditionalWeakTable<WaterFlowFMModel, FMOutputSnappedFeaturesGroupLayerData>();


        private static readonly ILog Log = LogManager.GetLogger(typeof(FlowFMMapLayerProvider));

        private static readonly string ModelName = typeof (WaterFlowFMModel).Name;

        public const string BoundariesLayerName = "Boundaries";
        public const string BoundaryConditionsLayerName = "Boundary Conditions";
        public const string SourcesAndSinksLayerName = "Sources and Sinks";
        public const string OutputSnappedFeaturesLayerName = "Output Snapped features";
        public const string GridSnappedFeaturesLayerName = "Estimated Grid-snapped features";

        /// <summary>
        /// Creates a maplayer. 
        /// </summary>
        /// <param name="data">The data object for which the layer is created.</param>
        /// <param name="parent">The parent object.</param>
        /// <returns>The layer that is created for the data object.</returns>
        public ILayer CreateLayer(object data, object parent)
        {
            var waterFlowFmModel = data as WaterFlowFMModel;
            if (waterFlowFmModel != null)
            {
                return new ModelGroupLayer { Name = waterFlowFmModel.Name, Model = waterFlowFmModel, NameIsReadOnly = true};
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

            var feature2Ds = data as IEventedList<Feature2D>;
            if (feature2Ds != null && parent is WaterFlowFMModel)
            {
                var fmModel = (WaterFlowFMModel) parent;
                if (Equals(feature2Ds, fmModel.Boundaries))
                {
                    return new VectorLayer(BoundariesLayerName)
                        {
                            DataSource =
                                new Feature2DCollection().Init(feature2Ds, "Boundary", ModelName, fmModel.CoordinateSystem),
                            FeatureEditor =
                                new Boundary2DEditor(fmModel)
                                    {
                                        AllowRemovePoint = new RemoveBoundaryPointDialog(fmModel).ShowDialogForFeature
                                    },
                            Style = AreaLayerStyles.BoundariesStyle,
                            NameIsReadOnly = true
                        };
                }
                if (Equals(feature2Ds, fmModel.Pipes))
                {
                    return new VectorLayer(SourcesAndSinksLayerName)
                        {
                            DataSource =
                                new Feature2DCollection().Init(feature2Ds, "SourceSink", ModelName, fmModel.CoordinateSystem),
                            FeatureEditor =
                                new Feature2DEditor(fmModel),
                            Style = AreaLayerStyles.SourcesAndSinksStyle,
                            NameIsReadOnly = true,
                            CustomRenderers =
                                new[] {new ArrowLineStringAdornerRenderer {Orientation = Orientation.Forward, Opacity = 1}}
                        };
                }
            }

            var allBoundaryConditionSets = data as IEventedList<BoundaryConditionSet>;
            if (allBoundaryConditionSets != null && parent is WaterFlowFMModel)
            {
                var fmModel = (WaterFlowFMModel) parent;
                var theme = CreateBoundaryConditionsTheme();
                return new VectorLayer(BoundaryConditionsLayerName)
                    {
                        DataSource = new Feature2DCollection().Init(allBoundaryConditionSets, "BoundaryCondition", ModelName, fmModel.CoordinateSystem),
                        Theme = theme,
                        Style = (VectorStyle) theme.DefaultStyle,
                        NameIsReadOnly = true,
                        ShowInTreeView = true,
                        ShowInLegend = false,
                        Selectable = false
                    };
            }

            if (data is FMMapFileFunctionStore)
            {
                var groupLayer = new GroupLayer("Output (map)")
                    {
                        LayersReadOnly = true,
                        NameIsReadOnly = true
                    };
                groupLayer.Layers.CollectionChanged += MapGroupLayerLayersCollectionChanged;
                return groupLayer;  
            }

            if (data is FMHisFileFunctionStore)
            {
                return new GroupLayer("Output (his)")
                    {
                        LayersReadOnly = true,
                        NameIsReadOnly = true
                    };
            }

            if (data is FMClassMapFileFunctionStore)
            {
                var groupLayer = new GroupLayer("Output (class)")
                {
                    LayersReadOnly = true,
                    NameIsReadOnly = true
                };
                return groupLayer;
            }

            var outputSnappedGroupLayerData = data as FMOutputSnappedFeaturesGroupLayerData;
            if (outputSnappedGroupLayerData != null)
            {
                var groupLayer = new GroupLayer(OutputSnappedFeaturesLayerName) { Visible = false, NameIsReadOnly = true };

                groupLayer.Layers.AddRange(outputSnappedGroupLayerData.CreateLayers());
                groupLayer.LayersReadOnly = true;
                return groupLayer;
            }

            var snappedGroupLayerData = data as FMSnappedFeaturesGroupLayerData;
            if (snappedGroupLayerData != null)
            {
                var groupLayer = new GroupLayer(GridSnappedFeaturesLayerName) {Visible = false, NameIsReadOnly = true};
                foreach (var snappedFeatures in snappedGroupLayerData.ChildData)
                {
                    var layer = new VectorLayer(snappedFeatures.LayerName)
                        {
                            Style = snappedFeatures.SnappedLayerStyle,
                            DataSource = snappedFeatures,
                            Selectable = false,
                            NameIsReadOnly = true,
                        };
                    groupLayer.Layers.Add(layer);
                    snappedFeatures.Layer = layer;
                }
                groupLayer.LayersReadOnly = true;
                return groupLayer;
            }

            var grouping = data as IGrouping<string, IFunction>;
            if (grouping != null)
            {
                var functions = grouping.ToList();
                if (functions.Any())
                {
                    var groupLayerName = GetCommonFunctionName(functions);
                    return new GroupLayer(string.IsNullOrEmpty(groupLayerName) ? grouping.Key : groupLayerName);
                }
            }

            return null;
        }

        private static string GetCommonFunctionName(IList<IFunction> functions)
        {
            if (!functions.Any()) return string.Empty;
            var commonFunctionName = functions[0].Name.ToCharArray();

            for (var i = 1; i < functions.Count; i++)
            {
                var functionName = functions[i].Name.ToCharArray();
                var commonCharacters = new List<char>();
                for (int j = 0; j < Math.Min(commonFunctionName.Length, functionName.Length); j++)
                {
                    if (commonFunctionName[j] == functionName[j]) commonCharacters.Add(commonFunctionName[j]);
                }

                commonFunctionName = new string(commonCharacters.ToArray()).Replace("()", string.Empty).ToCharArray();
            }
            return new string(commonFunctionName).Trim();
        }

        private void MapGroupLayerLayersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var layer = e.GetRemovedOrAddedItem() as UnstructuredGridLayer;
            if (layer == null || e.Action != NotifyCollectionChangedAction.Add) return;

            layer.GridColor = Color.Gray;
        }

        /// <summary>
        /// Determines whether this instance can create a layer for the specified data object.
        /// </summary>
        /// <param name="data">The data object for which will be determined whether a layer can be created.</param>
        /// <param name="parentObject">The parent object.</param>
        /// <returns>
        ///   <c>true</c> if this instance [can create layer for] the specified data; otherwise, <c>false</c>.
        /// </returns>
        public bool CanCreateLayerFor(object data, object parentObject)
        {
            return data is WaterFlowFMModel
                   || data is IGrouping<string, IFunction>
                   || data is FMMapFileFunctionStore
                   || data is FMHisFileFunctionStore
                   || data is FMClassMapFileFunctionStore
                   || data is ImportedFMNetFile
                   || (data is IEventedList<BoundaryConditionSet> && parentObject is WaterFlowFMModel)
                   || data is FMSnappedFeaturesGroupLayerData
                   || data is FMOutputSnappedFeaturesGroupLayerData
                   || data is CoverageDepthLayersList
                   || data is IEventedList<Feature2D>;  // Boundaries and sources&sinks
        }

        /// <summary>
        /// Child objects for <paramref name="data" />. Objects will be used to create child layers
        /// for the group layer (<paramref name="data" />)
        /// </summary>
        /// <param name="data">Group layer data</param>
        /// <returns>
        /// Child objects for <paramref name="data" />
        /// </returns>
        public IEnumerable<object> ChildLayerObjects(object data)
        {
            var model = data as WaterFlowFMModel;
            if (model != null)
            {
                var rootModel = GetRootModel(model);
                if (rootModel == null || rootModel is WaterFlowFMModel || model.GetDataItemByValue(model.Area).LinkedTo == null)
                {
                    if( model.Area.Enclosures.Count > 0 )
                    {
                        foreach( var enclosure in model.Area.Enclosures)
                        {
                            var geoAsPol = enclosure.Geometry as Polygon;
                            if( geoAsPol == null || !geoAsPol.IsValid)
                            {
                                Log.WarnFormat(Resources.WaterFlowFMEnclosureValidator_Validate_Drawn_polygon_not__0__not_valid, enclosure.Name);
                            }
                        }
                    }
                    yield return model.Area;
                }

                yield return model.BoundaryConditionSets;
                yield return model.Boundaries;
                yield return model.Pipes;

                FMSnappedFeaturesGroupLayerData layerData;
                if (!snappedGroupLayerDataMapping.TryGetValue(model, out layerData))
                {
                    layerData = new FMSnappedFeaturesGroupLayerData(model);
                    snappedGroupLayerDataMapping.Add(model, layerData);
                }
                yield return layerData;

                if (model.WriteSnappedFeatures && Directory.Exists(model.OutputSnappedFeaturesPath))
                {
                    FMOutputSnappedFeaturesGroupLayerData outputLayerData;
                    if (!outputSnappedGroupLayerDataMapping.TryGetValue(model, out outputLayerData)
                        || model.Status == ActivityStatus.Finished)
                    {
                        outputLayerData = new FMOutputSnappedFeaturesGroupLayerData(model);
                        //Clear model
                        outputSnappedGroupLayerDataMapping.Remove(model);
                        outputSnappedGroupLayerDataMapping.Add(model, outputLayerData);
                    }

                    outputLayerData.coordinateSystem = model.CoordinateSystem;

                    yield return outputLayerData;
                }
                yield return model.Grid;

                yield return model.InitialWaterLevel;
                yield return model.Roughness;
                yield return model.Viscosity;
                yield return model.Diffusivity;

                if (model.HeatFluxModelType != HeatFluxModelType.None)
                {
                    yield return model.InitialTemperature;
                }
                if (model.UseSalinity)
                {
                    foreach (var coverage in model.InitialSalinity.Coverages)
                    {
                        yield return coverage;
                    }
                }
                foreach (var tracer in model.InitialTracers)
                {
                    yield return tracer;
                }
                if (model.UseMorSed)
                {
                    foreach (var fraction in model.InitialFractions)
                    {
                        yield return fraction;
                    }
                }
                yield return model.Bathymetry;

                if (model.OutputMapFileStore != null)
                    yield return model.OutputMapFileStore;
                
                if (model.OutputHisFileStore != null)
                    yield return model.OutputHisFileStore;

                if (model.OutputClassMapFileStore != null)
                    yield return model.OutputClassMapFileStore;
            }

            var coverageDepthLayersList = data as CoverageDepthLayersList;
            if (coverageDepthLayersList != null)
            {
                foreach (var coverage in coverageDepthLayersList.Coverages)
                {
                    yield return coverage;
                }
            }

            var outputStore = data as FMNetCdfFileFunctionStore;
            if (outputStore != null)
            {
                if (outputStore is FMMapFileFunctionStore fmMapFileFunctionStore)
                {
                    foreach (var output in GetMapOutputFunctions(fmMapFileFunctionStore))
                        yield return output;
                }
                else if (outputStore is FMClassMapFileFunctionStore fmClassMapFileFunctionStore)
                {
                    yield return fmClassMapFileFunctionStore.Grid;

                    foreach (var output in outputStore.Functions)
                        yield return output;
                }
                else
                {
                    foreach (var output in outputStore.Functions)
                        yield return output;
                }
            }

            // groupings currently used by FMMapFileFunctionStore (for sedimentation outputs)
            var grouping = data as IGrouping<string, IFunction>;
            if (grouping != null)
            {
                foreach (var function in grouping)
                {
                    yield return function;
                }
            }
        }

        private static IEnumerable<object> GetMapOutputFunctions(FMMapFileFunctionStore mapStore)
        {
            yield return mapStore.Grid;

            var functionGrouping = mapStore.GetFunctionGrouping();
            foreach (IGrouping<string, IFunction> group in functionGrouping)
            {
                if (@group.Count() == 1)
                {
                    yield return @group.ElementAt(0);
                    continue;
                }

                yield return @group;
            }

            // Needs to be handled separately since it would be grouped with EastwardSeaWaterVelocityStandardName
            if (mapStore.CustomVelocityCoverage != null)
                yield return mapStore.CustomVelocityCoverage;
        }

        private static CategorialTheme CreateBoundaryConditionsTheme()
        {
            var theme = new CategorialTheme
            {
                AttributeName =
                    TypeUtils.GetMemberName<BoundaryConditionSet>(
                        bc => bc.VariableDescription),
                DefaultStyle = null,
                NoDataValues = new List<string> {""}
            };

            foreach (var dataType in new FlowBoundaryConditionEditorController().AllSupportedDataTypes)
            {
                foreach (FlowBoundaryQuantityType qt in Enum.GetValues(typeof(FlowBoundaryQuantityType)))
                {
                    var style = new VectorStyle
                        {
                            GeometryType = typeof (IPoint),
                            Symbol = BoundaryDataMapSymbols.GetSymbol(qt, dataType)
                        };

                    var boundaryConditionName = FlowBoundaryCondition.GetDescription(qt, dataType);

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

        private IModel GetRootModel(IModel model)
        {
            var rootModelForModel = GetRootModelRecursive(model);
            return rootModelForModel == model ? null : rootModelForModel;
        }

        private static IModel GetRootModelRecursive(IModel model)
        {
            var ownerModel = model.Owner as IModel;
            return ownerModel != null
                ? GetRootModelRecursive(ownerModel)
                : model;
        }
    }
}