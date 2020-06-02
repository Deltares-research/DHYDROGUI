using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.ProjectExplorer
{
    public class DrainageBasinProjectTreeViewNodePresenter : TreeViewNodePresenterBaseForPluginGui<DrainageBasin>
    {
        public override bool CanRenameNode(ITreeNode node)
        {
            return true;
        }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, DrainageBasin nodeData)
        {
            node.Image = Resources.DrainageBasin;
        }

        public override DragOperations CanDrag(DrainageBasin nodeData)
        {
            if (nodeData.Parent != null)
            {
                return DragOperations.Link;
            }

            return DragOperations.Link | DragOperations.Move;
        }
    }
}