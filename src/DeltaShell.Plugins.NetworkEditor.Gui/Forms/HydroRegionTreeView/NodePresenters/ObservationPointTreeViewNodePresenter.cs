using System.Drawing;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    internal class ObservationPointTreeViewNodePresenter : BranchFeatureTreeViewNodePresenterBase<ObservationPoint>
    {
        public ObservationPointTreeViewNodePresenter(GuiPlugin guiPlugin)
            : base(guiPlugin) {}

        /// <summary>
        /// Lateral sources can be renamed by the user
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override bool CanRenameNode(ITreeNode node)
        {
            return true;
        }

        protected override bool CanRemove(ObservationPoint nodeData)
        {
            return true;
        }

        protected override bool RemoveNodeData(object parentNodeData, ObservationPoint observationPoint)
        {
            INetwork network = observationPoint.Branch.Network;
            network.BeginEdit(new DefaultEditAction("Delete feature " + observationPoint.Name));
            observationPoint.Branch.BranchFeatures.Remove(observationPoint);
            network.EndEdit();
            return true;
        }

        protected override Image GetImage(ObservationPoint feature)
        {
            return Resources.Observation;
        }
    }
}