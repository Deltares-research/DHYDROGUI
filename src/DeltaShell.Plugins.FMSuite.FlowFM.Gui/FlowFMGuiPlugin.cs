using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Resources;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Editors;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Forms;
using DelftTools.Shell.Gui.Swf.Validation;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.NGHS.Common.Gui.MapLayers;
using DeltaShell.NGHS.Common.Gui.PropertyGrid.PropertyInfoCreation;
using DeltaShell.NGHS.Common.Gui.WPF.SettingsView;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.Gui;
using DeltaShell.Plugins.FMSuite.Common.Gui.Editors;
using DeltaShell.Plugins.FMSuite.Common.Gui.Forms;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors.ModelFeatureCoordinateDataEditor;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms.Friction;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms.InitialConditions;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms.ModelMerge;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.GraphicsProviders;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.PropertyGrid.PropertyInfoCreation;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Views;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.CoverageViews;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using Mono.Addins;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api.Layers;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.UI.Forms;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui
{
    [Extension(typeof(IPlugin))]
    public class FlowFMGuiPlugin : GuiPlugin
    {
        private readonly GuiContainer guiContainer;
        private TableViewTimeSeriesGeneratorTool tableViewTimeSeriesGeneratorTool;

        private const string CoordinateSystemMemberName = nameof(WaterFlowFMModel.CoordinateSystem);

        private const string OutputHisFileStoreMemberName = nameof(WaterFlowFMModel.OutputHisFileStore);

        private const string HeatFluxModelTypeMemberName = nameof(WaterFlowFMModel.HeatFluxModelType);

        private static Func<MapView> getActiveMapViewFunc;
        private string _fmModelSettingsSuffix = " (FM model settings)";
        private Ribbon.Ribbon ribbon;
        private readonly PropertyInfoCreator propertyInfoCreator;

        public FlowFMGuiPlugin()
        {
            guiContainer = new GuiContainer();
            propertyInfoCreator = new PropertyInfoCreator(guiContainer);
            getActiveMapViewFunc = GetActiveMapView;
            GraphicsProvider = new FmGuiGraphicsProvider();
        }

        public override string Name =>
            "Delft3D FM (Gui)";

        public override string DisplayName =>
            "D-Flow Flexible Mesh Plugin (UI)";

        public override string Description =>
            FlowFM.Properties.Resources.FlowFMApplicationPlugin_Description;

        public override string Version =>
            GetType().Assembly.GetName().Version.ToString();

        public override string FileFormatVersion => "1.1.0.0";

        public override IGraphicsProvider GraphicsProvider { get; }

        public override ResourceManager Resources { get; set; }

        public override IMapLayerProvider MapLayerProvider { get; } =
            FlowFMMapLayerProviderFactory.ConstructMapLayerProvider();

        public override IGui Gui
        {
            get => base.Gui;
            set
            {
                if (base.Gui != null)
                {
                    UnsubscribeToProjectEvents();
                    UnSubscribeToActivityEvents();
                }
                base.Gui = value;
                if (base.Gui != null)
                {
                    SubscribeToProjectEvents();
                    SubscribeToActivityEvents();
                }

                // HACK: setting the Gui happens just before Activate in DeltaShellGui, 
                // hence we set the spatial operations flag here:
                if (SharpMapGisGuiPlugin.Instance != null)
                {
                    SharpMapGisGuiPlugin.Instance.SpatialOperationsEnabled = true;
                }

                tableViewTimeSeriesGeneratorTool = new TableViewTimeSeriesGeneratorTool
                {
                    GetStartTime = GetModelStartTime,
                    GetStopTime = GetModelStopTime,
                    GetTimeStep = GetModelTimeStep
                };
                guiContainer.Gui = value;
            }
        }

        public override IRibbonCommandHandler RibbonCommandHandler =>
            ribbon ?? (ribbon = new Ribbon.Ribbon());

        public static MapView ActiveMapView =>
            getActiveMapViewFunc();

        private IEnumerable<WaterFlowFMModel> FlowModels =>
            Gui == null ? Enumerable.Empty<WaterFlowFMModel>()
                        : Gui.Application.GetAllModelsInProject().OfType<WaterFlowFMModel>();

        public override IEnumerable<ITreeNodePresenter> GetProjectTreeViewNodePresenters()
        {
            yield return new WaterFlowFMModelNodePresenter(this);
            yield return new FmModelTreeShortcutNodePresenter { GuiPlugin = this };
            yield return new BoundaryConditionSetNodePresenter { GuiPlugin = this };
            yield return new SourceSinkNodePresenter { GuiPlugin = this };
            yield return new FMMapFileFunctionStoreNodePresenter { GuiPlugin = this };
            yield return new FMHisFileFunctionStoreNodePresenter();
            yield return new FMClassMapFileFunctionStoreNodePresenter { GuiPlugin = this };
            yield return new FM1DFileFunctionStoreNodePresenter { GuiPlugin = this };
            yield return new ImportedFMNetFileNodePresenter { GuiPlugin = this };
            yield return new HeatFluxModelNodePresenter { GuiPlugin = this };
            yield return new WindItemListNodePresenter { GuiPlugin = this };
            yield return new WindItemNodePresenter { GuiPlugin = this };
            yield return new FmMeteoItemNodePresenter { GuiPlugin = this };
            yield return new FmMeteoItemListNodePresenter { GuiPlugin = this };
        }

        public override IEnumerable<ViewInfo> GetViewInfoObjects()
        {
            yield return new ViewInfo<ProjectTemplate, CreateFmModelSettingView>
            {
                AdditionalDataCheck = t => t.Id?.Equals(FlowFMApplicationPlugin.FM_MODEL_DEFAULT_PROJECT_TEMPLATE_ID, StringComparison.CurrentCultureIgnoreCase) ?? false
            };
            yield return new ViewInfo<ProjectTemplate, MduTemplateView>
            {
                AdditionalDataCheck = t => t.Id == FlowFMApplicationPlugin.FM_MODEL_MDU_IMPORT_PROJECT_TEMPLATE_ID
            };
            yield return GetLateralSourceViewInfo();

            yield return new ViewInfo<WaterFlowFMModel, SettingsView>
            {
                Description = "FM Settings",
                GetViewName = (v, o) => o.Name + _fmModelSettingsSuffix,
                AfterCreate = (v, o) =>
                {
                    //Set the properties.
                    v.SettingsCategories = WaterFlowFmSettingsHelper.GetWpfGuiCategories(o, Gui);
                    v.GetChangedPropertyName = (sender, prop) => (sender as WaterFlowFMProperty)?.PropertyDefinition.MduPropertyName;
                }
            };

            yield return new ViewInfo<FmModelTreeShortcut, WaterFlowFMModel, SettingsView>
            {
                Description = "FM Settings",
                AdditionalDataCheck = o => o.ShortCutType == ShortCutType.SettingsTab,
                GetViewData = o => o.FlowFmModel,
                GetViewName = (v, o) => o.Name + _fmModelSettingsSuffix,
                OnActivateView = (v, o) =>
                {
                    var shortcut = o as FmModelTreeShortcut;
                    if (shortcut == null) return;
                    v.EnsureVisible(shortcut.Data);
                },
                AfterCreate = (v, o) =>
                {
                    //Set the properties.
                    v.SettingsCategories = WaterFlowFmSettingsHelper.GetWpfGuiCategories(o.FlowFmModel, Gui);
                    v.GetChangedPropertyName = (sender, prop) => (sender as WaterFlowFMProperty)?.PropertyDefinition.MduPropertyName;
                }
            };

            yield return new ViewInfo<WaterFlowFMModel, WaterFlowFMFileStructureView>
            {
                Description = "File tree"
            };

            yield return new ViewInfo<HydroNode, Model1DBoundaryNodeData, Model1DBoundaryNodeDataViewWpf>
            {
                Description = "Lateral Source Data View (Flow 1D)",
                AdditionalDataCheck = hydroNode => FlowModels.Any(fm => fm.BoundaryConditions1D.Any(d => ReferenceEquals(d.Feature, hydroNode))),
                GetViewData = hydroNode => FlowModels.SelectMany(fm => fm.BoundaryConditions1D).FirstOrDefault(d => ReferenceEquals(d.Feature, hydroNode))
            };

            yield return new ViewInfo<ILateralSource, Model1DLateralSourceData, Model1DLateralSourceDataViewWpf>
            {
                Description = "Lateral Source Data View (Flow 1D)",
                AdditionalDataCheck = lateralSource => FlowModels.Any(fm => fm.LateralSourcesData.Any(d => ReferenceEquals(d.Feature, lateralSource))),
                GetViewData = lateralSource => FlowModels.SelectMany(fm => fm.LateralSourcesData).FirstOrDefault(d => ReferenceEquals(d.Feature, lateralSource))
            };
            
            yield return new ViewInfo<FixedWeir, IModelFeatureCoordinateData, ModelFeatureCoordinateDataView>
            {
                Description = "Data for feature",
                GetViewName = (v, o) => $"{o?.Feature.ToString()} data",
                GetViewData = o => FlowModels.SelectMany(fm => fm.FixedWeirsProperties).FirstOrDefault(p => Equals(p.Feature, o)),
                AdditionalDataCheck = o => FlowModels.Any(m => m.FixedWeirsProperties.Any(d => ReferenceEquals(d.Feature, o)))
            };

            yield return new ViewInfo<BridgePillar, IModelFeatureCoordinateData, ModelFeatureCoordinateDataView>
            {
                Description = "Data for feature",
                GetViewName = (v, o) => $"{o?.Feature.ToString()} data",
                GetViewData = o => FlowModels.SelectMany(fm => fm.BridgePillarsDataModel).FirstOrDefault(p => Equals(p.Feature, o)),
                AdditionalDataCheck = o => FlowModels.Any(m => m.BridgePillarsDataModel.Any(d => ReferenceEquals(d.Feature, o)))
            };

            yield return new ViewInfo<FeatureCoverage, CoverageView>
            {
                Description = "Map (Separate view)",
                Image = Properties.Resources.Map,
                GetViewName = (v, o) => o.Name,
                AfterCreate = (v, o) => v.SaveViewContext = false
            };

            // Validation
            yield return new ViewInfo<WaterFlowFMModel, ValidationView>
            {
                Description = "Validation Report",
                Image = Common.Gui.Properties.Resources.validation,
                AfterCreate = (v, o) =>
                {
                    v.Gui = Gui;
                    v.OnValidate = d => (d as WaterFlowFMModel)?.ValidationReport
                                        ?? new ValidationReport("No report available", new List<ValidationIssue>());
                }
            };
            Func<object, string, string> FmSettingsPropertyChanged = (object sender, string propertyName) =>
            {
                var property = sender as WaterFlowFMProperty;
                if (property != null)
                {
                    return property.PropertyDefinition.MduPropertyName;
                }

                var model = sender as WaterFlowFMModel;
                if (model != null && propertyName == nameof(model.CoordinateSystem))
                {
                    return "CoordinateSystem";
                }

                return null;
            };
            yield return new ViewInfo<FmValidationShortcut, WaterFlowFMModel, SettingsView>
            {
                Description = "FM Settings",
                GetViewData = o => o.FlowFmModel,
                GetViewName = (v, o) => o.Name + _fmModelSettingsSuffix,
                OnActivateView = (v, o) =>
                {
                    var shortcut = o as FmValidationShortcut;
                    if (shortcut == null) return;
                    v.EnsureVisible(shortcut.TabName);
                },
                AfterCreate = (v, o) =>
                {
                    //Set the properties.
                    v.SettingsCategories = WaterFlowFmSettingsHelper.GetWpfGuiCategories(o.FlowFmModel, Gui);
                    v.GetChangedPropertyName = FmSettingsPropertyChanged;
                }
            };
            yield return new ViewInfo<IEnumerable<ICrossSection>, RefreshMainSectionWidthsDialog>
            {
                Description = "Refresh Main Section Width View (Cross sections in FlowFM 1D)"
            };
            yield return new ViewInfo<IEnumerable<IGrouping<Coordinate, INetworkLocation>>, RemoveDuplicateCalculationPointsDialog>
            {
                Description = "Remove duplicate calculation points view (multiple location points at same location)",
                AfterCreate = (dialog, grp) => dialog.ViewModel.NetworkDiscretization = FlowModels.FirstOrDefault(m => m.NetworkDiscretization.Locations.Values.Any(l => Equals(l, grp.First().First()))).NetworkDiscretization
            };

            // Boundary conditions
            var boundaryConditionSetViewInfo = new ViewInfo<BoundaryConditionSet, BoundaryConditionEditor>
            {
                Description = "Boundary Data Editor",
                AfterCreate = (v, o) =>
                {
                    var model = FlowModels.FirstOrDefault(m => m.BoundaryConditionSets.Contains(o));
                    if (model == null) return;

                    v.BoundaryConditionFactory = new FlowBoundaryConditionFactory()
                    {
                        Model = model
                    };
                    var controller = new FlowBoundaryConditionEditorController
                    {
                        Model = model
                    };
                    v.Controller = controller;
                    v.BoundaryConditionPropertiesControl = new FlowBoundaryConditionPropertiesControl
                    {
                        Controller = controller
                    };
                    v.ShowSupportPointNames = true;
                    var condition = o.BoundaryConditions.FirstOrDefault();
                    if (condition == null) return;
                    v.SelectedCategory = condition.ProcessName;
                    v.SelectedBoundaryCondition = condition;
                },
                CloseForData = (v, bcs) => Equals(v.Data, bcs)
            };

            // boundary condition set
            yield return boundaryConditionSetViewInfo;

            // boundary feature
            yield return ViewInfoWrapper<Feature2D>.Create(boundaryConditionSetViewInfo, FindSetForBoundary, IsModelBoundary);

            // boundary condition
            yield return ViewInfoWrapper<BoundaryCondition>.Create(boundaryConditionSetViewInfo, FindSetForBoundaryCondition,
                afterCreate: (view, condition) =>
                    {
                        view.SelectedCategory = condition.ProcessName;
                        view.SelectedBoundaryCondition = condition;
                    });

            var allBoundarySetsViewInfo = new ViewInfo<IEventedList<BoundaryConditionSet>, BoundaryConditionListView>
            {
                Description = "Boundary Conditions",
                GetViewName = (v, o) => "Boundary Conditions",
                CompositeViewType = typeof(ProjectItemMapView),
                GetCompositeViewData = o => FlowModels.FirstOrDefault(m => m.BoundaryConditionSets.Equals(o)),
                AfterCreate = (v, o) =>
                {
                    var centralMap = Gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault(vi => vi.MapView.GetLayerForData(o) != null);
                    if (centralMap == null) return;

                    v.OpenViewMethod = feature => Gui.CommandHandler.OpenView(feature);
                    v.ZoomToFeature = feature => centralMap.MapView.EnsureVisible(o.FirstOrDefault(bcs => bcs.BoundaryConditions.Contains(feature as IBoundaryCondition)));
                }
            };

            yield return allBoundarySetsViewInfo;

            yield return ViewInfoWrapper<FmModelTreeShortcut>.Create(allBoundarySetsViewInfo, o => o.Data, o => o.ShortCutType == ShortCutType.FeatureSet);

            yield return FeatureCollectionViewInfoHelper.CreateViewInfo<Feature2D, WaterFlowFMModel>("Boundaries", m => m.Boundaries, guiContainer);

            var viewInfo = FeatureCollectionViewInfoHelper.CreateViewInfo<ILink1D2D, WaterFlowFMModel>("Links", m => m.Links, guiContainer);

            yield return viewInfo;

            yield return ViewInfoWrapper<FmModelTreeShortcut>.Create(viewInfo, o => o.Data, o => o.ShortCutType == ShortCutType.FeatureSet);

            // Sources and sinks
            var sourceAndSinkViewInfo = new ViewInfo<SourceAndSink, SourceAndSinkView>
            {
                CloseForData = (v, o) => Equals(v.Data, o),
                AfterCreate = (v, o) =>
                {
                    v.Model = FlowModels.FirstOrDefault(m => m.SourcesAndSinks.Contains(o));
                    tableViewTimeSeriesGeneratorTool.ConfigureTableView(v.FunctionView.TableView);
                }
            };
            yield return sourceAndSinkViewInfo;

            yield return ViewInfoWrapper<Feature2D>.Create(sourceAndSinkViewInfo, FindDataForPipe, IsModelPipe);

            var pipesViewInfo = FeatureCollectionViewInfoHelper.CreateViewInfo<Feature2D, WaterFlowFMModel>("Sources and Sinks", m => m.Pipes, guiContainer);
            yield return ViewInfoWrapper<FmModelTreeShortcut>.Create(pipesViewInfo, GetPipesFromSourcesAndSinks, o => o.ShortCutType == ShortCutType.FeatureSet, (v, o) => v.CanAddDeleteAttributes = false);

            var model1DBoundaryConditionsViewInfo = FeatureCollectionViewInfoHelper.CreateViewInfo<Model1DBoundaryNodeData, WaterFlowFMModel>("1D Boundary Conditions", m => m.BoundaryConditions1D, guiContainer);
            yield return ViewInfoWrapper<FmModelTreeShortcut>.Create(model1DBoundaryConditionsViewInfo, o => o.Data, o => o.ShortCutType == ShortCutType.FeatureSet, (v, o) => v.CanAddDeleteAttributes = false);

            var model1DLateralSourceViewInfo = FeatureCollectionViewInfoHelper.CreateViewInfo<Model1DLateralSourceData, WaterFlowFMModel>("Lateral Sources", m => m.LateralSourcesData, guiContainer);

            yield return ViewInfoWrapper<FmModelTreeShortcut>.Create(model1DLateralSourceViewInfo, o => o.Data, o => o.ShortCutType == ShortCutType.FeatureSet, (v, o) =>
            {
                v.CanAddDeleteAttributes = false;
                ProjectItemMapView centralMap1 = Gui.DocumentViews.OfType<ProjectItemMapView>()
                                                    .FirstOrDefault(vi => vi.MapView.GetLayerForData(o) != null);
                if (centralMap1 == null)
                {
                    return;
                }

                v.DeleteSelectedFeatures = () => centralMap1.MapView.MapControl.DeleteTool.DeleteSelection();
                v.OpenViewMethod = ob => Gui.CommandHandler.OpenView(ob);
                v.ZoomToFeature = feature => centralMap1.MapView.EnsureVisible(feature);
                v.CanAddDeleteAttributes = false;
                SetLateralSourceCompartmentComboBoxTypeEditor(v);

                v.TableView.FocusedRowChanged += (sender, args) => { SetLateralSourceCompartmentComboBoxTypeEditor(v); };
            });
            var networkDiscretizationCoverageViewInfo = new ViewInfo<ICoverage, CoverageTableView>
            {
                Description = "Network Discretization",
                AdditionalDataCheck = o => o is IDiscretization,
                CompositeViewType = typeof(ProjectItemMapView),
                GetCompositeViewData = o => FlowModels.FirstOrDefault(m => m.NetworkDiscretization.Equals(o)),
            };
            yield return networkDiscretizationCoverageViewInfo;
            yield return ViewInfoWrapper<FmModelTreeShortcut>.Create(networkDiscretizationCoverageViewInfo, o => o.Data);
            // Heat flux model
            yield return new ViewInfo<HeatFluxModel, HeatFluxModelView>
            {
                GetViewData = m =>
                {
                    if (m == null) return null;
                    var waterFlowFMModel = FlowModels.FirstOrDefault(wfm => Equals(wfm.ModelDefinition.HeatFluxModel, m));

                    if (m.MeteoData != null)
                    {
                        TimeArgumentConfigurer.Configure(m.MeteoData, waterFlowFMModel);
                    }
                    return m;
                },
                AdditionalDataCheck =
                    t => FlowModels.FirstOrDefault(m => Equals(m.ModelDefinition.HeatFluxModel, t)) != null,
                AfterCreate =
                    (v, o) =>
                        v.FunctionView.ConfigureTableView = t => tableViewTimeSeriesGeneratorTool.ConfigureTableView(t)
            };

            //Wind
            yield return new ViewInfo<IEventedList<IWindField>, WindFieldListView>
            {
                AdditionalDataCheck = t => FlowModels.FirstOrDefault(m => Equals(m.WindFields, t)) != null,
                AfterCreate = (v, o) => v.TimeSeriesGeneratorTool = tableViewTimeSeriesGeneratorTool
            };

            yield return new ViewInfo<UniformWindField, IFunction, FunctionView>
            {
                GetViewData = w => w.Data,
                AdditionalDataCheck = t => FlowModels.FirstOrDefault(m => m.WindFields.Contains(t)) != null,
                AfterCreate = (v, o) => tableViewTimeSeriesGeneratorTool.ConfigureTableView(v.TableView)
            };

            yield return new ViewInfo<GriddedWindField, GriddedWindView>
            {
                AdditionalDataCheck = t => FlowModels.FirstOrDefault(m => m.WindFields.Contains(t)) != null,
            };

            yield return new ViewInfo<SpiderWebWindField, GriddedWindView>
            {
                AdditionalDataCheck = t => FlowModels.FirstOrDefault(m => m.WindFields.Contains(t)) != null,
            };

            yield return new ViewInfo<IEventedList<IFmMeteoField>, FmMeteoFieldListView>
            {
                AdditionalDataCheck = t => FlowModels.FirstOrDefault(m => Equals(m.FmMeteoFields, t)) != null,
                AfterCreate = (v, o) => v.TimeSeriesGeneratorTool = tableViewTimeSeriesGeneratorTool
            };

            // Importers and exporters
            yield return new ViewInfo<WaterFlowFMIntoWaterFlowFMFileImporter, ModelMergeView>()
            {
                AfterCreate = (v, o) =>
                {
                    v.OriginalModel = Gui.SelectedModel as WaterFlowFMModel;
                }
            };
            yield return new ViewInfo<BcmFileImporter, BcmFileImportDialog>();
            yield return new ViewInfo<BcmFileExporter, BcmFileExportDialog>();
            yield return new ViewInfo<BcFileImporter, BcFileImportDialog>();
            yield return new ViewInfo<BcFileExporter, BcFileExportDialog>();
            yield return new ViewInfo<FMModelPartitionExporter, FMPartitionExportDialog>
            {
                AfterCreate = (v, o) =>
                {
                    v.Extension = o.FileFilter;
                    v.EnableSolverSelection = true;
                }
            };
            yield return new ViewInfo<FMGridPartitionExporter, FMPartitionExportDialog>
            {
                AfterCreate = (v, o) =>
                {
                    v.Extension = o.FileFilter;
                    v.EnableSolverSelection = false;
                }
            };

            yield return GetFeature2DImportDialogViewInfo<PliFileImporterExporter<Embankment, Embankment>>();
            yield return GetGisToFeature2DImportDialogViewInfo<GisToFeature2DImporter<ILineString, Embankment>>();
            yield return GetFeature2DImportDialogViewInfo<PliFileImporterExporter<FixedWeir, FixedWeir>>();
            yield return GetGisToFeature2DImportDialogViewInfo<GisToFeature2DImporter<ILineString, FixedWeir>>();
            yield return GetFeature2DImportDialogViewInfo<PliFileImporterExporter<ObservationCrossSection2D, ObservationCrossSection2D>>();
            yield return GetFeature2DImportDialogViewInfo<PliFileImporterExporter<ThinDam2D, ThinDam2D>>();
            yield return GetGisToFeature2DImportDialogViewInfo<GisToFeature2DImporter<ILineString, ThinDam2D>>();
            yield return GetFeature2DImportDialogViewInfo<PliFileImporterExporter<IWeir, Feature2D>>();
            yield return GetFeature2DImportDialogViewInfo<PliFileImporterExporter<IPump, Feature2D>>();
            yield return GetFeature2DImportDialogViewInfo<PliFileImporterExporter<IGate, Feature2D>>();
            yield return GetFeature2DImportDialogViewInfo<PliFileImporterExporter<SourceAndSink, Feature2D>>();
            yield return GetFeature2DImportDialogViewInfo<PliFileImporterExporter<BoundaryConditionSet, Feature2D>>();
            yield return GetFeature2DImportDialogViewInfo<PliFileImporterExporter<BridgePillar, BridgePillar>>();

            yield return GetFeature2DImportDialogViewInfo<PointFileImporterExporter>();
            yield return GetFeature2DImportDialogViewInfo<PolFileImporterExporter>();
            yield return GetFeature2DImportDialogViewInfo<LdbFileImporterExporter>();

            yield return GetFeature2DImportDialogViewInfo<PliFileImporterExporter<Feature2D, LeveeBreach>>();

            yield return GetGisToFeature2DImportDialogViewInfo<GisToFeature2DImporter<ILineString, LeveeBreach>>();
            yield return GetGisToFeature2DImportDialogViewInfo<GisToFeature2DImporter<IPolygon, GroupableFeature2DPolygon>>();
            yield return GetGisToFeature2DImportDialogViewInfo<GisToFeature2DImporter<IPoint, Gully>>();

            var channelFrictionDefinitionsViewInfo = new ViewInfo<IEventedList<ChannelFrictionDefinition>, ILayer, ChannelFrictionDefinitionsView>
            {
                Description = "1D Roughness - Channels",
                GetViewName = (view, layer) => layer?.Name,
                GetViewData = data =>
                {
                    return Gui?.DocumentViews
                              .OfType<ProjectItemMapView>()
                              .Select(projectItemMapView => projectItemMapView.MapView.GetLayerForData(data))
                              .FirstOrDefault(layerData => layerData != null);
                },
                CompositeViewType = typeof(ProjectItemMapView),
                AdditionalDataCheck = data => FlowModels.Any(waterFlowFmModel => Equals(data, waterFlowFmModel.ChannelFrictionDefinitions)),
                GetCompositeViewData = data => FlowModels.FirstOrDefault(waterFlowFmModel => Equals(data, waterFlowFmModel.ChannelFrictionDefinitions)),
                AfterCreate = (view, data) =>
                {
                    var flowFmModel = FlowModels.FirstOrDefault(waterFlowFmModel => Equals(data, waterFlowFmModel.ChannelFrictionDefinitions));

                    if (flowFmModel != null)
                    {
                        view.SetWaterFlowFmModel(flowFmModel);

                        view.SetOpenGlobalFrictionSettingsMethod(() =>
                        {
                            Gui?.DocumentViewsResolver.OpenViewForData(new FmValidationShortcut
                            {
                                FlowFmModel = flowFmModel,
                                TabName = "Physical Parameters"
                            });
                        });
                    }

                    var centralMap2 = view.GetViewsOfType<ProjectItemMapView>().FirstOrDefault(vi => vi.MapView.GetLayerForData(data) != null);
                    if (centralMap2 == null) return;

                    view.SetZoomToFeatureMethod(feature => centralMap2.MapView.EnsureVisible(feature));
                }
            };
            yield return channelFrictionDefinitionsViewInfo;
            yield return ViewInfoWrapper<FmModelTreeShortcut>.Create(channelFrictionDefinitionsViewInfo, o => o.Data, o => o.ShortCutType == ShortCutType.FeatureSet);

            var pipeFrictionDefinitionsViewInfo = new ViewInfo<IEventedList<PipeFrictionDefinition>, ILayer, VectorLayerAttributeTableView>
            {
                Description = "1D Roughness - Sewer",
                GetViewName = (view, layer) => layer?.Name,
                GetViewData = pipeFrictionDefinitions =>
                {
                    return Gui?.DocumentViews
                              .OfType<ProjectItemMapView>()
                              .Select(projectItemMapView => projectItemMapView.MapView.GetLayerForData(pipeFrictionDefinitions))
                              .FirstOrDefault(layerData => layerData != null);
                },
                CompositeViewType = typeof(ProjectItemMapView),
                AdditionalDataCheck = pipeFrictionDefinitions => FlowModels.Any(waterFlowFmModel => Equals(pipeFrictionDefinitions, waterFlowFmModel.PipeFrictionDefinitions)),
                GetCompositeViewData = pipeFrictionDefinitions => FlowModels.FirstOrDefault(waterFlowFmModel => Equals(pipeFrictionDefinitions, waterFlowFmModel.PipeFrictionDefinitions)),
                AfterCreate = (view, pipeFrictionDefinitions) =>
                {
                    var centralMap4 = view.GetViewsOfType<MapView>().FirstOrDefault(mv => mv.GetLayerForData(pipeFrictionDefinitions) != null);
                    if (centralMap4 == null) return;

                    view.ZoomToFeature = feature => centralMap4.EnsureVisible(feature);
                }
            };
            yield return pipeFrictionDefinitionsViewInfo;
            yield return ViewInfoWrapper<FmModelTreeShortcut>.Create(pipeFrictionDefinitionsViewInfo, o => o.Data, o => o.ShortCutType == ShortCutType.FeatureSet);

            var channelInitialConditionDefinitionsViewInfo = new ViewInfo<IEventedList<ChannelInitialConditionDefinition>, ILayer, ChannelInitialConditionDefinitionsView>
            {
                Description = "1D Initial Conditions - Channels",
                GetViewName = (view, layer) => layer?.Name,
                GetViewData = channelInitialConditionDefinitions =>
                {
                    return Gui?.DocumentViews
                              .OfType<ProjectItemMapView>()
                              .Select(projectItemMapView => projectItemMapView.MapView.GetLayerForData(channelInitialConditionDefinitions))
                              .FirstOrDefault(layerData => layerData != null);
                },
                CompositeViewType = typeof(ProjectItemMapView),
                AdditionalDataCheck = channelInitialConditionDefinitions => FlowModels.Any(waterFlowFmModel => Equals(channelInitialConditionDefinitions, waterFlowFmModel.ChannelInitialConditionDefinitions)),
                GetCompositeViewData = channelInitialConditionDefinitions => FlowModels.FirstOrDefault(waterFlowFmModel => Equals(channelInitialConditionDefinitions, waterFlowFmModel.ChannelInitialConditionDefinitions)),
                AfterCreate = (view, channelInitialConditionDefinitions) =>
                {
                    var flowFmModel1 = FlowModels.FirstOrDefault(waterFlowFmModel => Equals(channelInitialConditionDefinitions, waterFlowFmModel.ChannelInitialConditionDefinitions));

                    view.SetWaterFlowFmModel(flowFmModel1);
                    view.SetInitialConditionValuesByQuantity();
                    view.SetCurrentQuantity();

                    view.SetOpenGlobalInitialConditionSettingsMethod(() =>
                    {
                        Gui?.DocumentViewsResolver.OpenViewForData(new FmValidationShortcut
                        {
                            FlowFmModel = flowFmModel1,
                            TabName = "Initial Conditions"
                        });
                    });

                    var centralMap3 = view.GetViewsOfType<MapView>().FirstOrDefault(mv => mv.GetLayerForData(channelInitialConditionDefinitions) != null);
                    if (centralMap3 == null) return;

                    view.SetZoomToFeatureMethod(feature => centralMap3.EnsureVisible(feature));
                }
            };
            yield return channelInitialConditionDefinitionsViewInfo;
            yield return ViewInfoWrapper<FmModelTreeShortcut>.Create(channelInitialConditionDefinitionsViewInfo, o => o.Data, o => o.ShortCutType == ShortCutType.FeatureSet);
        }

        private ViewInfo<LateralSourceImporter, LateralSourceImportDialog> GetLateralSourceViewInfo()
        {
            return new ViewInfo<LateralSourceImporter, LateralSourceImportDialog>
            {
                Description = "LateralSource Data CSV Importer",
                AfterCreate = (v, o) =>
                {
                    var guiSelectionDataItem = Gui.Selection as FmModelTreeShortcut;
                    switch (guiSelectionDataItem?.Data)
                    {
                        case IEventedList<Model1DLateralSourceData> _:
                            v.BatchMode = true;
                            v.ForBoundaryConditions = false;
                            break;
                        case IEventedList<Model1DBoundaryNodeData> _:
                            v.BatchMode = true;
                            v.ForBoundaryConditions = true;
                            break;
                        case Model1DLateralSourceData _:
                            v.BatchMode = false;
                            v.ForBoundaryConditions = false;
                            break;
                        case Model1DBoundaryNodeData _:
                            v.BatchMode = false;
                            v.ForBoundaryConditions = true;
                            break;
                    }
                }
            };
        }

        public override IEnumerable<PropertyInfo> GetPropertyInfos()
        {
            yield return CreatePropertyInfoDynamic<WaterFlowFMModel>();
            yield return CreatePropertyInfoDynamic<PointCloudLayer>();
            yield return propertyInfoCreator.Create(new FMWeirPropertyInfoCreationContext());
            yield return new PropertyInfo<FmModelTreeShortcut, HydroNetworkProperties>
            {
                AdditionalDataCheck = w => w.Data is IHydroNetwork,
                GetObjectPropertiesData = o => o.Data
            };
            yield return new PropertyInfo<FmModelTreeShortcut, DiscretizationProperties>
            {
                AdditionalDataCheck = w => w.Data is IDiscretization,
                GetObjectPropertiesData = o => o.Data
            };

            yield return propertyInfoCreator.Create(new NetworkLocationPropertyInfoCreationContext());
        }

        public override void OnActiveViewChanged(IView view)
        {
            if (view == null)
                return;

            var mapView = FindMapView(view);
            if (mapView != null)
                FlowFMMapViewDecorator.AddMapToolsIfMissing(mapView);
            var projectItemMapView = GetFocusedProjectItemMapView();

            if (projectItemMapView != null)
            {
                // get available layers from the map view and set them in the combobox ribbon
                IEnumerable<ILayer> layers = projectItemMapView.GetAvailableSpatialOperationLayers();
                var layer = layers.OfType<UnstructuredGridCellCoverageLayer>().SingleOrDefault(l =>
                                                                                                   Equals(l.Coverage, FlowModels.FirstOrDefault()?.InitialWaterLevel));
                if (layer != null && layer.Name != layer.Coverage.Name)
                {
                    layer.SetName(layer.Coverage.Name);
                }

            }



        }

        public override bool CanCopy(IProjectItem item)
        {
            if (item is WaterFlowFMModel)
                return false;
            return true;
        }

        public override bool CanCut(IProjectItem item)
        {
            return CanCopy(item);
        }

        private static void SetLateralSourceCompartmentComboBoxTypeEditor(VectorLayerAttributeTableView view)
        {
            var model1DBoundaryNode = view.TableView.CurrentFocusedRowObject as Model1DLateralSourceData;
            var list = Enumerable.Empty<ICompartment>();
            if (model1DBoundaryNode == null) return;
            var node = Math.Abs(model1DBoundaryNode.Feature.Branch.Length - model1DBoundaryNode.Feature.Chainage) < 0.001 ? model1DBoundaryNode.Feature.Branch.Target :
                Math.Abs(model1DBoundaryNode.Feature.Chainage) < 0.001 ? model1DBoundaryNode.Feature.Branch.Source : null;
            if (node is Manhole manhole)
                list = manhole.Compartments;

            var column = view.TableView.Columns.FirstOrDefault(c => c.Caption.Equals(nameof(Model1DLateralSourceData.Compartment), StringComparison.InvariantCultureIgnoreCase));
            if (column != null)
                column.Editor = new ComboBoxTypeEditor
                {

                    Items = list,
                    ItemsMandatory = false
                };
        }
        private object GetPipesFromSourcesAndSinks(FmModelTreeShortcut treeShortCut)
        {
            var sourcesAndSinks = treeShortCut.Data as IEventedList<SourceAndSink>;
            if (sourcesAndSinks == null) return null;
            var model = FlowModels.FirstOrDefault(m => Equals(sourcesAndSinks, m.SourcesAndSinks));
            return model == null ? null : model.Pipes;
        }

        private ViewInfo<TImporter, Feature2DImportExportDialog> GetFeature2DImportDialogViewInfo<TImporter>()
            where TImporter : IFeature2DImporterExporter, new()
        {
            return new ViewInfo<TImporter, Feature2DImportExportDialog>
            {
                AfterCreate = (v, o) =>
                    {
                        var model = FlowModels.FirstOrDefault();
                        v.ModelCoordinateSystem = model == null ? null : model.CoordinateSystem;
                        v.ImportMode = o.Mode == Feature2DImportExportMode.Import;
                        v.FileFilter = o.FileFilter;
                    }
            };
        }

        private ViewInfo<TImporter, Feature2DImportExportDialog> GetGisToFeature2DImportDialogViewInfo<TImporter>()
    where TImporter : MapFeaturesImporterBase, new()
        {
            return new ViewInfo<TImporter, Feature2DImportExportDialog>
            {
                AfterCreate = (v, o) =>
                {
                    var model = FlowModels.FirstOrDefault();

                    v.ModelCoordinateSystem = model == null ? null : model.CoordinateSystem;
                    v.Title = o.Name;
                    v.FileFilter = o.FileFilter;
                    v.ImportMode = true;
                }
            };
        }

        private BoundaryConditionSet FindSetForBoundaryCondition(BoundaryCondition arg)
        {
            var model = FlowModels.FirstOrDefault(m => m.BoundaryConditionSets.SelectMany(bcs => bcs.BoundaryConditions).Contains(arg));
            if (model == null)
                return null;

            return model.BoundaryConditionSets.FirstOrDefault(
                bcs => bcs.BoundaryConditions.Contains(arg));
        }

        private bool IsModelBoundary(Feature2D arg)
        {
            return FlowModels.Any(m => m.Boundaries.Contains(arg));
        }

        private BoundaryConditionSet FindSetForBoundary(Feature2D arg)
        {
            var model = FlowModels.FirstOrDefault(m => m.Boundaries.Contains(arg));
            if (model != null)
                return model.BoundaryConditionSets.First(bcs => Equals(bcs.Feature, arg));
            return null;
        }

        private bool IsModelPipe(Feature2D arg)
        {
            return FlowModels.Any(m => m.Pipes.Contains(arg));
        }

        private SourceAndSink FindDataForPipe(Feature2D arg)
        {
            var model = FlowModels.FirstOrDefault(m => m.Pipes.Contains(arg));
            if (model != null)
                return model.SourcesAndSinks.First(bcs => Equals(bcs.Feature, arg));
            return null;
        }

        private DateTime? GetModelStartTime()
        {
            var model = FlowModels.FirstOrDefault();
            return model != null ? model.StartTime : (DateTime?)null;
        }

        private DateTime? GetModelStopTime()
        {
            var model = FlowModels.FirstOrDefault();
            return model != null ? model.StopTime : (DateTime?)null;
        }

        private TimeSpan? GetModelTimeStep()
        {
            var model = FlowModels.FirstOrDefault();
            return model != null ? model.TimeStep : (TimeSpan?)null;
        }

        private void SubscribeToProjectEvents()
        {
            if (base.Gui == null || base.Gui.Application == null) return;
            base.Gui.Application.ProjectService.ProjectOpened += SubscribeToProjectPropertyChanged;
            base.Gui.Application.ProjectService.ProjectCreated += SubscribeToProjectPropertyChanged;
            base.Gui.Application.ProjectService.ProjectClosing += UnsubscribeToProjectPropertyChanged;
            base.Gui.Application.ProjectService.ProjectSaving += ApplicationOnProjectSaving;
            base.Gui.Application.ProjectService.ProjectSaved += ApplicationOnProjectSaved;
            base.Gui.Application.FileImporters.OfType<RasterFileImporter>().ForEach(rfi => rfi.MakeLayerVisibleAfterImport = MakeLayerVisibleAfterImport);

            var project = base.Gui.Application.Project;
            if (project != null)
            {
                SubscribeToProjectPropertyChanged(project);
            }
        }

        private void SubscribeToActivityEvents()
        {
            Gui.Application.ActivityRunner.ActivityStatusChanged += OnActivityRunnerStatusChanged;
        }

        private void UnsubscribeToProjectEvents()
        {
            if (base.Gui == null || base.Gui.Application == null) return;
            base.Gui.Application.ProjectService.ProjectOpened -= SubscribeToProjectPropertyChanged;
            base.Gui.Application.ProjectService.ProjectCreated -= SubscribeToProjectPropertyChanged;
            base.Gui.Application.ProjectService.ProjectClosing -= UnsubscribeToProjectPropertyChanged;
            base.Gui.Application.ProjectService.ProjectSaving -= ApplicationOnProjectSaving;
            base.Gui.Application.ProjectService.ProjectSaved -= ApplicationOnProjectSaved;
            var project = base.Gui.Application.Project;
            if (project != null)
            {
                UnsubscribeToProjectPropertyChanged(project);
            }
        }

        private void UnSubscribeToActivityEvents()
        {
            Gui.Application.ActivityRunner.ActivityStatusChanged -= OnActivityRunnerStatusChanged;
        }
        
        private void SubscribeToProjectPropertyChanged(object sender, EventArgs<Project> e)
        {
            SubscribeToProjectPropertyChanged(e.Value);
        }

        private void SubscribeToProjectPropertyChanged(Project project)
        {
            if (project == null) return;
            ((INotifyPropertyChange)project).PropertyChanging += ProjectPropertyChanging;
            ((INotifyPropertyChanged)project).PropertyChanged += ProjectPropertyChanged;
        }

        private void UnsubscribeToProjectPropertyChanged(object sender, EventArgs<Project> e)
        {
            UnsubscribeToProjectPropertyChanged(e.Value);
        }

        private void UnsubscribeToProjectPropertyChanged(Project project)
        {
            if (project == null) return;
            ((INotifyPropertyChange)project).PropertyChanging -= ProjectPropertyChanging;
            ((INotifyPropertyChanged)project).PropertyChanged -= ProjectPropertyChanged;
        }

        void ProjectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender is WaterFlowFMModel))
                return; //early exit

            if (!e.PropertyName.Equals(CoordinateSystemMemberName))
            {
                return;
            }

            var model = sender as WaterFlowFMModel;
            if (!model.WriteSnappedFeatures) return;

            // Set coordinate system to OutputSnappedFeatures
            var mapViews = Gui.DocumentViews.OfType<ProjectItemMapView>().Where(m => (m.Data as WaterFlowFMModel) == model);
            foreach (var mapView in mapViews)
            {
                var modelLayer = mapView.MapView.GetLayerForData(model);
                var groupModelLayer = modelLayer as GroupLayer;
                if (groupModelLayer != null)
                {
                    var snappedOutputLayer = groupModelLayer.Layers.FirstOrDefault(l => l.Name == FlowFMLayerNames.OutputSnappedFeaturesLayerName) as GroupLayer;
                    if (snappedOutputLayer == null) continue;

                    snappedOutputLayer.Layers.ForEach(l => l.DataSource.CoordinateSystem = model.CoordinateSystem);
                }
            }
        }

        void ProjectPropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            if (!(sender is WaterFlowFMModel))
                return; //early exit

            if (e.PropertyName.Equals(OutputHisFileStoreMemberName))
            {

                var fmHisFileFunctionStore = ((WaterFlowFMModel)sender).OutputHisFileStore;
                if (fmHisFileFunctionStore != null)
                {
                    CloseViewDataForOutdatedStore(fmHisFileFunctionStore);
                }
            }

            if (e.PropertyName.Equals(HeatFluxModelTypeMemberName))
            {
                var heatFluxModel = ((WaterFlowFMModel)sender).ModelDefinition.HeatFluxModel;
                if (heatFluxModel != null)
                {
                    CloseViewsForOutDatedHeatFluxModel(heatFluxModel);
                }
            }
        }
        private void ApplicationOnProjectSaving(object sender, EventArgs<Project> e)
        {
            foreach (WaterFlowFMModel model in FlowModels)
            {
                FreeSnappedOutputLayers(model);
                model.OutputSnappedFeaturesPathPropertyChanged += OnModelOutputSnappedFeaturesPathPropertyChanged;
            }
        }

        private void ApplicationOnProjectSaved(object sender, EventArgs<Project> e)
        {
            foreach (WaterFlowFMModel model in FlowModels)
            {
                model.OutputSnappedFeaturesPathPropertyChanged -= OnModelOutputSnappedFeaturesPathPropertyChanged;
            }
        }

        private void OnModelOutputSnappedFeaturesPathPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var waterflowFmModel = sender as WaterFlowFMModel;
            if (waterflowFmModel == null ||
                !Equals(e.PropertyName, nameof(waterflowFmModel.OutputSnappedFeaturesPath)))
            {
                return;
            }

            UpdateOutputSnappedFeaturesPaths(waterflowFmModel);
        }

        private void UpdateOutputSnappedFeaturesPaths(WaterFlowFMModel waterflowFmModel)
        {
            string targetDirectory = waterflowFmModel.OutputSnappedFeaturesPath;
            foreach (ShapeFile shapeFile in GetShapeFilesOfSnappedOutputLayersForModel(waterflowFmModel))
            {
                string fileName = Path.GetFileName(shapeFile.Path);
                string targetPath = targetDirectory != null && fileName != null
                                        ? Path.Combine(targetDirectory, fileName)
                                        : null;

                shapeFile.Close();
                if (targetPath != null)
                {
                    shapeFile.Path = targetPath;
                }
            }
        }

        private void FreeSnappedOutputLayers(WaterFlowFMModel model)
        {
            GetShapeFilesOfSnappedOutputLayersForModel(model).ForEach(s => s.Close());
        }

        private IEnumerable<ShapeFile> GetShapeFilesOfSnappedOutputLayersForModel(WaterFlowFMModel model)
        {
            IEnumerable<ILayer> layers = Gui.DocumentViews
                                            .OfType<ProjectItemMapView>()
                                            .Select(m => m.MapView.GetLayerForData(model))
                                            .Where(l => l != null);

            return layers.SelectMany(GetOutputSnappedFeaturesLayerShapeFiles);
        }

        private static IEnumerable<ShapeFile> GetOutputSnappedFeaturesLayerShapeFiles(ILayer modelLayer)
        {
            var groupModelLayer = modelLayer as GroupLayer;
            if (groupModelLayer == null)
            {
                return Enumerable.Empty<ShapeFile>();
            }

            var snappedOutputLayer =
                groupModelLayer.Layers.FirstOrDefault(l => l.Name == FlowFMLayerNames.OutputSnappedFeaturesLayerName) as
                    GroupLayer;
            if (snappedOutputLayer == null)
            {
                return Enumerable.Empty<ShapeFile>();
            }

            return snappedOutputLayer.Layers.Select(l => l.DataSource).OfType<ShapeFile>();
        }
        
        private void OnActivityRunnerStatusChanged(object sender, ActivityStatusChangedEventArgs activityStatusChangedEventArgs)
        {
            switch (sender)
            {
                case FileImportActivity fileImportActivity:
                {
                    OnImporterActivityChanged(activityStatusChangedEventArgs, fileImportActivity.FileImporter);
                    break;
                }
                case WaterFlowFMModel model:
                {
                    if (activityStatusChangedEventArgs.NewStatus == ActivityStatus.Initializing && model.WriteSnappedFeatures)
                    {
                        CleanOutputSnappedLayersAndReleaseFileLocks(model);
                    }

                    if (activityStatusChangedEventArgs.NewStatus == ActivityStatus.Failed)
                    {
                        ShowValidationView(model);
                    }

                    break;
                }
            }
        }

        private void OnImporterActivityChanged(ActivityStatusChangedEventArgs activityStatusChangedEventArgs, IFileImporter importer)
        {
            if (activityStatusChangedEventArgs.NewStatus != ActivityStatus.Finished) return;

            switch (importer)
            {
                case IFeature2DImporterExporter _:
                case FlowFMNetFileImporter _:
                case RasterFileImporter _:
                case WaterFlowFMFileImporter _:
                    RefreshTreeViewAndZoomActiveMapViewToExtends();
                    break;
            }
        }

        [InvokeRequired]
        private void CleanOutputSnappedLayersAndReleaseFileLocks(WaterFlowFMModel model)
        {
            // The Snapped Output Layers use a ShapeFile as their DataSource. The ShapeFile
            // keeps it files actively locked (for whatever reason). As such it needs to
            // be closed, before it can be deleted, otherwise it will result in a locked file
            // exception.
            // For whatever ungodly reason, instead of implementing an actual functioning 
            // approach to dealing with this, it was opted to fix this here in GuiPlugin,
            // and just query all views to check if we have open views with ShapeFiles. smh.

            //Clean output snapped layers;
            // release file locks
            IEnumerable<ProjectItemMapView> mapViews =
                Gui.DocumentViews
                   .OfType<ProjectItemMapView>()
                   .Where(m => (m.Data as WaterFlowFMModel) == model);

            foreach (ProjectItemMapView mapView in mapViews)
            {
                ILayer modelLayer = mapView.MapView.GetLayerForData(model);
                // Currently, it is possible to have the shape files nested under the model layer.
                // As such we need to search recursively in all children.
                // Note that this fails horribly if let users change the names of layers and they
                // opt to being cheeky.
                var snappedOutputLayer =
                    FindLayerByNameRecursively(modelLayer, FlowFMLayerNames.OutputSnappedFeaturesLayerName)
                        as IGroupLayer;
                snappedOutputLayer?.Layers
                                  .Select(l => l.DataSource)
                                  .OfType<ShapeFile>()
                                  .ForEach(sf => sf.Close());
            }
        }

        private static ILayer FindLayerByNameRecursively(ILayer rootLayer, string layerName)
        {
            var layers = new Queue<ILayer>();
            layers.Enqueue(rootLayer);

            while (layers.Any())
            {
                ILayer nextLayer = layers.Dequeue();

                if (nextLayer.Name == layerName)
                {
                    return nextLayer;
                }

                if (nextLayer is IGroupLayer groupLayer)
                {
                    groupLayer.Layers.ForEach(layers.Enqueue);
                }
            }

            return null;
        }

        [InvokeRequired]
        private void ShowValidationView(object sender)
        {
            Gui.CommandHandler.OpenView(sender, typeof(ValidationView));
        }

        [InvokeRequired]
        private void RefreshTreeViewAndZoomActiveMapViewToExtends()
        {
            Gui.MainWindow.ProjectExplorer.TreeView.Refresh();
            ActiveMapView?.Map.ZoomToExtents();
        }

        [InvokeRequired]
        private void CloseViewDataForOutdatedStore(FMHisFileFunctionStore fmHisFileFunctionStore)
        {
            var datas = Gui.DocumentViews.Select(v => v.Data).ToList();

            var obsoleteData =
                datas.OfType<IFunction>().Where(f => ReferenceEquals(f.Store, fmHisFileFunctionStore)).Cast<object>().ToList();

            obsoleteData.AddRange(
                datas.OfType<IList<IFunction>>()
                     .Where(l => l.Any(f => ReferenceEquals(f.Store, fmHisFileFunctionStore))));

            foreach (var data in obsoleteData)
            {
                Gui.CommandHandler.RemoveAllViewsForItem(data);
            }
        }

        [InvokeRequired]
        private void CloseViewsForOutDatedHeatFluxModel(HeatFluxModel heatFluxModel)
        {
            var datas = Gui.DocumentViews.Select(v => v.Data).ToList();

            var obsoleteData = datas.OfType<HeatFluxModel>().Where(hfm => hfm.Equals(heatFluxModel));

            foreach (var data in obsoleteData)
            {
                Gui.CommandHandler.RemoveAllViewsForItem(data);
            }
        }

        private void MakeLayerVisibleAfterImport(object layerData)
        {
            var layer = GetActiveMapView()?.GetLayerForData(layerData);
            if (layer != null)
            {
                layer.Visible = true;
            }
        }

        private MapView GetActiveMapView()
        {
            if (Gui == null)
                return null;

            var activeView = Gui.DocumentViews.ActiveView;
            if (activeView == null)
                return null;

            var mapView = FindMapView(activeView);
            if (mapView != null)
                return mapView;
            return null;
        }

        private ProjectItemMapView GetFocusedProjectItemMapView()
        {
            var mapView = GetFocusedMapControl();
            if (mapView != null)
            {
                var projectItemMapViews = Gui.DocumentViews.AllViews.OfType<ProjectItemMapView>().ToList();

                // could be null
                return projectItemMapViews.FirstOrDefault(p => p.MapView.MapControl == mapView);
            }
            else return null;
        }
        internal MapView GetFocusedMapView(IView view = null)
        {
            var viewToSearch = view ?? Gui?.DocumentViews?.ActiveView;
            return viewToSearch?.GetViewsOfType<MapView>().FirstOrDefault();
        }

        internal MapControl GetFocusedMapControl(IView view = null)
        {
            return GetFocusedMapView(view)?.MapControl;
        }
        private static MapView FindMapView(IView activeView)
        {
            var mapView = activeView as MapView;

            if (mapView != null)
                return mapView;

            var compositeView = activeView as ICompositeView;
            
            return compositeView != null ? compositeView.ChildViews.OfType<MapView>().FirstOrDefault() : null;
        }

        private static PropertyInfo CreatePropertyInfoDynamic<T>()
        {
            return new PropertyInfo<T, DynamicObjectProperties>();
        }
    }
}
