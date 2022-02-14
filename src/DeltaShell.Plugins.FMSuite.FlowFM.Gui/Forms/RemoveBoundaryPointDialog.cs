using System.Linq;
using System.Windows.Forms;
using NetTopologySuite.Extensions.Features;
using MessageBox = DelftTools.Controls.Swf.MessageBox;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    internal class RemoveBoundaryPointDialog
    {
        private readonly IWaterFlowFMModel waterFlowFMModel;

        public RemoveBoundaryPointDialog(IWaterFlowFMModel waterFlowFMModel)
        {
            this.waterFlowFMModel = waterFlowFMModel;
        }

        public bool ShowDialogForFeature(Feature2D boundary, int index)
        {
            if (boundary.Geometry.Coordinates.Count() < 3) return false;
            if (waterFlowFMModel.BoundaryConditions.Any(
                    bc => ReferenceEquals(bc.Feature, boundary) && bc.DataPointIndices.Contains(index)))
            {
                var result = MessageBox.Show("The selected point contains boundary condition data. Continue delete?",
                                             "Warning",
                                             MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                return result == DialogResult.OK;
            }
            return true;
        }
    }
}
