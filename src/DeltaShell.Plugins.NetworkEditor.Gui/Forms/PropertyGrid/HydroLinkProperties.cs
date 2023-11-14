using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "HydroLinkProperties_DisplayName")]
    public class HydroLinkProperties : ObjectProperties<HydroLink>
    {
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Length")]
        [PropertyOrder(2)]
        public double Length
        {
            get { if (data.Geometry != null) return data.Geometry.Length; else return -1.0; }
        }

        [Category(PropertyWindowCategoryHelper.RelationsCategory)]
        [DisplayName("Source")]
        [PropertyOrder(1)]
        public string Source
        {
            get { return data.Source.Name; }
        }

        [Category(PropertyWindowCategoryHelper.RelationsCategory)]
        [DisplayName("Target")]
        [PropertyOrder(2)]
        public string Target
        {
            get { return data.Target.Name; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Name")]
        [PropertyOrder(1)]
        public string Name
        {
            get { return data.Name; }
            set { data.SetNameIfValid(value); }
        }
    }
}
