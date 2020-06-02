using System.Drawing;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    public class RunoffBoundaryTreeViewNodePresenter : TreeViewNodePresenterBaseForPluginGui<RunoffBoundary>
    {
        private static readonly Image BoundaryImage = Resources.runoff;

        public RunoffBoundaryTreeViewNodePresenter(GuiPlugin guiPlugin)
            : base(guiPlugin) {}

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, RunoffBoundary nodeData)
        {
            node.Image = BoundaryImage;
            UpdateNodeText(nodeData, node);
        }

        public override bool CanRenameNode(ITreeNode node)
        {
            return true;
        }

        protected override bool CanRemove(RunoffBoundary nodeData)
        {
            return true;
        }

        protected override bool RemoveNodeData(object parentNodeData, RunoffBoundary source)
        {
            source.Basin.Boundaries.Remove(source);
            return true;
        }

        private static void UpdateNodeText(RunoffBoundary source, ITreeNode node)
        {
            node.Text = string.IsNullOrEmpty(source.Name)
                            ? string.Format("<no name>")
                            : string.Format("{0}", source.Name);
        }
    }
}