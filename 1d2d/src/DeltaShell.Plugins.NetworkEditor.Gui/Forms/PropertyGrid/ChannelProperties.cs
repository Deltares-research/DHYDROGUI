using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DelftTools.Utils.Validation;
using DelftTools.Utils.Validation.NameValidation;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.CommonTools.Gui.Property;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "ChannelProperties_DisplayName")]
    public class ChannelProperties : ObjectProperties<IChannel>
    {
        private NameValidator nameValidator = NameValidator.CreateDefault();
        
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Attributes")]
        [Description("All the (custom) attributes for this object.")]
        [PropertyOrder(999)]
        [TypeConverter(typeof(AttributeArrayConverter<object>))]
        public AttributeProperties<object>[] Attributes
        {
            get { return data.Attributes.Select(x => new AttributeProperties<object>(data.Attributes, x.Key)).ToArray(); }
        }

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
        [DisplayName("From node")]
        [PropertyOrder(3)]
        public string FromNode
        {
            get { return data.Source.ToString(); }
        }

        [Category(PropertyWindowCategoryHelper.RelationsCategory)]
        [PropertyOrder(4)]
        [DisplayName("To node")]
        public string ToNode
        {
            get { return data.Target.ToString(); }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Length")]
        [Description("Length used for simulation when IsLengthCustom is true")]
        [PropertyOrder(6)]
        [DynamicReadOnly]
        public string Length
        {
            get { return string.Format("{0:0.##}", data.Length); }
            set
            {
                double result;
                if (double.TryParse(value, out result))
                {
                    data.Length = result;
                }
            }
        }

        [DynamicReadOnlyValidationMethod]
        public bool DynamicReadOnlyValidationMethod(string propertyName)
        {
            if (propertyName == "Length")
            {
                return !IsLengthCustom;
            }

            return true;
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Geometry length")]
        [Description("Length of the channel on the map.")]
        [PropertyOrder(7)]
        public string GeometryLength
        {
            get { return string.Format("{0:0.##}", data.Geometry.Length); }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Geodetic length")]
        [Description("Length of an ellopsoid channel on the map.")]
        [PropertyOrder(12)]
        [DynamicVisible]
        public string GeodeticLength
        {
            get { return string.Format("{0:0.##}", data.GeodeticLength); }
        }
        [DynamicVisibleValidationMethod]
        public bool IsVisible(string propertyName)
        {
            if (propertyName == nameof(data.GeodeticLength) )
            {
                return !double.IsNaN(data.GeodeticLength); 
            }
            return true;
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Has custom length")]
        [Description("Length of the channel on the map is ignored for simulation.")]
        [PropertyOrder(5)]
        public bool IsLengthCustom
        {
            get { return data.IsLengthCustom; }
            set { data.IsLengthCustom = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Order number")]
        [Description("Order number will be used for interpolation over branches. A chain of branches with the same order number will be treated as one.")]
        [PropertyOrder(20)]
        public int OrderNumber
        {
            get { return data.OrderNumber; }
            set { data.OrderNumber = value; }
        }

        [Category(PropertyWindowCategoryHelper.BranchFeaturesCategory)]
        [PropertyOrder(1)]
        [DisplayName("Number of cross-sections")]
        public int CrossSections
        {
            get { return data.CrossSections.Count(); }
        }

        [Category(PropertyWindowCategoryHelper.BranchFeaturesCategory)]
        [DisplayName("Number of structures")]
        [PropertyOrder(2)]
        public int Structures
        {
            get { return data.Structures.Count(s => s.ParentStructure == null); }
        }

        [Category(PropertyWindowCategoryHelper.BranchFeaturesCategory)]
        [DisplayName("Number of pumps")]
        [PropertyOrder(3)]
        public int Pumps
        {
            get { return data.Pumps.Count(); }
        }

        [Category(PropertyWindowCategoryHelper.BranchFeaturesCategory)]
        [DisplayName("Number of culverts")]
        [PropertyOrder(4)]
        public int Culverts
        {
            get { return data.Culverts.Count(); }
        }

        [Category(PropertyWindowCategoryHelper.BranchFeaturesCategory)]
        [DisplayName("Number of bridges")]
        [PropertyOrder(5)]
        public int Bridges
        {
            get { return data.Bridges.Count(); }
        }

        [Category(PropertyWindowCategoryHelper.BranchFeaturesCategory)]
        [DisplayName("Number of weirs")]
        [PropertyOrder(6)]
        public int Weirs
        {
            get { return data.Weirs.Count(); }
        }

        [Category(PropertyWindowCategoryHelper.BranchFeaturesCategory)]
        [DisplayName("Number of gates")]
        [PropertyOrder(7)]
        public int Gates
        {
            get { return data.Gates.Count(); }
        }

        [Category(PropertyWindowCategoryHelper.BranchFeaturesCategory)]
        [DisplayName("Number of lateral sources")]
        [PropertyOrder(8)]
        public int BranchSources
        {
            get { return data.BranchSources.Count(); }
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