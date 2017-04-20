using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Forms;
using DelftTools.Shell.Gui.Swf.Validation;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.CommonTools.Gui.Forms;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extentions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.DataSetManager;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.FunctionListView;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.ProjectExplorer;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.PropertyGrid;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.WaterQualityModelWizard;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.MapTools;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Ribbon;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Commands.SpatialOperations;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.CoverageViews;
using GeoAPI.Extensions.Coverages;
using Mono.Addins;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;

using SharpMap.Layers;
using SharpMap.UI.Helpers;
using SharpMap.UI.Tools;
using log4net;
using MessageBox = System.Windows.Forms.MessageBox;
using WaqResources = DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties.Resources;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui
{
    [Extension(typeof(IPlugin))]
    public class WaterQualityModelGuiPlugin : GuiPlugin, ISpatialOperationProviderPlugin
    {
        private static readonly Cursor AddObservationPointCursor = MapCursors.CreateArrowOverlayCuror(DelftModels.WaterQualityModel.Properties.Resources.Observation);
        private static readonly Cursor AddLoadCursor = MapCursors.CreateArrowOverlayCuror(DelftModels.WaterQualityModel.Properties.Resources.weight);
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterQualityModelGuiPlugin));

        private bool showingSyncMessage;
        private IGui gui;
        private WaterQualityRibbon ribbon;
        internal const string FindGridCellMapToolName = "FindGridCell";
        internal const string AddObservationPointMapToolName = "AddObservationPoint";
        internal const string AddWaterQualityLoadMapToolName = "AddWaterQualityLoadMapTool";

        private BloomInfo bloomInfo;

        public override string Name
        {
            get { return "Water quality model (UI)"; }
        }

        public override string DisplayName
        {
            get { return "D-Water Quality Plugin (UI)"; }
        }

        public override string Description
        {
            get { return "Allows to simulate water quality in rivers and channels."; }
        }

        public override string Version
        {
            get { return GetType().Assembly.GetName().Version.ToString(); }
        }

        public override string FileFormatVersion
        {
            get { return "1.1.0.0"; }
        }

        public override IGui Gui
        {
            get { return gui; }
            set
            {
                if (gui != null)
                {
                    gui.Application.ProjectOpened -= ApplicationProjectOpened;
                    gui.Application.ProjectClosing -= ApplicationProjectClosing;

                    if (gui.Application.Project != null)
                    {
                        gui.Application.Project.RootFolder.CollectionChanged -= RootFolderCollectionChanged;
                    }

                    gui.Application.ActivityRunner.ActivityStatusChanged -= ActivityRunnerActivityStatusChanged;
                    gui.Application.ActivityRunner.ActivityCompleted -= ActivityRunnerOnActivityCompleted;

                    var applicationPlugin = gui.Application.Plugins.OfType<WaterQualityModelApplicationPlugin>().FirstOrDefault();
                    if (applicationPlugin != null)
                    {
                        applicationPlugin.HydFileNotFoundGuiHandler = null;    
                    }
                }

                gui = value;

                if (gui != null)
                {
                    gui.Application.ProjectOpened += ApplicationProjectOpened;
                    gui.Application.ProjectClosing += ApplicationProjectClosing;

                    if (gui.Application.Project != null)
                    {
                        gui.Application.Project.RootFolder.CollectionChanged += RootFolderCollectionChanged;
                    }

                    gui.Application.ActivityRunner.ActivityStatusChanged += ActivityRunnerActivityStatusChanged;
                    gui.Application.ActivityRunner.ActivityCompleted += ActivityRunnerOnActivityCompleted;

                    var applicationPlugin = gui.Application.Plugins.OfType<WaterQualityModelApplicationPlugin>().FirstOrDefault();
                    if (applicationPlugin != null)
                    {
                        applicationPlugin.HydFileNotFoundGuiHandler = OnHydFileNotFound;
                        applicationPlugin.ProcessDefinitionFilesNotFoundGuiHandler = OnProcessDefinitionFilesNotFound;
                    }

                    // HACK: setting the Gui happens just before Activate in DeltaShellGui, 
                    // hence we set the spatial operations flag here:
                    var sharpMapGisGuiPlugin = gui.Plugins.OfType<SharpMapGisGuiPlugin>().FirstOrDefault();
                    if (sharpMapGisGuiPlugin != null)
                    {
                        sharpMapGisGuiPlugin.SpatialOperationsEnabled = true;
                    }

                    gui.UndoRedoManager.Enabled = false;
                }
            }
        }

        public override void Activate()
        {
            base.Activate();

            // read the spe file to exclude the algae parameters
            bloomInfo = BloomSpeFileReader.Read(Path.Combine(DelwaqFileStructureHelper.GetDelwaqDataDefaultFolderPath(), "bloom.spe"));
        }

        [InvokeRequired]
        private void OnProcessDefinitionFilesNotFound(WaterQualityModel model, string processDefinitionPath)
        {
            
            FileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = Path.GetDirectoryName(processDefinitionPath);
            dialog.Filter = "Process definition file (*.def)|*.def";
            dialog.Title = "Process definition files could not be found. Please refer to the *.def file.";

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                Log.ErrorFormat("Could not find process definition files: {0}", processDefinitionPath);
                return;
            }
             


            var processDefinitionFilePath = Path.GetDirectoryName(dialog.FileName);
            var processDefinitionFileName = Path.GetFileNameWithoutExtension(dialog.FileName);
            
            if (string.IsNullOrEmpty(processDefinitionFilePath) ||
                string.IsNullOrEmpty(processDefinitionFileName))
            {
                Log.ErrorFormat("Could not find process definition files: {0}", processDefinitionPath);
                return;
            };

            var newProcessDefinitionFilesPath = Path.Combine(processDefinitionFilePath, processDefinitionFileName);
            
            Log.WarnFormat("Could not find process definition files: {0}, but now using {1}", processDefinitionPath, newProcessDefinitionFilesPath);
            
            model.SubstanceProcessLibrary.ProcessDefinitionFilesPath = newProcessDefinitionFilesPath;
        }

        [InvokeRequired]
        private void OnHydFileNotFound(WaterQualityModel model, string hydPath)
        {
            Log.ErrorFormat("Could not find hyd file {0}", hydPath);

            FileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = Path.GetDirectoryName(hydPath);
            dialog.Filter = "Hydrodynamics file (*.hyd)|*.hyd";
            dialog.Title = "Hydrodynamics file could not be found. Please refer to the *.hyd file.";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                new HydFileImporter().ImportItem(dialog.FileName, model);
            }
        }

        public override IEnumerable<PropertyInfo> GetPropertyInfos()
        {
            yield return new PropertyInfo<WaterQualityModel,WaterQualityModelProperties>();
            yield return new PropertyInfo<WaterQualityFunctionWrapper, WaterQualityModelUnstructuredGridCellCoverageProperties>
                {
                    AdditionalDataCheck = o => o.Function != null && o.Function.IsUnstructuredGridCellCoverage(),
                    GetObjectPropertiesData = o => o.Function
                };
            yield return new PropertyInfo<WaterQualityFunctionDataWrapper, WaterQualityFunctionDataWrapperProperties>();
            yield return new PropertyInfo<SubstanceProcessLibrary, SubstanceProcessLibraryProperties>();
            yield return new PropertyInfo<WaterQualityObservationVariableOutput, WaterQualityObservationVariableOutputProperties>();
            yield return new PropertyInfo<WaterQualityLoad, WaterQualityLoadProperties>();
            yield return new PropertyInfo<WaterQualityBoundary, WaterQualityBoundaryProperties>();
            yield return new PropertyInfo<WaterQualityObservationPoint, WaterQualityObservationPointProperties>();

            yield return new PropertyInfo<SetLabelOperation, SetLabelOperationProperties>();
            yield return new PropertyInfo<OverwriteLabelOperation, OverwriteLabelOperationProperties>();
            yield return new PropertyInfo<UnstructuredGridFeature, UnstructuredGridCellWaqProperties>();
        }

        public override IEnumerable<ViewInfo> GetViewInfoObjects()
        {
            yield return new ViewInfo<SubstanceProcessLibrary, SubstanceProcessLibraryView>
                {
                    Description = "Substances process library",
                    GetViewName = (v, o) => "Substance process library",
                    Image = Properties.Resources.Library
                };
            yield return new ViewInfo<WaterQualityFunctionDataWrapper, IEventedList<IFunction>, FunctionListView>
            {
                Description = "Functions",
                GetViewData = o => o.Functions,
                Image = Properties.Resources.Folder,
                AfterCreate = (v, o) =>
                {
                    v.Gui = gui;
                    if (Gui.SelectedModel is WaterQualityModel)
                    {
                        FunctionListViewExtraActions(Gui.SelectedModel as WaterQualityModel, v);
                    }
                }
            };

            yield return new ViewInfo<WaterQualityBloomFunctionWrapper, IEventedList<IFunction>, BloomFunctionsTableView>()
            {
                Description = "Bloom Algae",
                GetViewData = o => o.Functions,
                GetViewName = (v, o) => "Bloom Algae",
                Image = Properties.Resources.Folder,
                AfterCreate = (v, o) =>
                {
                    v.Gui = gui;
                    v.BloomInfo = bloomInfo;

                    if (Gui.SelectedModel is WaterQualityModel)
                    {
                        v.DataOwner = Gui.SelectedModel as WaterQualityModel;
                    }
                },
            };

            yield return new ViewInfo<WaterQualityFunctionWrapper, IFunction, FunctionView>
                {
                    Description = "Time dependent function",
                    AdditionalDataCheck = o => o.Function.IsTimeSeries(),
                    GetViewData = o => o.Function,
                    GetViewName = (v, o) => o.Name,
                    Image = Properties.Resources.TimeSeries,
                };
            yield return new ViewInfo<WaterQualityFunctionWrapper, ICoverage, CoverageTableView>
                {
                    Description = "Time dependent function",
                    AdditionalDataCheck = o => o.Function.IsNetworkCoverage(),
                    CompositeViewType = typeof(ProjectItemMapView),
                    GetCompositeViewData = o => gui.Application.DataItemService.GetDataItemByValue(gui.Application.Project,o.Function),
                    Image = Properties.Resources.LocationFunction,
                    GetViewData = o => (ICoverage) o.Function,
                    GetViewName = (v, o) => o.Name
                };

            yield return new ViewInfo<TimeSeries, FunctionView>
                {
                    Description = "Function",
                    AdditionalDataCheck = ts => Gui.SelectedModel is WaterQualityModel,
                    GetViewName = (v, o) => o.Name
                };

            yield return new ViewInfo<WaterQualityObservationVariableOutput, IEnumerable<IFunction>, MultipleFunctionView>
                    {
                        Description = "Monitoring output view",
                        AdditionalDataCheck = o => o.TimeSeriesList.Any(),
                        GetViewData = o => null,
                        GetViewName = (view, functions) => view.Text,
                        AfterCreate = (v, o) =>
                            {
                                v.Data = GetMultipleFunctionViewData(o);
                                v.Text = o.Name;
                            },
                        CloseForData = (v, o) => v.Data != null && o.TimeSeriesList.Any(ts => ((IEnumerable<IFunction>)v.Data).Contains(ts))
                    };

            yield return new ViewInfo<SubFileImporter, SubstanceProcessLibraryWizard>
                {
                    Description = "Substance process library wizard",
                    AfterCreate = (v, o) =>
                    {
                        if (Gui.SelectedModel is WaterQualityModel)
                        {
                            v.WaterQualityModel = Gui.SelectedModel as WaterQualityModel;
                        }
                    },
                };

            yield return new ViewInfo<WaterQualityModel, ValidationView>
            {
                Description = "Validation Report",
                AfterCreate = (v, o) =>
                {
                    v.Gui = Gui;
                    v.OnValidate = m => new WaterQualityModelValidator().Validate(m as WaterQualityModel);
                }
            };

            yield return new ViewInfo<DataTableManager, DataTableManagerView>
            {
                Description = "Data Table Manager",
                GetViewName = (v, o) => o.Name,
                Image = Properties.Resources.DataTableManager
            };

            yield return SharpMapGisGuiPlugin.CreateAttributeTableViewInfo<WaterQualityLoad, WaterQualityModel>(m => m.Loads, () => Gui );
            yield return SharpMapGisGuiPlugin.CreateAttributeTableViewInfo<WaterQualityObservationPoint, WaterQualityModel>(m => m.ObservationPoints, () => Gui);
            yield return SharpMapGisGuiPlugin.CreateAttributeTableViewInfo<WaterQualityBoundary, WaterQualityModel>(m => m.Boundaries, () => Gui);
        }

        public override void OnViewAdded(IView view)
        {
            var coverageView = view as CoverageView;
            if (coverageView != null)
            {
                // Do not show locations when opening a coverageView for WaterQualityModel1D
                var mapView = (coverageView).ChildViews.OfType<MapView>().FirstOrDefault();
                if (mapView == null) return;

                var networkCoverageGroupLayer = mapView.Map.Layers.OfType<NetworkCoverageGroupLayer>().FirstOrDefault();
                if (networkCoverageGroupLayer == null) return;

                if (networkCoverageGroupLayer.SegmentLayer != null) // True for discretization
                {
                    var dataItem = gui.SelectedModel.AllDataItems.FirstOrDefault(t => t.Value == view.Data);
                    if (dataItem != null && dataItem.Role == DataItemRole.Output)
                    {
                        networkCoverageGroupLayer.LocationLayer.Visible = false;
                        networkCoverageGroupLayer.SegmentLayer.ShowInLegend = true;
                    }
                }
            }

            var textDocumentView = view as TextDocumentView;
            if (textDocumentView != null)
            {
                textDocumentView.Font = new Font(FontFamily.GenericMonospace, 10);
            }

            var centralMapView = view as ProjectItemMapView;
            if (centralMapView != null)
            {
                centralMapView.MapView.MapControl.Tools.AddRange(new IMapTool[]
                    {
                        new NewPointFeatureTool<WaterQualityLoad>(AddWaterQualityLoadMapToolName)
                            {
                                Cursor = AddLoadCursor,
                                NewNameFormat = LoadsImporter.NewNameFormat
                            },
                        new NewPointFeatureTool<WaterQualityObservationPoint>(AddObservationPointMapToolName)
                            {
                                Cursor = AddObservationPointCursor,
                                NewNameFormat = ObservationPointImporter.NewNameFormat
                            },
                        new FindGridCellTool(FindGridCellMapToolName)
                        {
                            GetWaqModelForGrid = GetWaqModelForGrid
                        }
                    });
            }
        }

        /// <summary>
        /// Gets the <see cref="WaterQualityModel"/> that has the given <see cref="UnstructuredGrid"/>.
        /// </summary>
        /// <param name="grid">The grid to match with.</param>
        /// <returns>The model instance that has the given grid, or null if no match can be found.</returns>
        private WaterQualityModel GetWaqModelForGrid(UnstructuredGrid grid)
        {
            return gui.Application.Project.RootFolder.GetAllItemsRecursive()
                .OfType<WaterQualityModel>()
                .FirstOrDefault(m => Equals(grid, m.Grid));
        }

        public override IMapLayerProvider MapLayerProvider
        {
            get { return new WaterQualityModelMapLayerProvider(); }
        }

        public override IRibbonCommandHandler RibbonCommandHandler
        {
            get
            {
                if (ribbon == null)
                {
                    ribbon = new WaterQualityRibbon();
                }

                return ribbon;
            }
        }

        public BloomInfo BloomInfo { get { return bloomInfo; } }

        public override IEnumerable<ITreeNodePresenter> GetProjectTreeViewNodePresenters()
        {
            yield return new SubstanceProcessLibraryNodePresenter{GuiPlugin = this};
            yield return new WaterQualityFunctionDataWrapperNodePresenter();
            yield return new WaterQualityFunctionWrapperNodePresenter();
            yield return new WaterQualityObservationVariableOutputNodePresenter();
            yield return new WaterQualityModelNodePresenter(this);
            yield return new DataTableManagerNodePresenter();
            yield return new WaterQualityLoadListNodePresenter();
            yield return new WaterQualityBoundaryListNodePresenter();
            yield return new WaterQualityObservationPointNodePresenter();
            yield return new WaterQualityBloomFunctionWrapperNodePresenter();
        }

        public override bool CanCopy(IProjectItem item)
        {
            return !(item is WaterQualityModel);
        }

        public override bool CanCut(IProjectItem item)
        {
            return !(item is WaterQualityModel);
        }

        private void ApplicationProjectOpened(Project project)
        {
            project.RootFolder.CollectionChanged += RootFolderCollectionChanged;
        }

        private void ApplicationProjectClosing(Project project)
        {
            project.RootFolder.CollectionChanged -= RootFolderCollectionChanged;
        }

        private void RootFolderCollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            var model = e.Item as WaterQualityModel;
            if (model != null)
            {
                model.HydroDataChanged += ModelOnHydroDataChanged;
            }

            if (e.Item is IDataItemSet && ((IDataItemSet) e.Item).Value is IEventedList<WaterQualityObservationVariableOutput>)
            {
                // Close any opened view for observation points/areas data if the corresponding model output is removed
                CloseMonitoringOutputViews((IDataItemSet) e.Item);
                return;
            }

            if (e.Item is IDataItem && ((IDataItem) e.Item).Value is WaterQualityObservationVariableOutput)
            {
                // Close any opened view for observation points/areas data if the corresponding model output is removed
                CloseMonitoringOutputViews((WaterQualityObservationVariableOutput) ((IDataItem) e.Item).Value);
                return;
            }
        }

        private void ModelOnHydroDataChanged(object sender, EventArgs eventArgs)
        {
            if (showingSyncMessage) return;
            
            var model = (WaterQualityModel) sender;
            var mainWindow = (ISynchronizeInvoke)Gui.MainWindow;

            showingSyncMessage = true;
            
            mainWindow.BeginInvoke(new Action(() =>
                {
                    var dialog = new ImportHydFileDialog(model, model.HydroData.FilePath)
                        {
                            Message = string.Format("Hydro data of '{0}' has changed ('{1}', or related files).\nPress Ok to reload the hydro data.", model.Name, Path.GetFileName(model.HydroData.FilePath)),
                            ShowCancelButton = false
                        };

                    dialog.ShowDialog((IWin32Window)Gui.MainWindow);
                    showingSyncMessage = false;
                }), null);
        }

        [InvokeRequired]
        private void ActivityRunnerActivityStatusChanged(object sender, ActivityStatusChangedEventArgs e)
        {
            var fileImportActivity = sender as FileImportActivity;
            if (fileImportActivity != null)
            {
                var importer = fileImportActivity.FileImporter as HydFileImporter;
                if (importer == null) return;

                if (e.NewStatus == ActivityStatus.Initialized)
                {
                    importer.ExpandModelNode = ExpandModelInputNodes;
                }

                if (e.NewStatus == ActivityStatus.Done || 
                    e.NewStatus == ActivityStatus.Failed ||
                    e.NewStatus == ActivityStatus.Cancelled)
                {
                    importer.ExpandModelNode = null;
                }
            }

            if (!(sender is WaterQualityModel) || e.NewStatus != ActivityStatus.Failed) return;

            gui.CommandHandler.OpenView(sender, typeof(ValidationView));
        }

        [InvokeRequired]
        private void ExpandModelInputNodes(WaterQualityModel model)
        {
            var modelNode = Gui.MainWindow.ProjectExplorer.TreeView.GetNodeByTag(model);
            if (modelNode == null) return;

            modelNode.Expand();
            modelNode.Nodes[0].Expand(); // Expand input folder
        }

        [InvokeRequired]
        private void ActivityRunnerOnActivityCompleted(object sender, ActivityEventArgs e)
        {
            // Update the text of any opened/related substance process library view after a sub file import finished
            var fileImportActivity = e.Activity as FileImportActivity;
            if (fileImportActivity == null) return;

            var subFileImporter = fileImportActivity.FileImporter as SubFileImporter;
            if (subFileImporter != null)
            {
                var substanceProcessLibrary = fileImportActivity.ImportedItemOwner is IDataItem ? ((IDataItem) fileImportActivity.ImportedItemOwner).Value as SubstanceProcessLibrary : null;
                if (substanceProcessLibrary != null)
                {
                    gui.DocumentViews
                        .OfType<SubstanceProcessLibraryView>()
                        .Where(v => Equals(v.Data, substanceProcessLibrary))
                        .ForEach(v => v.Text = string.Format("{0}:{1}", gui.SelectedModel.Name, substanceProcessLibrary.Name));
                }
            }
        }

        [InvokeRequired]
        private void CloseMonitoringOutputViews(WaterQualityObservationVariableOutput observationVariableOutput)
        {
            var multipleFunctionViews = gui.DocumentViews.OfType<MultipleFunctionView>().Where(mfv => mfv.Data is IEnumerable<IFunction>);
            var viewsToClose = multipleFunctionViews.Where(v => observationVariableOutput.TimeSeriesList.Intersect(((IEnumerable<IFunction>) v.Data).OfType<TimeSeries>()).Any()).ToList();

            foreach (var viewToClose in viewsToClose)
            {
                gui.DocumentViews.Remove(viewToClose);
            }
        }

        private void CloseMonitoringOutputViews(IDataItemSet observationVariableOutputsDataItemSet)
        {
            var multipleFunctionViews = gui.DocumentViews.OfType<MultipleFunctionView>().Where(mfv => mfv.Data is IEnumerable<IFunction>).ToList();
            var observationVariableOutputs = observationVariableOutputsDataItemSet.DataItems.Select(di => (WaterQualityObservationVariableOutput) di.Value);

            foreach (var observationVariableOutput in observationVariableOutputs)
            {
                var viewsToClose = multipleFunctionViews.Where(v => observationVariableOutput.TimeSeriesList.Intersect(((IEnumerable<IFunction>) v.Data).OfType<TimeSeries>()).Any()).ToList();

                foreach (var viewToClose in viewsToClose)
                {
                    gui.DocumentViews.Remove(viewToClose);
                }
            }
        }

        private static IEnumerable<IFunction> GetMultipleFunctionViewData(WaterQualityObservationVariableOutput variableOutput)
        {
            var dialogData = variableOutput.TimeSeriesList.Select(ts => new { variableOutput.Name, TimeSeries = ts }).ToArray();
            var dialog = new GridBasedDialog
            {
                Text = "Select time series",
                MasterDataSource = dialogData,
                SingleList = true,
                MasterMultiSelect = true
            };

            return (dialog.ShowDialog() == DialogResult.OK)
                       ? dialog.MasterSelectedIndices.Reverse().Select(i => dialogData[i].TimeSeries).ToArray()
                       : null;
        }

        private void FunctionListViewExtraActions(WaterQualityModel model, FunctionListView view)
        {
            // Add data owner to view
            view.DataOwner = model;

            // Add function creators
            view.FunctionCreators.Add(FunctionTypeCreatorFactory.CreateConstantCreator());

            var viewInfoClone = (ViewInfo)view.ViewInfo.Clone();
            if (Equals(view.Data, model.InitialConditions))
            {
                viewInfoClone.GetViewName = (v, o) => "Initial Conditions";
                view.FunctionCreators.Add(FunctionTypeCreatorFactory.CreateUnstructuredGridCoverageCreator());

                view.IsDefaultValueCellReadOnly = function => function.IsUnstructuredGridCellCoverage();
            }

            if (Equals(view.Data, model.ProcessCoefficients))
            {
                viewInfoClone.GetViewName = (v, o) => "Process coefficients";
                view.FunctionCreators.Add(FunctionTypeCreatorFactory.CreateTimeseriesCreator());
                view.FunctionCreators.Add(FunctionTypeCreatorFactory.CreateUnstructuredGridCoverageCreator());
                view.FunctionCreators.Add(FunctionTypeCreatorFactory.CreateFunctionFromHydroDynamicsCreator(model.HasDataInHydroDynamics,
                    model.GetFilePathFromHydroDynamics));
                view.FunctionCreators.Add(FunctionTypeCreatorFactory.CreateSegmentFileCreator());

                view.IsDefaultValueCellReadOnly = function => function.IsFromHydroDynamics();

                ExcludeBloomParametersFromFunctionListView(view);
                view.UpdateTableView();
            }

            if (Equals(view.Data, model.Dispersion))
            {
                viewInfoClone.GetViewName = (v, o) => "Dispersion";
                view.FunctionCreators.Add(FunctionTypeCreatorFactory.CreateUnstructuredGridCoverageCreator());

                view.IsDefaultValueCellReadOnly = function => function.IsUnstructuredGridCellCoverage();
            }

            view.ViewInfo = viewInfoClone;
        }

        private void ExcludeBloomParametersFromFunctionListView(FunctionListView view)
        {
            view.ExcludeList.Clear();
            foreach (var parameter in bloomInfo.AllParameters)
            {
                view.ExcludeList.Add(parameter);
            }
        }

        public IEnumerable<Type> GetExcludedLayerDataTypesForSpatialOperation(SpatialOperationCommandBase operationCommand)
        {
            // if this is not a command that is specific for water quality, send the water quality observation areas type
            // as the coverage that is not compatible with that command
            if(ribbon.RibbonContainsSpatialOperationCommand(operationCommand))
            {
                yield return typeof (WaterQualityObservationAreaCoverage);
            }
        }

        public override IMenuItem GetContextMenu(object sender, object data)
        {
            var baseContextMenu = base.GetContextMenu(sender, data) ?? new MenuItemContextMenuStripAdapter(new ContextMenuStrip());
            var contextMenuStrip = ((MenuItemContextMenuStripAdapter)baseContextMenu).ContextMenuStrip;

            var waqModel = data as WaterQualityModel;
            if (waqModel != null)
            {
                var items = GetModelsOfType<IHydFileModel>().Select(m => CreateHydFileModelMenuItem(waqModel, m)).OfType<ToolStripItem>().ToArray();

                if (items.Any())
                {
                    var useHydFileFrom = new ClonableToolStripMenuItem
                    {
                        Text = Properties.Resources.WaterQualityModelGuiPlugin_GetContextMenu_Use_the__HYD_File_from___
                    };
                    useHydFileFrom.DropDownItems.AddRange(items);
                    contextMenuStrip.Items.Add(useHydFileFrom);
                }
            }
            
            if (data is TextDocument && GetModelsOfType<WaterQualityModel>().Any(m => Equals(m.InputFile, data)))
            {
                var revertToOriginalTemplateMenuItem = new ClonableToolStripMenuItem
                    {
                        Text = Properties.Resources.WaterQualityModelGuiPlugin_GetContextMenu_Revert_to_Original_Template,
                        Tag = data
                    };
                revertToOriginalTemplateMenuItem.Click += RevertInputFileClick;
                contextMenuStrip.Items.Add(revertToOriginalTemplateMenuItem);
            }

            if (data is SubstanceProcessLibrary)
            {
                var model = GetModelsOfType<WaterQualityModel>().FirstOrDefault(m => Equals(m.SubstanceProcessLibrary, data));
                if (model != null)
                {
                    var generateFractionsMenuItem = new ClonableToolStripMenuItem
                    {
                        Text = Properties.Resources.WaterQualityModelGuiPlugin_GetContextMenu_Generate_Fractions,
                        Tag = data
                    };

                    generateFractionsMenuItem.Click += (s, e) =>
                    {
                        model.SubstanceProcessLibrary.Clear();
                        model.SubstanceProcessLibrary.Substances.AddRange(CreateFractionSubstances(model));
                    };
                    contextMenuStrip.Items.Add(generateFractionsMenuItem);
                }
            }

            var modelForBoundaryData = GetModelsOfType<WaterQualityModel>().FirstOrDefault(m => Equals(m.BoundaryDataManager, data));
            if (data is DataTableManager && modelForBoundaryData != null)
            {
                var generateFractionsMenuItem = new ClonableToolStripMenuItem
                {
                    Text = Properties.Resources.WaterQualityModelGuiPlugin_GetContextMenu_Generate_Fractions_Data,
                    Tag = data
                };

                generateFractionsMenuItem.Click += (s, e) =>
                    {
                        var fractionDataContent = string.Concat(modelForBoundaryData.Boundaries.Select(b => string.Format("ITEM '{0}' CONCENTRATIONS '{0}' DATA 1 ", b.Name) + "\r\n"));
                        modelForBoundaryData.BoundaryDataManager.DataTables.Clear();
                        modelForBoundaryData.BoundaryDataManager.CreateNewDataTable("FractionData", fractionDataContent, "Fractions.userfor", "", true);
                    };
                contextMenuStrip.Items.Add(generateFractionsMenuItem);
            }

            return baseContextMenu;
        }

        private static IEnumerable<WaterQualitySubstance> CreateFractionSubstances(WaterQualityModel model)
        {
            var substances = model.Boundaries.Select(b => new WaterQualitySubstance
                {
                    Name = b.Name,
                    Active = true,
                    Description = String.Format("Water from boundary type {0}", b.Name),
                    InitialValue = 0,
                    ConcentrationUnit = "g/m3",
                    WasteLoadUnit = "g"
                }).ToList();

            substances.Add(new WaterQualitySubstance
                {
                    Name = "Initial",
                    Active = true,
                    Description = "Water in system at start of simulation",
                    InitialValue = 1,
                    ConcentrationUnit = "g/m3",
                    WasteLoadUnit = "g"
                });
            return substances;
        }
        
        private IEnumerable<T> GetModelsOfType<T>() where T : IModel
        {
            return Gui.Application.GetAllModelsInProject().OfType<T>();
        }

        private static ClonableToolStripMenuItem CreateHydFileModelMenuItem(WaterQualityModel waqModel, IHydFileModel hydFileModel)
        {
            var hydFileExists = File.Exists(hydFileModel.HydFilePath);
            var toolTipText = hydFileExists
                ? "Use hyd file '{0}'"
                : "hyd file is not present at '{0}'. Make sure that you have run the flow model and that the hyd file has been generated.";
            
            var hydFileModelMenuItem = new ClonableToolStripMenuItem
                {
                    Enabled = hydFileExists,
                    ToolTipText = string.Format(toolTipText, hydFileModel.HydFilePath),
                    Text = hydFileModel.Name
                };
            hydFileModelMenuItem.Click += (s, e) =>
                {
                    var dialog = new ImportHydFileDialog(waqModel, hydFileModel.HydFilePath)
                        {
                            Message = string.Format("Choose the options to use for importing the hyd file '{0}' from model '{1}'", Path.GetFileName(hydFileModel.HydFilePath), hydFileModel.Name)
                        };
                    dialog.ShowDialog();
                };

            return hydFileModelMenuItem;
        }

        private void RevertInputFileClick(object sender, EventArgs e)
        {
            var message = Properties.Resources.WaterQualityModelGuiPlugin_RevertInputFileToTemplate;
            var caption = Properties.Resources.WaterQualityModelGuiPlugin_RevertInputFileToTemplate_Caption;

            var result = MessageBox.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result != DialogResult.Yes) return;

            var document = (TextDocument)((ToolStripMenuItem) sender).Tag;
            document.Content = WaqResources.TemplateInpFileNew; // revert input file back to template.
        }
    }
}