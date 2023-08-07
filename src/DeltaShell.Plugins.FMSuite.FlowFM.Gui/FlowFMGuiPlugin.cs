using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Resources;
using DelftTools.Controls;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Forms;
using DelftTools.Shell.Gui.Swf.Validation;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.Common.Gui.Restart;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.Gui;
using DeltaShell.Plugins.FMSuite.Common.IO.ImportExport;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors.ModelFeatureCoordinateDataEditor;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.ImportersExporters;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Restart;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.CoverageViews;
using Mono.Addins;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api.Layers;
using SharpMap.Data.Providers;
using SharpMap.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui
{
    [Extension(typeof(IPlugin))]
    public class FlowFMGuiPlugin : GuiPlugin
    {
        private static readonly string CoordinateSystemMemberName = nameof(WaterFlowFMModel.CoordinateSystem);

        private static readonly string OutputHisFileStoreMemberName = nameof(WaterFlowFMModel.OutputHisFileStore);

        private static readonly string HeatFluxModelTypeMemberName = nameof(WaterFlowFMModel.HeatFluxModelType);

        private static Func<MapView> getActiveMapViewFunc;

        private TableViewTimeSeriesGeneratorTool tableViewTimeSeriesGeneratorTool;
        private string _fmModelSettingsSuffix = " (FM model settings)";

        public FlowFMGuiPlugin()
        {
            getActiveMapViewFunc = GetActiveMapView;
        }

        public override string Name => "Delft3D FM (Gui)";

        public override string DisplayName => "D-Flow Flexible Mesh Plugin (UI)";

        public override string Description => FlowFM.Properties.Resources.FlowFMApplicationPlugin_Description;

        public override string Version => AssemblyUtils.GetAssemblyInfo(GetType().Assembly).Version;

        public override string FileFormatVersion => "1.1.0.0";

        public override ResourceManager Resources { get; set; }

        public override IMapLayerProvider MapLayerProvider => new FlowFMMapLayerProvider();

        public override IGui Gui
        {
            get
            {
                return base.Gui;
            }
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
                    Gui.UndoRedoManager.Enabled = false;
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
            }
        }

        public override IRibbonCommandHandler RibbonCommandHandler => new Ribbon.Ribbon();

        public static MapView ActiveMapView => getActiveMapViewFunc();

        /// <summary>
        /// Gets the project TreeView node presenters.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<ITreeNodePresenter> GetProjectTreeViewNodePresenters()
        {
            yield return new WaterFlowFMModelNodePresenter(this);
            yield return new FmModelTreeShortcutNodePresenter { GuiPlugin = this };
            yield return new BoundaryConditionSetNodePresenter { GuiPlugin = this };
            yield return new SourceSinkNodePresenter { GuiPlugin = this };
            yield return new FMMapFileFunctionStoreNodePresenter { GuiPlugin = this };
            yield return new FMHisFileFunctionStoreNodePresenter();
            yield return new FMClassMapFileFunctionStoreNodePresenter { GuiPlugin = this };
            yield return new ImportedFMNetFileNodePresenter { GuiPlugin = this };
            yield return new HeatFluxModelNodePresenter { GuiPlugin = this };
            yield return new WindItemListNodePresenter { GuiPlugin = this };
            yield return new WindItemNodePresenter { GuiPlugin = this };
            yield return new RestartFileNodePresenter<WaterFlowFMRestartFile>(this);

            yield return new Feature2DPolygonTreeViewNodePresenter { GuiPlugin = this };
            yield return new FeatureProjectTreeViewNodePresenter<LandBoundary2D>(HydroAreaLayerNames.LandBoundariesPluralName, Properties.Resources.landboundary) { GuiPlugin = this };
            yield return new FeatureProjectTreeViewNodePresenter<GroupablePointFeature>(HydroAreaLayerNames.DryPointsPluralName, Properties.Resources.dry_point) { GuiPlugin = this };
            yield return new FeatureProjectTreeViewNodePresenter<ThinDam2D>(HydroAreaLayerNames.ThinDamsPluralName, Properties.Resources.thindam) { GuiPlugin = this };
            yield return new FeatureProjectTreeViewNodePresenter<FixedWeir>(HydroAreaLayerNames.FixedWeirsPluralName, Properties.Resources.fixedweir) { GuiPlugin = this };
            yield return new FeatureProjectTreeViewNodePresenter<GroupableFeature2DPoint>(HydroAreaLayerNames.ObservationPointsPluralName, Properties.Resources.Observation) { GuiPlugin = this };
            yield return new FeatureProjectTreeViewNodePresenter<ObservationCrossSection2D>(HydroAreaLayerNames.ObservationCrossSectionsPluralName, Properties.Resources.observationcs2d) { GuiPlugin = this };
            yield return new FeatureProjectTreeViewNodePresenter<Pump>(HydroAreaLayerNames.PumpsPluralName, Properties.Resources.Pump) { GuiPlugin = this };
            yield return new FeatureProjectTreeViewNodePresenter<Structure>(HydroAreaLayerNames.StructuresPluralName, Properties.Resources.Weir) { GuiPlugin = this };
            yield return new FeatureProjectTreeViewNodePresenter<BridgePillar>(HydroAreaLayerNames.BridgePillarsPluralName, Properties.Resources.BridgeSmall) { GuiPlugin = this };
        }

        public override IEnumerable<ViewInfo> GetViewInfoObjects()
        {
            yield return new ViewInfo<WaterFlowFMModel, WpfSettingsView>
            {
                Description = "FM Settings",
                GetViewName = (v, o) => o.Name + _fmModelSettingsSuffix,
                AfterCreate = ConfigureWpfSettingsView
            };

            yield return new ViewInfo<FmModelTreeShortcut, WaterFlowFMModel, WpfSettingsView>
            {
                Description = "FM Settings",
                AdditionalDataCheck = o => o.ShortCutType == ShortCutType.SettingsTab,
                GetViewData = o => o.FlowFmModel,
                GetViewName = (v, o) => o.Name + _fmModelSettingsSuffix,
                OnActivateView = (v, o) =>
                {
                    var shortcut = o as FmModelTreeShortcut;
                    if (shortcut == null)
                    {
                        return;
                    }

                    v.EnsureVisible(shortcut.Value);
                },
                AfterCreate = (v, o) => ConfigureWpfSettingsView(v, o.FlowFmModel)
            };

            yield return new ViewInfo<FmValidationShortcut, WaterFlowFMModel, WpfSettingsView>
            {
                Description = "FM Settings",
                GetViewData = o => o.FlowFmModel,
                GetViewName = (v, o) => o.Name + _fmModelSettingsSuffix,
                OnActivateView = (v, o) =>
                {
                    var shortcut = o as FmValidationShortcut;
                    if (shortcut == null)
                    {
                        return;
                    }

                    v.EnsureVisible(shortcut.TabName);
                },
                AfterCreate = (v, o) => ConfigureWpfSettingsView(v, o.FlowFmModel)
            };

            yield return new ViewInfo<WaterFlowFMModel, WaterFlowFMFileStructureView> { Description = "File tree" };

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

            yield return new ViewInfo<FileBasedFeatureCoverage, CoverageView>
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
                    v.OnValidate = d => (d as WaterFlowFMModel)?.Validate();
                }
            };

            // Boundary conditions
            var boundaryConditionSetViewInfo = new ViewInfo<BoundaryConditionSet, BoundaryConditionEditor>
            {
                Description = "Boundary Data Editor",
                AfterCreate = (v, o) =>
                {
                    WaterFlowFMModel model = FlowModels.FirstOrDefault(m => m.BoundaryConditionSets.Contains(o));
                    if (model == null)
                    {
                        return;
                    }

                    // Retrieve the current selected boundary condition. As soon as the controller is set, 
                    // the selected category from the previous screen defaults back to the first entry of 
                    // the FlowBoundaryQuantityType.
                    string currentSelectedCategory = v.SelectedCategory;

                    v.BoundaryConditionFactory = new FlowBoundaryConditionFactory { Model = model };

                    var controller = new FlowBoundaryConditionEditorController { Model = model };
                    v.Controller = controller;
                    v.BoundaryConditionPropertiesControl = new FlowBoundaryConditionPropertiesControl { Controller = controller };

                    v.ShowSupportPointNames = true;

                    IBoundaryCondition boundaryConditionToSelect;
                    if (!string.IsNullOrEmpty(currentSelectedCategory))
                    {
                        boundaryConditionToSelect = o.BoundaryConditions.FirstOrDefault(
                            bc => string.Equals(bc.ProcessName, currentSelectedCategory));
                    }
                    else
                    {
                        boundaryConditionToSelect = o.BoundaryConditions.FirstOrDefault();
                    }

                    // This can occur when the BoundaryConditionSet does not contain a 
                    // boundary condition with an earlier selected name. Setting the 
                    // selected category to the initial selected category will force the 
                    // old window to retain the current selection. 
                    if (boundaryConditionToSelect == null)
                    {
                        // If both the boundaryConditionToSelect and the currentSelectedCategory
                        // are null, return to prevent a null/empty option in the dropdown.
                        if (currentSelectedCategory == null)
                        {
                            return;
                        }

                        v.SelectedCategory = currentSelectedCategory;
                    }
                    else
                    {
                        v.SelectedCategory = boundaryConditionToSelect.ProcessName;
                    }

                    v.SelectedBoundaryCondition = boundaryConditionToSelect;
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
                    ProjectItemMapView centralMap = Gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault(vi => vi.MapView.GetLayerForData(o) != null);
                    if (centralMap == null)
                    {
                        return;
                    }

                    v.OpenViewMethod = feature => Gui.CommandHandler.OpenView(feature);
                    v.ZoomToFeature = feature => centralMap.MapView.EnsureVisible(o.FirstOrDefault(bcs => bcs.BoundaryConditions.Contains(feature as IBoundaryCondition)));
                }
            };

            yield return allBoundarySetsViewInfo;

            yield return ViewInfoWrapper<FmModelTreeShortcut>.Create(allBoundarySetsViewInfo, o => o.Value, o => o.ShortCutType == ShortCutType.FeatureSet);

            yield return FeatureCollectionViewInfoHelper.CreateViewInfo<Feature2D, WaterFlowFMModel>("Boundaries", m => m.Boundaries, () => Gui);

            // Sources and sinks
            var sourceAndSinkViewInfo = new ViewInfo<SourceAndSink, SourceAndSinkView>
            {
                CloseForData = (v, o) => Equals(v.Data, o),
                AfterCreate = (v, o) =>
                {
                    v.Model = FlowModels.FirstOrDefault(m => m.SourcesAndSinks.Contains(o));
                    var tableViewGenerator = new TableViewTimeSeriesGeneratorTool
                    {
                        GetStartTime = GetModelStartTime,
                        GetStopTime = GetModelStopTime,
                        GetTimeStep = () => new TimeSpan(0, 12, 0, 0)
                    };
                    tableViewGenerator.ConfigureTableView(v.FunctionView.TableView);
                }
            };
            yield return sourceAndSinkViewInfo;

            yield return ViewInfoWrapper<Feature2D>.Create(sourceAndSinkViewInfo, FindDataForPipe, IsModelPipe);

            ViewInfo<IEventedList<Feature2D>, ILayer, VectorLayerAttributeTableView> pipesViewInfo = FeatureCollectionViewInfoHelper.CreateViewInfo<Feature2D, WaterFlowFMModel>("Sources and Sinks", m => m.Pipes, () => Gui);
            yield return ViewInfoWrapper<FmModelTreeShortcut>.Create(pipesViewInfo, GetPipesFromSourcesAndSinks, o => o.ShortCutType == ShortCutType.FeatureSet, (v, o) => v.CanAddDeleteAttributes = false);

            // Heat flux model
            yield return new ViewInfo<HeatFluxModel, HeatFluxModelView>
            {
                GetViewData = m =>
                {
                    if (m == null)
                    {
                        return null;
                    }

                    WaterFlowFMModel waterFlowFMModel = FlowModels.FirstOrDefault(wfm => Equals(wfm.ModelDefinition.HeatFluxModel, m));

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

            yield return new ViewInfo<GriddedWindField, GriddedWindView> { AdditionalDataCheck = t => FlowModels.FirstOrDefault(m => m.WindFields.Contains(t)) != null };

            yield return new ViewInfo<SpiderWebWindField, GriddedWindView> { AdditionalDataCheck = t => FlowModels.FirstOrDefault(m => m.WindFields.Contains(t)) != null };

            // Importers and exporters
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

            yield return GetFeature2DImportDialogViewInfo<PliFileImporterExporter<FixedWeir, FixedWeir>>();
            yield return GetFeature2DImportDialogViewInfo<PliFileImporterExporter<ObservationCrossSection2D, ObservationCrossSection2D>>();
            yield return GetFeature2DImportDialogViewInfo<PliFileImporterExporter<ThinDam2D, ThinDam2D>>();
            yield return GetFeature2DImportDialogViewInfo<PliFileImporterExporter<Structure, Feature2D>>();
            yield return GetFeature2DImportDialogViewInfo<PliFileImporterExporter<Pump, Feature2D>>();
            yield return GetFeature2DImportDialogViewInfo<PliFileImporterExporter<SourceAndSink, Feature2D>>();
            yield return GetFeature2DImportDialogViewInfo<PliFileImporterExporter<BoundaryConditionSet, Feature2D>>();
            yield return GetFeature2DImportDialogViewInfo<PliFileImporterExporter<BridgePillar, BridgePillar>>();

            yield return GetFeature2DImportDialogViewInfo<PointFileImporterExporter>();
            yield return GetFeature2DImportDialogViewInfo<PolFileImporterExporter>();
            yield return GetFeature2DImportDialogViewInfo<LdbFileImporterExporter>();
        }

        public override IEnumerable<PropertyInfo> GetPropertyInfos()
        {
            yield return CreatePropertyInfoDynamic<WaterFlowFMModel>();
            yield return CreatePropertyInfoDynamic<PointCloudLayer>();
            yield return new PropertyInfo<Structure, FMWeirProperties> { AdditionalDataCheck = w => FlowModels.Any(m => m.Area.Structures.Contains(w)) };
        }

        public override void OnActiveViewChanged(IView view)
        {
            if (view == null)
            {
                return;
            }

            MapView mapView = FindMapView(view);
            if (mapView != null)
            {
                FlowFMMapViewDecorator.AddMapToolsIfMissing(mapView);
            }
        }

        public override bool CanCopy(IProjectItem item)
        {
            if (item is WaterFlowFMModel)
            {
                return false;
            }

            return true;
        }

        public override bool CanCut(IProjectItem item)
        {
            return CanCopy(item);
        }

        private IEnumerable<WaterFlowFMModel> FlowModels
        {
            get
            {
                return Gui == null ? Enumerable.Empty<WaterFlowFMModel>() : Gui.Application.GetAllModelsInProject().OfType<WaterFlowFMModel>();
            }
        }

        private object GetPipesFromSourcesAndSinks(FmModelTreeShortcut treeShortCut)
        {
            var sourcesAndSinks = treeShortCut.Value as IEventedList<SourceAndSink>;
            if (sourcesAndSinks == null)
            {
                return null;
            }

            WaterFlowFMModel model = FlowModels.FirstOrDefault(m => Equals(sourcesAndSinks, m.SourcesAndSinks));
            return model == null ? null : model.Pipes;
        }

        private ViewInfo<TImporter, Feature2DImportExportDialog> GetFeature2DImportDialogViewInfo<TImporter>()
            where TImporter : IFeature2DImporterExporter, new()
        {
            return new ViewInfo<TImporter, Feature2DImportExportDialog>
            {
                AfterCreate = (v, o) =>
                {
                    WaterFlowFMModel model = FlowModels.FirstOrDefault();

                    v.ModelCoordinateSystem = model == null ? null : model.CoordinateSystem;
                    v.ImportMode = o.Mode == Feature2DImportExportMode.Import;
                    v.FileFilter = o.FileFilter;
                }
            };
        }

        private BoundaryConditionSet FindSetForBoundaryCondition(BoundaryCondition arg)
        {
            WaterFlowFMModel model = FlowModels.FirstOrDefault(m => m.BoundaryConditionSets.SelectMany(bcs => bcs.BoundaryConditions).Contains(arg));
            if (model == null)
            {
                return null;
            }

            return model.BoundaryConditionSets.FirstOrDefault(
                bcs => bcs.BoundaryConditions.Contains(arg));
        }

        private bool IsModelBoundary(Feature2D arg)
        {
            return FlowModels.Any(m => m.Boundaries.Contains(arg));
        }

        private BoundaryConditionSet FindSetForBoundary(Feature2D arg)
        {
            WaterFlowFMModel model = FlowModels.FirstOrDefault(m => m.Boundaries.Contains(arg));
            if (model != null)
            {
                return model.BoundaryConditionSets.First(bcs => Equals(bcs.Feature, arg));
            }

            return null;
        }

        private bool IsModelPipe(Feature2D arg)
        {
            return FlowModels.Any(m => m.Pipes.Contains(arg));
        }

        private SourceAndSink FindDataForPipe(Feature2D arg)
        {
            WaterFlowFMModel model = FlowModels.FirstOrDefault(m => m.Pipes.Contains(arg));
            if (model != null)
            {
                return model.SourcesAndSinks.First(bcs => Equals(bcs.Feature, arg));
            }

            return null;
        }

        private DateTime? GetModelStartTime()
        {
            WaterFlowFMModel model = FlowModels.FirstOrDefault();
            return model != null ? model.StartTime : (DateTime?)null;
        }

        private DateTime? GetModelStopTime()
        {
            WaterFlowFMModel model = FlowModels.FirstOrDefault();
            return model != null ? model.StopTime : (DateTime?)null;
        }

        private TimeSpan? GetModelTimeStep()
        {
            WaterFlowFMModel model = FlowModels.FirstOrDefault();
            return model != null ? model.TimeStep : (TimeSpan?)null;
        }

        private void SubscribeToProjectEvents()
        {
            if (base.Gui == null || base.Gui.Application == null)
            {
                return;
            }

            base.Gui.Application.ProjectOpened += SubscribeToProjectPropertyChanged;
            base.Gui.Application.ProjectClosing += UnsubscribeToProjectPropertyChanged;
            base.Gui.Application.ProjectSaving += ApplicationOnProjectSaving;
            base.Gui.Application.ProjectSaved += ApplicationOnProjectSaved;

            Project project = base.Gui.Application.Project;
            if (project != null)
            {
                SubscribeToProjectPropertyChanged(project);
            }

            base.Gui.Application.ProjectOpened += CleanFlowFmViewContextUponLoadingProjectHack;
        }

        private void SubscribeToActivityEvents()
        {
            Gui.Application.ActivityRunner.ActivityStatusChanged += OnActivityRunnerStatusChanged;
        }

        private void UnsubscribeToProjectEvents()
        {
            if (base.Gui == null || base.Gui.Application == null)
            {
                return;
            }

            base.Gui.Application.ProjectOpened -= SubscribeToProjectPropertyChanged;
            base.Gui.Application.ProjectClosing -= UnsubscribeToProjectPropertyChanged;

            Project project = base.Gui.Application.Project;
            if (project != null)
            {
                UnsubscribeToProjectPropertyChanged(project);
            }

            base.Gui.Application.ProjectOpened -= CleanFlowFmViewContextUponLoadingProjectHack;
        }

        /// <summary>
        /// This method has been added to "refresh" the min max values of the bathymetry.
        /// Due to the way they are currently handled, the wrong min and max data is loaded on
        /// the bed level. We fix this by setting the min max value of the bathymetry directly
        /// on the layer.
        /// </summary>
        /// <param name="project"> The project which is loaded. </param>
        /// <remarks>
        /// See issue: D3DFMIQ-1099.
        /// This is somewhat of a nuclear option, and should really be fixed when grid is read.
        /// </remarks>
        private static void CleanFlowFmViewContextUponLoadingProjectHack(Project project)
        {
            IEnumerable<IModel> models = project?.RootFolder?.Models?.Where(m => m is WaterFlowFMModel);
            if (models == null)
            {
                return;
            }

            foreach (IModel model in models)
            {
                if (!(model is WaterFlowFMModel fmModel))
                {
                    continue;
                }

                UnstructuredGridCoverage bathymetry = fmModel.SpatialData.Bathymetry;

                var viewContext = (project.ViewContextManager as GuiContextManager)?
                                  .GetViewContext(typeof(ProjectItemMapView), fmModel) as ProjectItemMapViewContext;

                GeneratedMapLayerInfo bedLevelLayer = viewContext?.GeneratedMapLayerInfoList?
                                                                 .Where(l => l != null && l.Name != null)
                                                                 .FirstOrDefault(l => l.Name.Equals("Bed Level"));

                if (bathymetry?.Components?[0]?.MinValue is double minValue &&
                    bathymetry?.Components?[0]?.MaxValue is double maxValue)
                {
                    bedLevelLayer?.Theme?.ScaleTo(minValue, maxValue);
                }
            }
        }

        private void UnSubscribeToActivityEvents()
        {
            Gui.Application.ActivityRunner.ActivityStatusChanged -= OnActivityRunnerStatusChanged;
        }

        private void SubscribeToProjectPropertyChanged(Project project)
        {
            if (project == null)
            {
                return;
            }

            ((INotifyPropertyChange)project).PropertyChanging += ProjectPropertyChanging;
            ((INotifyPropertyChanged)project).PropertyChanged += ProjectPropertyChanged;
        }

        private void UnsubscribeToProjectPropertyChanged(Project project)
        {
            if (project == null)
            {
                return;
            }

            ((INotifyPropertyChange)project).PropertyChanging -= ProjectPropertyChanging;
            ((INotifyPropertyChanged)project).PropertyChanged -= ProjectPropertyChanged;
        }

        private void ProjectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var waterFlowFmModel = sender as WaterFlowFMModel;
            if (waterFlowFmModel == null)
            {
                return;
            }

            string propertyName = e.PropertyName;

            if (propertyName.Equals(nameof(waterFlowFmModel.OutputSnappedFeaturesPath)))
            {
                UpdateOutputSnappedFeaturesPaths(waterFlowFmModel);
            }

            if (!propertyName.Equals(CoordinateSystemMemberName) ||
                !waterFlowFmModel.WriteSnappedFeatures)
            {
                return;
            }

            // Set coordinate system to OutputSnappedFeatures
            IEnumerable<ProjectItemMapView> mapViews = Gui.DocumentViews.OfType<ProjectItemMapView>().Where(m => m.Data as WaterFlowFMModel == waterFlowFmModel);
            foreach (ProjectItemMapView mapView in mapViews)
            {
                ILayer modelLayer = mapView.MapView.GetLayerForData(waterFlowFmModel);
                var groupModelLayer = modelLayer as GroupLayer;

                if (groupModelLayer == null)
                {
                    continue;
                }

                var snappedOutputLayer = groupModelLayer.Layers.FirstOrDefault(l => l.Name == FlowFMMapLayerProvider.OutputSnappedFeaturesLayerName) as GroupLayer;
                if (snappedOutputLayer == null)
                {
                    continue;
                }

                snappedOutputLayer.Layers.ForEach(l => l.DataSource.CoordinateSystem = waterFlowFmModel.CoordinateSystem);
            }
        }

        private void ProjectPropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            if (!(sender is WaterFlowFMModel))
            {
                return; //early exit
            }

            if (e.PropertyName.Equals(OutputHisFileStoreMemberName))
            {
                IFMHisFileFunctionStore fmHisFileFunctionStore = ((WaterFlowFMModel)sender).OutputHisFileStore;
                if (fmHisFileFunctionStore != null)
                {
                    CloseViewDataForOutdatedStore(fmHisFileFunctionStore);
                }
            }

            if (e.PropertyName.Equals(HeatFluxModelTypeMemberName))
            {
                HeatFluxModel heatFluxModel = ((WaterFlowFMModel)sender).ModelDefinition.HeatFluxModel;
                if (heatFluxModel != null)
                {
                    CloseViewsForOutDatedHeatFluxModel(heatFluxModel);
                }
            }
        }

        private void ApplicationOnProjectSaving(Project project)
        {
            foreach (WaterFlowFMModel model in FlowModels)
            {
                FreeSnappedOutputLayers(model);
                model.OutputSnappedFeaturesPathPropertyChanged += OnModelOutputSnappedFeaturesPathPropertyChanged;
            }
        }

        private void ApplicationOnProjectSaved(Project obj)
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
                groupModelLayer.Layers.FirstOrDefault(l => l.Name == FlowFMMapLayerProvider.OutputSnappedFeaturesLayerName) as
                    GroupLayer;
            if (snappedOutputLayer == null)
            {
                return Enumerable.Empty<ShapeFile>();
            }

            return snappedOutputLayer.Layers.Select(l => l.DataSource).OfType<ShapeFile>();
        }

        [InvokeRequired]
        private void OnActivityRunnerStatusChanged(object sender, ActivityStatusChangedEventArgs activityStatusChangedEventArgs)
        {
            IFileImporter importer = (sender as FileImportActivity)?.FileImporter;
            if (importer != null && activityStatusChangedEventArgs.NewStatus == ActivityStatus.Finished)
            {
                if (importer is FlowFMNetFileImporter ||
                    importer is IFeature2DImporterExporter ||
                    importer is WaterFlowFMFileImporter)
                {
                    ActiveMapView?.Map?.ZoomToExtents();
                }

                if (importer is WaterFlowFMFileImporter ||
                    importer is IFeature2DImporterExporter)
                {
                    Gui?.MainWindow?.ProjectExplorer?.TreeView?.Refresh();
                }

                return;
            }

            var model = sender as WaterFlowFMModel;
            if (model != null && model.WriteSnappedFeatures && activityStatusChangedEventArgs.NewStatus == ActivityStatus.Initializing)
            {
                FreeSnappedOutputLayers(model);
            }

            var fmModel = sender as WaterFlowFMModel;
            if (fmModel != null && fmModel.ValidateBeforeRun && activityStatusChangedEventArgs.NewStatus == ActivityStatus.Failed)
            {
                Gui.CommandHandler.OpenView(sender, typeof(ValidationView));
            }
        }

        [InvokeRequired]
        private void CloseViewDataForOutdatedStore(IFMHisFileFunctionStore fmHisFileFunctionStore)
        {
            List<object> datas = Gui.DocumentViews.Select(v => v.Data).ToList();

            List<object> obsoleteData =
                datas.OfType<IFunction>().Where(f => ReferenceEquals(f.Store, fmHisFileFunctionStore)).Cast<object>().ToList();

            obsoleteData.AddRange(
                datas.OfType<IList<IFunction>>()
                     .Where(l => l.Any(f => ReferenceEquals(f.Store, fmHisFileFunctionStore))));

            foreach (object data in obsoleteData)
            {
                Gui.CommandHandler.RemoveAllViewsForItem(data);
            }
        }

        [InvokeRequired]
        private void CloseViewsForOutDatedHeatFluxModel(HeatFluxModel heatFluxModel)
        {
            List<object> datas = Gui.DocumentViews.Select(v => v.Data).ToList();

            IEnumerable<HeatFluxModel> obsoleteData = datas.OfType<HeatFluxModel>().Where(hfm => hfm.Equals(heatFluxModel));

            foreach (HeatFluxModel data in obsoleteData)
            {
                Gui.CommandHandler.RemoveAllViewsForItem(data);
            }
        }

        private MapView GetActiveMapView()
        {
            if (Gui == null)
            {
                return null;
            }

            IView activeView = Gui.DocumentViews.ActiveView;
            if (activeView == null)
            {
                return null;
            }

            MapView mapView = FindMapView(activeView);
            if (mapView != null)
            {
                return mapView;
            }

            return null;
        }

        private static MapView FindMapView(IView activeView)
        {
            var mapView = activeView as MapView;

            if (mapView != null)
            {
                return mapView;
            }

            var compositeView = activeView as ICompositeView;

            return compositeView?.ChildViews.OfType<MapView>().FirstOrDefault();
        }

        private static PropertyInfo CreatePropertyInfoDynamic<T>()
        {
            return new PropertyInfo<T, DynamicObjectProperties>();
        }

        private void ConfigureWpfSettingsView(WpfSettingsView view, WaterFlowFMModel flowFmModel)
        {
            Func<object, string, string> fmSettingsPropertyChanged = (sender, propertyName) =>
            {
                if (sender is WaterFlowFMProperty property)
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

            ObservableCollection<WpfGuiCategory> wpfGuiCategories = WaterFlowFmSettingsHelper.GetWpfGuiCategories(flowFmModel, Gui);

            // Look for the time properties to synchronize the model updates with
            IEnumerable<WpfGuiProperty> guiProperties = wpfGuiCategories.SelectMany(gp => gp.Properties).ToArray();

            WpfGuiProperty[] propertiesToSynchronize =
            {
                guiProperties.Single(prop => string.Equals(prop.Name, KnownProperties.DtUser, StringComparison.OrdinalIgnoreCase)),
                guiProperties.Single(prop => string.Equals(prop.Name, KnownProperties.StopDateTime, StringComparison.OrdinalIgnoreCase)),
                guiProperties.Single(prop => string.Equals(prop.Name, KnownProperties.StartDateTime, StringComparison.OrdinalIgnoreCase))
            };

            view.SettingsCategories = wpfGuiCategories;
            view.SetSynchronizedProperties(propertiesToSynchronize);
            view.GetChangedPropertyName = fmSettingsPropertyChanged;
        }
    }
}