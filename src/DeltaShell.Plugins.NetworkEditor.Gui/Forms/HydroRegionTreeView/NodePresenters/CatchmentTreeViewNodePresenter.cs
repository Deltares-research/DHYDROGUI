using System.Collections;
using System.Drawing;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    public class CatchmentTreeViewNodePresenter : TreeViewNodePresenterBaseForPluginGui<Catchment>
    {
        private static readonly Image CatchmentImage = Resources.catchment;

        public CatchmentTreeViewNodePresenter(GuiPlugin guiPlugin) : base(guiPlugin) {}

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, Catchment nodeData)
        {
            node.Image = nodeData.CatchmentType != null ? nodeData.CatchmentType.Icon : null;
            UpdateNodeText(nodeData, node);
        }

        public override bool CanRenameNode(ITreeNode node)
        {
            return true;
        }

        public override IEnumerable GetChildNodeObjects(Catchment parentNodeData, ITreeNode node)
        {
            return parentNodeData.SubCatchments.Cast<object>();
        }

        protected override bool CanRemove(Catchment nodeData)
        {
            return true;
        }

        protected override bool RemoveNodeData(object parentNodeData, Catchment source)
        {
            var parentCatchment = parentNodeData as Catchment;
            if (parentCatchment != null) //subcatchment
            {
                parentCatchment.SubCatchments.Remove(source);
                return true;
            }

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