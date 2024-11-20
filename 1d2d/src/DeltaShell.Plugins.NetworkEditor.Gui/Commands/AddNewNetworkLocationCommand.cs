using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using SharpMap.UI.Forms;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Commands
{
    public class AddNewNetworkLocationCommand : NetworkEditorCommand
    {
        protected override void OnExecute(params object[] arguments)
        {
            if (AddNetworkLocationMapTool.IsActive)
            {
                AddNetworkLocationMapTool.IsActive = false;
                AddNetworkLocationMapTool.MapControl.SelectTool.IsActive = true;
            }
            else
            {
                MapView.MapControl.ActivateTool(AddNetworkLocationMapTool);
            }
        }

        protected static IMapTool AddNetworkLocationMapTool
        {
            get
            {
                return MapView.MapControl.GetToolByName(HydroRegionEditorMapTool.AddNetworkLocationToolName);
            }
        }

        public override bool Enabled
        {
            get
            {
                if (MapView == null || AddNetworkLocationMapTool == null)
                {
                    return false;
                }

                //should have an active networkcoverage layer with a coverage that is editable
                MapControl mapControl = MapView.MapControl;
                var hydroNetworkEditorMapTool = mapControl.GetToolByType<HydroRegionEditorMapTool>();
                return (null != hydroNetworkEditorMapTool) &&
                       (hydroNetworkEditorMapTool.ActiveNetworkCoverageGroupLayer != null) &&
                       (hydroNetworkEditorMapTool.ActiveNetworkCoverageGroupLayer.Coverage.IsEditable);
            }
        }

        public override bool Checked
        {
            get { return (null != MapView) && (null != AddNetworkLocationMapTool) && AddNetworkLocationMapTool.IsActive; }
        }

    }
}