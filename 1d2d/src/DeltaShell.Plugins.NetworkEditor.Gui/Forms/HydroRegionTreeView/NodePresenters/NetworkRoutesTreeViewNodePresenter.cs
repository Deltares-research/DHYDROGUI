using System.Collections;
using System.Drawing;
using DelftTools.Controls;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils.Collections.Generic;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    public class NetworkRoutesTreeViewNodePresenter : TreeViewNodePresenterBaseForPluginGui<IEventedList<Route>>
    {
        private static readonly Image RoutesImage = Properties.Resources.routes;

        public NetworkRoutesTreeViewNodePresenter(GuiPlugin guiPlugin)
            : base(guiPlugin)
        {
        }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, IEventedList<Route> nodeData)
        {
            node.Text = "Routes";
            node.Image = RoutesImage;
        }

        public override IEnumerable GetChildNodeObjects(IEventedList<Route> parentNodeData, ITreeNode node)
        {
            foreach (var route in parentNodeData)
            {
                yield return route;
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
    }
}