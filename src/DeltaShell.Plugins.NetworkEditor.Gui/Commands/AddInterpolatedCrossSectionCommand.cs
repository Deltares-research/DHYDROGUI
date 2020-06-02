using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using log4net;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Commands
{
    public class AddInterpolatedCrossSectionCommand : NetworkEditorCommand
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AddInterpolatedCrossSectionCommand));

        public override bool Checked
        {
            get
            {
                if (null != MapView && null != CurrentTool && Enabled)
                {
                    return CurrentTool.IsActive;
                }

                return false;
            }
        }

        public override bool Enabled
        {
            get
            {
                if (base.Enabled && CurrentTool is NewPointFeatureTool)
                {
                    if (((NewPointFeatureTool) CurrentTool).GetFeaturePerProvider != null)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

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
    }
}