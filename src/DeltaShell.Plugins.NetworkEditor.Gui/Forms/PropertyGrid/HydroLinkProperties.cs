using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "HydroLinkProperties_DisplayName")]
    public class HydroLinkProperties : ObjectProperties<HydroLink>
    {
        public double Length
        {
            get { if (data.Geometry != null) return data.Geometry.Length; else return -1.0; }
        }

        public string Source
        {
            get { return data.Source.Name; }
        }

        public string Target
        {
            get { return data.Target.Name; }
        }

        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
        }
    }
}
