using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    public class HydroAreaTreeViewNodePresenter : TreeViewNodePresenterBaseForPluginGui<HydroArea>
    {
        public HydroAreaTreeViewNodePresenter(GuiPlugin guiPlugin) : base(guiPlugin)
        {
        }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, HydroArea nodeData)
        {
            node.Text = nodeData.Name;
            node.Image = Properties.Resources.hydroarea;
        }
    }
}