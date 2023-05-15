using System.Drawing;
using DelftTools.Controls;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    public class NetworkRouteTreeViewNodePresenter : TreeViewNodePresenterBaseForPluginGui<Route>
    {
        private static readonly Image RouteImage = Properties.Resources.route;

        public NetworkRouteTreeViewNodePresenter(GuiPlugin guiPlugin)
            : base(guiPlugin)
        {
        }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, Route nodeData)
        {
            string postFix = nodeData.Locations.Values.Count == 0 ? " (empty)" : "";
            postFix = nodeData.Locations.Values.Count == 1 ? " (one node)" : postFix;
            node.Text = string.Format("{0}{1}", nodeData.Name, postFix);
            node.Image = RouteImage;
        }

        public override bool CanRenameNode(ITreeNode node)
        {
            return true;
        }

        public override void OnNodeRenamed(Route data, string newName)
        {
            if (data.Name != newName)
            {
                data.Name = newName;
            }
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            if (GuiPlugin == null)
            {
                return null;
            }
            return GuiPlugin.GetContextMenu(sender, nodeData);
        }

        protected override bool CanRemove(Route nodeData)
        {
            return true;
        }

        protected override bool RemoveNodeData(object parentNodeData, Route route)
        {
            HydroNetworkHelper.RemoveRoute(route);
            return true;
        }
    }
}