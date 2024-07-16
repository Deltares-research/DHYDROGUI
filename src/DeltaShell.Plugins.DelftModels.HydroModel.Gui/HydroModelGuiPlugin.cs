using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Controls.Wpf.Services;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf.Validation;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.ProjectExplorer;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.GraphicsProviders;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Views;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using Mono.Addins;
using NetTopologySuite.Extensions.Coverages;
using SharpMap.Api.Layers;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using HydroModelGuiProperties = DeltaShell.Plugins.DelftModels.HydroModel.Gui.Properties;
using MessageBox = DelftTools.Controls.Swf.MessageBox;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui
{
    [Extension(typeof(IPlugin))]
    public class HydroModelGuiPlugin : GuiPlugin
    {
        private readonly HydroModelMapLayerProvider hydroModelMapLayerProvider = new HydroModelMapLayerProvider();
        private ClonableToolStripMenuItem modelMergeMenuItem;
        private ContextMenuStrip modelMergeMenu;
        private readonly IGraphicsProvider graphicsProvider;

        public HydroModelGuiPlugin()
        {
            InitializeComponent();
            graphicsProvider = new HydroModelGuiGraphicsProvider();
        }

        private void InitializeComponent()
        {
            modelMergeMenuItem = new ClonableToolStripMenuItem
            {
                Name = "mergeModelToolStripMenuItem",
                Size = new Size(201, 22),
                Text = "Merge with:"
            };

            modelMergeMenu = new ContextMenuStrip
            {
                Name = "Merge",
                Size = new Size(202, 48)
            };

            modelMergeMenu.Items.AddRange(new ToolStripItem[] { modelMergeMenuItem });
        }

        public override string Name => "Hydro Model (UI)";

        public override string DisplayName => "Hydro Model Plugin (UI)";

        public override string Description => DelftModels.HydroModel.Properties.Resources.HydroModelApplicationPlugin_Description;

        public override string Version => GetType().Assembly.GetName().Version.ToString();

        public override string FileFormatVersion => "1.1.0.0";

        public override IGraphicsProvider GraphicsProvider => graphicsProvider;

        public override IGui Gui 
        { 
            get { return base.Gui; }
            set
            {
                if (base.Gui != null)
                {
                    Gui.Application.ActivityRunner.ActivityStatusChanged -= ActivityRunnerActivityStatusChanged;
                    Gui.Application.ProjectService.ProjectClosing -= ApplicationOnProjectClosing;
                    Gui.Application.ProjectService.ProjectOpened -= ApplicationOnProjectOpened;
                }

                base.Gui = value;

                if (base.Gui != null)
                {
                    Gui.Application.ActivityRunner.ActivityStatusChanged += ActivityRunnerActivityStatusChanged;
                    Gui.Application.ProjectService.ProjectClosing += ApplicationOnProjectClosing;
                    Gui.Application.ProjectService.ProjectOpened += ApplicationOnProjectOpened;
                }
            }
        }

        [InvokeRequired]
        private void ApplicationOnProjectOpened(object sender, EventArgs<Project> e)
        {
            Gui.Selection = e.Value.RootFolder.Models.FirstOrDefault();
            Gui.CommandHandler.OpenViewForSelection();
        }

        public override void OnViewRemoved(IView view)
        {
            view.GetViewsOfType<MapView>()
                .ForEach(mv => mv.Map.Layers.GetLayersRecursive(true, true)
                                 .ForEach(CleanUpLayer));
        }

        private static void CleanUpLayer(ILayer layer)
        {
            switch (layer)
            {
                // set with dummy coverage to release event handlers -> null gives null ref exception
                // should be handled in NetworkCoverageGroupLayer dispose

                case NetworkCoverageGroupLayer networkCoverageGroupLayer:
                    networkCoverageGroupLayer.NetworkCoverage = new NetworkCoverage("dummy", false);
                    break;
                case FeatureCoverageLayer featureCoverageLayer:
                    var coverage = new FeatureCoverage();
                    coverage.Components.Add(new Variable<double>());
                    featureCoverageLayer.FeatureCoverage = coverage;
                    break;
            }

            if (layer.DataSource is FeatureCollection featureCollection)
            {
                featureCollection.AddNewFeatureFromGeometryDelegate = null;
            }
        }

        private void ApplicationOnProjectClosing(object sender, EventArgs<Project> e)
        {
            // remove memory leaks
            Gui.Selection = null;
            Gui.SelectedProjectItem = null;
        }

        private void ActivityRunnerActivityStatusChanged(object sender, ActivityStatusChangedEventArgs e)
        {
            switch (sender)
            {
                case FileImportActivity fileImportActivity when e.NewStatus == ActivityStatus.Cleaning:
                    // remove memory leaks
                    fileImportActivity.FileImporter.ProgressChanged = null;
                    TypeUtils.SetField(fileImportActivity, "target", null);
                    break;

                case HydroModel hydroModel when e.NewStatus == ActivityStatus.Failed:
                    OpenValidationView(hydroModel);
                    break;
            }
        }

        [InvokeRequired]
        private void OpenValidationView(HydroModel hydroModel)
        {
            Gui.CommandHandler.OpenView(hydroModel, typeof(ValidationView));
        }

        public override IEnumerable<ViewInfo> GetViewInfoObjects()
        {
            yield return new ViewInfo<ProjectTemplate, CreateHydroModelSettingView>
            {
                AdditionalDataCheck = t => t.Id?.Equals(HydroModelApplicationPlugin.RHUINTEGRATEDMODEL_TEMPLATE_ID, StringComparison.CurrentCultureIgnoreCase) ?? false
            };
            yield return new ViewInfo<ProjectTemplate, DimrTemplateView>
            {
                AdditionalDataCheck = t => t.Id == HydroModelApplicationPlugin.DimrProjectTemplateId,
                AfterCreate = (view, _) => view.DHydroConfigXmlImporter = Gui.Application.FileImporters.OfType<DHydroConfigXmlImporter>().First()
            };
            
            yield return new ViewInfo<HydroModel, HydroModelSettings>
                {
                    Description = "Hydro Model Settings",
                    GetViewName = (v, o) => o?.Name + " Settings",
                    CompositeViewType = typeof(ProjectItemMapView),
                    GetCompositeViewData = o => o,
                    AfterCreate = (v, o) =>
                        {
                            v.RunCallback = (m) =>
                                {
                                    Gui.Selection = m;
                                    Gui.CommandHandler.RunSelectedModel();
                                };
                            v.WorkflowSelectedCallback = (a) =>
                                {
                                    Gui.Selection = a;
                                };
                            v.AddNewActivityCallback = (m) =>
                                {
                                    Gui.Selection = m;
                                    return Gui.CommandHandler.AddNewModel();
                                };
                        }
                };

            yield return
                new ViewInfo<DHydroConfigXmlExporter, DHydroExporterDialog>
                {
                    AfterCreate = (v, o) =>
                    {
                        v.Gui = Gui;
                        v.FolderDialogService = new FolderDialogService();
                    }
                };

            yield return new ViewInfo<ValidateMergeModelObjects, MergeModelValidationView>
            {
                Description = "Validation Report",
                AfterCreate = (v, o) =>
                {
                    v.Gui = Gui;
                    v.OnMergeValidate = (destination, source) =>
                    {
                        var destinationModel = (destination as IModelMerge);
                        return destinationModel == null ? null : destinationModel.ValidateMerge(source as IModelMerge);
                    };
                    v.OnMerge = (destination, source) =>
                    {
                        var destinationModel = (destination as IModelMerge);
                        return destinationModel != null && destinationModel.Merge(source as IModelMerge, null);
                    };
                }
            };
            yield return new ViewInfo<HydroModel, ValidationView>
            {
                Description = "Validation Report",
                AfterCreate = (v, o) =>
                {
                    v.Gui = Gui;
                    v.OnValidate = delegate(object d) { return o.Validate(); };
                }
            };
        }

        public override IMapLayerProvider MapLayerProvider
        {
            get { return hydroModelMapLayerProvider; }
        }

        public override IEnumerable<ITreeNodePresenter> GetProjectTreeViewNodePresenters()
        {
            yield return new HydroModelTreeViewNodePresenter(this);
        }

        public override IMenuItem GetContextMenu(object sender, object data)
        {
            var mergeMenu = SetupMergeMenu(data);
            var model = data as HydroModel;
            if (model == null)
            {
                var projectExplorerContextMenu = Gui.MainWindow.ProjectExplorer.GetContextMenu(null, data);
                if (projectExplorerContextMenu != null)
                {
                    var projectExplorerContextMenuStrip =
                        ((MenuItemContextMenuStripAdapter)projectExplorerContextMenu).ContextMenuStrip;

                    var contextMenuItems =
                        projectExplorerContextMenuStrip.Items.OfType<ClonableToolStripMenuItem>()
                            .Where(i => i.Text == HydroModelGuiProperties.Resources.HydroModelGuiPlugin_GetContextMenu_Validate___)
                            .ToList();

                    foreach (var menuItem in contextMenuItems)
                    {
                        menuItem.Click -= OnValidateClicked;
                        projectExplorerContextMenuStrip.Items.Remove(menuItem);
                    }
                }
                var hydroModel = data as IHydroModel;
                if (hydroModel != null)
                {
                    var allCompositeHydroModels = Gui.Application.GetAllModelsInProject().OfType<HydroModel>().ToArray();
                    var isChildModel = allCompositeHydroModels.Any(m => m.Activities.Contains(hydroModel));
                    
                    var folder = GetFolderContaining(hydroModel);
                    if (folder != null && !isChildModel)
                    {
                        var topItem = new ClonableToolStripMenuItem
                        {
                            Text =
                                Properties.Resources
                                    .HydroModelGuiPlugin_GetContextMenu_Turn_into_or_Move_to_Integrated_Model,
                        };

                        var upgradeItem = new ClonableToolStripMenuItem
                        {
                            Text = Properties.Resources.HydroModelGuiPlugin_GetContextMenu_Turn_into_Integrated_Model
                        };
                        upgradeItem.Click += (s, e) => hydroModel.UpgradeModelIntoIntegratedModel(folder, Gui?.Application);
                        topItem.DropDownItems.Add(upgradeItem);

                        if (allCompositeHydroModels.Length > 0)
                        {
                            topItem.DropDownItems.Add(new ToolStripSeparator());
                        }

                        foreach (var compositeHydroModel in allCompositeHydroModels)
                        {
                            var moveItem = new ClonableToolStripMenuItem
                            {
                                Text =
                                    Properties.Resources.HydroModelGuiPlugin_GetContextMenu_Move_into_ +
                                    compositeHydroModel.Name
                            };
                            moveItem.Click += (s, e) =>
                            {
                                if (
                                    compositeHydroModel.Activities.Any(a => a.GetType().Implements(hydroModel.GetType())) &&
                                    MessageBox.Show(
                                        Properties.Resources
                                            .HydroModelGuiPlugin_GetContextMenu_This_will_overwrite_the_existing_model__Are_you_sure_,
                                        Properties.Resources
                                            .HydroModelGuiPlugin_GetContextMenu_Overwrite_existing_model_,
                                        MessageBoxButtons.YesNo) != DialogResult.Yes)
                                {
                                    return;
                                }
                                hydroModel.MoveModelIntoIntegratedModel(folder, compositeHydroModel);
                            };
                            topItem.DropDownItems.Add(moveItem);
                        }
                        var strip = new ContextMenuStrip();
                        strip.Items.Add(topItem);
                        var contextMenu = new MenuItemContextMenuStripAdapter(strip);

                        if (modelMergeMenu != null)
                        {
                            CleanFileExplorerContextMenu(data);
                            contextMenu.Insert(0, new MenuItemContextMenuStripAdapter(modelMergeMenu));
                        }

                        return contextMenu;
                    }
                    else
                    {
                        if (modelMergeMenu != null)
                        {
                            CleanFileExplorerContextMenu(data);
                        }
                        return mergeMenu;
                    }
                }
                CleanFileExplorerContextMenu(data);
                return null;
            }
            else
            {
                var projectExplorerContextMenu = Gui.MainWindow.ProjectExplorer.GetContextMenu(null, data);
                if (projectExplorerContextMenu != null)
                {
                    if (projectExplorerContextMenu.OfType<ClonableToolStripMenuItem>().All(mi => mi.Text != HydroModelGuiProperties.Resources.HydroModelGuiPlugin_GetContextMenu_Validate___))
                    {
                        var subMenu = new ContextMenuStrip();
                        var validateItem = new ClonableToolStripMenuItem
                        {
                            Text = HydroModelGuiProperties.Resources.HydroModelGuiPlugin_GetContextMenu_Validate___,
                            Tag = model,
                            Image = HydroModelGuiProperties.Resources.validation
                        };
                        validateItem.Click += OnValidateClicked;
                        subMenu.Items.Add(validateItem);
                        projectExplorerContextMenu.Add(new MenuItemContextMenuStripAdapter(subMenu));
                    }
                    else
                    {
                        // Data item is persistent, but the Tag is lost. Resetting validation tag item again.
                        var validateItems = projectExplorerContextMenu.OfType<ClonableToolStripMenuItem>().Where(mi => mi.Text == HydroModelGuiProperties.Resources.HydroModelGuiPlugin_GetContextMenu_Validate___ && mi.Tag == null);
                        foreach (var clonableToolStripMenuItem in validateItems)
                        {
                            clonableToolStripMenuItem.Tag = model;
                        }
                    }
                }
            }
            CleanFileExplorerContextMenu(data);
            return mergeMenu;
        }

        private void OnValidateClicked(object sender, EventArgs e)
        {
            var model = (HydroModel)((ToolStripItem)sender).Tag;
            Gui.DocumentViewsResolver.OpenViewForData(model, typeof(ValidationView));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        private void CleanFileExplorerContextMenu(object data)
        {
            var projectExplorerContextMenu = Gui.MainWindow.ProjectExplorer.GetContextMenu(null, data);
            if (projectExplorerContextMenu == null) return;
            var projectExplorerContextMenuStrip =
                ((MenuItemContextMenuStripAdapter) projectExplorerContextMenu).ContextMenuStrip;
            foreach (
                var menuItemName in
                    modelMergeMenu.Items.OfType<ClonableToolStripMenuItem>()
                        .Select(item => item.Text)
                        .Where(name => !string.IsNullOrWhiteSpace(name)))
            {
                var name = menuItemName;
                var menuItems =
                    projectExplorerContextMenuStrip.Items.OfType<ClonableToolStripMenuItem>()
                        .Where(i => i.Text == name)
                        .ToList();

                foreach (var menuItem in menuItems)
                {
                    menuItem.Click -= ModelMergeMenuItemClick;
                    projectExplorerContextMenuStrip.Items.Remove(menuItem);
                }
            }
            
        }

        private IMenuItem SetupMergeMenu(object nodeData)
        {
            var destinationModel = nodeData as IModelMerge;
            if (destinationModel == null) return null;
            // model merge
            var mergeModelNamesInProject = Gui.Application.GetAllModelsInProject().OfType<IModelMerge>().Where(m => m != destinationModel && destinationModel.CanMerge(m)).Select(m => m.Name).ToList();
            modelMergeMenuItem.Available = mergeModelNamesInProject.Count != 0;

            foreach (var dropDownItem in modelMergeMenuItem.DropDownItems)
            {
                var mergeModelMenu = dropDownItem as ClonableToolStripMenuItem;
                if (mergeModelMenu != null) mergeModelMenu.Click -= ModelMergeMenuItemClick;
            }
            modelMergeMenuItem.DropDownItems.Clear();

            if (modelMergeMenuItem.Available)
            {
                foreach (var sourceModelNames in mergeModelNamesInProject)
                {
                    var mergeModelMenuItem = new ClonableToolStripMenuItem
                    {
                        Name = "modelMergeToolStripMenuItem_" + sourceModelNames.Replace(" ", "_"),
                        Size = new Size(201, 22),
                        Text = sourceModelNames
                    };
                    mergeModelMenuItem.Click += ModelMergeMenuItemClick;
                    modelMergeMenuItem.DropDownItems.Add(mergeModelMenuItem);
                }
            }
            return new MenuItemContextMenuStripAdapter(modelMergeMenu);
        }

        private void ModelMergeMenuItemClick(object sender, EventArgs e)
        {
            var modelToMergeWith = ((ClonableToolStripMenuItem)sender).Text;

            var destinationModel = Gui.Selection as IModelMerge;
            if (destinationModel == null) return;
            var sourceModel = (IModelMerge)Gui.Application.GetAllModelsInProject().OfType<IModelMerge>().Cast<IModel>().FirstOrDefault(m => m.Name == modelToMergeWith);
            Gui.DocumentViewsResolver.OpenViewForData(new ValidateMergeModelObjects { DestinationModel = destinationModel, SourceModel = sourceModel }, typeof(MergeModelValidationView));
        }
        private static Folder GetFolderContaining(IProjectItem projectItem)
        {
            while (projectItem != null)
            {
                var dataItem = projectItem as IDataItem;
                var parent = dataItem != null && dataItem.Parent != null
                                 ? dataItem.Parent
                                 : projectItem.Owner();

                var folder = parent as Folder;
                if (folder != null)
                    return folder;
                
                projectItem = parent;
            }
            return null;
        }

        public override bool CanLink(IDataItem item)
        {
            var ownerModel = item.Owner as IModel;
            if (ownerModel != null && ownerModel.Owner is HydroModel)
            {
                return false; // no link for items contained in child models of HydroModel
            }

            return base.CanLink(item);
        }

        public override bool CanUnlink(IDataItem item)
        {
            var ownerModel = item.Owner as IModel;
            if (ownerModel != null && ownerModel.Owner is HydroModel)
            {
                return false; // no unlink for items contained in child models of HydroModel
            }

            return base.CanUnlink(item);
        }

        public override bool CanCopy(IProjectItem item)
        {
            var dataItem = item as IDataItem;
            if (dataItem != null)
            {
                var region = dataItem.Value as IHydroRegion;
                if (region != null && region.Parent != null)
                {
                    return false; // copy/paste of child items is not implemented
                }
            }

            var model = item as IModel;
            if (model != null && model.Owner is HydroModel)
            {
                return false; // for now disable copying of any submodel in HydroModel
            }

            return base.CanCopy(item);
        }

        public override bool CanCut(IProjectItem item)
        {
            var dataItem = item as IDataItem;
            if (dataItem != null)
            {
                var region = dataItem.Value as IHydroRegion;
                if (region != null && region.Parent != null)
                {
                    return false; // copy/paste of child items is not implemented
                }
            }

            var model = item as IModel;
            if (model != null && model.Owner is HydroModel)
            {
                return false; // for now disable copying of any submodel in HydroModel
            }

            return base.CanCut(item);
        }

        public override bool CanPaste(IProjectItem item, IProjectItem container)
        {
            if (container is HydroModel)
            {
                return false; // for now disable pasting anything into HydroModel
            }

            return base.CanPaste(item, container);
        }

        public override bool CanDelete(IProjectItem item)
        {
            var dataItem = item as IDataItem;
            if (dataItem == null)
            {
                return true;
            }

            var region = dataItem.Value as IHydroRegion;
            if (region == null)
            {
                return true;
            }

            var model = Gui.Application.ModelService.GetModelByDataItem(Gui.Application.Project, dataItem);
            if(model is HydroModel && region.Parent != null && dataItem.LinkedBy.Count > 0)
            {
                return false; // data item is a sub-region and it is being used - delete model first (unlink)
            }

            return base.CanDelete(item);
        }
    }
}