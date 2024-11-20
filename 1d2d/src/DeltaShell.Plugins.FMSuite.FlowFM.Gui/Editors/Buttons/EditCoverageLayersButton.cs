using System.Windows.Forms;
using DeltaShell.Plugins.FMSuite.Common.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors.Buttons
{
    public static class EditCoverageLayersHelper
    {
        public static void ButtonAction(object inputObject)
        {
            var model = inputObject as WaterFlowFMModel;
            if (model == null) return;

            var depthLayerDefinition = model.InitialSalinity;

            var dialog = new VerticalProfileDialog();
            dialog.SetSupportedProfileTypes(SupportedVerticalProfileTypes.InitialConditionProfileTypes);
            dialog.ModelDepthLayerDefinition = model.DepthLayerDefinition;
            dialog.VerticalProfileDefinition = depthLayerDefinition.VerticalProfile;

            dialog.ShowDialog();
            if (dialog.DialogResult == DialogResult.OK)
            {
                model.BeginEdit("replacing salinity vertical profile definition");
                depthLayerDefinition.VerticalProfile = dialog.VerticalProfileDefinition;
                model.EndEdit();
            }
        }

        public static string DepthLayersToString(object inputObject)
        {
            var model = inputObject as WaterFlowFMModel;
            if (model == null) return string.Empty;

            return model.InitialSalinity?.VerticalProfile.Type.ToString() ?? string.Empty;
        }
    }
}
