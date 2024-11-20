using System.Collections;
using System.Drawing;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    public class DrainageBasinTreeViewNodePresenter : TreeViewNodePresenterBaseForPluginGui<IDrainageBasin>
    {
        private static readonly Image BasinImage = Properties.Resources.DrainageBasin;

        public DrainageBasinTreeViewNodePresenter(GuiPlugin guiPlugin) : base(guiPlugin) { }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, IDrainageBasin basin)
        {
            node.Text = basin.Name;
            node.Image = BasinImage;
        }

        public override bool CanRenameNode(ITreeNode node)
        {
            return true;
        }
        
        public override void OnNodeRenamed(IDrainageBasin basin, string newName)
        {
            if (basin.Name != newName)
            {
                basin.Name = newName;
            }
        }

        public override IEnumerable GetChildNodeObjects(IDrainageBasin basin, ITreeNode node)
        {
            yield return basin.CatchmentTypes;
            yield return basin.Catchments;
            yield return basin.WasteWaterTreatmentPlants;
            yield return basin.Boundaries;
        }
    }
}