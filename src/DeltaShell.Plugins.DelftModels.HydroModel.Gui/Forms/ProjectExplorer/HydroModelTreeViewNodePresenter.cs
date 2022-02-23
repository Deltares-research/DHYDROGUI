using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Drawing;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.ProjectExplorer
{
    public class HydroModelTreeViewNodePresenter : TreeViewNodePresenterBaseForPluginGui<HydroModel>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (HydroModelTreeViewNodePresenter));
        private readonly IGui gui;
        
        public HydroModelTreeViewNodePresenter(GuiPlugin guiPlugin) : base(guiPlugin)
        {
            gui = guiPlugin.Gui;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            
        }

        public override Type NodeTagType
        {
            get { return typeof (HydroModel); }
        }

        public override bool CanRenameNode(ITreeNode node)
        {
            return true;
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            var contextMenu = gui.MainWindow.ProjectExplorer.GetContextMenu(sender, nodeData);

            if (GuiPlugin != null)
            {
                var hydroModelPluginContextMenu = GuiPlugin.GetContextMenu(null, nodeData);
                if (hydroModelPluginContextMenu != null)
                {
                    contextMenu.Insert(0, hydroModelPluginContextMenu);
                }
            }
                
            return contextMenu;
        }

        public override bool CanRenameNodeTo(ITreeNode node, string newName)
        {
            // Needs to be a valid filename because a model work directory will be created based on model name
            if (!FileUtils.IsValidFileName(newName))
            {
                log.ErrorFormat("The name {0} is not a valid model name. It contains invalid characters.", newName);
                return false;
            }

            return true;
        }

        public override void OnNodeRenamed(HydroModel data, string newName)
        {
            if (data.Name != newName)
            {
                data.Name = newName;
            }
        }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, HydroModel nodeData)
        {
            node.Text = nodeData.Name;
            node.Tag = nodeData;
            UpdateNodeImage(node, nodeData);

            // first time added hydro model
            if (!node.IsLoaded && nodeData.Id == 0)
            {
                node.Expand(); // model
                node.Nodes.ForEach(n => n.Expand());
            }
        }

        private static void UpdateNodeImage(ITreeNode node, HydroModel model)
        {
            Image image = model.Status == ActivityStatus.Executing ? Resources.cog_active : Resources.CompositeModel;

            if (model.OutputOutOfSync)
            {
                image = image.AddOverlayImage(Resources.ExclamationOverlay, 4, -2);
            }

            if (node.Image == image) return;
            node.Image = image;
        }

        public override IEnumerable GetChildNodeObjects(HydroModel parentNodeData, ITreeNode node)
        {
            foreach (var activity in parentNodeData.Activities)
            {
                yield return activity;
            }

            var outputs = new List<TreeFolder>();

            if (parentNodeData.CurrentWorkflow is IHydroModelWorkFlow hydroModelWorkFlow && hydroModelWorkFlow.Data != null && hydroModelWorkFlow.Data.OutputDataItems.Any())
            {
                outputs.Add(new TreeFolder(parentNodeData, hydroModelWorkFlow.Data.OutputDataItems, "1D-2D Spatial Data", FolderImageType.None));
            }
            outputs.Add(new TreeFolder(parentNodeData, parentNodeData.GetDataItems<TextDocument>(DataItemRole.Output), "Reports", FolderImageType.None));
            
            yield return new TreeFolder(parentNodeData, outputs, "Output", FolderImageType.Output);
        }

        public override DragOperations CanDrag(HydroModel nodeData)
        {
            return DragOperations.Move | DragOperations.Copy;
        }

        public override DragOperations CanDrop(object item, ITreeNode sourceNode, ITreeNode targetNode, DragOperations validOperations)
        {
            return DragOperations.None;
        }
        ///<exception cref="NotSupportedException">When a <paramref name="item"/> has an unlinked <see cref="DataItem"/>s <see cref="DataItem.Value"/> does not inherit from <see cref="ICloneable"/>, is not null, or is not a value type.</exception>
        ///<exception cref="InvalidOperationException">
        /// When <paramref name="item"/> contains a <see cref="IModel"/> with a <see cref="DataItemSet"/> for which a <see cref="IDataItem"/>s <see cref="IDataItem.Owner"/> is not the data item set.</exception>
        public override void OnDragDrop(object item, object sourceParentNodeData, HydroModel target, DragOperations operation, int position)
        {
            if (!(item is IModel model))
            {
                log.Error("Only models can be dropped onto a hydro model.");
                return;
            }

            if ((operation & DragOperations.Move) != 0)
            {
                gui.CopyPasteHandler.Cut(model);
                gui.CopyPasteHandler.Paste(gui.Application.Project, target, position);
            }
            else if ((operation & DragOperations.Copy) != 0)
            {
                var modelCopy = (IModel) model.DeepClone();
                target.Activities.Insert(position, modelCopy);
            }
        }

        protected override bool CanRemove(HydroModel nodeData)
        {
            var models = gui.Application.ModelService.GetModelsLinkedBy(nodeData) //models linked to hydro model
                .Where(m => !nodeData.Activities.Contains(m)) //not contained by hydro model itself
                .ToList();

            if (models.Count <= 0) return true;

            log.Debug($"Model cannot be deleted because model '{models[0]}' contain links to it");
            return false;
        }

        protected override bool RemoveNodeData(object parentNodeData, HydroModel nodeData)
        {
            return gui.CommandHandler.DeleteCurrentProjectItem();
        }

        protected override void OnPropertyChanged(HydroModel item, ITreeNode node, PropertyChangedEventArgs e)
        {
            if (node == null) return;

            if (e.PropertyName == "Name")
            {
                node.Text = item.Name;
            }
            else
            {
                UpdateNodeImage(node, item);
            }
        }

        protected override void OnCollectionChanged(HydroModel childNodeData, ITreeNode parentNode, NotifyCollectionChangedEventArgs e, int newNodeIndex)
        {
            base.OnCollectionChanged(childNodeData, parentNode, e, newNodeIndex);

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    //make model node selected node in treeview.
                    ITreeNode node = TreeView.GetNodeByTag(childNodeData);
                    if (node != null) TreeView.SelectedNode = node;
                    break;

                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();
            }
        }
    }
}