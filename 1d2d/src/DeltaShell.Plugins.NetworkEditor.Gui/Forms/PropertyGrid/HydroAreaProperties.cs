using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using GeoAPI.CoordinateSystems;
using SharpMap.UI.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "AreaProperties_DisplayName")]
    public class HydroAreaProperties : ObjectProperties<HydroArea>
    {
        [Description("Name of Area.")]
        [Category("General")]
        [PropertyOrder(1)]
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
        }

        [TypeConverter(typeof(CoordinateSystemStringTypeConverter))]
        [Description("Coordinate System (can be set in model)")]
        [DisplayName("Coordinate System")]
        public ICoordinateSystem CoordinateSystem
        {
            get { return data.CoordinateSystem; }
        }
    }
}