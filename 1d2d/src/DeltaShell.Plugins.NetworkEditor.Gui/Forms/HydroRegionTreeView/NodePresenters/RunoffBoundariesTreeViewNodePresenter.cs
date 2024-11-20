using System.Collections;
using System.Drawing;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    public class RunoffBoundariesTreeViewNodePresenter : TreeViewNodePresenterBaseForPluginGui<IEventedList<RunoffBoundary>>
    {
        public RunoffBoundariesTreeViewNodePresenter(GuiPlugin guiPlugin)
            : base(guiPlugin)
        {
        }

        private static readonly Image BoundariesImage = Properties.Resources.runoff;

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, IEventedList<RunoffBoundary> nodeData)
        {
            node.Text = "Runoff Boundaries";
            node.Image = BoundariesImage;
        }

        public override IEnumerable GetChildNodeObjects(IEventedList<RunoffBoundary> parentNodeData, ITreeNode node)
        {
            return parentNodeData.Cast<object>();
        }
    }
}