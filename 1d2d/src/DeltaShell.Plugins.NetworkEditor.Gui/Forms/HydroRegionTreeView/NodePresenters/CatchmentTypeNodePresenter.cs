using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    public class CatchmentTypeNodePresenter : TreeViewNodePresenterBaseForPluginGui<CatchmentType>
    {
        public CatchmentTypeNodePresenter(GuiPlugin guiPlugin) : base(guiPlugin) { }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, CatchmentType nodeData)
        {
            node.Text = nodeData.Name;
            node.Image = nodeData.Icon;
        }
    }
}