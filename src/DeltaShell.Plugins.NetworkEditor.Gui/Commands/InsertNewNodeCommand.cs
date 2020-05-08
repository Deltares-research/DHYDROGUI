using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Commands
{
    internal class InsertNewNodeCommand : NetworkEditorCommand
    {
        public override bool Checked
        {
            get
            {
                if (null != MapView && null != CurrentTool)
                {
                    return CurrentTool.IsActive;
                }
                else
                {
                    return false;
                }
            }
        }

        protected IMapTool CurrentTool
        {
            get
            {
                return MapView.MapControl.GetToolByName(HydroRegionEditorMapTool.InsertNodeToolName);
            }
        }

        protected override void OnExecute(params object[] arguments)
        {
            MapView.MapControl.ActivateTool(CurrentTool); // TODO: use better tool lookup algoritm here
        }
    }
}