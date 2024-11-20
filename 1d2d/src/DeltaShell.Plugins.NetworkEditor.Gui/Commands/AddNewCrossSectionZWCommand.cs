using System.Windows.Forms;
using DelftTools.Hydro.CrossSections;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using SharpMap.Api.Layers;
using SharpMap.UI.Helpers;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Commands
{
    public class AddNewCrossSectionZWCommand : AddNewCrossSectionCommandBase
    {
        protected override Cursor Cursor
        {
            get { return MapCursors.CreateArrowOverlayCuror(Resources.CrossSectionTabulatedSmall); }
        }

        protected override ICrossSection CreateDefault(ILayer layer)
        {
            return CrossSection.CreateDefault(CrossSectionType.ZW, null);
        }

        protected override IMapTool CurrentTool
        {
            get { return MapView.MapControl.GetToolByName(HydroRegionEditorMapTool.AddPointCrossSectionToolName); }
        }
    }
}