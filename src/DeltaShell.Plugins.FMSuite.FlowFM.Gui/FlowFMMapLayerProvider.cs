using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.CoverageDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
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
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers;
using SharpMap.Api;

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
        public const string LayerName1D2DLinks = "1D/2D links";

        public ILayer CreateLayer(object data, object parent)
        {
            if (data is FlowFMTreeShortcut)
            {
                return CreateLayer(((FlowFMTreeShortcut) data).TargetData, parent);
            }

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

            var links = data as IEventedList<WaterFlowFM1D2DLink>;
            if (links != null && parent is WaterFlowFMModel)
            {
                var fmModel = (WaterFlowFMModel)parent;
                var theme = Create1D2DLinksTheme();

                return new VectorLayer(LayerName1D2DLinks)
                {
                    //DataSource = new WaterFlowFM1D2DLinkFeatureCollection(fmModel),
                    DataSource = new Feature2DCollection().Init(links, "1d2dLink", ModelName, fmModel.CoordinateSystem),
                    FeatureEditor = new Feature2DEditor(fmModel),
                    CanBeRemovedByUser = true,
                    SmoothingMode = SmoothingMode.AntiAlias,
                    Opacity = 0.7f,
                    Theme = theme,
                    Style = (VectorStyle)theme.DefaultStyle,
                    Selectable = true,
                    NameIsReadOnly = true,

                };
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
                    if (snappedFeatures.LayerName == FMSnappedFeaturesGroupLayerData.SNAPPED_LEVEE_BREACH_LAYER_NAME)
                    {
                        layer.Style = null;
                        layer.CustomRenderers = new List<IFeatureRenderer>(new[] { new LeveeBreachRenderer(AreaLayerStyles.LeveeSnappedStyle, AreaLayerStyles.BreachSnappedStyle)});
                    }
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

            var coverage = data as FileBasedFeatureCoverage;
            if (coverage != null && IsCoverageLeveeBreachWidth(coverage))
            {
                // Create feature coverage layer
                var featureCoverageLayer = new FeatureCoverageLayer(new LeveeBreachWidthRenderer())
                {
                    FeatureCoverage = coverage,
                    Name = coverage.Name,
                    NameIsReadOnly = !coverage.IsEditable,
                };
                
                if (coverage.CoordinateSystem != null)
                {
                    featureCoverageLayer.DataSource.CoordinateSystem = coverage.CoordinateSystem;
                }

                return featureCoverageLayer;
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

        private void MapGroupLayerLayersCollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            var layer = e.Item as UnstructuredGridLayer;
            if (layer == null || e.Action != NotifyCollectionChangeAction.Add) return;

            layer.GridColor = Color.Gray;
        }

        public bool CanCreateLayerFor(object data, object parentObject)
        {
            if (data is FlowFMTreeShortcut)
                return true;

            return data is WaterFlowFMModel
                // TODO Sil: add check if data is Featurecoverage with a certain name/type (find the breach width coverage)
                   || data is FileBasedFeatureCoverage && IsCoverageLeveeBreachWidth((FileBasedFeatureCoverage)data)
                   || data is IEventedList<WaterFlowFM1D2DLink> && parentObject is WaterFlowFMModel
                   || data is IGrouping<string, IFunction>
                   || data is FMMapFileFunctionStore
                   || data is FMHisFileFunctionStore
                   || data is ImportedFMNetFile
                   || data is IEventedList<BoundaryConditionSet> && parentObject is WaterFlowFMModel
                   || data is FMSnappedFeaturesGroupLayerData
                   || data is FMOutputSnappedFeaturesGroupLayerData
                   || data is CoverageDepthLayersList
                   || data is IEventedList<Feature2D>;  // Boundaries and sources&sinks
        }

        private bool IsCoverageLeveeBreachWidth(INameable data)
        {
            if (data == null) return false;

            return data.Name == "dambreak breach width (dambreak_breach_width)";
        }

        public IEnumerable<object> ChildLayerObjects(object data)
        {
            if (data is FlowFMTreeShortcut)
            {
                foreach (var item in ChildLayerObjects(((FlowFMTreeShortcut)data).TargetData))
                {
                    yield return item;
                }
                yield break;
            }
            
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
                yield return model.Network;
                yield return model.NetworkDiscretization;
                yield return model.Links;

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
                var mapStore = outputStore as FMMapFileFunctionStore;
                if (mapStore != null)
                {
                    foreach (var output in GetMapOutputFunctions(mapStore))
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

        private static CategorialTheme Create1D2DLinksTheme()
        {
            const int lineWidth = 3;
            var linkEndCap = new AdjustableArrowCap(4, 4, true) { BaseCap = LineCap.Triangle };

            var lateralName = EnumDescriptionAttributeTypeConverter.GetEnumDescription(GridApiDataSet.LinkType.Lateral);
            var lateralStyle = new VectorStyle
            {
                GeometryType = typeof(ILineString),
                Line = new Pen(Color.Violet, lineWidth)
                {
                    CustomEndCap = linkEndCap,
                    CustomStartCap = linkEndCap
                },
                EnableOutline = false
            };

            var embeddedName = EnumDescriptionAttributeTypeConverter.GetEnumDescription(GridApiDataSet.LinkType.Embedded);
            var embeddedStyle = new VectorStyle
            {
                GeometryType = typeof(ILineString),
                Line = new Pen(Color.ForestGreen, lineWidth)
                {
                    CustomEndCap = linkEndCap,
                    CustomStartCap = linkEndCap
                },
                EnableOutline = false
            };

            var roofSewerName = EnumDescriptionAttributeTypeConverter.GetEnumDescription(GridApiDataSet.LinkType.RoofSewer);
            var roofSewerStyle = new VectorStyle
            {
                GeometryType = typeof(ILineString),
                Line = new Pen(Color.RoyalBlue, lineWidth)
                {
                    CustomEndCap = linkEndCap,
                    CustomStartCap = linkEndCap
                },
                EnableOutline = false
            };

            var gullySewerName = EnumDescriptionAttributeTypeConverter.GetEnumDescription(GridApiDataSet.LinkType.GullySewer);
            var gullyWaterStyle = new VectorStyle
            {
                GeometryType = typeof(ILineString),
                Line = new Pen(Color.RoyalBlue, lineWidth)
                {
                    CustomEndCap = linkEndCap,
                    CustomStartCap = linkEndCap
                },
                EnableOutline = false
            };

            var inhabitantsSewerName = EnumDescriptionAttributeTypeConverter.GetEnumDescription(GridApiDataSet.LinkType.InhabitantsSewer);
            var inhabitantsSewerStyle = new VectorStyle
            {
                GeometryType = typeof(ILineString),
                Line = new Pen(Color.IndianRed, lineWidth)
                {
                    CustomEndCap = linkEndCap,
                    CustomStartCap = linkEndCap
                },
                EnableOutline = false
            };

            return new CategorialTheme
            {
                AttributeName = "Type of 1D2D link",
                DefaultStyle = embeddedStyle,
                ThemeItems = new EventedList<IThemeItem>
                {
                    new CategorialThemeItem(embeddedName, embeddedStyle, null, GridApiDataSet.LinkType.Embedded),
                    new CategorialThemeItem(lateralName, lateralStyle, null, GridApiDataSet.LinkType.Lateral),
                    new CategorialThemeItem(roofSewerName, roofSewerStyle, null, GridApiDataSet.LinkType.RoofSewer),
                    new CategorialThemeItem(gullySewerName, gullyWaterStyle, null, GridApiDataSet.LinkType.GullySewer),
                    new CategorialThemeItem(inhabitantsSewerName, inhabitantsSewerStyle, null, GridApiDataSet.LinkType.InhabitantsSewer),
                }
            };

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