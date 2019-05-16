using System.Drawing;
using System.Windows.Forms;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.FMSuite.Common.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors.Buttons
{
    public static class EditCoverageLayersHelper
    {
        public static string ToolTip = "Edit number of depth layers.";
        public static string Label = "Depth layers";
        public static Bitmap ButtonImage = Properties.Resources.waterLayers;

        public static void ButtonAction(object inputObject)
        {
            var model = inputObject as WaterFlowFMModel.WaterFlowFMModel;
            if (model == null) return;

            var depthLayerDefinition = model.InitialSalinity;

            var dialog = new VerticalProfileDialog();
            dialog.SetSupportedProfileTypes(SupportedVerticalProfileTypes.InitialConditionProfileTypes);
            dialog.ModelDepthLayerDefinition = model.DepthLayerDefinition;
            dialog.VerticalProfileDefinition = depthLayerDefinition.VerticalProfile;

            dialog.ShowDialog();
            if (dialog.DialogResult == DialogResult.OK)
            {
                model.BeginEdit(new DefaultEditAction("replacing salinity vertical profile definition"));
                depthLayerDefinition.VerticalProfile = dialog.VerticalProfileDefinition;
                model.EndEdit();
            }
        }

        public static string DepthLayersToString(object inputObject)
        {
            var model = inputObject as WaterFlowFMModel.WaterFlowFMModel;
            if (model == null) return string.Empty;

            return model.InitialSalinity?.VerticalProfile.Type.ToString() ?? string.Empty;
        }
    }
}
