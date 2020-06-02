using System.Collections.Generic;
using System.IO;
using DeltaShell.Plugins.FMSuite.Common.Gui.Editors.Buttons;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    public class FMToolboxesPanel : ToolboxesPanel
    {
        protected override string GetToolBoxesDirectory()
        {
            return Path.Combine(Path.GetDirectoryName(typeof(WaterFlowFMModel).Assembly.Location), "toolboxes");
        }

        protected override Dictionary<string, object> GetScriptPredefinedVariables()
        {
            return new Dictionary<string, object>
            {
                {"Model", Model},
                {"ModelDirectory", ((WaterFlowFMModel) Model).ModelDefinition.ModelDirectory},
                {"MapControl", FlowFMGuiPlugin.ActiveMapView != null ? FlowFMGuiPlugin.ActiveMapView.MapControl : null}
            };
        }
    }
}