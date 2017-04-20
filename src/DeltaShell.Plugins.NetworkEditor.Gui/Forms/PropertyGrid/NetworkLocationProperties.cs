using System.ComponentModel;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "NetworkLocationProperties_DisplayName")]
    public class NetworkLocationProperties : ObjectProperties<INetworkLocation>
    {
        public string Branch
        {
            get { return null != data.Branch ? data.Branch.Name : ""; }
        }

        [Description("Chainage of the network location in the channel on the map.")]
        [DisplayName("Chainage (Geometry)")]
        public double ChainageUsingGeometry
        {
            get { return NetworkHelper.MapChainage(data); }
        }

        [Description("Chainage of the network location in the channel as used in the simulation.")]
        [DisplayName("Chainage")]
        public double Chainage
        {
            get { return data.Chainage; }
        }

        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
        }

        public string LongName
        {
            get { return data.LongName; }
            set { data.LongName = value; }
        }
    }
}
