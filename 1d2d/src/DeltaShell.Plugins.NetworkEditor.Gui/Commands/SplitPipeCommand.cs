using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Commands
{
    public class SplitPipeCommand : NetworkEditorCommand
    {
        protected IMapTool CurrentTool
        {
            get { return MapView.MapControl.GetToolByName(HydroRegionEditorMapTool.InsertManholeToolName); }
        }

        protected override void OnExecute(params object[] arguments)
        {
            var newLineTool = (NewPointFeatureTool)CurrentTool;
            MapView.MapControl.ActivateTool(newLineTool);
        }

        public override bool Checked
        {
            get { return (null != MapView) && (null != CurrentTool) && CurrentTool.IsActive; }
        }
    }
}
