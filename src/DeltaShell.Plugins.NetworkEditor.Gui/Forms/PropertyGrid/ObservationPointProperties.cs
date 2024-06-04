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
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "ObservationPointProperties_DisplayName")]
    public class ObservationPointProperties : ObjectProperties<ObservationPoint>
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
        public string LongName
        {
            get { return data.LongName; }
            set { data.LongName = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Attributes")]
        [Description("All the (custom) attributes for this object.")]
        [TypeConverter(typeof(AttributeArrayConverter<object>))]
        public AttributeProperties<object>[] Attributes
        {
            get { return data.Attributes.Select(x => new AttributeProperties<object>(data.Attributes, x.Key)).ToArray(); }
        }

        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [DisplayName("Branch")]
        [Description("Channel in which the point is located.")]
        [PropertyOrder(2)]
        public string Channel
        {
            get { return data.Branch.ToString(); }
        }

        [Description("Chainage of the point in the channel on the map.")]
        [PropertyOrder(3)]
        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [DisplayName("Chainage (map)")]
        public double Chainage
        {
            get { return NetworkHelper.MapChainage(data); }
        }

        [Description("Chainage of the point in the channel as used in the simulation.")]
        [PropertyOrder(4)]
        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [DisplayName("Chainage")]
        public double CompuChainage
        {
            get { return data.Chainage; }
            set { HydroRegionEditorHelper.MoveBranchFeatureTo(data, value); }
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
