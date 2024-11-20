using System.ComponentModel;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "NetworkSegmentProperties_DisplayName")]
    public class NetworkSegmentProperties : ObjectProperties<INetworkSegment>
    {
        [ReadOnly((true))]
        [Description("Name of the branch")]
        public string Branch
        {
            get { return data.Branch.Name; }
        }

        [ReadOnly((true))]
        [Description("Name of the network")]
        public string Network
        {
            get { return data.Branch.Network.Name; }
        }

        [ReadOnly((true))]
        [Description("Positive direction")]
        public bool DirectionIsPositive
        {
            get { return data.DirectionIsPositive; }
        }

        [Category("General")]
        [Description("Start chainage of the segment")]
        public string StartChainage
        {
            get { return string.Format("{0:g6}", data.Chainage); }
        }

        [Category("General")]
        [Description("End chainage of the segment")]
        public string EndChainage
        {
            get { return string.Format("{0:g6}", data.EndChainage); }
        }

        [Category("General")]
        [Description("Length of the segment")]
        public string Length
        {
            get { return string.Format("{0:g6}", data.Length); }
        }

        // note: value not accessible via NetworkSegment but only via coverage
    }
}
