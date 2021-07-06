using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.CommonTools.Gui.Property;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "ExtraResistanceProperties_DisplayName")]
    public class ExtraResistanceProperties : ObjectProperties<IExtraResistance>
    {
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Name")]
        [PropertyOrder(1)]
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Long name")]
        [PropertyOrder(2)]
        public string LongName
        {
            get { return data.LongName; }
            set { data.LongName = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Type")]
        [PropertyOrder(3)]
        public ExtraResistanceType ExtraResistanceType
        {
            get { return data.ExtraResistanceType; }
            set { data.ExtraResistanceType = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Attributes")]
        [Description("All the (custom) attributes for this object.")]
        [PropertyOrder(99)]
        [TypeConverter(typeof(AttributeArrayConverter<object>))]
        public AttributeProperties<object>[] Attributes
        {
            get { return data.Attributes.Select(x => new AttributeProperties<object>(data.Attributes, x.Key)).ToArray(); }
        }

        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [DisplayName("Branch")]
        [Description("Channel in which the Extra Resistance is located.")]
        [PropertyOrder(1)]
        public string Channel
        {
            get { return data.Branch.ToString(); }
        }

        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [DisplayName("Compound structure")]
        [Description("Compound structure in which the Extra Resistance is located.")]
        [PropertyOrder(2)]
        public string CompositeStructure
        {
            get { return data.ParentStructure.ToString(); }
        }

        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [DisplayName("Chainage (map)")]
        [Description("Chainage of the extra resistance in the channel on the map.")]
        [PropertyOrder(3)]
        public double Chainage
        {
            get { return NetworkHelper.MapChainage(data.ParentStructure); }
        }

        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [DisplayName("Chainage")]
        [Description("Chainage of the extra resistance in the channel as used in the simulation.")]
        [PropertyOrder(4)]
        public double CompuChainage
        {
            get { return data.ParentStructure.Chainage; }
            set { HydroRegionEditorHelper.MoveBranchFeatureTo(data, value); }
        }

        
    }
}
