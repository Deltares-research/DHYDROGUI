using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.CommonTools.Gui.Property;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "HydroNodeProperties_DisplayName")]
    public class HydroNodeProperties : ObjectProperties<HydroNode>
    {
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

        [Category(PropertyWindowCategoryHelper.RelationsCategory)]
        [DisplayName("Incoming branches")]
        [Description("Number of branches that end in this node.")]
        [PropertyOrder(5)]
        public int IncomingBranches
        {
            get { return data.IncomingBranches.Count; }
        }

        [Category(PropertyWindowCategoryHelper.RelationsCategory)]
        [DisplayName("Outgoing branches")]
        [Description("Number of branches that start in this node.")]
        [PropertyOrder(6)]
        public int OutgoingBranches
        {
            get { return data.OutgoingBranches.Count; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("X coordinate")]
        [PropertyOrder(10)]
        public double X
        {
            get { return data.Geometry.Centroid.X; }
            set
            {
                //unwanted relation..also causes a crash when setting with no mapcontrol open.
                HydroRegionEditorHelper.MoveNodeTo(data, value, Y);
            }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Y coordinate")]
        [PropertyOrder(11)]
        public double Y
        {
            get { return data.Geometry.Centroid.Y; }
            set { HydroRegionEditorHelper.MoveNodeTo(data, X, value); }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Attributes")]
        [Description("All the (custom) attributes for this object.")]
        [PropertyOrder(999)]
        [TypeConverter(typeof(AttributeArrayConverter<object>))]
        public AttributeProperties<object>[] Attributes
        {
            get { return data.Attributes.Select(x => new AttributeProperties<object>(data.Attributes, x.Key)).ToArray(); }
        }

        [Category(PropertyWindowCategoryHelper.RelationsCategory)]
        [DisplayName("Is on single branch")]
        [PropertyOrder(30)]
        public bool IsOnSingleBranch
        {
            get { return data.IsOnSingleBranch; }
        }
    }
}
