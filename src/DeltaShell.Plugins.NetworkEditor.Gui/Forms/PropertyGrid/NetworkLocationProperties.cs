using System.ComponentModel;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "NetworkLocationProperties_DisplayName")]
    public class NetworkLocationProperties : ObjectProperties<INetworkLocation>
    {
        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [DisplayName("Branch")]
        [PropertyOrder(1)]
        public string Branch
        {
            get { return null != data.Branch ? data.Branch.Name : ""; }
        }

        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [Description("Chainage of the network location in the channel on the map.")]
        [DisplayName("Chainage (map)")]
        [PropertyOrder(2)]
        public double ChainageUsingGeometry
        {
            get { return NetworkHelper.MapChainage(data); }
        }

        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [Description("Chainage of the network location in the channel as used in the simulation.")]
        [DisplayName("Chainage")]
        [PropertyOrder(3)]
        public double Chainage
        {
            get { return data.Chainage; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Name")]
        [PropertyOrder(1)]
        public string Name
        {
            get { return data.Name; }
            set { data.SetNameIfValid(value); }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Long name")]
        [PropertyOrder(2)]
        public string LongName
        {
            get { return data.LongName; }
            set { data.LongName = value; }
        }

    }
}
