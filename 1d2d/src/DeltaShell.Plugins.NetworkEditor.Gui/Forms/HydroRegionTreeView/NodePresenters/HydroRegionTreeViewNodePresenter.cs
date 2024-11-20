using System.Collections;
using System.Drawing;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    public class HydroRegionTreeViewNodePresenter : TreeViewNodePresenterBaseForPluginGui<HydroRegion>
    {
        private static readonly Image RegionImage = Properties.Resources.HydroRegion;

        public HydroRegionTreeViewNodePresenter(GuiPlugin guiPlugin) : base(guiPlugin){ }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, HydroRegion region)
        {
            node.Text = region.Name;
            node.Image = RegionImage;

            // first time added region
            if (!node.IsLoaded && region.Id == 0)
            {
                node.Expand(); // region

                foreach (var treeNode in node.Nodes)
                {
                    treeNode.Expand(); // sub-region
                }
            }
        }

        public override bool CanRenameNode(ITreeNode node)
        {
            return true;
        }

        public override void OnNodeRenamed(HydroRegion region, string newName)
        {
            if (region.Name != newName)
            {
                region.Name = newName;
            }
        }

        public override IEnumerable GetChildNodeObjects(HydroRegion region, ITreeNode node)
        {
            return region.SubRegions.OfType<IHydroRegion>().Cast<object>();
        }
    }
}