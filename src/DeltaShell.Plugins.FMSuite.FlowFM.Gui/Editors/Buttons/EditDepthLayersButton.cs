using System.Drawing;
using System.Windows.Forms;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using DeltaShell.Plugins.FMSuite.Common.DepthLayers;
using DeltaShell.Plugins.FMSuite.Common.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors.Buttons
{
    public class EditDepthLayersHelper : IButtonBehaviour
    {
        public const string ToolTip = "Adjust layers";
        public const string Label = "Layer";
        public static readonly Bitmap ButtonImage = Resources.waterLayers;

        public void Execute(object inputObject)
        {
            var model = inputObject as WaterFlowFMModel;
            if (model == null)
            {
                return;
            }

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