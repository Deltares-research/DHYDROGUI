using System.Drawing;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Editing;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    class ObservationPointTreeViewNodePresenter : BranchFeatureTreeViewNodePresenterBase<ObservationPoint>
    {
        public ObservationPointTreeViewNodePresenter(GuiPlugin guiPlugin)
            : base(guiPlugin)
        {
        }

        protected override bool CanRemove(ObservationPoint nodeData)
        {
            return true;
        }

        protected override bool RemoveNodeData(object parentNodeData, ObservationPoint observationPoint)
        {
            var network = observationPoint.Branch.Network;
            network.BeginEdit("Delete feature " + observationPoint.Name);
            observationPoint.Branch.BranchFeatures.Remove(observationPoint);
            network.EndEdit();
            return true;
        }

        /// <summary>
        /// Lateral sources can be renamed by the user
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override bool CanRenameNode(ITreeNode node)
        {
            return true;
        }

        protected override Image GetImage(ObservationPoint feature)
        {
            return Properties.Resources.Observation;
        }
    }
}
