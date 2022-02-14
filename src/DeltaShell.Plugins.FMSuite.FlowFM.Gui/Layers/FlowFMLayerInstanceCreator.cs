using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Drawing;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput2D.SnappedFeatures;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Providers;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using SharpMap.Api;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Data.Providers;
using SharpMap.Editors;
using SharpMap.Editors.Interactors;
using SharpMap.Layers;
using SharpMap.Rendering;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers
{
    /// <summary>
    /// <see cref="FlowFMLayerInstanceCreator"/> implements the <see cref="IFlowFMLayerInstanceCreator"/>
    /// and is used to create the actual layers of the Flow FM Gui plugin.
    /// </summary>
    /// <seealso cref="IFlowFMLayerInstanceCreator"/>
    public sealed class FlowFMLayerInstanceCreator : IFlowFMLayerInstanceCreator
    {
        private const string modelName = nameof(WaterFlowFMModel);

        public ILayer CreateModelGroupLayer(IWaterFlowFMModel model)
        {
            Ensure.NotNull(model, nameof(model));

            return new ModelGroupLayer
            {
                Name = model.Name,
                Model = model,
                LayersReadOnly = true,
                NameIsReadOnly = true,
            };
        }

        public ILayer Create1DGroupLayer() =>
            ReadOnlyGroupLayer(FlowFMLayerNames.GroupLayer1DName);

        public ILayer Create2DGroupLayer() =>
            ReadOnlyGroupLayer(FlowFMLayerNames.GroupLayer2DName);

        public ILayer CreateInputGroupLayer() =>
            ReadOnlyGroupLayer(FlowFMLayerNames.InputGroupLayerName);

        public ILayer CreateOutputGroupLayer() =>
            ReadOnlyGroupLayer(FlowFMLayerNames.OutputGroupLayerName);

        public ILayer CreateBoundariesLayer(IWaterFlowFMModel model)
        {
            Ensure.NotNull(model, nameof(model));

            CoordinateSystemAwareFeature2DCollection<IWaterFlowFMModel> dataSource = 
                new CoordinateSystemAwareFeature2DCollection<IWaterFlowFMModel>()
                    .Init(model.Boundaries,
                          "Boundary",
                          model,
                          nameof(WaterFlowFMModel));
            IFeatureEditor featureEditor =
                new Boundary2DEditor(model) { AllowRemovePoint = new RemoveBoundaryPointDialog(model).ShowDialogForFeature };

            return new VectorLayer(FlowFMLayerNames.BoundariesLayerName)
            { 
                DataSource = dataSource,
                FeatureEditor = featureEditor,
                Style = AreaLayerStyles.BoundariesStyle,
                NameIsReadOnly = true
            };
        }

        public ILayer CreatePipesLayer(IWaterFlowFMModel model)
        {
            Ensure.NotNull(model, nameof(model));

            CoordinateSystemAwareFeature2DCollection<IWaterFlowFMModel> dataSource =
                new CoordinateSystemAwareFeature2DCollection<IWaterFlowFMModel>().Init(
                    model.Pipes,
                    "SourceSink",
                    model, 
                    nameof(WaterFlowFMModel));

            IFeatureRenderer[] featureRenderer =
                {
                    new ArrowLineStringAdornerRenderer
                    {
                        Orientation = Orientation.Forward,
                        Opacity = 1
                    }
                };

            return new VectorLayer(FlowFMLayerNames.SourcesAndSinksLayerName)
            {
                DataSource = dataSource,
                FeatureEditor = new Feature2DEditor(model),
                Style = AreaLayerStyles.SourcesAndSinksStyle,
                NameIsReadOnly = true,
                CustomRenderers = featureRenderer
            };
        }

        public ILayer CreateLinks1D2DLayer(IWaterFlowFMModel model)
        {
            Ensure.NotNull(model, nameof(model));
            
            CoordinateSystemAwareFeature2DCollection<IWaterFlowFMModel> dataSource =
                new CoordinateSystemAwareFeature2DCollection<IWaterFlowFMModel>().Init(
                    model.Links, 
                    "1d2dLink", 
                    model,
                    nameof(WaterFlowFMModel));

            var featureEditor = new Feature2DEditor(model);

            return CreateLinks1D2DLayer(dataSource, featureEditor);
        }

        public ILayer CreateLinks1D2DLayer(IList<ILink1D2D> data, ICoordinateSystem coordinateSystem)
        {
            Ensure.NotNull(data, nameof(data));

            Feature2DCollection dataSource =
                new Feature2DCollection().Init(new EventedList<ILink1D2D>(data), 
                                               "1d2dLink", 
                                               modelName, 
                                               coordinateSystem);

            return CreateLinks1D2DLayer(dataSource);
        }

        private static ILayer CreateLinks1D2DLayer(IFeatureProvider dataSource,
                                                   IFeatureEditor featureEditor = null)
        {
            CategorialTheme theme = Create1D2DLinksTheme();

            var layer = new VectorLayer(FlowFMLayerNames.Links1D2DLayerName)
            {
                DataSource = dataSource,
                CanBeRemovedByUser = featureEditor != null,
                SmoothingMode = SmoothingMode.AntiAlias,
                Opacity = 0.7f,
                Theme = theme,
                Style = (VectorStyle)theme.DefaultStyle,
                Selectable = true,
                NameIsReadOnly = true
            };

            if (!(featureEditor is null))
            {
                layer.FeatureEditor = featureEditor;
            }

            return layer;
        }

        public ILayer CreateBoundaryConditionSetsLayer(IWaterFlowFMModel model)
        {
            Ensure.NotNull(model, nameof(model));

            CategorialTheme theme = CreateBoundaryConditionsTheme();
            CoordinateSystemAwareFeature2DCollection<IWaterFlowFMModel> dataSource =
                new CoordinateSystemAwareFeature2DCollection<IWaterFlowFMModel>().Init(
                    model.BoundaryConditionSets,
                    "BoundaryCondition",
                    model,
                    nameof(WaterFlowFMModel));

            return new VectorLayer(FlowFMLayerNames.BoundaryConditionsLayerName)
            {
                DataSource = dataSource,
                Theme = theme,
                Style = (VectorStyle)theme.DefaultStyle,
                NameIsReadOnly = true,
                ShowInTreeView = true,
                ShowInLegend = false,
                Selectable = false
            };
        }

        public ILayer CreateBoundaryNodeDataLayer(IWaterFlowFMModel model)
        {
            Ensure.NotNull(model, nameof(model));

            CategorialTheme theme = CreateBoundaryNodeTheme();
            var dataSource = new ComplexFeatureCollection(model.Network, 
                                                          (IList) model.BoundaryConditions1D, 
                                                          typeof(Model1DBoundaryNodeData));

            return new VectorLayer(FlowFMLayerNames.BoundaryData1DLayerName)
            {
                Visible = false,
                Selectable = true,
                NameIsReadOnly = true,
                DataSource = dataSource,
                Theme =  theme
            };
        }

        public ILayer CreateLateralDataLayer(IWaterFlowFMModel model)
        {
            Ensure.NotNull(model, nameof(model));

            CategorialTheme theme = CreateLateralDataTheme();
            var dataSource = new ComplexFeatureCollection(model.Network,
                                                          (IList) model.LateralSourcesData,
                                                          typeof(Model1DLateralSourceData));

            return new VectorLayer(FlowFMLayerNames.LateralData1DLayerName)
            {
                Visible = false,
                Selectable = true,
                NameIsReadOnly = true,
                DataSource = dataSource,
                Theme = theme

            };
        }

        public ILayer CreateMapFileFunctionStore1DLayer() =>
            CreateFunctionStoreGroupLayer(FlowFMLayerNames.MapFile1DGroupLayerName);

        public ILayer CreateMapFileFunctionStore2DLayer() =>
            CreateFunctionStoreGroupLayer(FlowFMLayerNames.MapFile2DGroupLayerName);

        public ILayer CreateHisFileFunctionStoreLayer() =>
            CreateFunctionStoreGroupLayer(FlowFMLayerNames.HistoryFileGroupLayerName);

        public ILayer CreateClassMapFileFunctionStoreLayer() =>
            CreateFunctionStoreGroupLayer(FlowFMLayerNames.ClassMapFileGroupLayerName);

        public ILayer CreateFouFileFunctionStoreLayer() =>
            ReadOnlyGroupLayer(FlowFMLayerNames.FouFileGroupLayerName);

        private static ILayer CreateFunctionStoreGroupLayer(string layerName)
        {
            var groupLayer = ReadOnlyGroupLayer(layerName);
            groupLayer.Layers.CollectionChanged += MapGroupLayerLayersCollectionChanged;
            return groupLayer;
        }

        public ILayer CreateFunctionGroupingLayer(IGrouping<string, IFunction> grouping)
        {
            IList<IFunction> functions = grouping.ToList();
            string commonFunctionName = GetCommonFunctionName(functions);
            string groupLayerName = !string.IsNullOrEmpty(commonFunctionName)
                                        ? commonFunctionName
                                        : grouping.Key;
            return ReadOnlyGroupLayer(groupLayerName);
        }

        public ILayer CreateImportedFMNetFileLayer(ImportedFMNetFile netFile) => 
            new UnstructuredGridLayer {
                Grid = netFile.Grid,
                NameIsReadOnly = true,
                FeatureEditor = new FeatureEditor()
            };

        public ILayer CreateLeveeBreachWidthCoverageLayer(FeatureCoverage coverage)
        {
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


        public ILayer CreateFrictionGroupLayer() =>
            ReadOnlyGroupLayer(FlowFMLayerNames.Friction1DGroupLayerName);

        public ILayer CreateInitialConditionsGroupLayer() =>
            ReadOnlyGroupLayer(FlowFMLayerNames.InitialConditions1DGroupLayerName);

        public ILayer CreateDefinitionsLayer<TDefinition>(string layerName,
                                                          IEventedList<TDefinition> definitions,
                                                          IHydroNetwork network) where TDefinition : IFeature => 
            new VectorLayer(layerName)
            {
                Visible = false,
                Selectable = true,
                NameIsReadOnly = true,
                CanBeRemovedByUser = false,
                DataSource = new ComplexFeatureCollection(network, (IList)definitions, typeof(TDefinition))
            };

        public ILayer CreateOutputSnappedFeatureGroupLayer() =>
            new GroupLayer(FlowFMLayerNames.OutputSnappedFeaturesLayerName)
            {
                Visible = false, 
                NameIsReadOnly = true
            };

        public ILayer CreateOutputSnappedFeatureLayer(string layerName,
                                                      string featureDataPath,
                                                      IWaterFlowFMModel model)
        {
            var shapeFile = new ShapeFile(featureDataPath, false);
            ILayer layer = SharpMapLayerFactory.CreateLayer(shapeFile);
            layer.Name = layerName;
            layer.DataSource.CoordinateSystem = model.CoordinateSystem;

            return layer;
        }
        public ILayer CreateEstimatedSnappedFeatureGroupLayer() =>
            ReadOnlyGroupLayer(FlowFMLayerNames.EstimatedSnappedFeaturesLayerName, visible:false);

        private static GroupLayer ReadOnlyGroupLayer(string layerName, bool visible=true) =>
            new GroupLayer(layerName)
            {
                NameIsReadOnly = true,
                LayersReadOnly = true,
                Visible = visible,
            };

        public ILayer CreateEstimatedSnappedFeatureLayer(IWaterFlowFMModel model,
                                                         EstimatedSnappedFeatureType featureType)
        {
            switch(featureType)
            {
                case EstimatedSnappedFeatureType.ObservationPoints:
                    return CreateEstimatedSnappedFeatureVectorLayer(
                        CreateSnappedFeatureCollection(model,
                                                       (IList)model.Area.ObservationPoints,
                                                       AreaLayerStyles.ObservationPointStyle,
                                                       FlowFMLayerNames.EstimatedSnappedObservationPoints,
                                                       UnstrucGridOperationApi.ObsPoint)
                    );
                case EstimatedSnappedFeatureType.ThinDams:
                    return CreateEstimatedSnappedFeatureVectorLayer(
                        CreateSnappedFeatureCollection(model, 
                                                       (IList)model.Area.ThinDams, 
                                                       AreaLayerStyles.ThinDamStyle, 
                                                       FlowFMLayerNames.EstimatedSnappedThinDams,
                                                       UnstrucGridOperationApi.ThinDams)
                    );
                case EstimatedSnappedFeatureType.FixedWeirs:
                    return CreateEstimatedSnappedFeatureVectorLayer(
                        CreateSnappedFeatureCollection(model, 
                                                       (IList)model.Area.FixedWeirs,
                                                       AreaLayerStyles.FixedWeirStyle, 
                                                       FlowFMLayerNames.EstimatedSnappedFixedWeirs,
                                                       UnstrucGridOperationApi.FixedWeir)
                    );
                case EstimatedSnappedFeatureType.LeveeBreaches:
                    VectorLayer layer = CreateEstimatedSnappedFeatureVectorLayer(
                        new SnappedLeveeBreachCollection(model, 
                                                         model.Area, 
                                                         (IList)model.Area.LeveeBreaches,
                                                         AreaLayerStyles.LeveeStyle, 
                                                         FlowFMLayerNames.EstimatedSnappedLeveeBreaches,
                                                         UnstrucGridOperationApi.LeveeBreach));

                    layer.Style = null;
                    layer.CustomRenderers = new List<IFeatureRenderer>(new[]
                    {
                        new LeveeBreachRenderer(AreaLayerStyles.LeveeSnappedStyle, 
                                                AreaLayerStyles.BreachSnappedStyle, 
                                                AreaLayerStyles.WaterLevelStreamSnappedStyle)
                    });
                    layer.Visible = false;
                    return layer;
                case EstimatedSnappedFeatureType.RoofAreas:
                    return CreateEstimatedSnappedFeatureVectorLayer(
                        CreateSnappedFeatureCollection(model, 
                                                       (IList)model.Area.RoofAreas, 
                                                       AreaLayerStyles.RoofAreaStyle, 
                                                       FlowFMLayerNames.EstimatedSnappedRoofAreas,
                                                       UnstrucGridOperationApi.RoofArea)
                    );
                case EstimatedSnappedFeatureType.DryPoints:
                    return CreateEstimatedSnappedFeatureVectorLayer(
                        CreateSnappedFeatureCollection(model, 
                                                       (IList)model.Area.DryPoints, 
                                                       AreaLayerStyles.DryPointStyle, 
                                                       FlowFMLayerNames.EstimatedSnappedDryPoints,
                                                       UnstrucGridOperationApi.ObsPoint)
                    );
                case EstimatedSnappedFeatureType.DryAreas:
                    return CreateEstimatedSnappedFeatureVectorLayer(
                        CreateSnappedFeatureCollection(model, 
                                                       (IList)model.Area.DryAreas, 
                                                       AreaLayerStyles.DryAreaStyle, 
                                                       FlowFMLayerNames.EstimatedSnappedDryAreas,
                                                       UnstrucGridOperationApi.ObsCrossSection)
                    );
                case EstimatedSnappedFeatureType.Enclosures:
                    return CreateEstimatedSnappedFeatureVectorLayer(
                        CreateSnappedFeatureCollection(model, 
                                                       (IList)model.Area.Enclosures, 
                                                       AreaLayerStyles.EnclosureStyle, 
                                                       FlowFMLayerNames.EstimatedSnappedEnclosures,
                                                       UnstrucGridOperationApi.ObsCrossSection)
                    );
                case EstimatedSnappedFeatureType.Pumps:
                    return CreateEstimatedSnappedFeatureVectorLayer(
                        CreateSnappedFeatureCollection(model, 
                                                       (IList)model.Area.Pumps, 
                                                       AreaLayerStyles.PumpStyle, 
                                                       FlowFMLayerNames.EstimatedSnappedPumps,
                                                       UnstrucGridOperationApi.Pump)
                    );
                case EstimatedSnappedFeatureType.Weirs:
                    return CreateEstimatedSnappedFeatureVectorLayer(
                        CreateSnappedFeatureCollection(model, 
                                                       (IList)model.Area.Weirs, 
                                                       AreaLayerStyles.WeirStyle, 
                                                       FlowFMLayerNames.EstimatedSnappedWeirs,
                                                       UnstrucGridOperationApi.Weir)
                    );
                case EstimatedSnappedFeatureType.Gates:
                    return CreateEstimatedSnappedFeatureVectorLayer(
                        CreateSnappedFeatureCollection(model, 
                                                       (IList)model.Area.Gates, 
                                                       AreaLayerStyles.GateStyle, 
                                                       FlowFMLayerNames.EstimatedSnappedGates,
                                                       UnstrucGridOperationApi.Gate)
                    );
                case EstimatedSnappedFeatureType.ObservationCrossSections:
                    return CreateEstimatedSnappedFeatureVectorLayer(
                        CreateSnappedFeatureCollection(model, 
                                                       (IList)model.Area.ObservationCrossSections, 
                                                       AreaLayerStyles.ObsCrossSectionStyle, 
                                                       FlowFMLayerNames.EstimatedSnappedObservationCrossSections,
                                                       UnstrucGridOperationApi.ObsCrossSection)
                    );
                case EstimatedSnappedFeatureType.Embankments: 
                    return CreateEstimatedSnappedFeatureVectorLayer(
                        CreateSnappedFeatureCollection(model, 
                                                       (IList)model.Area.Embankments, 
                                                       AreaLayerStyles.EmbankmentStyle, 
                                                       FlowFMLayerNames.EstimatedSnappedEmbankments,
                                                       UnstrucGridOperationApi.Embankment)
                    );
                case EstimatedSnappedFeatureType.SourcesAndSinks:
                    return CreateEstimatedSnappedFeatureVectorLayer(
                        CreateSnappedFeatureCollection(model, 
                                                       (IList)model.SourcesAndSinks, 
                                                       AreaLayerStyles.SnappedSourcesAndSinksStyle, 
                                                       FlowFMLayerNames.EstimatedSnappedSourcesAndSinks,
                                                       UnstrucGridOperationApi.SourceSink)
                    );
                case EstimatedSnappedFeatureType.Boundaries:
                    return CreateEstimatedSnappedFeatureVectorLayer(
                        CreateSnappedFeatureCollection(model, 
                                                       (IList)model.Boundaries, 
                                                       AreaLayerStyles.BoundariesStyle, 
                                                       FlowFMLayerNames.EstimatedSnappedBoundaries,
                                                       UnstrucGridOperationApi.Boundary)
                    );
                case EstimatedSnappedFeatureType.BoundariesWaterLevel:
                    return CreateEstimatedSnappedFeatureVectorLayer(
                        CreateSnappedFeatureCollection(model, 
                                                       (IList)model.Boundaries, 
                                                       AreaLayerStyles.BoundariesWaterLevelPointsStyle, 
                                                       FlowFMLayerNames.EstimatedSnappedBoundariesWaterLevel,
                                                       UnstrucGridOperationApi.WaterLevelBnd)
                    );
                case EstimatedSnappedFeatureType.BoundariesVelocity:
                    return CreateEstimatedSnappedFeatureVectorLayer(
                        CreateSnappedFeatureCollection(model, 
                                                       (IList)model.Boundaries, 
                                                       AreaLayerStyles.BoundariesVelocityPointsStyle,
                                                       FlowFMLayerNames.EstimatedSnappedBoundariesVelocity,
                                                       UnstrucGridOperationApi.VelocityBnd)
                    );
                default:
                    throw new ArgumentOutOfRangeException(nameof(featureType), featureType, null);
            }
        }

        private static SnappedFeatureCollection CreateSnappedFeatureCollection(IWaterFlowFMModel model,
                                                                               IList features,
                                                                               VectorStyle style,
                                                                               string layerName,
                                                                               string snapApiFeatureType) =>
            new SnappedFeatureCollection(model, model.Area, features, style, layerName, snapApiFeatureType);

        private static VectorLayer CreateEstimatedSnappedFeatureVectorLayer(SnappedFeatureCollection featureCollection)
        {
            var layer = new VectorLayer(featureCollection.LayerName)
            {
                Style = featureCollection.SnappedLayerStyle,
                DataSource = featureCollection,
                Selectable = false,
                NameIsReadOnly = true,
            };

            featureCollection.Layer = layer;
            return layer;
        }

        private static string GetCommonFunctionName(IList<IFunction> functions)
        {
            if (!functions.Any()) return string.Empty;
            char[] commonFunctionName = functions[0].Name.ToCharArray();

            for (var i = 1; i < functions.Count; i++)
            {
                char[] functionName = functions[i].Name.ToCharArray();
                var commonCharacters = new List<char>();
                for (int j = 0; j < Math.Min(commonFunctionName.Length, functionName.Length); j++)
                {
                    if (commonFunctionName[j] == functionName[j]) commonCharacters.Add(commonFunctionName[j]);
                }

                commonFunctionName = new string(commonCharacters.ToArray()).Replace("()", string.Empty).ToCharArray();
            }
            return new string(commonFunctionName).Trim();
        }

        private static CategorialTheme Create1D2DLinksTheme()
        {
            const int lineWidth = 3;
            var linkEndCap = new AdjustableArrowCap(4, 4, true) { BaseCap = LineCap.Triangle };

            var embeddedName = LinkStorageType.Embedded.GetDescription();
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

            var gullySewerName = LinkStorageType.GullySewer.GetDescription();
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

            return new CategorialTheme
            {
                AttributeName = "TypeOfLink",
                DefaultStyle = embeddedStyle,
                ThemeItems = new EventedList<IThemeItem>
                {
                    new CategorialThemeItem(embeddedName, embeddedStyle, null, LinkStorageType.Embedded),
                    new CategorialThemeItem(gullySewerName, gullyWaterStyle, null, LinkStorageType.GullySewer),
                }
            };
        }

        private static CategorialTheme CreateBoundaryConditionsTheme()
        {
            var theme = new CategorialTheme
            {
                AttributeName =
                    nameof(
                        BoundaryConditionSet.VariableDescription),
                DefaultStyle = null,
                NoDataValues = new List<string> {""}
            };

            foreach (BoundaryConditionDataType dataType in new FlowBoundaryConditionEditorController().AllSupportedDataTypes)
            {
                foreach (FlowBoundaryQuantityType qt in Enum.GetValues(typeof(FlowBoundaryQuantityType)))
                {
                    var style = new VectorStyle
                        {
                            GeometryType = typeof (IPoint),
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

        private static CategorialTheme CreateBoundaryNodeTheme() =>
            new CategorialTheme
            {
                AttributeName = nameof(Model1DBoundaryNodeData.DataType),
                DefaultStyle = new VectorStyle
                {
                    GeometryType = typeof(IPoint),
                    Fill = new SolidBrush(Color.Transparent),
                    EnableOutline = false
                },
                NoDataValues = new List<string> { "" },
                ThemeItems = new EventedList<IThemeItem>
                {
                    CreateCategorialThemeItem(Model1DBoundaryNodeDataType.WaterLevelConstant, Properties.Resources.HConst),
                    CreateCategorialThemeItem(Model1DBoundaryNodeDataType.WaterLevelTimeSeries, Properties.Resources.HBoundary),
                    CreateCategorialThemeItem(Model1DBoundaryNodeDataType.FlowConstant, Properties.Resources.QConst),
                    CreateCategorialThemeItem(Model1DBoundaryNodeDataType.FlowTimeSeries, Properties.Resources.QBoundary),
                    CreateCategorialThemeItem(Model1DBoundaryNodeDataType.FlowWaterLevelTable, Properties.Resources.QHBoundary)
                }
            };

        private static CategorialTheme CreateLateralDataTheme() => 
            new CategorialTheme 
            {
                AttributeName = nameof(Model1DLateralSourceData.DataType),
                DefaultStyle = new VectorStyle(),
                NoDataValues = new List<string> { "" },
                ThemeItems = new EventedList<IThemeItem>
                {
                    CreateCategorialThemeItem(Model1DLateralDataType.FlowConstant, Properties.Resources.QConst),
                    CreateCategorialThemeItem(Model1DLateralDataType.FlowTimeSeries, Properties.Resources.QBoundary),
                    CreateCategorialThemeItem(Model1DLateralDataType.FlowWaterLevelTable, Properties.Resources.QHBoundary),
                    CreateCategorialThemeItem(Model1DLateralDataType.FlowRealTime, Properties.Resources.realtime)
                }
            };

        private static CategorialThemeItem CreateCategorialThemeItem<T>(T enumValue, Image overlayImage) where T : struct, IConvertible
        {
            var value = (Enum)Enum.Parse(typeof(T), enumValue.ToString());

            return new CategorialThemeItem
            {
                Value = value,
                Label = value.GetDescription(),
                Style = new VectorStyle
                {
                    Symbol = new Bitmap(Properties.Resources.Boundary_1d.AddOverlayImage(overlayImage, 1, 1))
                }
            };
        }

        private static void MapGroupLayerLayersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add ||
                !(e.GetRemovedOrAddedItem() is ILayer layer))
            {
                return;
            }

            if (layer is UnstructuredGridLayer unstructuredGridLayer)
            {
                AdjustUnstructuredGridLayer(unstructuredGridLayer);
            }

            if (layer is INetworkCoverageGroupLayer networkCoverageGroupLayer)
            {
                AdjustNetworkCoverageGroupLayer(networkCoverageGroupLayer);
            }
        }

        private static void AdjustUnstructuredGridLayer(UnstructuredGridLayer layer) =>
            layer.GridColor = Color.Gray;

        private static void AdjustNetworkCoverageGroupLayer(INetworkCoverageGroupLayer layer)
        {
            if (layer.SegmentLayer == null)
            {
                return;
            }

            IDictionary<string, string> attributes = ((IFunction) layer.NetworkCoverage).Attributes;
            NetworkDataLocation location = 
                attributes.ContainsKey(FM1DFileFunctionStore.LocationAttributeName) 
                    ? (NetworkDataLocation)Enum.Parse(typeof(NetworkDataLocation), attributes[FM1DFileFunctionStore.LocationAttributeName])
                    : NetworkDataLocation.UnKnown;

            if (location == NetworkDataLocation.Node)
            {
                // hide SegmentLayer
                layer.SegmentLayer.ShowInLegend = false;
                layer.SegmentLayer.ShowInTreeView = false;
            }

            layer.LocationLayer.Visible = location != NetworkDataLocation.Edge;
            layer.SegmentLayer.Visible = location == NetworkDataLocation.Edge;
        }
    }
}