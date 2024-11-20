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
    public class CatchmentTypesNodePresenter : TreeViewNodePresenterBaseForPluginGui<IEventedList<CatchmentType>>
    {
        public CatchmentTypesNodePresenter(GuiPlugin guiPlugin) : base(guiPlugin) { }

        private static Image catchmentTypeImage = null;

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, IEventedList<CatchmentType> nodeData)
        {
            node.Text = "Catchment Types";
            node.Image = catchmentTypeImage;
        }

        public override IEnumerable GetChildNodeObjects(IEventedList<CatchmentType> parentNodeData, ITreeNode node)
        {
            return parentNodeData.Cast<object>();
        }
    }
}