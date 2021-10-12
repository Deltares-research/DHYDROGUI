using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    public class CatchmentTreeViewNodePresenter : TreeViewNodePresenterBaseForPluginGui<Catchment>
    {
        public CatchmentTreeViewNodePresenter(GuiPlugin guiPlugin) : base(guiPlugin)
        {
        }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, Catchment nodeData)
        {
            node.Image = nodeData.CatchmentType?.Icon;
            UpdateNodeText(nodeData, node);
        }

        protected override bool CanRemove(Catchment nodeData)
        {
            return true;
        }

        public override bool CanRenameNode(ITreeNode node)
        {
            return true;
        }

        protected override bool RemoveNodeData(object parentNodeData, Catchment source)
        {
            source.Basin.Catchments.Remove(source);
            return true;
        }

        private static void UpdateNodeText(Catchment source, ITreeNode node)
        {
            node.Text = string.Format("{0} ({1})",
                                      string.IsNullOrEmpty(source.Name)
                                          ? string.Format("<no name>")
                                          : string.Format("{0}", source.Name),
                                          source.CatchmentType != null ? source.CatchmentType.Name : "<no type>");
        }
    }
}