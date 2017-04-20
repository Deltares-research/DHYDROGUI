using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Commands
{
    class AddNewExtraResistanceCommand : NetworkEditorCommand
    {
        protected IMapTool CurrentTool
        {
            get
            {
                return MapView.MapControl.GetToolByName(HydroRegionEditorMapTool.AddExtraResistanceToolName);
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
                return false;
            }
        }
    }
}
