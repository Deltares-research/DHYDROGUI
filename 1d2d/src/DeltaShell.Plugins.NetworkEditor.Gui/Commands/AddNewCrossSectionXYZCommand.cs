using System.Windows.Forms;
using DelftTools.Hydro.CrossSections;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using SharpMap.Api.Layers;
using SharpMap.UI.Helpers;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Commands
{
    public class AddNewCrossSectionXYZCommand : AddNewCrossSectionCommandBase
    {
        protected override Cursor Cursor
        {
            get { return MapCursors.CreateArrowOverlayCuror(Resources.CrossSectionSmallXYZ); }
        }

        protected override ICrossSection CreateDefault(ILayer layer)
        {
            return CrossSection.CreateDefault(CrossSectionType.GeometryBased, null);
        }

        protected override IMapTool CurrentTool
        {
            get
            {
                return MapView.MapControl.GetToolByName(HydroRegionEditorMapTool.AddLineCrossSectionToolName);
            }
        }
    }
}