using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Shell.Gui.Swf.Validation;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using log4net;
using MessageBox = DelftTools.Controls.Swf.MessageBox;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.NodePresenters
{
    public class RealTimeControlModelNodePresenter : ModelNodePresenterBase<RealTimeControlModel>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RealTimeControlModelNodePresenter));

        public static readonly string InputFolderName = "Input";
        public static readonly string OutputFolderName = "Output";
        
        private IGui gui;

        public RealTimeControlModelNodePresenter(GuiPlugin guiPlugin)
            : base(guiPlugin)
        {
            gui = guiPlugin.Gui;
        }

        public override Type NodeTagType
        {
            get { return typeof (RealTimeControlModel); }
        }

        public override bool CanRenameNode(ITreeNode node)
        {
            return true;
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            var contextMenu = base.GetContextMenu(sender, nodeData);

            var subMenu = new ContextMenuStrip();

            var model = nodeData as RealTimeControlModel;

            if (model != null)
            {
                var validateItem = new ClonableToolStripMenuItem
                {
                    Text = Properties.Resources.RealTimeControlModelNodePresenter_GetContextMenu_Validate___, 
                    Tag = model, 
                    Image = Resources.validation
                };
                validateItem.Click += OnValidateClicked;
                subMenu.Items.Add(validateItem);
            }

            var item = new ClonableToolStripMenuItem
            {
                Text = Properties.Resources.RealTimeControlModelNodePresenter_GetContextMenu_Open_Last_Working_Directory___
            };
            item.Click += (o, e) => OnOpenLastWorkingDirectoryClicked(nodeData as RealTimeControlModel);
            subMenu.Items.Add(item);

            contextMenu.Add(new MenuItemContextMenuStripAdapter(subMenu));

            return contextMenu;
        }

        private void OnValidateClicked(object sender, EventArgs e)
        {
            var model = (RealTimeControlModel)((ToolStripItem)sender).Tag;
            Gui.DocumentViewsResolver.OpenViewForData(model, typeof (ValidationView));
        }

        private void OnOpenLastWorkingDirectoryClicked(RealTimeControlModel model)
        {
            var workingDir = model.LastWorkingDirectory;
            if (String.IsNullOrEmpty(workingDir))
            {
                MessageBox.Show(Properties.Resources.RealTimeControlModelNodePresenter_OnOpenLastWorkingDirectoryClicked_Working_directory_not_created_yet__Model_must_run_first_,
                                Properties.Resources.RealTimeControlModelNodePresenter_OnOpenLastWorkingDirectoryClicked_Cannot_open_working_directory,
                                MessageBoxButtons.OK, 
                                MessageBoxIcon.Information);
            }
            else
            {
                Process.Start(workingDir); //open directory in explorer
            }
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
            if (((item is IModel) && (!rtcModel.ControlledModels.Any())))
            {
                return GetDefaultDropOperation(TreeView, item, sourceNode, targetNode, validOperations);
            }
            return DragOperations.None;

        }
    
        public override IEnumerable GetChildNodeObjects(RealTimeControlModel rtcModel, ITreeNode node)
        {
            yield return new TreeFolder(rtcModel, GetInputItems(rtcModel), InputFolderName, FolderImageType.Input);
            yield return new TreeFolder(rtcModel, GetOutputItems(rtcModel), OutputFolderName, FolderImageType.Output);
        }

        private IEnumerable GetInputItems(RealTimeControlModel rtcModel)
        {
            yield return new TreeFolder(rtcModel, GetInitialConditions(rtcModel), "Initial Conditions", FolderImageType.None);
            yield return rtcModel.ControlGroups;
        }

        private IEnumerable GetInitialConditions(RealTimeControlModel rtcModel)
        {
            yield return rtcModel.GetDataItemByValue(rtcModel.RestartInput);
        }

        private static IEnumerable GetOutputItems(RealTimeControlModel model)
        {
            yield return GetRestartFolder(model);

            foreach (var featureCoverage in model.OutputFeatureCoverages)
            {
                yield return model.GetDataItemByValue(featureCoverage);
            }

            //find a better way :(
            var logItem = model.DataItems.FirstOrDefault(di => di.Tag == "lastRunLogFileDataItem");
            if (logItem != null)
            {
                yield return logItem;
            }
        }

        private static object GetRestartFolder(RealTimeControlModel model)
        {
            return new TreeFolder(model, GetRestartStates(model), "States", FolderImageType.None);
        }

        private static IEnumerable GetRestartStates(RealTimeControlModel model)
        {
            var restartStates = model.DataItems.Where(IsOutputRestartFile);
            foreach (var restartState in restartStates)
            {
                yield return restartState;
            }
        }

        private static bool IsOutputRestartFile(IDataItem dataItem)
        {
            return dataItem.Value is FileBasedRestartState && dataItem.Role == DataItemRole.Output;
        }

        protected override bool CanRemove(RealTimeControlModel nodeData)
        {
            return true;
        }

        protected override bool RemoveNodeData(object parentNodeData, RealTimeControlModel nodeData)
        {
            return gui.CommandHandler.DeleteCurrentProjectItem();
        }
    }
}
