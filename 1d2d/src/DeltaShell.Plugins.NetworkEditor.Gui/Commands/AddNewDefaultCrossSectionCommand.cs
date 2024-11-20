using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using SharpMap.Api.Layers;
using SharpMap.UI.Helpers;
using SharpMap.UI.Tools;
using MessageBox = DelftTools.Controls.Swf.MessageBox;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Commands
{
    public class AddNewDefaultCrossSectionCommand : AddNewCrossSectionCommandBase
    {
        private ICrossSectionDefinition DefaultDefinition 
        {
            get
            {
                var network = HydroRegionEditorMapTool.HydroRegions.OfType<IHydroNetwork>().FirstOrDefault();

                if(network == null)
                {
                    return null;
                }

                return network.DefaultCrossSectionDefinition;
            }
        }

        protected override void OnExecute(params object[] arguments)
        {
            if (DefaultDefinition == null)
            {
                ShowNoDefaultMessage();
            }
            else
            {
                base.OnExecute(arguments);
            }

        }

        protected override Cursor Cursor
        {
            get { return MapCursors.CreateArrowOverlayCuror(Resources.AddDefaultCrossSectionDefinition); }
        }

        private void ShowNoDefaultMessage()
        {
            MessageBox.Show("No default was selected, please select a default cross section definition in the network content view.",
                            "No default selected.");
        }

        protected override IMapTool CurrentTool
        {
            get
            {
                return MapView.MapControl.GetToolByName(HydroRegionEditorMapTool.AddPointCrossSectionToolName);
            }
        }

        private static HydroRegionEditorMapTool HydroRegionEditorMapTool
        {
            get
            {
                if (null != MapView)
                {
                    var mapControl = MapView.MapControl;
                    return mapControl.GetToolByType<HydroRegionEditorMapTool>();
                }
                return null;
            }
        }

        protected override ICrossSection CreateDefault(ILayer layer)
        {
            if (DefaultDefinition != null)
            {
                var cs = new CrossSection(new CrossSectionDefinitionProxy(DefaultDefinition)); 

                if (Control.ModifierKeys == Keys.Alt)
                {
                    cs.MakeDefinitionLocal();
                }

                return cs;
            }

            ShowNoDefaultMessage();

            return CrossSection.CreateDefault();
        }
    }
}