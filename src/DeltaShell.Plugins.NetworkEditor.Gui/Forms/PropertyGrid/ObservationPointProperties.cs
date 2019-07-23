using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.CommonTools.Gui.Property;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "ObservationPointProperties_DisplayName")]
    public class ObservationPointProperties : ObjectProperties<ObservationPoint>
    {
        [Category("General")]
        [PropertyOrder(1)]
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
        }

        [Category("General")]
        public string LongName
        {
            get { return data.LongName; }
            set { data.LongName = value; }
        }

        [Category("General")]
        [Description("All the (custom) attributes for this object.")]
        [TypeConverter(typeof(AttributeArrayConverter<object>))]
        public AttributeProperties<object>[] Attributes
        {
            get { return data.Attributes.Select(x => new AttributeProperties<object>(data.Attributes, x.Key)).ToArray(); }
        }

        [Category("Administration")]
        [Description("Channel in which the point is located.")]
        [PropertyOrder(2)]
        public string Channel
        {
            get { return data.Branch.ToString(); }
        }

        [Description("Chainage of the point in the channel on the map.")]
        [PropertyOrder(3)]
        [Category("Administration")]
        [DisplayName("Chainage (Map)")]
        public double Chainage
        {
            get { return NetworkHelper.MapChainage(data); }
        }

        [Description("Chainage of the point in the channel as used in the simulation.")]
        [PropertyOrder(4)]
        [Category("Administration")]
        [DisplayName("Chainage")]
        public double CompuChainage
        {
            get { return data.Chainage; }
            set { HydroRegionEditorHelper.MoveBranchFeatureTo(data, value); }
        }
    }
}
