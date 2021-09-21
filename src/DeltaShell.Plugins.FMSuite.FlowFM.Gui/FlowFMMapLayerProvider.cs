using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Drawing;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.NGHS.IO.FileWriters.Roughness;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.CoverageDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Providers;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using SharpMap.Api;
using SharpMap.Api.Layers;
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

        private static readonly ConditionalWeakTable<WaterFlowFMModel, FrictionGroupLayerData> frictionGroupLayerDataMapping =
            new ConditionalWeakTable<WaterFlowFMModel, FrictionGroupLayerData>();

        private static readonly ConditionalWeakTable<WaterFlowFMModel, InitialConditionGroupLayerData> initialConditionGroupLayerDataMapping =
            new ConditionalWeakTable<WaterFlowFMModel, InitialConditionGroupLayerData>();

        private static readonly ILog Log = LogManager.GetLogger(typeof(FlowFMMapLayerProvider));

        private static readonly string ModelName = typeof (WaterFlowFMModel).Name;

        public const string BoundariesLayerName = "Boundaries";
        public const string BoundaryConditionsLayerName = "Boundary Conditions";
        public const string SourcesAndSinksLayerName = "Sources and Sinks";
        public const string OutputSnappedFeaturesLayerName = "Output Snapped features";
        public const string GridSnappedFeaturesLayerName = "Estimated Grid-snapped features";
        public const string LayerName1D2DLinks = "1D/2D links";

        public ILayer CreateLayer(object data, object parentData)
        {
            switch (data)
            {
                case WaterFlowFMModel waterFlowFmModel:
                    return new ModelGroupLayer
                    {
                        Name = waterFlowFmModel.Name, 
                        Model = waterFlowFmModel, 
                        LayersReadOnly = true,
                        NameIsReadOnly = true
                    };
                case ImportedFMNetFile importedGridFile:
                    return new UnstructuredGridLayer
                    {
                        Grid = importedGridFile.Grid,
                        NameIsReadOnly = true,
                        FeatureEditor = new FeatureEditor()
                    };
            }

            if (parentData is WaterFlowFMModel fmModel)
            {
                switch (data)
                {
                    case IEventedList<Feature2D> feature2Ds when Equals(feature2Ds, fmModel.Boundaries):
                        return new VectorLayer(BoundariesLayerName)
                        {
                            DataSource = new WaterFlowFmModelFeature2DCollection().Init(feature2Ds, "Boundary", fmModel),
                            FeatureEditor = new Boundary2DEditor(fmModel)
                            {
                                AllowRemovePoint = new RemoveBoundaryPointDialog(fmModel).ShowDialogForFeature
                            },
                            Style = AreaLayerStyles.BoundariesStyle,
                            NameIsReadOnly = true
                        };
                    case IEventedList<Feature2D> feature2Ds when Equals(feature2Ds, fmModel.Pipes):
                        return new VectorLayer(SourcesAndSinksLayerName)
                        {
                            DataSource = new WaterFlowFmModelFeature2DCollection().Init(feature2Ds, "SourceSink", fmModel),
                            FeatureEditor = new Feature2DEditor(fmModel),
                            Style = AreaLayerStyles.SourcesAndSinksStyle,
                            NameIsReadOnly = true,
                            CustomRenderers =
                                new[] { new ArrowLineStringAdornerRenderer { Orientation = Orientation.Forward, Opacity = 1 } }
                        };
                    case IEventedList<ILink1D2D> links:
                    {
                        var theme = Create1D2DLinksTheme();

                        return new VectorLayer(LayerName1D2DLinks)
                        {
                            DataSource = new WaterFlowFmModelFeature2DCollection().Init(links, "1d2dLink", fmModel),
                            FeatureEditor = new Feature2DEditor(fmModel),
                            CanBeRemovedByUser = true,
                            SmoothingMode = SmoothingMode.AntiAlias,
                            Opacity = 0.7f,
                            Theme = theme,
                            Style = (VectorStyle)theme.DefaultStyle,
                            Selectable = true,
                            NameIsReadOnly = true
                        };
                    }
                    case IEventedList<BoundaryConditionSet> allBoundaryConditionSets:
                    {
                        var theme = CreateBoundaryConditionsTheme();
                        return new VectorLayer(BoundaryConditionsLayerName)
                        {
                            DataSource = new WaterFlowFmModelFeature2DCollection().Init(allBoundaryConditionSets, "BoundaryCondition", fmModel),
                            Theme = theme,
                            Style = (VectorStyle)theme.DefaultStyle,
                            NameIsReadOnly = true,
                            ShowInTreeView = true,
                            ShowInLegend = false,
                            Selectable = false
                        };
                    }
                    case IEventedList<Model1DBoundaryNodeData> boundaryNodeData:
                        return CreateBoundaryNodeDataLayer(boundaryNodeData, fmModel);
                    case IEventedList<Model1DLateralSourceData> lateralSourceData:
                        return CreateLateralDataLayer(lateralSourceData, fmModel);
                }
            }

            if (parentData is FMMapFileFunctionStore mapFileFunctionStore && data is IList<ILink1D2D> linksMapfile && linksMapfile.Any())
            {
                var coordinateSystem = mapFileFunctionStore.Grid.CoordinateSystem;
                var theme = Create1D2DLinksTheme();

                return new VectorLayer(LayerName1D2DLinks)
                {
                    DataSource = new WaterFlowFmModelFeature2DCollection().Init(new EventedList<ILink1D2D>(linksMapfile), "1d2dLink", ModelName, coordinateSystem),
                    CanBeRemovedByUser = false,
                    SmoothingMode = SmoothingMode.AntiAlias,
                    Opacity = 0.7f,
                    Theme = theme,
                    Style = (VectorStyle)theme.DefaultStyle,
                    Selectable = true,
                    NameIsReadOnly = true,
                };
            }
            if (parentData is FMClassMapFileFunctionStore classMapFileFunctionStore && data is IList<ILink1D2D> linksClassMapfile && linksClassMapfile.Any())
            {
                var coordinateSystem = classMapFileFunctionStore.Grid.CoordinateSystem;
                var theme = Create1D2DLinksTheme();

                return new VectorLayer(LayerName1D2DLinks)
                {
                    DataSource = new WaterFlowFmModelFeature2DCollection().Init(new EventedList<ILink1D2D>(linksClassMapfile), "1d2dLink", ModelName, coordinateSystem),
                    CanBeRemovedByUser = false,
                    SmoothingMode = SmoothingMode.AntiAlias,
                    Opacity = 0.7f,
                    Theme = theme,
                    Style = (VectorStyle)theme.DefaultStyle,
                    Selectable = true,
                    NameIsReadOnly = true,
                };
            }

            if (data is FMMapFileFunctionStore)
            {
                var groupLayer = new GroupLayer("Output 2D (map file)")
                    {
                        LayersReadOnly = true,
                        NameIsReadOnly = true
                    };
                groupLayer.Layers.CollectionChanged += MapGroupLayerLayersCollectionChanged;
                return groupLayer;
            }

            if (data is FMClassMapFileFunctionStore)
            {
                var groupLayer = new GroupLayer("Output (class map file)")
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

            if (data is FouFileFunctionStore)
            {
                return new GroupLayer("Output (fou)")
                {
                    LayersReadOnly = true,
                    NameIsReadOnly = true
                };
            }

            if (data is FM1DFileFunctionStore)
            {
                var groupLayer = new GroupLayer("Output 1D (map file)")
                {
                    LayersReadOnly = true,
                    NameIsReadOnly = true
                };
                groupLayer.Layers.CollectionChanged += MapGroupLayerLayersCollectionChanged;
                return groupLayer;
            }

            if (data is FMOutputSnappedFeaturesGroupLayerData outputSnappedGroupLayerData)
            {
                var groupLayer = new GroupLayer(OutputSnappedFeaturesLayerName) { Visible = false, NameIsReadOnly = true };

                groupLayer.Layers.AddRange(outputSnappedGroupLayerData.CreateLayers());
                groupLayer.LayersReadOnly = true;
                return groupLayer;
            }

            if (data is FMSnappedFeaturesGroupLayerData snappedGroupLayerData)
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
                        layer.CustomRenderers = new List<IFeatureRenderer>(new[] { new LeveeBreachRenderer(AreaLayerStyles.LeveeSnappedStyle, AreaLayerStyles.BreachSnappedStyle, AreaLayerStyles.WaterLevelStreamSnappedStyle)});
                    }
                    groupLayer.Layers.Add(layer);
                    snappedFeatures.Layer = layer;
                }
                groupLayer.LayersReadOnly = true;
                return groupLayer;
            }

            if (data is IGrouping<string, IFunction> grouping)
            {
                var functions = grouping.ToList();
                if (functions.Any())
                {
                    var groupLayerName = GetCommonFunctionName(functions);
                    return new GroupLayer(string.IsNullOrEmpty(groupLayerName) ? grouping.Key : groupLayerName);
                }
            }

            var coverage = data as FeatureCoverage;
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

            if (data is FrictionGroupLayerData)
            {
                return new GroupLayer("Roughness Data 1D")
                {
                    LayersReadOnly = true,
                    Selectable = false,
                    NameIsReadOnly = true
                };
            }

            if (data is IEventedList<ChannelFrictionDefinition> channelFrictionDefinitions)
            {
                return new VectorLayer(Properties.Resources.ChannelFrictionDefinitions_Name)
                {
                    Visible = false,
                    Selectable = true,
                    NameIsReadOnly = true,
                    CanBeRemovedByUser = false,
                    DataSource = new ComplexFeatureCollection(((FrictionGroupLayerData)parentData).Network, (IList)channelFrictionDefinitions, typeof(ChannelFrictionDefinition))
                };
            }

            if (data is IEventedList<PipeFrictionDefinition> pipeFrictionDefinitions)
            {
                return new VectorLayer(Properties.Resources.PipeFrictionDefinitions_Name)
                {
                    Visible = false,
                    Selectable = true,
                    NameIsReadOnly = true,
                    CanBeRemovedByUser = false,
                    DataSource = new ComplexFeatureCollection(((FrictionGroupLayerData)parentData).Network, (IList)pipeFrictionDefinitions, typeof(PipeFrictionDefinition))
                };
            }

            if (data is InitialConditionGroupLayerData)
            {
                return new GroupLayer("Initial Conditions 1D")
                {
                    LayersReadOnly = true,
                    Selectable = false,
                    NameIsReadOnly = true
                };
            }

            if (data is IEventedList<ChannelInitialConditionDefinition> channelInitialConditionDefinitions)
            {
                return new VectorLayer(RoughnessDataRegion.SectionId.DefaultValue)
                {
                    Visible = false,
                    Selectable = true,
                    NameIsReadOnly = true,
                    CanBeRemovedByUser = false,
                    DataSource = new ComplexFeatureCollection(((InitialConditionGroupLayerData)parentData).Network, (IList)channelInitialConditionDefinitions, typeof(ChannelInitialConditionDefinition))
                };
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
            if (e.Action != NotifyCollectionChangedAction.Add) return;

            var layer = e.GetRemovedOrAddedItem() as ILayer;
            if (layer == null) return;
            
            if (layer is UnstructuredGridLayer unstructuredGridLayer)
            {
                unstructuredGridLayer.GridColor = Color.Gray;
            }

            if (layer is INetworkCoverageGroupLayer networkCoverageGroupLayer)
            {
                var attributes = ((IFunction) networkCoverageGroupLayer.NetworkCoverage).Attributes;
                var location = attributes.ContainsKey(FM1DFileFunctionStore.LocationAttributeName)
                    ? (NetworkDataLocation)Enum.Parse(typeof(NetworkDataLocation), attributes[FM1DFileFunctionStore.LocationAttributeName])
                    : NetworkDataLocation.UnKnown;

                if (networkCoverageGroupLayer.SegmentLayer == null) return;

                if (location == NetworkDataLocation.Node)
                {
                    // hide SegmentLayer
                    networkCoverageGroupLayer.SegmentLayer.ShowInLegend = false;
                    networkCoverageGroupLayer.SegmentLayer.ShowInTreeView = false;
                }

                networkCoverageGroupLayer.LocationLayer.Visible = location != NetworkDataLocation.Edge;
                networkCoverageGroupLayer.SegmentLayer.Visible = location == NetworkDataLocation.Edge;
            }
        }

        public bool CanCreateLayerFor(object data, object parentObject)
        {
            return data is WaterFlowFMModel
                   || data is IEventedList<Model1DBoundaryNodeData>
                   || data is IEventedList<Model1DLateralSourceData>
                   || data is FeatureCoverage && IsCoverageLeveeBreachWidth((FeatureCoverage) data)
                   || data is IEventedList<ILink1D2D> &&
                   (parentObject is WaterFlowFMModel || parentObject is FMMapFileFunctionStore)
                   || data is IList<ILink1D2D> &&
                   (parentObject is FMClassMapFileFunctionStore || parentObject is FMMapFileFunctionStore)
                   || data is IGrouping<string, IFunction>
                   || data is FMMapFileFunctionStore
                   || data is FMHisFileFunctionStore
                   || data is FMClassMapFileFunctionStore
                   || data is FouFileFunctionStore
                   || data is FM1DFileFunctionStore
                   || data is ImportedFMNetFile
                   || data is IEventedList<BoundaryConditionSet> && parentObject is WaterFlowFMModel
                   || data is FMSnappedFeaturesGroupLayerData
                   || data is FMOutputSnappedFeaturesGroupLayerData
                   || data is CoverageDepthLayersList
                   || data is IEventedList<Feature2D> // Boundaries and sources&sinks
                   || data is FrictionGroupLayerData
                   || data is IEventedList<ChannelFrictionDefinition>
                   || data is IEventedList<PipeFrictionDefinition>
                   || data is InitialConditionGroupLayerData
                   || data is IEventedList<ChannelInitialConditionDefinition>;
        }

        private bool IsCoverageLeveeBreachWidth(INameable data)
        {
            if (data == null) return false;

            return data.Name == "dambreak breach width (dambreak_breach_width)";
        }

        public IEnumerable<object> ChildLayerObjects(object data)
        {
            var model = data as WaterFlowFMModel;
            if (model != null)
            {
                if (model.GetDataItemByTag(WaterFlowFMModelDataSet.NetworkTag).LinkedTo == null)
                {
                    yield return model.Network;
                }

                yield return model.NetworkDiscretization;

                if (!frictionGroupLayerDataMapping.TryGetValue(model, out var frictionGroupLayerDataElement))
                {
                    frictionGroupLayerDataElement = new FrictionGroupLayerData(model);
                    frictionGroupLayerDataMapping.Add(model, frictionGroupLayerDataElement);
                }
                yield return frictionGroupLayerDataElement;

                if (!initialConditionGroupLayerDataMapping.TryGetValue(model, out var initialConditionGroupLayerDataElement))
                {
                    initialConditionGroupLayerDataElement = new InitialConditionGroupLayerData(model);
                    initialConditionGroupLayerDataMapping.Add(model, initialConditionGroupLayerDataElement);
                }
                yield return initialConditionGroupLayerDataElement;

                yield return model.BoundaryConditions1D;
                yield return model.LateralSourcesData;

                var rootModel = GetRootModel(model);
                if (rootModel == null || rootModel is WaterFlowFMModel || model.GetDataItemByValue(model.Area).LinkedTo == null)
                {
                    if (model.Area.Enclosures.Count > 0)
                    {
                        foreach (var enclosure in model.Area.Enclosures)
                        {
                            var geoAsPol = enclosure.Geometry as Polygon;
                            if (geoAsPol == null || !geoAsPol.IsValid)
                            {
                                Log.WarnFormat(Resources.WaterFlowFMEnclosureValidator_Validate_Drawn_polygon_not__0__not_valid, enclosure.Name);
                            }
                        }
                    }
                    yield return model.Area;
                }
                
                yield return model.Grid;
                yield return model.Bathymetry;
                yield return model.InitialWaterLevel;
                yield return model.BoundaryConditionSets;
                yield return model.Boundaries;

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

                yield return model.Roughness;
                yield return model.Viscosity;
                yield return model.Diffusivity;

                if (model.UseInfiltration)
                {
                    yield return model.Infiltration;
                }
                
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

                yield return model.Pipes;
                yield return model.Links;

                if (model.Output1DFileStore != null)
                    yield return model.Output1DFileStore;

                if (model.OutputMapFileStore != null)
                    yield return model.OutputMapFileStore;

                if (model.OutputHisFileStore != null)
                    yield return model.OutputHisFileStore;
                
                if (model.OutputClassMapFileStore != null)
                {
                    yield return model.OutputClassMapFileStore;
                }

                if (model.OutputFouFileStore!= null)
                {
                    yield return model.OutputFouFileStore;
                }
            }

            var coverageDepthLayersList = data as CoverageDepthLayersList;
            if (coverageDepthLayersList != null)
            {
                foreach (var coverage in coverageDepthLayersList.Coverages)
                {
                    yield return coverage;
                }
            }

            var output1dStore = data as FM1DFileFunctionStore;
            if (output1dStore != null)
            {
                yield return output1dStore.OutputNetwork;
                yield return output1dStore.OutputDiscretization;
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
                    var classMapStore = outputStore as FMClassMapFileFunctionStore;
                    if (classMapStore != null)
                    {
                        yield return classMapStore.Network;
                        yield return classMapStore.Grid;
                        yield return classMapStore.Discretization;
                        yield return classMapStore.Links;
                    }

                    foreach (var output in outputStore.Functions)
                        yield return output;
                }
            }

            if (data is FouFileFunctionStore outputFouFileStore)
            {
                foreach (IFunction function in outputFouFileStore.Functions)
                {
                    yield return function;
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

            if (data is FrictionGroupLayerData frictionGroupLayerData)
            {
                foreach (var childLayerObject in frictionGroupLayerData.ChildLayerObjects())
                {
                    yield return childLayerObject;
                }
            }

            if (data is InitialConditionGroupLayerData initialConditionGroupLayerData)
            {
                foreach (var childLayerObject in initialConditionGroupLayerData.ChildLayerObjects())
                {
                    yield return childLayerObject;
                }
            }
        }

        public void AfterCreate(ILayer layer, object layerObject, object parentObject, IDictionary<ILayer, object> objectsLookup)
        {
            if (!(layerObject is WaterFlowFMModel model) ||
                !(layer is IGroupLayer groupLayer))
            {
                return;
            }

            var objectsInRenderOrder = new List<object>
                {
                    model.OutputHisFileStore,
                    model.OutputClassMapFileStore,
                    model.Output1DFileStore,
                    model.LateralSourcesData,
                    model.InitialWaterLevel,
                    model.ChannelFrictionDefinitions,
                    model.PipeFrictionDefinitions,
                    model.ChannelInitialConditionDefinitions,
                    model.RoughnessSections,
                    model.NetworkDiscretization,
                    model.Links,
                    model.Network,
                    model.BoundaryConditions1D,
                    model.Boundaries,
                    model.SourcesAndSinks,
                    model.Area,
                    model.Grid,
                    model.Roughness,
                    model.Bathymetry,
                    model.Viscosity,
                    model.Diffusivity,
                    model.Infiltration
                }
                .Where(o => o != null)
                .ToList();

            groupLayer.SetRenderOrderByObjectOrder(objectsInRenderOrder, objectsLookup);
        }

        private static IEnumerable<object> GetMapOutputFunctions(FMMapFileFunctionStore mapStore)
        {
            yield return mapStore.Grid;
            yield return mapStore.Links;
            
            var functionGrouping = mapStore.GetFunctionGrouping();
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
                yield return mapStore.CustomVelocityCoverage;
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

        private static VectorLayer CreateBoundaryNodeDataLayer(
            IEventedList<Model1DBoundaryNodeData> boundaryNodeDataList, WaterFlowFMModel fmModel)
        {
            return new VectorLayer("Boundary Data 1D")
            {
                Visible = false,
                Selectable = true,
                NameIsReadOnly = true,
                DataSource = new ComplexFeatureCollection(fmModel.Network, (IList) boundaryNodeDataList, typeof(Model1DBoundaryNodeData)),
                Theme = new CategorialTheme
                {

                    AttributeName = nameof(Model1DBoundaryNodeData.DataType),
                    DefaultStyle = new VectorStyle
                    {
                        GeometryType = typeof(IPoint),
                        Fill = new SolidBrush(Color.Transparent),
                        EnableOutline = false
                    }
                    ,
                    NoDataValues = new List<string> { "" },
                    ThemeItems = new EventedList<IThemeItem>
                    {
                        CreateCategorialThemeItem(Model1DBoundaryNodeDataType.WaterLevelConstant, Properties.Resources.HConst),
                        CreateCategorialThemeItem(Model1DBoundaryNodeDataType.WaterLevelTimeSeries, Properties.Resources.HBoundary),
                        CreateCategorialThemeItem(Model1DBoundaryNodeDataType.FlowConstant, Properties.Resources.QConst),
                        CreateCategorialThemeItem(Model1DBoundaryNodeDataType.FlowTimeSeries, Properties.Resources.QBoundary),
                        CreateCategorialThemeItem(Model1DBoundaryNodeDataType.FlowWaterLevelTable, Properties.Resources.QHBoundary)
                    }
                }
            };
        }

        private static VectorLayer CreateLateralDataLayer(IEventedList<Model1DLateralSourceData> lateralSourceDataList,
            WaterFlowFMModel fmModel)
        {
            return new VectorLayer("Lateral Data 1D")
            {
                Visible = false,
                Selectable = true,
                NameIsReadOnly = true,
                DataSource = new ComplexFeatureCollection(fmModel.Network, (IList)lateralSourceDataList, typeof(Model1DLateralSourceData)),
                Theme = new CategorialTheme
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
                }
            };
        }

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
                },

            };
        }


        private class FrictionGroupLayerData
        {
            private readonly WaterFlowFMModel model;

            public FrictionGroupLayerData(WaterFlowFMModel model)
            {
                this.model = model;
            }

            public IHydroNetwork Network => model.Network;
            public IEnumerable<object> ChildLayerObjects()
            {
                yield return model.ChannelFrictionDefinitions;
                yield return model.PipeFrictionDefinitions;
                yield return model.RoughnessSections;
            }
        }

        private class InitialConditionGroupLayerData
        {
            private readonly WaterFlowFMModel model;

            public InitialConditionGroupLayerData(WaterFlowFMModel model)
            {
                this.model = model;
            }
            public IHydroNetwork Network => model.Network;

            public IEnumerable<object> ChildLayerObjects()
            {
                yield return model.ChannelInitialConditionDefinitions;
            }
        }
    }
}