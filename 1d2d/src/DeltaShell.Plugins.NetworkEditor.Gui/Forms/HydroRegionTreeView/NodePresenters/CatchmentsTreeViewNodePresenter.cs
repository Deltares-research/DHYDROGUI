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
    public class CatchmentsTreeViewNodePresenter : TreeViewNodePresenterBaseForPluginGui<IEventedList<Catchment>>
    {
        public CatchmentsTreeViewNodePresenter(GuiPlugin guiPlugin) : base(guiPlugin)
        {
        }

        private static readonly Image CatchmentImage = Properties.Resources.catchment;

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, IEventedList<Catchment> nodeData)
        {
            node.Text = "Catchments";
            node.Image = CatchmentImage;
        }

        public override IEnumerable GetChildNodeObjects(IEventedList<Catchment> parentNodeData, ITreeNode node)
        {
            return parentNodeData.Cast<object>();
        }
    }
}