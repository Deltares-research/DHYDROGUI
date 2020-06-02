using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Commands
{
    internal class AddNewExtraResistanceCommand : NetworkEditorCommand
    {
        public override bool Checked
        {
            get
            {
                if (null != MapView && null != CurrentTool)
                {
                    return CurrentTool.IsActive;
                }

                return false;
            }
        }

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
    }
}