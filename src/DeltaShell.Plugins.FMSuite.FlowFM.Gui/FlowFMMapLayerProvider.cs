using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.CoverageDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
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

        private static readonly string ModelName = typeof (WaterFlowFMModel).Name;

        public const string BoundariesLayerName = "Boundaries";
        public const string BoundaryConditionsLayerName = "Boundary Conditions";
        public const string SourcesAndSinksLayerName = "Sources and Sinks";

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

            var snappedGroupLayerData = data as FMSnappedFeaturesGroupLayerData;
            if (snappedGroupLayerData != null)
            {
                var groupLayer = new GroupLayer("Grid-snapped features") {Visible = false, NameIsReadOnly = true};
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

            return null;
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
                   || data is FMMapFileFunctionStore
                   || data is FMHisFileFunctionStore
                   || data is ImportedFMNetFile
                   || (data is IEventedList<BoundaryConditionSet> && parentObject is WaterFlowFMModel)
                   || data is FMSnappedFeaturesGroupLayerData
                   || data is CoverageDepthLayersList
                   || data is IEventedList<Feature2D>;  // Boundaries and sources&sinks
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
                    yield return model.Area;
                }
                yield return model.Network;
                yield return model.NetworkDiscretization;

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
                    yield return mapStore.Grid;
                }

                foreach (var output in outputStore.Functions)
                    yield return output;
            }
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