using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Controls.Wpf.Services;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf.Validation;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using Mono.Addins;
using MessageBox = DelftTools.Controls.Swf.MessageBox;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui
{
    [Extension(typeof(IPlugin))]
    public class HydroModelGuiPlugin : GuiPlugin
    {
        private readonly HydroModelMapLayerProvider hydroModelMapLayerProvider = new HydroModelMapLayerProvider();
        private ClonableToolStripMenuItem modelMergeMenuItem;
        private ContextMenuStrip modelMergeMenu;

        public HydroModelGuiPlugin()
        {
            InitializeComponent();
        }

        public override string Name => Properties.Resources.HydroModelGuiPlugin_Name_Hydro_Model_UI_;

        public override string DisplayName => Properties.Resources.HydroModelGuiPlugin_DisplayName_Hydro_Model_Plugin__UI_;

        public override string Description => Properties.Resources.HydroModelGuiPlugin_Description_Provides_functionality_to_create_and_run_integrated_models_;

        public override string Version => AssemblyUtils.GetAssemblyInfo(GetType().Assembly).Version;

        public override string FileFormatVersion => "1.1.0.0";

        public override IGui Gui
        {
            get => base.Gui;
            set
            {
                if (base.Gui != null)
                {
                    Gui.AfterRun -= OnGuiAfterRun;
                    Gui.Application.ActivityRunner.ActivityStatusChanged -= ActivityRunnerActivityStatusChanged;
                }

                base.Gui = value;

                if (base.Gui != null)
                {
                    Gui.AfterRun += OnGuiAfterRun;
                    Gui.Application.ActivityRunner.ActivityStatusChanged += ActivityRunnerActivityStatusChanged;
                }
            }
        }

        public override IMapLayerProvider MapLayerProvider => hydroModelMapLayerProvider;

        public override IEnumerable<PropertyInfo> GetPropertyInfos()
        {
            return Enumerable.Empty<PropertyInfo>();
        }

        public override IEnumerable<ViewInfo> GetViewInfoObjects()
        {
            yield return new ViewInfo<HydroModel, HydroModelSettings>
            {
                Description = "Hydro Model Settings",
                GetViewName = (v, o) => o.Name + " Settings",
                CompositeViewType = typeof(ProjectItemMapView),
                GetCompositeViewData = o => o,
                AfterCreate = (v, o) =>
                {
                    ProjectItemMapView mapView = Gui.DocumentViews.OfType<ProjectItemMapView>().SingleOrDefault(view => view.MapView.GetLayerForData(o) != null);
                    mapView?.MapView?.Map?.Render();

                    v.RunCallback = (m) =>
                    {
                        Gui.Selection = m;
                        Gui.CommandHandler.RunSelectedModel();
                    };
                    v.WorkflowSelectedCallback = (a) => { Gui.Selection = a; };
                    v.AddNewActivityCallback = (m) =>
                    {
                        Gui.Selection = m;
                        return Gui.CommandHandler.AddNewModel();
                    };
                    v.RemoveActivityCallback = (a) => Gui.CommandHandler.DeleteProjectItem(a);
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
                        var destinationModel = destination as IModelMerge;
                        return destinationModel?.ValidateMerge(source as IModelMerge);
                    };
                    v.OnMerge = (destination, source) =>
                    {
                        var destinationModel = destination as IModelMerge;
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

        public override IEnumerable<ITreeNodePresenter> GetProjectTreeViewNodePresenters()
        {
            yield return new HydroModelTreeViewNodePresenter(this);
        }

        /// <summary>
        /// Build and return a context menu given the <paramref name="sender"/> and
        /// <paramref name="data"/>.
        /// </summary>
        /// <param name="sender"> The object responsible for triggering this action. </param>
        /// <param name="data"> The data necessary for this action. </param>
        /// <returns> A new <see cref="IMenuItem"/> for the specified parameters. </returns>
        public override IMenuItem GetContextMenu(object sender, object data)
        {
            IMenuItem projectExplorerContextMenu = Gui.MainWindow.ProjectExplorer.GetContextMenu(null, data);
            IMenuItem mergeMenu = SetupMergeMenu(data);
            var model = data as HydroModel;
            if (model == null)
            {
                if (projectExplorerContextMenu != null)
                {
                    ContextMenuStrip projectExplorerContextMenuStrip =
                        ((MenuItemContextMenuStripAdapter) projectExplorerContextMenu).ContextMenuStrip;

                    List<ClonableToolStripMenuItem> contextMenuItems =
                        projectExplorerContextMenuStrip.Items.OfType<ClonableToolStripMenuItem>()
                                                       .Where(i => i.Text == Properties.Resources.HydroModelGuiPlugin_GetContextMenu_Validate___)
                                                       .ToList();

                    foreach (ClonableToolStripMenuItem menuItem in contextMenuItems)
                    {
                        menuItem.Click -= OnValidateClicked;
                        projectExplorerContextMenuStrip.Items.Remove(menuItem);
                    }
                }

                if (data is IHydroModel hydroModel)
                {
                    HydroModel[] allCompositeHydroModels = Gui.Application.ProjectService.Project.RootFolder.GetAllModelsRecursive().OfType<HydroModel>().ToArray();
                    bool isChildModel = allCompositeHydroModels.Any(m => m.Activities.Contains(hydroModel));

                    Folder folder = GetFolderContaining(hydroModel);
                    if (folder != null && !isChildModel)
                    {
                        MenuItemContextMenuStripAdapter contextMenu = CreateTurnIntoOrMoveToIntegratedModelMenuItem(hydroModel, folder, allCompositeHydroModels);
                        CleanFileExplorerContextMenu(data);
                        contextMenu.Insert(0, new MenuItemContextMenuStripAdapter(modelMergeMenu));
                        return contextMenu;
                    }
                    
                    CleanFileExplorerContextMenu(data);
                    return mergeMenu;
                }

                CleanFileExplorerContextMenu(data);
                return null;
            }

            if (projectExplorerContextMenu != null)
            {
                AddValidateMenuItems(data, projectExplorerContextMenu, model);
            }

            CleanFileExplorerContextMenu(data);
            return mergeMenu;
        }

        private void AddValidateMenuItems(object data, IMenuItem projectExplorerContextMenu, HydroModel model)
        {
            bool missesValidateMenuItems = projectExplorerContextMenu.OfType<ClonableToolStripMenuItem>()
                                                                     .All(mi => mi.Text != Properties.Resources.HydroModelGuiPlugin_GetContextMenu_Validate___);
            if (missesValidateMenuItems)
            {
                var subMenu = new ContextMenuStrip();
                var validateItem = new ClonableToolStripMenuItem
                {
                    Text = Properties.Resources.HydroModelGuiPlugin_GetContextMenu_Validate___,
                    Tag = model,
                    Image = Properties.Resources.validation
                };
                validateItem.Click += OnValidateClicked;
                subMenu.Items.Add(validateItem);
                projectExplorerContextMenu.Add(new MenuItemContextMenuStripAdapter(subMenu));
            }
            else
            {
                // Data item is persistent, but the Tag is lost. Resetting validation tag item again.
                IEnumerable<ClonableToolStripMenuItem> validateItems =
                    projectExplorerContextMenu.OfType<ClonableToolStripMenuItem>()
                                              .Where(mi => mi.Text == Properties.Resources.HydroModelGuiPlugin_GetContextMenu_Validate___ &&
                                                           mi.Tag != data);

                validateItems.ForEach(menuItem => menuItem.Tag = model);
            }
        }

        private static MenuItemContextMenuStripAdapter CreateTurnIntoOrMoveToIntegratedModelMenuItem(IHydroModel hydroModel, Folder folder, HydroModel[] allCompositeHydroModels)
        {
            var topItem = new ClonableToolStripMenuItem { Text = Properties.Resources.HydroModelGuiPlugin_GetContextMenu_Turn_into_or_Move_to_Integrated_Model };

            var upgradeItem = new ClonableToolStripMenuItem { Text = Properties.Resources.HydroModelGuiPlugin_GetContextMenu_Turn_into_Integrated_Model };
            upgradeItem.Click += (s, e) => hydroModel.UpgradeModelIntoIntegratedModel(folder);
            topItem.DropDownItems.Add(upgradeItem);

            if (allCompositeHydroModels.Length > 0)
            {
                topItem.DropDownItems.Add(new ToolStripSeparator());
            }

            foreach (HydroModel compositeHydroModel in allCompositeHydroModels)
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
                        MessageBox.Show(Properties.Resources.HydroModelGuiPlugin_GetContextMenu_This_will_overwrite_the_existing_model__Are_you_sure_,
                                        Properties.Resources.HydroModelGuiPlugin_GetContextMenu_Overwrite_existing_model_,
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
            return contextMenu;
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

            IModel model = Gui.Application.ModelService.GetModelByDataItem(Gui.Application.ProjectService.Project, dataItem);
            if (model is HydroModel && region.Parent != null && dataItem.LinkedBy.Count > 0)
            {
                return false; // data item is a sub-region and it is being used - delete model first (unlink)
            }

            return base.CanDelete(item);
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

            modelMergeMenu.Items.AddRange(new ToolStripItem[]
            {
                modelMergeMenuItem
            });
        }

        [InvokeRequired]
        private void ActivityRunnerActivityStatusChanged(object sender, ActivityStatusChangedEventArgs e)
        {
            if (!(sender is HydroModel) || e.NewStatus != ActivityStatus.Failed ||
                e.NewStatus == ActivityStatus.Failed && e.OldStatus == ActivityStatus.Executing)
            {
                return;
            }

            Gui.CommandHandler.OpenView(sender, typeof(ValidationView));
        }

        private void OnValidateClicked(object sender, EventArgs e)
        {
            var model = (HydroModel) ((ToolStripItem) sender).Tag;
            Gui.DocumentViewsResolver.OpenViewForData(model, typeof(ValidationView));
        }

        /// <summary>
        /// </summary>
        /// <param name="data"></param>
        private void CleanFileExplorerContextMenu(object data)
        {
            IMenuItem projectExplorerContextMenu = Gui.MainWindow.ProjectExplorer.GetContextMenu(null, data);
            if (projectExplorerContextMenu == null)
            {
                return;
            }

            ContextMenuStrip projectExplorerContextMenuStrip =
                ((MenuItemContextMenuStripAdapter) projectExplorerContextMenu).ContextMenuStrip;
            foreach (string menuItemName in modelMergeMenu.Items.OfType<ClonableToolStripMenuItem>()
                                                          .Select(item => item.Text)
                                                          .Where(name => !string.IsNullOrWhiteSpace(name)))
            {
                string name = menuItemName;
                List<ClonableToolStripMenuItem> menuItems = projectExplorerContextMenuStrip.Items.OfType<ClonableToolStripMenuItem>()
                                                                                           .Where(i => i.Text == name)
                                                                                           .ToList();

                foreach (ClonableToolStripMenuItem menuItem in menuItems)
                {
                    menuItem.Click -= ModelMergeMenuItemClick;
                    projectExplorerContextMenuStrip.Items.Remove(menuItem);
                }
            }
        }

        private IMenuItem SetupMergeMenu(object nodeData)
        {
            var destinationModel = nodeData as IModelMerge;
            if (destinationModel == null)
            {
                return null;
            }

            // model merge
            List<string> mergeModelNamesInProject = Gui.Application.ProjectService.Project.RootFolder.GetAllModelsRecursive().OfType<IModelMerge>().Where(m => m != destinationModel && destinationModel.CanMerge(m)).Select(m => m.Name).ToList();
            modelMergeMenuItem.Available = mergeModelNamesInProject.Count != 0;

            foreach (object dropDownItem in modelMergeMenuItem.DropDownItems)
            {
                var mergeModelMenu = dropDownItem as ClonableToolStripMenuItem;
                if (mergeModelMenu != null)
                {
                    mergeModelMenu.Click -= ModelMergeMenuItemClick;
                }
            }

            modelMergeMenuItem.DropDownItems.Clear();

            if (modelMergeMenuItem.Available)
            {
                foreach (string sourceModelNames in mergeModelNamesInProject)
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
            string modelToMergeWith = ((ClonableToolStripMenuItem) sender).Text;

            var destinationModel = Gui.Selection as IModelMerge;
            if (destinationModel == null)
            {
                return;
            }

            var sourceModel = (IModelMerge) Gui.Application.ProjectService.Project.RootFolder.GetAllModelsRecursive().OfType<IModelMerge>().Cast<IModel>().FirstOrDefault(m => m.Name == modelToMergeWith);
            Gui.DocumentViewsResolver.OpenViewForData(new ValidateMergeModelObjects
            {
                DestinationModel = destinationModel,
                SourceModel = sourceModel
            }, typeof(MergeModelValidationView));
        }

        private static Folder GetFolderContaining(IProjectItem projectItem)
        {
            while (projectItem != null)
            {
                var dataItem = projectItem as IDataItem;
                IProjectItem parent = dataItem != null && dataItem.Parent != null
                                          ? dataItem.Parent
                                          : projectItem.Owner();

                var folder = parent as Folder;
                if (folder != null)
                {
                    return folder;
                }

                projectItem = parent;
            }

            return null;
        }

        private void OnGuiAfterRun()
        {
            // speed-up hydro model (including all sub-models) creation
            HydroModel.BuildModel(ModelGroup.All);
        }
    }
}