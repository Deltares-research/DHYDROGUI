using System.Windows.Forms;
using DelftTools.Hydro.CrossSections;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using SharpMap.Api.Layers;
using SharpMap.UI.Helpers;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Commands
{
    public class AddNewCrossSectionStandardCommand : AddNewCrossSectionCommandBase
    {
        protected override Cursor Cursor
        {
            get { return MapCursors.CreateArrowOverlayCuror(Resources.CrossSectionStandardSmall); }
        }

        protected override IMapTool CurrentTool
        {
            get { return MapView.MapControl.GetToolByName(HydroRegionEditorMapTool.AddPointCrossSectionToolName); }
            
        }

        protected override ICrossSection CreateDefault(ILayer layer)
        {
            return CrossSection.CreateDefault(CrossSectionType.Standard, null);
        }
    }
}