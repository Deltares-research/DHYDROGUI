using System.Collections.Generic;
using System.IO;
using DeltaShell.Plugins.FMSuite.Common.Gui.Editors.Buttons;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    public class FMToolboxesPanel : ToolboxesPanel
    {
        protected override string GetToolBoxesDirectory()
        {
            return Path.Combine(Path.GetDirectoryName(typeof (WaterFlowFMModel.WaterFlowFMModel).Assembly.Location), "toolboxes");
        }

        protected override Dictionary<string, object> GetScriptPredefinedVariables()
        {
            return new Dictionary<string, object>
                {
                    {"Model", Model},
                    {"ModelDirectory", ((WaterFlowFMModel.WaterFlowFMModel) Model).ModelDefinition.ModelDirectory},
                    {"MapControl", FlowFMGuiPlugin.ActiveMapView != null ? FlowFMGuiPlugin.ActiveMapView.MapControl : null},
                };
        }
    }
}