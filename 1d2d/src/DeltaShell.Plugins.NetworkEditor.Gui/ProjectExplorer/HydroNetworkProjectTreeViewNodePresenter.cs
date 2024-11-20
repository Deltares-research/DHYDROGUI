using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.ProjectExplorer
{
    public class HydroNetworkProjectTreeViewNodePresenter : TreeViewNodePresenterBaseForPluginGui<IHydroNetwork>
    {
        public override bool CanRenameNode(ITreeNode node)
        {
            return true;
        }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, IHydroNetwork nodeData)
        {
            node.Image = Resources.Network;
        }

        public override DragOperations CanDrag(IHydroNetwork nodeData)
        {
            if (nodeData.Parent != null)
            {
                return DragOperations.Link;
            }

            return (DragOperations.Link | DragOperations.Move);
        }

        protected override bool RemoveNodeData(object parentNodeData, IHydroNetwork nodeData)
        {
            if(nodeData.Parent is IHydroRegion)
            {
                var region = nodeData.Parent;
                region.SubRegions.Remove(nodeData);
                return true;
            }

            return false;
        }
    }
}