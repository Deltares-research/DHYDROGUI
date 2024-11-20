using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Shell.Gui.Swf.Validation;
using DeltaShell.Plugins.CommonTools.TextData;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Properties;
using GeoAPI.Extensions.Coverages;
using MessageBox = DelftTools.Controls.Swf.MessageBox;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.NodePresenters
{
    public class RealTimeControlModelNodePresenter : ModelNodePresenterBase<RealTimeControlModel>
    {
        public static readonly string InputFolderName = "Input";
        public static readonly string OutputFolderName = "Output";

        private IGui gui;

        public RealTimeControlModelNodePresenter(GuiPlugin guiPlugin)
            : base(guiPlugin)
        {
            gui = guiPlugin.Gui;
        }

        public override Type NodeTagType => typeof(RealTimeControlModel);

        public override bool CanRenameNode(ITreeNode node) => true;

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            IMenuItem contextMenu = base.GetContextMenu(sender, nodeData);

            var subMenu = new ContextMenuStrip();

            var model = nodeData as RealTimeControlModel;

            if (model != null)
            {
                var validateItem = new ClonableToolStripMenuItem
                {
                    Text = Resources.RealTimeControlModelNodePresenter_GetContextMenu_Validate___,
                    Tag = model,
                    Image = RealTimeControl.Properties.Resources.validation
                };
                validateItem.Click += OnValidateClicked;
                subMenu.Items.Add(validateItem);
            }

            var item = new ClonableToolStripMenuItem {Text = Resources.RealTimeControlModelNodePresenter_GetContextMenu_Open_Last_Working_Directory___};
            item.Click += (o, e) => OnOpenLastWorkingDirectoryClicked(nodeData as RealTimeControlModel);
            subMenu.Items.Add(item);

            contextMenu.Add(new MenuItemContextMenuStripAdapter(subMenu));

            return contextMenu;
        }

        public override DragOperations CanDrop(object item, ITreeNode sourceNode, ITreeNode targetNode,
                                               DragOperations validOperations)
        {
            if (item is RealTimeControlModel)
            {
                return DragOperations.None;
            }

            var rtcModel = (RealTimeControlModel) targetNode.Tag;
            //can accept only one model which is IEIP
            if (item is IModel && !rtcModel.ControlledModels.Any())
            {
                return GetDefaultDropOperation(TreeView, item, sourceNode, targetNode, validOperations);
            }

            return DragOperations.None;
        }

        /// <summary>
        /// Gets the child node objects.
        /// </summary>
        /// <param name="parentNodeData">The RTC model.</param>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        public override IEnumerable GetChildNodeObjects(RealTimeControlModel parentNodeData, ITreeNode node)
        {
            foreach (var inputItem in GetInputItems(parentNodeData))
            {
                yield return inputItem;
            }

            yield return new OutputTreeFolder(parentNodeData, GetOutputItems(parentNodeData), OutputFolderName);
        }

        protected override void OnPropertyChanged(RealTimeControlModel item, ITreeNode node, PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(item, node, e);

            if (e.PropertyName == nameof(RealTimeControlModel.RestartInput))
            {
                TreeView.RefreshChildNodes(node);
            }
        }

        protected override bool CanRemove(RealTimeControlModel nodeData)
        {
            return true;
        }

        protected override bool RemoveNodeData(object parentNodeData, RealTimeControlModel nodeData)
        {
            return gui.CommandHandler.DeleteCurrentProjectItem();
        }

        private void OnValidateClicked(object sender, EventArgs e)
        {
            var model = (RealTimeControlModel) ((ToolStripItem) sender).Tag;
            Gui.DocumentViewsResolver.OpenViewForData(model, typeof(ValidationView));
        }

        private static void OnOpenLastWorkingDirectoryClicked(RealTimeControlModel model)
        {
            string workingDir = model.LastWorkingDirectory;
            if (string.IsNullOrEmpty(workingDir))
            {
                MessageBox.Show(Resources.RealTimeControlModelNodePresenter_OnOpenLastWorkingDirectoryClicked_Working_directory_not_created_yet__Model_must_run_first_,
                                Resources.RealTimeControlModelNodePresenter_OnOpenLastWorkingDirectoryClicked_Cannot_open_working_directory,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
            else
            {
                Process.Start(workingDir); //open directory in explorer
            }
        }

        private static IEnumerable GetInputItems(RealTimeControlModel rtcModel)
        {
            yield return new TreeFolder(rtcModel, GetInitialConditions(rtcModel), "Initial Conditions", FolderImageType.None);
            yield return rtcModel.ControlGroups;
        }

        private static IEnumerable GetInitialConditions(RealTimeControlModel rtcModel)
        {
            yield return rtcModel.RestartInput;
        }

        private static IEnumerable GetOutputItems(RealTimeControlModel model)
        {
            yield return GetRestartFolder(model);

            foreach (IFeatureCoverage outputFeatureCoverage in model.OutputFeatureCoverages)
            {
                yield return outputFeatureCoverage;
            }

            foreach (ReadOnlyTextFileData textDocument in model.OutputDocuments)
            {
                yield return textDocument;
            }
        }

        private static object GetRestartFolder(RealTimeControlModel model)
        {
            return new TreeFolder(model, model.RestartOutput, NGHS.Common.Gui.Properties.Resources.RestartFolderName, FolderImageType.None);
        }
    }
}