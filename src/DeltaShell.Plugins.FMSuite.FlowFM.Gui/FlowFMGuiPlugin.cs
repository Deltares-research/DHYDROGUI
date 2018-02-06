using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Resources;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Functions;
using DelftTools.Hydro;
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
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.Gui;
using DeltaShell.Plugins.FMSuite.Common.Gui.Editors;
using DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.Common.Gui.RgfGrid;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.CoverageViews;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using Mono.Addins;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using SharpMap.CoordinateSystems;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using FeatureCollectionViewInfoHelper = DeltaShell.Plugins.FMSuite.Common.Gui.FeatureCollectionViewInfoHelper;
using FixedWeir = DelftTools.Hydro.Structures.FixedWeir;
using LdbFile = SharpMap.Extensions.Data.Providers.LdbFile;
using ObservationCrossSection2D = DelftTools.Hydro.ObservationCrossSection2D;
using ThinDam2D = DelftTools.Hydro.Structures.ThinDam2D;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui
{
    [Extension(typeof (IPlugin))]
    public class FlowFMGuiPlugin : GuiPlugin
    {
        public override string Name
        {
            get { return "Delft3D FM (Gui)"; }
        }

        public override string DisplayName
        {
            get { return "D-Flow Flexible Mesh Plugin (UI)"; }
        }

        public override string Description
        {
            get { return FlowFM.Properties.Resources.FlowFMApplicationPlugin_Description; }
        }

        public override string Version
        {
            get { return GetType().Assembly.GetName().Version.ToString(); }
        }

        public override string FileFormatVersion
        {
            get { return "1.1.0.0"; }
        }

        public Action<string, bool, IEnumerable<string>> GridHandler;

        public override IEnumerable<ITreeNodePresenter> GetProjectTreeViewNodePresenters()
        {
            yield return new WaterFlowFMModelNodePresenter(this);
            yield return new FlowFMTreeShortcutNodePresenter {GuiPlugin = this};
            yield return new BoundaryConditionSetNodePresenter { GuiPlugin = this };
            yield return new SourceSinkNodePresenter {GuiPlugin = this};
            yield return new FMMapFileFunctionStoreNodePresenter {GuiPlugin = this};
            yield return new FMHisFileFunctionStoreNodePresenter();
            yield return new ImportedFMNetFileNodePresenter {GuiPlugin = this};
            yield return new HeatFluxModelNodePresenter {GuiPlugin = this};
            yield return new WindItemListNodePresenter {GuiPlugin = this};
            yield return new WindItemNodePresenter {GuiPlugin = this};
            yield return
                new SpatialOperationCoverageTreeShortcutNodePresenter<WaterFlowFMModel, WaterFlowFMModelView>
                {
                    GuiPlugin = this
                };
        }

        public override IEnumerable<ViewInfo> GetViewInfoObjects()
        {
            yield return new ViewInfo<WaterFlowFMModel, WaterFlowFMModelView>
            {
                Description = "Map",
                CompositeViewType = typeof (ProjectItemMapView),
                GetCompositeViewData = o => o,
                GetViewName = (v, o) => o.Name + " (FM model)",
                AfterCreate = (v, o) =>
                {
                    v.Gui = Gui;
                }
            };

            yield return new ViewInfo<WaterFlowFMModel, WaterFlowFMFileStructureView>
            {
                Description = "File tree"
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
                    v.OnValidate = d => (d as WaterFlowFMModel).Validate(d as WaterFlowFMModel);
                }
            };

            // Spatial operations
            yield return
                new ViewInfo
                    <SpatialOperationCoverageTreeShortcut<WaterFlowFMModel, WaterFlowFMModelView>, WaterFlowFMModel,
                        WaterFlowFMModelView>
                {
                    Description = "Spatial operation on coverage",
                    GetViewName = (v, o) => o.Name + " (FM model)",
                    GetViewData = o => o.Model,
                    CompositeViewType = typeof(ProjectItemMapView),
                    GetCompositeViewData = o => o.Model,
                    OnActivateView =
                        (v, o) =>
                        {
                            var treeShortcut =
                                (SpatialOperationCoverageTreeShortcut<WaterFlowFMModel, WaterFlowFMModelView>)o;
                            treeShortcut.FocusSpatialEditor(Gui);
                            treeShortcut.NavigateToInView(v);
                        },
                    AfterCreate = (v, o) =>
                    {
                        v.Gui = Gui;
                        o.FocusSpatialEditor(Gui);
                        o.NavigateToInView(v);
                    },
                };

            // 'General'
            yield return new ViewInfo<FlowFMTreeShortcut, WaterFlowFMModel, WaterFlowFMModelView>
            {
                Description = "FM Model",
                GetViewName = (v, o) => o.Name + " (FM model)",
                AdditionalDataCheck = o => o.CanSwitchToTab,
                GetViewData = o => o.Model,
                CompositeViewType = typeof(ProjectItemMapView),
                GetCompositeViewData = o => o.Model,
                OnActivateView = (v, o) => ((FlowFMTreeShortcut)o).NavigateToInView(v),
                AfterCreate = (v, o) =>
                {
                    v.Gui = Gui;
                    o.NavigateToInView(v);
                }
            };

            // 'Grid', launches rgfgrid. 
            var gridViewInfo = new ViewInfo<UnstructuredGrid, WaterFlowFMModel, WaterFlowFMModelView>
            {
                Description = "FM Model",
                GetViewName = (v, o) => "FM Model (" + o.Name + ")",
                CompositeViewType = typeof(ProjectItemMapView),
                GetCompositeViewData = o => FlowModels.First(m => Equals(m.Grid, o)),
                GetViewData = o => FlowModels.First(m => Equals(m.Grid, o)),
                AdditionalDataCheck = o => FlowModels.FirstOrDefault(m => Equals(m.Grid, o)) != null,
                OnActivateView = (v, o) =>
                {
                    v.Gui = Gui;

                    var model = FlowModels.First(m => Equals(m.Grid, o));
                    var paths = GetLbdAndShapeFilePaths(v);

                    var writer = new MduFile();
                    var targetMduFilePath = model.MduFilePath;
                    writer.WriteLandBoundaries(targetMduFilePath, model.ModelDefinition, model.Area);

                    var modelLdbPath = model.ModelDefinition.GetModelProperty(KnownProperties.LandBoundaryFile).GetValueAsString();

                    if (!string.IsNullOrEmpty(modelLdbPath))
                    {
                        var landBoundariesFilePath = Path.Combine(Path.GetDirectoryName(targetMduFilePath), modelLdbPath);
                        paths.Add(landBoundariesFilePath);
                    }

                    //Unless specified otherwise (declared in the FlowFMGuiPlugin constructor), this will execute the following: 
                    //            RgfGridEditor.OpenGrid(model.NetFilePath, model.Grid == null || model.Grid.IsEmpty, paths);
                    GridHandler?.Invoke(model.NetFilePath, model.Grid == null || model.Grid.IsEmpty, paths);
                    ReloadGrid(model, v);
                }
            };
            yield return gridViewInfo;
            yield return ViewInfoWrapper<FlowFMTreeShortcut>.Create(gridViewInfo, o => o.TargetData, o => o.TargetData is UnstructuredGrid);

            

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
                    if(condition == null) return;
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
                    var centralMap =
                        Gui.DocumentViews.OfType<ProjectItemMapView>()
                            .FirstOrDefault(vi => vi.MapView.GetLayerForData(o) != null);
                    if (centralMap == null) return;

                    v.OpenViewMethod = feature => Gui.CommandHandler.OpenView(feature);
                    v.ZoomToFeature =
                        feature =>
                            centralMap.MapView.EnsureVisible(
                                o.FirstOrDefault(
                                    bcs => bcs.BoundaryConditions.Contains(feature as IBoundaryCondition)));
                }
            };

            yield return allBoundarySetsViewInfo;

            yield return ViewInfoWrapper<FlowFMTreeShortcut>.Create(allBoundarySetsViewInfo, o => o.TargetData);

            yield return
                FeatureCollectionViewInfoHelper.CreateViewInfo<Feature2D, WaterFlowFMModel>("Boundaries",
                    m => m.Boundaries, () => Gui);

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

            var pipesViewInfo =
                            FeatureCollectionViewInfoHelper.CreateViewInfo<Feature2D, WaterFlowFMModel>("Sources and Sinks",
                                m => m.Pipes, () => Gui);
            yield return
                            ViewInfoWrapper<FlowFMTreeShortcut>.Create(pipesViewInfo, GetPipesFromSourcesAndSinks,
                                afterCreate: (v, o) => v.CanAddDeleteAttributes = false);



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
            yield return
                new ViewInfo<IEventedList<IWindField>, WindFieldListView>
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

            // Importers and exporters
            yield return new ViewInfo<BcmFileImporter, BcmFileImportDialog>();
            yield return new ViewInfo<BcmFileExporter, BcmFileExportDialog>();
            yield return new ViewInfo<BcFileImporter, BcFileImportDialog>();
            yield return new ViewInfo<BcFileExporter, BcFileExportDialog>();
            yield return
                new ViewInfo<FMModelPartitionExporter, FMPartitionExportDialog>
                {
                    AfterCreate = (v, o) =>
                    {
                        v.Extension = o.FileFilter;
                        v.EnableSolverSelection = true;
                    }
                };
            yield return
                new ViewInfo<FMGridPartitionExporter, FMPartitionExportDialog>
                {
                    AfterCreate = (v, o) =>
                    {
                        v.Extension = o.FileFilter;
                        v.EnableSolverSelection = false;
                    }
                };

            yield return GetFeature2DImportDialogViewInfo<PliFileImporterExporter<Embankment, Embankment>>();
            yield return GetFeature2DImportDialogViewInfo<PliFileImporterExporter<FixedWeir, FixedWeir>>();
            yield return GetFeature2DImportDialogViewInfo<PliFileImporterExporter<ObservationCrossSection2D, ObservationCrossSection2D>>();
            yield return GetFeature2DImportDialogViewInfo<PliFileImporterExporter<ThinDam2D, ThinDam2D>>();
            yield return GetFeature2DImportDialogViewInfo<PliFileImporterExporter<IWeir, Feature2D>>();
            yield return GetFeature2DImportDialogViewInfo<PliFileImporterExporter<IPump, Feature2D>>();
            yield return GetFeature2DImportDialogViewInfo<PliFileImporterExporter<IGate, Feature2D>>();
            yield return GetFeature2DImportDialogViewInfo<PliFileImporterExporter<SourceAndSink, Feature2D>>();
            yield return GetFeature2DImportDialogViewInfo<PliFileImporterExporter<BoundaryConditionSet, Feature2D>>();

            yield return GetFeature2DImportDialogViewInfo<PointFileImporterExporter>();
            yield return GetFeature2DImportDialogViewInfo<PolFileImporterExporter>();
            yield return GetFeature2DImportDialogViewInfo<LdbFileImporterExporter>();

            yield return new ViewInfo<BoundaryConditionWpsImporter, BoundaryConditionWpsDialog>
            {
                AfterCreate = (v, o) =>
                {
                    v.AllowSelectedSupportPointImport = false;

                    var model = FlowModels.FirstOrDefault();

                    v.StartTime = model.StartTime;
                    v.StopTime = model.StopTime;
                    v.TimeStep = model.TimeStep;
                    v.CoordinateSystem = model.CoordinateSystem;
                }
            };
        }

        private static void ReloadGrid(WaterFlowFMModel model, WaterFlowFMModelView modelView)
        {
            try
            {
                if (SharpMapGisGuiPlugin.Instance != null)
                {
                    SharpMapGisGuiPlugin.Instance.Gui.MainWindow.SetWaitCursorOn();
                }
                // D3DFMIQ-16: This if-statement should be removed after the fix in DELFT3DFM-1413, where the user should be 
                // prompted by RGFGRID if he/she wants to save the grid.
                if (File.Exists(model.NetFilePath) && new FileInfo(model.NetFilePath).Length == 0)
                {
                    throw new FileFormatException(new Uri(model.NetFilePath),
                        "Empty file detected. Changes in the grid were not saved.\nPlease save your project before exiting RGFGRID.");
                }
                if (!File.Exists(model.NetFilePath))
                {
                    model.RemoveGrid();
                    return;
                }

                if (model.CoordinateSystem != null)
                {
                    var netfile = new ImportedFMNetFile(model.NetFilePath);
                    var coordinates = netfile.Grid.Vertices;
                    if (!CoordinateSystemValidator.CanAssignCoordinateSystem(coordinates,
                        model.CoordinateSystem))
                    {
                        throw new Exception(
                            "Grid coordinates are incompatible with current model coordinate system");
                    }
                }
                model.ReloadGrid(false);
            }
            // D3DFMIQ-16: This catch block should be removed after the fix in DELFT3DFM-1413, where the user should be 
            // prompted by RGFGRID if he/she wants to save the grid.
            catch (FileFormatException exception)
            {
                DelftTools.Controls.Swf.MessageBox.Show(exception.Message, "Grid was not saved in RGFGRID",
                    MessageBoxButtons.OK);
                model.Grid = NetFileImporter.ImportGrid(model.NetFilePath) ?? new UnstructuredGrid();
            }
            catch (Exception exception)
            {
                var dialogResult =
                    DelftTools.Controls.Swf.MessageBox.Show(
                        "Failed to reload grid after RGFGrid edits: " + exception.Message +
                        Environment.NewLine + "Continue with new grid?", "Failed to reload grid.",
                        MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    model.Grid = NetFileImporter.ImportGrid(model.NetFilePath) ?? new UnstructuredGrid();
                }
                else
                {
                    if (File.Exists(model.NetFilePath)) File.Delete(model.NetFilePath);
                    model.WriteNetFile(model.NetFilePath);
                }
            }
            finally
            {
                modelView.Layer?.Map.ZoomToFit(modelView.Layer?.Envelope);
                if (SharpMapGisGuiPlugin.Instance != null)
                {
                    SharpMapGisGuiPlugin.Instance.Gui.MainWindow.SetWaitCursorOff();
                }
            }
        }

        private static List<string> GetLbdAndShapeFilePaths(WaterFlowFMModelView v)
        {
            if (v.Layer == null) return new List<string>();

            var featureProviders = v.Layer.Map.Layers
                .OfType<VectorLayer>()
                .Select(l => l.DataSource)
                .ToList();

            var shapeFiles = featureProviders.OfType<ShapeFile>().OfType<IFileBased>();
            var ldbFiles = featureProviders.OfType<LdbFile>().OfType<IFileBased>();

            return shapeFiles.Concat(ldbFiles).Select(s => s.Path).ToList();
        }

        private object GetPipesFromSourcesAndSinks(FlowFMTreeShortcut treeShortCut)
        {
            var sourcesAndSinks = treeShortCut.TargetData as IEventedList<SourceAndSink>;
            if (sourcesAndSinks == null) return null;
            var model = FlowModels.FirstOrDefault(m => Equals(sourcesAndSinks, m.SourcesAndSinks));
            return model == null ? null : model.Pipes;
        }

        private IEnumerable<WaterFlowFMModel> FlowModels
        {
            get { return Gui == null ? Enumerable.Empty<WaterFlowFMModel>() : Gui.Application.GetAllModelsInProject().OfType<WaterFlowFMModel>(); }
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

        public override ResourceManager Resources { get; set; }

        public override IEnumerable<PropertyInfo> GetPropertyInfos()
        {
            yield return CreatePropertyInfoDynamic<WaterFlowFMModel>();
            yield return CreatePropertyInfoDynamic<PointCloudLayer>();
            yield return new PropertyInfo<IWeir, FMWeirProperties>{ AdditionalDataCheck = w => FlowModels.Any(m => m.Area.Weirs.Contains(w)) };
        }

        public override IMapLayerProvider MapLayerProvider
        {
            get { return new FlowFMMapLayerProvider(); }
        }

        public override IGui Gui
        {
            get { return base.Gui; }
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

        private DateTime? GetModelStartTime()
        {
            var model = FlowModels.FirstOrDefault();
            return model != null ? model.StartTime : (DateTime?) null;
        }

        private DateTime? GetModelStopTime()
        {
            var model = FlowModels.FirstOrDefault();
            return model != null ? model.StopTime : (DateTime?) null;
        }

        private TimeSpan? GetModelTimeStep()
        {
            var model = FlowModels.FirstOrDefault();
            return model != null ? model.TimeStep : (TimeSpan?) null;
        }

        private TableViewTimeSeriesGeneratorTool tableViewTimeSeriesGeneratorTool;

        private void SubscribeToProjectEvents()
        {
            if (base.Gui == null || base.Gui.Application == null) return;
            base.Gui.Application.ProjectOpened += SubscribeToProjectPropertyChanged;
            base.Gui.Application.ProjectClosing += UnsubscribeToProjectPropertyChanged;

            // DELFT3DFM-371: Disable Model Inspection
            // base.Gui.Application.ActivityRunner.Activities.CollectionChanged += Activities_CollectionChanged;

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
            base.Gui.Application.ProjectOpened -= SubscribeToProjectPropertyChanged;
            base.Gui.Application.ProjectClosing -= UnsubscribeToProjectPropertyChanged;

            // DELFT3DFM-371: Disable Model Inspection
            //base.Gui.Application.ActivityRunner.Activities.CollectionChanged -= Activities_CollectionChanged;

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

        private void SubscribeToProjectPropertyChanged(Project project)
        {
            if (project == null) return;
            ((INotifyPropertyChange)project).PropertyChanging += ProjectPropertyChanging;
            ((INotifyPropertyChanged) project).PropertyChanged += ProjectPropertyChanged;
        }

        private void UnsubscribeToProjectPropertyChanged(Project project)
        {
            if (project == null) return;
            ((INotifyPropertyChange)project).PropertyChanging -= ProjectPropertyChanging;
            ((INotifyPropertyChanged)project).PropertyChanged -= ProjectPropertyChanged;
        }

        private static readonly string CoordinateSystemMemberName =
            TypeUtils.GetMemberName<WaterFlowFMModel>(m => m.CoordinateSystem);

        private static readonly string OutputHisFileStoreMemberName =
            TypeUtils.GetMemberName<WaterFlowFMModel>(m => m.OutputHisFileStore);

        private static readonly string HeatFluxModelTypeMemberName =
            TypeUtils.GetMemberName<WaterFlowFMModel>(m => m.HeatFluxModelType);

        void ProjectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender is WaterFlowFMModel))
                return; //early exit

            if (e.PropertyName.Equals(CoordinateSystemMemberName))
            {
                var model = sender as WaterFlowFMModel;
                if (! model.WriteSnappedFeatures) return;

                // Set coordinate system to OutputSnappedFeatures
                var mapViews = Gui.DocumentViews.OfType<ProjectItemMapView>().Where(m => (m.Data as WaterFlowFMModel) == model);
                foreach (var mapView in mapViews)
                {
                    var modelLayer = mapView.MapView.GetLayerForData(model);
                    var groupModelLayer = modelLayer as GroupLayer;
                    if (groupModelLayer != null)
                    {
                        var snappedOutputLayer = groupModelLayer.Layers.FirstOrDefault(l => l.Name == FlowFMMapLayerProvider.OutputSnappedFeaturesLayerName) as GroupLayer;
                        if (snappedOutputLayer == null) continue;

                        snappedOutputLayer.Layers.ForEach(l => l.DataSource.CoordinateSystem = model.CoordinateSystem);
                    }
                }

            }
        }

        void ProjectPropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            if (!(sender is WaterFlowFMModel))
                return; //early exit

            if (e.PropertyName.Equals(OutputHisFileStoreMemberName))
            {

                var fmHisFileFunctionStore = ((WaterFlowFMModel) sender).OutputHisFileStore;
                if (fmHisFileFunctionStore != null)
                {
                    CloseViewDataForOutdatedStore(fmHisFileFunctionStore);
                }
            }

            if (e.PropertyName.Equals(HeatFluxModelTypeMemberName))
            {
                var heatFluxModel = ((WaterFlowFMModel) sender).ModelDefinition.HeatFluxModel;
                if (heatFluxModel != null)
                {
                    CloseViewsForOutDatedHeatFluxModel(heatFluxModel);
                }
            }
        }

        [InvokeRequired]
        private void OnActivityRunnerStatusChanged(object sender,
            ActivityStatusChangedEventArgs activityStatusChangedEventArgs)
        {
            if (sender is FileImportActivity)
            {
                var importer = ((FileImportActivity) sender).FileImporter;

                if (importer is FlowFMNetFileImporter || importer is IFeature2DImporterExporter)
                {
                    if (activityStatusChangedEventArgs.NewStatus == ActivityStatus.Finished)
                    {
                        if (ActiveMapView != null)
                        {
                            ActiveMapView.Map.ZoomToExtents();
                        }                        
                    }
                }
                if (importer is WaterFlowFMFileImporter &&
                    activityStatusChangedEventArgs.NewStatus == ActivityStatus.Finished)
                {
                    Gui.MainWindow.ProjectExplorer.TreeView.Refresh();
                    if (ActiveMapView != null)
                    {
                        ActiveMapView.Map.ZoomToExtents();
                    }
                }
            }

            var model = sender as WaterFlowFMModel;
            if ( model != null && model.WriteSnappedFeatures && activityStatusChangedEventArgs.NewStatus == ActivityStatus.Initializing)
            {
                //Clean output snapped layers;
                // release file locks
                var mapViews = Gui.DocumentViews.OfType<ProjectItemMapView>().Where( m => (m.Data as WaterFlowFMModel) == model);

                foreach (var mapView in mapViews)
                {
                    var modelLayer = mapView.MapView.GetLayerForData(model);
                    var groupModelLayer = modelLayer as GroupLayer;
                    if (groupModelLayer != null)
                    {
                        var snappedOutputLayer = groupModelLayer.Layers.FirstOrDefault(l => l.Name == FlowFMMapLayerProvider.OutputSnappedFeaturesLayerName) as GroupLayer;
                        if (snappedOutputLayer == null) continue;

                        snappedOutputLayer.Layers.Select(l => l.DataSource).OfType<ShapeFile>().ForEach(sf => sf.Close());
                    }
                }
            }

            if (!(sender is WaterFlowFMModel) || activityStatusChangedEventArgs.NewStatus != ActivityStatus.Failed) return;

            Gui.CommandHandler.OpenView(sender, typeof(ValidationView));
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

        public override IRibbonCommandHandler RibbonCommandHandler
        {
            get { return new Ribbon.Ribbon(); }
        }

        public FlowFMGuiPlugin()
        {
            getActiveMapViewFunc = GetActiveMapView;
            GridHandler = RgfGridEditor.OpenGrid;
        }

        private static Func<MapView> getActiveMapViewFunc;
        
        public static MapView ActiveMapView
        {
            get { return getActiveMapViewFunc(); }
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

        public override void OnActiveViewChanged(IView view)
        {
            if (view == null)
                return;

            var mapView = FindMapView(view);
            if (mapView != null)
                FlowFMMapViewDecorator.AddMapToolsIfMissing(mapView);
        }

        private static MapView FindMapView(IView activeView)
        {
            var mapView = activeView as MapView;

            if (mapView != null)
                return mapView;

            var compositeView = activeView as ICompositeView;

            //todo: recursion
            return compositeView != null ? compositeView.ChildViews.OfType<MapView>().FirstOrDefault() : null;
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

        private static PropertyInfo CreatePropertyInfoDynamic<T>()
        {
            return new PropertyInfo<T, DynamicObjectProperties>();
        }
    }
}
