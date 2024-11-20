using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Commands
{
    public class AddInterpolatedCrossSectionCommand : NetworkEditorCommand
    {
        protected IMapTool CurrentTool
        {
            get
            {
                return MapView.MapControl.GetToolByName(HydroRegionEditorMapTool.AddInterpolatedCrossSectionToolName);
            }
        }

        protected override void OnExecute(params object[] arguments)
        {
            MapView.MapControl.ActivateTool(CurrentTool);
        }

        public override bool Checked
        {
            get
            {
                if ((null != MapView) && (null != CurrentTool) && Enabled)
                    return CurrentTool.IsActive;
                return false;
            }
        }

        public override bool Enabled
        {
            get
            {
                return base.Enabled && 
                       CurrentTool is NewPointFeatureTool tool && 
                       tool.GetFeaturePerProvider != null;
            }
        }
    }
}