using System.Drawing;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    public class RetentionNodePresenter : TreeViewNodePresenterBaseForPluginGui<IRetention>
    {
        public RetentionNodePresenter(GuiPlugin guiPlugin)
            : base(guiPlugin)
        {
        }
        private static Image nodeImage;

        private static Image NodeImage
        {
            get
            {
                if (nodeImage == null)
                {
                    nodeImage = Resources.Retention;
                }
                return nodeImage;
            }
        }



        public override bool CanRenameNode(ITreeNode node)
        {           
            return true;
        }

        public override bool CanRenameNodeTo(ITreeNode node, string newName)
        {
            return true;
        }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, IRetention data)
        {
            node.Tag = data;
            node.Image = NodeImage;
            node.Text = data.Name;
        }

        protected override bool CanRemove(IRetention nodeData)
        {
            return true;
        }

        protected override bool RemoveNodeData(object parentNodeData, IRetention nodeData)
        {
            var network = nodeData.Network;
            network.BeginEdit("Delete feature " + nodeData.Name);
            nodeData.Branch.BranchFeatures.Remove(nodeData);
            network.EndEdit();
            return true;
        }
    }
}
