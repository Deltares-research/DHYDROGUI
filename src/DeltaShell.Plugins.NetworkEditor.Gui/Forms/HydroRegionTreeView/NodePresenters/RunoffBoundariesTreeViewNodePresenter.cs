using System.Collections;
using System.Drawing;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    public class RunoffBoundariesTreeViewNodePresenter : TreeViewNodePresenterBaseForPluginGui<IEventedList<RunoffBoundary>>
    {
        private static readonly Image BoundariesImage = Resources.runoff;

        public RunoffBoundariesTreeViewNodePresenter(GuiPlugin guiPlugin)
            : base(guiPlugin) {}

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