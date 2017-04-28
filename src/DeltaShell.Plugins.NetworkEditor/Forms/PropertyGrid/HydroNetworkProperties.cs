using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Utils;

namespace DeltaShell.Plugins.NetworkEditor.Forms.PropertyGrid
{
    [TypeConverter(typeof(PropertySorter))] //todo: replace by propertydescriptionprovider or something
    public class HydroNetworkProperties
    {
        private readonly IHydroNetwork network;

        public HydroNetworkProperties(IHydroNetwork network)
        {
            this.network = network;
        }

        [Description("Name of network.")]
        [Category("General")]
        [PropertyOrder(1)]
        public string Name
        {
            get { return network.Name; }
            set { network.Name = value; }
        }


        [Description("Number of branches")]
        [Category("General")]
        [PropertyOrder(2)]
        public int BranchCount
        {
            get { return network.Branches.Count; }
        }


        [Description("Number of nodes")]
        [Category("General")]
        [PropertyOrder(3)]
        public int NodeCount
        {
            get { return network.Nodes.Count; }
        }
    }
}