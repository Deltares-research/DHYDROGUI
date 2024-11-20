using System.Drawing;
using System.Windows.Forms;
using DeltaShell.Plugins.FMSuite.Common.DepthLayers;
using DeltaShell.Plugins.FMSuite.Common.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors.Buttons
{
    public static class EditDepthLayersHelper
    {
        public static Bitmap ButtonImage { get; } = Properties.Resources.waterLayers;

        public static void ButtonAction(object inputObject)
        {
            var model = inputObject as WaterFlowFMModel;
            if (model == null) return;
            var view = new DepthLayerDialog(WaterFlowFMModelDefinition.SupportedDepthLayerTypes)
                {
                    CanSpecifyLayerThicknesses = WaterFlowFMModelDefinition.CanSpecifyLayerThicknesses,
                    DepthLayerDefinition = model.DepthLayerDefinition.Clone() as DepthLayerDefinition
                };

            if (view.ShowDialog() == DialogResult.OK)
            {
                model.DepthLayerDefinition = view.DepthLayerDefinition;
            }
        }
    }
}