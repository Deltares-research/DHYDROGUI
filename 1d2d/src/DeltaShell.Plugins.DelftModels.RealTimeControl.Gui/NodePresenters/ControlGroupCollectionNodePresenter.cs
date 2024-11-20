using System.Collections;
using System.Drawing;
using DelftTools.Controls;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.NodePresenters
{
    /// <summary>
    /// Node presenter for a collection of <see cref="ControlGroup"/> objects
    /// </summary>
    public sealed class ControlGroupCollectionNodePresenter : TreeViewNodePresenterBaseForPluginGui<IEventedList<ControlGroup>>
    {
        /// <summary>
        /// Icon used to represent a <see cref="ControlGroup"/> collection in the Project Explorer
        /// </summary>
        private static readonly Bitmap folderIcon = Resources.folder;

        /// <inheritdoc/>
        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, IEventedList<ControlGroup> nodeData)
        {
            node.Text = "Control Groups";
            node.Tag = nodeData;
            node.Image = folderIcon;
        }

        /// <inheritdoc/>
        public override IEnumerable GetChildNodeObjects(IEventedList<ControlGroup> parentNodeData, ITreeNode node)
        {
            foreach (ControlGroup controlGroup in parentNodeData)
            {
                yield return controlGroup;
            }
        }

        /// <summary>
        /// Override the GetContextMenu to attach the RTC model to the menuitem
        /// </summary>
        /// <param name="sender"><see cref="ITreeNode"/> for which a context menu must be created</param>
        /// <param name="nodeData">Node data that holds a reference to it's <see cref="RealTimeControlModel"/> domain object</param>
        /// <returns>A ContextMenu for this node or null.</returns>
        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            var model = sender?.Parent?.Tag as RealTimeControlModel;
            return model != null 
                       ? GuiPlugin?.GetContextMenu(model, nodeData) 
                       : null;
        }
    }
}