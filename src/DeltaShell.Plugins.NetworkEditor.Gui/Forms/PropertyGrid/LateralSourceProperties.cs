using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.Validation;
using DelftTools.Utils.Validation.NameValidation;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.CommonTools.Gui.Property;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "LateralSourceProperties_DisplayName")]
    public class LateralSourceProperties : ObjectProperties<LateralSource>
    {
        private NameValidator nameValidator = NameValidator.CreateDefault();
        
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Long name")]
        [PropertyOrder(1)]
        public string LongName
        {
            get { return data.LongName; }
            set { data.LongName = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Name")]
        [PropertyOrder(0)]
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
        [Description("Channel in which the source is located.")]
        [PropertyOrder(1)]
        public string Channel
        {
            get { return data.Branch.ToString(); }
        }

        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [DisplayName("Chainage (map)")]
        [Description("Chainage of the source in the channel.")]
        [PropertyOrder(2)]
        public double Chainage
        {
            get { return NetworkHelper.MapChainage(data); }
        }

        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [DisplayName("Chainage")]
        [Description("Chainage of the source in the channel as used in the simulation.")]
        [PropertyOrder(3)]
        public double CompuChainage
        {
            get { return data.Chainage; }
            set { HydroRegionEditorHelper.MoveBranchFeatureTo(data, value); }
        }

        [Category(PropertyWindowCategoryHelper.LateralDiffusionCategory)]
        [Description("Length of the reach segment of diffuse lateral into with it is discharging")]
        [DisplayName("Length")]
        [PropertyOrder(7)]
        public double LengthLateralSource
        {
            get { return data.Length; }
            set { HydroRegionEditorHelper.UpdateBranchFeatureGeometry(data, value); }
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