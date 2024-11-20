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
    [ResourcesDisplayName(typeof(Resources), "NetworkProperties_DisplayName")]
    public class HydroNetworkProperties : ObjectProperties<IHydroNetwork>
    {
        [Description("Name of network.")]
        [Category("General")]
        [PropertyOrder(1)]
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
        }

        [Description("Number of branches")]
        [Category("General")]
        [PropertyOrder(2)]
        public int BranchCount
        {
            get { return data.Branches.Count; }
        }

        [Description("Number of nodes")]
        [Category("General")]
        [PropertyOrder(3)]
        public int NodeCount
        {
            get { return data.Nodes.Count; }
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