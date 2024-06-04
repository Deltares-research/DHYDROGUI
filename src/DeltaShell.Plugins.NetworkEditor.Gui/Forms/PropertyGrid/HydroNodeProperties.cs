using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.Plugins.CommonTools.Gui.Property;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "HydroNodeProperties_DisplayName")]
    public class HydroNodeProperties : ObjectProperties<HydroNode>
    {
        private NameValidator nameValidator = NameValidator.CreateDefault();
        
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Name")]
        [PropertyOrder(1)]
        public string Name
        {
            get { return data.Name; }
            set
            {
                if (nameValidator.ValidateWithLogging(value))
                {
                    data.Name = value;
                }
            }
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
        
        /// <summary>
        /// Get or set the <see cref="NameValidator"/> for this instance.
        /// Property is initialized with a default name validator. 
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="value"/> is <c>null</c>.
        /// </exception>
        public NameValidator NameValidator
        {
            get => nameValidator;
            set
            {
                Ensure.NotNull(value, nameof(value));
                nameValidator = value;
            }
        }
    }
}
