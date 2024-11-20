using System.ComponentModel;
using System.Drawing.Design;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using GeoAPI.Extensions.CoordinateSystems;
using SharpMap.UI.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "DrainageBasinProperties_DisplayName")]
    public class DrainageBasinProperties : ObjectProperties<IDrainageBasin>
    {
        [Description("Name of basin")]
        [Category("General")]
        [PropertyOrder(1)]
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
        }

        [DisplayName("Catchment count")]
        [Description("Number of catchments")]
        [Category("General")]
        [PropertyOrder(2)]
        public int CatchmentCount
        {
            get { return data.Catchments.Count; }
        }

        [DisplayName("WWTP count")]
        [Description("Number of waste water threatment plants")]
        [Category("General")]
        [PropertyOrder(3)]
        public int WWWTPCount
        {
            get { return data.WasteWaterTreatmentPlants.Count; }
        }

        [DisplayName("Runoff boundary count")]
        [Description("Number of runoff boundaries")]
        [Category("General")]
        [PropertyOrder(4)]
        public int RunoffBoundaryCount
        {
            get { return data.Boundaries.Count; }
        }

        [TypeConverter(typeof(CoordinateSystemStringTypeConverter))]
        [Editor(typeof(CoordinateSystemTypeEditor), typeof(UITypeEditor))]
        public ICoordinateSystem CoordinateSystem
        {
            get { return data.CoordinateSystem; }
            set { data.CoordinateSystem = value; }
        }
    }
}