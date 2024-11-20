using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Commands
{
    class AddNewRetentionCommand : NetworkEditorCommand
    {
        protected IMapTool CurrentTool
        {
            get
            {
                return MapView.MapControl.GetToolByName(HydroRegionEditorMapTool.AddRetentionToolName);
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
                if ((null != MapView) && (null != CurrentTool))
                    return CurrentTool.IsActive;
                else
                    return false;
            }
        }

        
    }
}