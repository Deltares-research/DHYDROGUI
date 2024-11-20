using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using GeoAPI.Extensions.CoordinateSystems;
using SharpMap.UI.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "HydroRegionProperties_DisplayName")]
    public class HydroRegionProperties : ObjectProperties<HydroRegion>
    {
        [Description("Name of region")]
        [Category("General")]
        [PropertyOrder(1)]
        public string Name
        {
            get
            {
                return data.Name;
            }
            set
            {
                data.Name = value;
            }
        }

        [DisplayName("Total hydro objects")]
        [Category("General")]
        [PropertyOrder(1)]
        public int TotalHydroObjects
        {
            get
            {
                return data.AllHydroObjects.Count();
            }
        }

        [DisplayName("Total regions")]
        [Category("General")]
        [PropertyOrder(1)]
        public int TotalRegions
        {
            get
            {
                return data.AllRegions.Count();
            }
        }

        [TypeConverter(typeof(CoordinateSystemStringTypeConverter))]
        [Editor(typeof(CoordinateSystemTypeEditor), typeof(UITypeEditor))]
        public ICoordinateSystem CoordinateSystem
        {
            get
            {
                return data.CoordinateSystem;
            }
            set
            {
                data.CoordinateSystem = value;
            }
        }
    }
}