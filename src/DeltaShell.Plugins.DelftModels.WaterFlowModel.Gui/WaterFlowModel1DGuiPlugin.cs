using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Roughness;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Shell.Gui.Swf.Validation;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.ProjectExplorer;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.PropertyGrid;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.Tools;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Validation;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Commands;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using Mono.Addins;
using NetTopologySuite.Extensions.Actions;
using SharpMap.UI.Tools;
using MessageBox = DelftTools.Controls.Swf.MessageBox;
using Size = System.Drawing.Size;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui
{
    [Extension(typeof(IPlugin))]
    public class WaterFlowModel1DGuiPlugin : GuiPlugin
    {
        //prevent multiple dialogs for a single editaction
        private bool editActionHandled;
        private ClonableToolStripMenuItem generateDataInSeriesToolStripMenuItem;
        private ClonableToolStripMenuItem zoomToToolStripMenuItem;
        private ContextMenuStrip generateDataMenu;
        
        private ClonableToolStripMenuItem modelMergeMenuItem;
        private ContextMenuStrip modelMergeMenu;

        public WaterFlowModel1DGuiPlugin()
        {
            InitializeComponent();
        }

        public override string Name
        {
            get { return "1D water flow model (UI)"; }
        }

        public override string DisplayName
        {
            get { return "D-Flow1D Plugin (UI)"; }
        }

        public override string Description
        {
            get { return WaterFlowModel.Properties.Resources.WaterFlowModel1DApplicationPlugin_Description; }
        }

        public override string Version
        {
            get { return GetType().Assembly.GetName().Version.ToString(); }
        }

        public override string FileFormatVersion
        {
            get { return "3.5.3.0"; }
        }

        public override IEnumerable<PropertyInfo> GetPropertyInfos()
        {
            yield return new PropertyInfo<TreeFolder, WaterFlowModel1DOutputSettingsProperties>
                {
                    AdditionalDataCheck = o => o.Text == WaterFlowModelConstants.OutputFolderName && o.Parent is WaterFlowModel1D,
                    GetObjectPropertiesData = o => o.Parent 
                };
            yield return new PropertyInfo<WaterFlowModel1DBoundaryNodeData, WaterFlowModel1DBoundaryNodeDataProperties>();
            yield return new PropertyInfo<WaterFlowModel1DLateralSourceData, WaterFlowModel1DLateralDataProperties>();
            yield return new PropertyInfo<WaterFlowModel1D, WaterFlowModel1DProperties>();
            yield return new PropertyInfo<WindFunction, WindFunctionProperties>();
            yield return new PropertyInfo<ReverseRoughnessSection, ReverseRoughnessSectionProperties>();
            yield return new PropertyInfo<RoughnessSection, RoughnessSectionPropertiesBase<RoughnessSection>>();
        }
        
        public override IEnumerable<ViewInfo> GetViewInfoObjects()
        {
            yield return new ViewInfo<RoughnessSection, RoughnessSectionCoverageTableView>
                {
                    CompositeViewType = typeof(ProjectItemMapView),
                    GetCompositeViewData = o => Gui.Application.DataItemService.GetDataItemByValue(Gui.Application.Project, o),
                };
            yield return new ViewInfo<FlowDataCsvImporter, FlowTimeSeriesCsvImportDialog>
                {
                    Description = "Flow1D CSV Importer",
                    AfterCreate = (v, o) =>
                    {
                        var guiSelectionDataItem = Gui.Selection as IDataItem; 
                        if (guiSelectionDataItem == null)
                            throw new InvalidOperationException("Can not start the Flow1D CSV importer.");

                        if (guiSelectionDataItem.Value is DataItemsEventedListAdapter<WaterFlowModel1DLateralSourceData>)
                        {
                            v.BatchMode = true;
                            v.ForBoundaryConditions = false;
                        }
                        else if (guiSelectionDataItem.Value is DataItemsEventedListAdapter<WaterFlowModel1DBoundaryNodeData>)
                        {
                            v.BatchMode = true;
                            v.ForBoundaryConditions = true;
                        }
                        else if (guiSelectionDataItem.Value is WaterFlowModel1DLateralSourceData)
                        {
                            v.BatchMode = false;
                            v.ForBoundaryConditions = false;
                        }
                        else if (guiSelectionDataItem.Value is WaterFlowModel1DBoundaryNodeData)
                        {
                            v.BatchMode = false;
                            v.ForBoundaryConditions = true;
                        }

                    }
                };
            yield return SharpMapGisGuiPlugin.CreateAttributeTableViewInfo<WaterFlowModel1DBoundaryNodeData, WaterFlowModel1D>( m => m.BoundaryConditions, () => Gui);
            yield return SharpMapGisGuiPlugin.CreateAttributeTableViewInfo<WaterFlowModel1DLateralSourceData, WaterFlowModel1D>(m => m.LateralSourceData, () => Gui);
            yield return new ViewInfo<WaterFlowModel1DBoundaryNodeData, WaterFlowModel1DBoundaryNodeDataViewWpf>
                {
                    Description = "Boundary Node Data View (Flow 1D)"
                };
            yield return new ViewInfo<WaterFlowModel1DLateralSourceData, WaterFlowModel1DLateralSourceDataViewWpf>
                {
                    Description = "Lateral Source Data View (Flow 1D)"
                };
            yield return new ViewInfo<RoughnessNetworkCoverage, RoughnessSection, RoughnessSectionCoverageTableView>
                {
                    CompositeViewType = typeof(ProjectItemMapView),
                    GetCompositeViewData = o => Gui.Application.DataItemService.GetDataItemByValue(Gui.Application.Project, o),
                    GetViewData = coverage => Gui.Application.GetAllModelsInProject()
                                       .OfType<WaterFlowModel1D>()
                                       .SelectMany(m => m.RoughnessSections)
                                       .FirstOrDefault(rs => Equals(rs.RoughnessNetworkCoverage, coverage))
                };
            yield return new ViewInfo<WaterFlowModel1D, ValidationView>
                {
                    Description = "Validation report",
                    AfterCreate = (v, o) =>
                    {
                        v.Gui = Gui;
                        v.OnValidate = d => new WaterFlowModel1DModelValidator().Validate(d as WaterFlowModel1D);
                    }
                };
            yield return new ViewInfo<IEnumerable<ICrossSection>, RefreshMainSectionWidthsDialog>
                {
                    Description = "Refresh Main Section Width View (Cross sections in Flow1D)"
                };
        }

        public override IMapLayerProvider MapLayerProvider
        {
            get { return new WaterFlowModel1DMapLayerProvider(); }
        }

        public override void Activate()
        {
            Gui.Application.ProjectOpened += ApplicationProjectOpened;
            Gui.Application.ProjectClosing += ApplicationProjectClosing;

            SubscribeToCurrentProject();
            base.Activate();
        }

        public override void Deactivate()
        {
            Gui.Application.ProjectOpened -= ApplicationProjectOpened;
            Gui.Application.ProjectClosing -= ApplicationProjectClosing;

            base.Deactivate();
        }

        public override IMenuItem GetContextMenu(object sender, object dataobject)
        {
            //TODO: method is a mess clean up.

            IFunction function;
            bool activeViewIsMapView = Gui != null && Gui.DocumentViews.GetActiveViews<MapView>().Count()==1;

            if (dataobject is WaterFlowModel1DBoundaryNodeData)
            {
                //add zoom to functionality to context menu
                var waterFlowModel1DBoundaryNodeData = (WaterFlowModel1DBoundaryNodeData) dataobject;
                if (waterFlowModel1DBoundaryNodeData.IsLinked || 
                    waterFlowModel1DBoundaryNodeData.DataType == WaterFlowModel1DBoundaryNodeDataType.FlowConstant ||
                    waterFlowModel1DBoundaryNodeData.DataType == WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant)
                {
                    if (activeViewIsMapView)
                    {
                        zoomToToolStripMenuItem.Available = true;
                        generateDataInSeriesToolStripMenuItem.Available = false;
                        return new MenuItemContextMenuStripAdapter(generateDataMenu);
                    }
                    return null;
                }
                function = waterFlowModel1DBoundaryNodeData.Data;
            }
            else if (dataobject is WaterFlowModel1DLateralSourceData)
            {
                var waterFlowModel1DLateralSourceData = (WaterFlowModel1DLateralSourceData)dataobject;
                if (waterFlowModel1DLateralSourceData.IsLinked || 
                    waterFlowModel1DLateralSourceData.DataType == WaterFlowModel1DLateralDataType.FlowConstant)
                {
                    if (activeViewIsMapView)
                    {
                        zoomToToolStripMenuItem.Available = true;
                        generateDataInSeriesToolStripMenuItem.Available = false;
                        return new MenuItemContextMenuStripAdapter(generateDataMenu);
                    }
                    return null;
                }
                function = waterFlowModel1DLateralSourceData.Data;
            }
            else
            {
                return null;
            }

            var node = sender as TreeNode;
            if (node != null && node.Tag is IDataItem)
            {
                if (((IDataItem)node.Tag).Role != DataItemRole.Input)
                {
                    return null;
                }
                if (((IDataItem)node.Tag).IsLinked)
                {
                    return null;
                }
            }

            if (function == null || function is IVariable)
            {
                return null;
            }

            if (function.Arguments.Count > 0)
            {
                if (function.Arguments[0].ValueType != typeof(DateTime))
                {
                    return null;  
                }
            }
            zoomToToolStripMenuItem.Available = activeViewIsMapView;
            generateDataInSeriesToolStripMenuItem.Available = true;
            generateDataInSeriesToolStripMenuItem.Tag = function;
            // only support dataserieswizard for function with one argument for now.
            generateDataInSeriesToolStripMenuItem.Enabled = (function.Arguments.Count == 1);
            return new MenuItemContextMenuStripAdapter(generateDataMenu);

        }

        public override IEnumerable<ITreeNodePresenter> GetProjectTreeViewNodePresenters()
        {
            //yield return new WaterFlowBoundaryConditionsNodePresenter(this);
            yield return new WaterFlowModel1DBoundaryNodeDataProjectNodePresenter {GuiPlugin = this};
            yield return new WaterFlowModel1DLateralDataProjectNodePresenter { GuiPlugin = this };
            yield return new RoughnessSectionNodePresenter { GuiPlugin = this };
            yield return new WaterFlowModel1DNodePresenter(this);
            yield return new WindFunctionNodePresenter();
            yield return new MeteoFunctionNodePresenter();
            yield return new TemperatureCoverageNodePresenter();
        }

        private void InitializeComponent()
        {   
            generateDataInSeriesToolStripMenuItem = new ClonableToolStripMenuItem
            {
                Image = Properties.Resources.generateDataInSeriesToolStripMenuItem_Image,
                Name = "generateDataInSeriesToolStripMenuItem",
                Text = Properties.Resources.WaterFlowModel1DGuiPlugin_InitializeComponent_Generate_Data_in_Series___
            };
           
            zoomToToolStripMenuItem = new ClonableToolStripMenuItem
            {
                Name = "zoomToToolStripMenuItem",
                Size = new Size(201, 22),
                Text = Properties.Resources.WaterFlowModel1DGuiPlugin_InitializeComponent_Zoom_to_Feature
            };
            
            modelMergeMenuItem = new ClonableToolStripMenuItem
            {
                //Image = Properties.Resources.,
                Name = "mergeModelToolStripMenuItem",
                Size = new Size(201, 22),
                Text = Properties.Resources.WaterFlowModel1DGuiPlugin_InitializeComponent_Merge_Model_with
            };

            generateDataInSeriesToolStripMenuItem.Click += GenerateDataInSeriesToolStripMenuItemClick;
            zoomToToolStripMenuItem.Click += ZoomToToolStripMenuItemClick;

            generateDataMenu = new ContextMenuStrip
            {
                Name = "generateDataMenu", 
                Size = new Size(202, 48)
            };
            generateDataMenu.Items.AddRange(new ToolStripItem[] { generateDataInSeriesToolStripMenuItem, zoomToToolStripMenuItem });

            
            modelMergeMenu = new ContextMenuStrip
            {
                Name = "Merge",
                Size = new Size(202, 48)
            };

            modelMergeMenu.Items.AddRange(new ToolStripItem[] { modelMergeMenuItem });
        }

        public override void OnViewAdded(IView view)
        {
            if (view is CrossSectionView)
            {
                var crossSectionView = view as CrossSectionView;
                crossSectionView.GetConveyanceCalculators = GetCalculatorsForCrossSection;
            }
            else if (view is ProjectItemMapView)
            {
                var centralMapView = (ProjectItemMapView)view;

                if (centralMapView.MapView.MapControl.GetToolByType<BoundaryNodeDataMapTool>() == null)
                {
                    centralMapView.MapView.MapControl.Tools.Add(new BoundaryNodeDataMapTool());
                }

                if (centralMapView.MapView.MapControl.GetToolByType<LateralSourceDataMapTool>() == null)
                {
                    centralMapView.MapView.MapControl.Tools.Add(new LateralSourceDataMapTool());
                }

                var hydroRegionEditorMapTool = centralMapView.MapView.MapControl.GetToolByType<HydroRegionEditorMapTool>();
                if (hydroRegionEditorMapTool == null) return;

                var addInterpolatedCrossSectionTool = hydroRegionEditorMapTool.MapControl.GetToolByName(HydroRegionEditorMapTool.AddInterpolatedCrossSectionToolName) as NewPointFeatureTool;

                if (null == addInterpolatedCrossSectionTool)
                {
                    return;
                }

                GetInterpolatedCrossSection.HydroNetwork = hydroRegionEditorMapTool.HydroRegions.OfType<IHydroNetwork>().FirstOrDefault();
                addInterpolatedCrossSectionTool.GetFeaturePerProvider = GetCrossSectionPerProvider;
            }
        }

        private IEnumerable<IConveyanceCalculator> GetCalculatorsForCrossSection(ICrossSection cs)
        {
            var flowModels = Gui.Application.Project.GetAllItemsRecursive().OfType<WaterFlowModel1D>();
            foreach (var waterFlowModel1D in flowModels)
            {
                if (waterFlowModel1D.Network.CrossSections.Contains(cs))
                {
                    yield return new WaterFlowModel1DConveyanceCalculator(waterFlowModel1D);
                }
            }
        }

        private IEnumerable<IFeature> GetCrossSectionPerProvider(IPoint point)
        {
            var hasReturnedAFeature = false;
            var flowModels = Gui.Application.Project.GetAllItemsRecursive().OfType<WaterFlowModel1D>();
            foreach (var waterFlowModel1D in flowModels)
            {
                if (waterFlowModel1D.Network == GetInterpolatedCrossSection.HydroNetwork)
                {
                    hasReturnedAFeature = true;
                    GetInterpolatedCrossSection.WaterFlowModel1D = waterFlowModel1D;
                    yield return GetInterpolatedCrossSection.GetInterpolatedCrossSectionAt(point);
                }
            }

            //extra provider for zw (no roughness values of model needed) cross-sections if there's no model
            if (!hasReturnedAFeature)
            {
                GetInterpolatedCrossSection.WaterFlowModel1D = null;
                yield return GetInterpolatedCrossSection.GetInterpolatedCrossSectionAt(point);  
            }
        }

        private void ApplicationProjectClosing(Project project)
        {
            ((INotifyPropertyChanged)project).PropertyChanged -= ProjectPropertyChanged; 
            GetInterpolatedCrossSection.DisposeInstance();
        }

        private void ApplicationProjectOpened(Project project)
        {
            SubscribeToCurrentProject();
        }

        private void SubscribeToCurrentProject()
        {
            ((INotifyPropertyChanged)Gui.Application.Project).PropertyChanged += ProjectPropertyChanged;
        }

        private void ProjectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((sender is IHydroNetwork) && (e.PropertyName == "IsEditing"))
            {
                var network = (sender as IHydroNetwork);
                //we just start
                if (network.IsEditing && network.CurrentEditAction is BranchMergeAction)
                {
                    if (!editActionHandled)
                    {
                        //let it go...the entities will remove the data from the merged branches
                        editActionHandled = true;

                        var coveragesWithDataOnMergedBranches =
                            GetCoveragesWithDataOnMergedBranches(network.CurrentEditAction as BranchMergeAction).ToList();
                        if ((coveragesWithDataOnMergedBranches.Count > 0) && ShouldCancelMerge(coveragesWithDataOnMergedBranches))
                        {
                            //cancel the merge
                            ((HydroNetwork) network).EditWasCancelled = true;
                        }
                    }
                }
                else
                {
                    editActionHandled = false;
                }
            }
            if (sender is ReverseRoughnessSection && e.PropertyName == "UseNormalRoughness")
            {
                Gui.DocumentViews.OfType<ProjectItemMapView>().ForEach(v => v.RefreshModelLayers());
            }
        }

        private IEnumerable<INetworkCoverage> GetCoveragesWithDataOnMergedBranches(BranchMergeAction branchMergeAction)
        {
            //find out if there are coverages with data defined on any of the branches..this is called a conflict
            //I suppose this can be slow..lucky we don't call this method all the time ;)
            var coveragesWithDataOnMergedBranch = Gui.Application.Project.GetAllItemsRecursive().OfType<INetworkCoverage>()
                //find a coverage which has locations for any of these branches
                .Where(c => c.GetLocationsForBranch(branchMergeAction.RemovedBranch).Count > 0 ||
                                     c.GetLocationsForBranch(branchMergeAction.ExtendedBranch).Count > 0);
                
            return coveragesWithDataOnMergedBranch;
        }

        private static bool ShouldCancelMerge(List<INetworkCoverage> coveragesWithDataOnMergedBranches)
        {
            var message = "If you merge these branches the following data will be removed for these branches:\n\n";
            //add names of the coverages
            foreach (var c in coveragesWithDataOnMergedBranches)
            {
                message += c.Name + "\n";
            }
            message += "\nDo you want to continue merging the branches?";
    
            return (DialogResult.No ==
                    MessageBox.Show(message,
                                    "Merge conflict", MessageBoxButtons.YesNo));
        }

        private void GenerateDataInSeriesToolStripMenuItemClick(object sender, EventArgs e)
        {
            // The function that was right-clicked on is in the menu item Tag
            var function = (IFunction) generateDataInSeriesToolStripMenuItem.Tag;
            var variable = function.Arguments[0];

            try
            {
                var wizard = new GenerateDataSeriesWizard(function, variable);

                if (variable.ValueType == typeof (DateTime))
                {
                    var time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                    wizard.StartValue = time;
                    wizard.EndValue = time + new TimeSpan(1, 0, 0, 0);
                    wizard.IntervalValue = new TimeSpan(0, 10, 0);
                    wizard.Value = function.Components[0].DefaultValue;
                }
                wizard.ShowDialog();
            }
            catch (Exception exception) //WTF never do this. Catch the exception you expect!?
            {
            }
        }

        private void ZoomToToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Send selection to all relevant views
            if ((Gui.Selection is DataItem) || (Gui.Selection == null))
            {
                var cmd = new MapZoomToFeatureCommand();
                var valueObject = ((DataItem) Gui.Selection).Value;
                if(valueObject is WaterFlowModel1DBoundaryNodeData)
                {
                    cmd.Execute(((WaterFlowModel1DBoundaryNodeData)valueObject).Feature);
                }
                if (valueObject is WaterFlowModel1DLateralSourceData)
                {
                    cmd.Execute(((WaterFlowModel1DLateralSourceData)valueObject).Feature);
                }
            }
        }
        public override IGui Gui
        {
            get { return base.Gui; }
            set
            {
                if (base.Gui != null)
                {
                    Gui.Application.ActivityRunner.ActivityStatusChanged -= ActivityRunnerActivityStatusChanged;
                }

                base.Gui = value;

                if (base.Gui != null)
                {
                    Gui.Application.ActivityRunner.ActivityStatusChanged += ActivityRunnerActivityStatusChanged;
                }
            }
        }

        [InvokeRequired]
        private void ActivityRunnerActivityStatusChanged(object sender, ActivityStatusChangedEventArgs e)
        {
            if (!(sender is WaterFlowModel1D) || e.NewStatus != ActivityStatus.Failed) return;

            Gui.CommandHandler.OpenView(sender, typeof(ValidationView));
        }
    }

    public interface IHydroModelGuiPlugin
    {
        Window GetValidationReportControl(object o);
    }
}