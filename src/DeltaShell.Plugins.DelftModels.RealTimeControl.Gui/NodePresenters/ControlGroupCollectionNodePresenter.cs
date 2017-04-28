using System.Collections;
using System.Drawing;
using DelftTools.Controls;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.NodePresenters
{
    class ControlGroupCollectionNodePresenter : TreeViewNodePresenterBaseForPluginGui<IEventedList<ControlGroup>>
    {
        private static readonly Bitmap FolderIcon = RealTimeControl.Properties.Resources.folder;

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, IEventedList<ControlGroup> nodeData)
        {
            node.Text = "Control Groups";
            node.Tag = nodeData;
            node.Image = FolderIcon;
        }

        public override IEnumerable GetChildNodeObjects(IEventedList<ControlGroup> parentNodeData, ITreeNode node)
        {
            foreach (var controlGroup in parentNodeData)
            {
                yield return controlGroup;
            }
        }

        /// <summary>
        /// Override the GetContextMenu to attacht the RTC model to the menuitem
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="nodeData"></param>
        /// <returns></returns>
        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            var model = (RealTimeControlModel) sender.Parent.Parent.Tag;
            return GuiPlugin == null ? null : GuiPlugin.GetContextMenu(model, nodeData);
        }
    }
}