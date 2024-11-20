using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using log4net;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Commands
{
    public class AddNewBranchCommand : NetworkEditorCommand
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AddNewBranchCommand));

        protected IMapTool CurrentTool
        {
            get { return MapView.MapControl.GetToolByName(HydroRegionEditorMapTool.AddChannelToolName); }
        }

        protected override void OnExecute(params object[] arguments)
        {
            var newLineTool = (NewLineTool) CurrentTool;
            
            MapView.MapControl.ActivateTool(newLineTool);

            Log.InfoFormat("New channel command: <{0}> remove last point, <{1}> toggle snapping", newLineTool.RemoveLastPointKey, newLineTool.SnapKey);
        }

        public override bool Checked
        {
            get { return (null != MapView) && (null != CurrentTool) && CurrentTool.IsActive; }
        }
    }
}