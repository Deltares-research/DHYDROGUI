using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.ProjectExplorer
{
    public class HydroRegionProjectTreeViewNodePresenter : TreeViewNodePresenterBaseForPluginGui<HydroRegion>
    {
        public override bool CanRenameNode(ITreeNode node)
        {
            return true;
        }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, HydroRegion region)
        {
            node.Image = Resources.HydroRegion;

            // first time added region
            if (!node.IsLoaded && region.Id == 0)
            {
                node.Expand(); // region
            }
        }

        public override DragOperations CanDrag(HydroRegion nodeData)
        {
            return (DragOperations.Link | DragOperations.Copy);
        }
    }
}