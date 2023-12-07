using System;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation.Common;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.Plugins.CommonTools.Gui.Property;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    /// <summary>
    /// Enables nice display of properties of a cross section in a propertygrid.
    /// </summary>
    [ResourcesDisplayName(typeof(Resources), "CrossSectionProperties_DisplayName")]
    public class CrossSectionProperties : ObjectProperties<ICrossSection>
    {
        private NameValidator nameValidator = NameValidator.CreateDefault();
        
        #region Cross Section Properties

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Name")]
        [Description("Id of the cross section.")]
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
        [Description("Name of the cross section.")]
        [PropertyOrder(2)]
        public string LongName
        {
            get { return data.LongName; }
            set { data.LongName = value; }
        }


        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [DisplayName("Branch")]
        [Description("Branch to which this cross section belongs.")]
        [PropertyOrder(1)]
        public string Branch
        {
            get { return data.Branch == null ? "<none>" : data.Branch.Name; }
        }
        
        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [Description("Chainage of the bridge in the channel as used in the simulation.")]
        [DisplayName("Chainage")]
        [PropertyOrder(3)]
        [DynamicReadOnly]
        public double CompuChainage
        {
            get { return data.Chainage; }
            set { HydroRegionEditorHelper.MoveBranchFeatureTo(data, value); }
        }

        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [DisplayName("Chainage (map)")]
        [Description("Chainage of the cross section on the branch.")]
        [PropertyOrder(2)]
        public double GeometryChainage
        {
            get { return data.Branch.IsLengthCustom ? BranchFeature.SnapChainage(data.Branch.Geometry.Length, (data.Branch.Geometry.Length / data.Branch.Length) * data.Chainage) : data.Chainage; }
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
        
        #endregion

        #region Definition Properties

        [Category(PropertyWindowCategoryHelper.DefinitionCategory)]
        [DisplayName("Cross section type")]
        [Description("Type of the cross section definition.")]
        [PropertyOrder(2)]
        public CrossSectionType CrossSectionType
        {
            get { return data.CrossSectionType; }
        }
        
        [Category(PropertyWindowCategoryHelper.DefinitionCategory)]
        [DisplayName("Thalweg")]
        [Description("Thalweg; offset in cross section where thalweg intersects channel.")]
        [PropertyOrder(3)]
        [DynamicReadOnly]
        public double Thalweg
        {
            get { return Math.Round(data.Definition.Thalweg, 2); }
            set
            {
                data.Definition.Thalweg = CrossSectionHelper.ValidateThalWay(data.Definition, value);
                data.Geometry = data.Definition.GetGeometry(data);
            }
        }

        [Category(PropertyWindowCategoryHelper.MetricsCategory)]
        [DisplayName("Lowest point")]
        [Description("Lowest level of the cross section (m)")]
        [PropertyOrder(1)]
        public double LowestPoint
        {
            get { return data.Definition.LowestPoint; }
        }

        [Category(PropertyWindowCategoryHelper.MetricsCategory)]
        [DisplayName("Highest point")]
        [Description("Highest level of the cross section (m)")]
        [PropertyOrder(2)]
        public double HighestPoint
        {
            get { return data.Definition.HighestPoint; }
        }

        [Category(PropertyWindowCategoryHelper.MetricsCategory)]
        [DisplayName("Width")]
        [Description("Width of the cross section (m)")]
        [PropertyOrder(3)]
        public double Width
        {
            get { return data.Definition.Width; }
        }

        #endregion

        [DynamicReadOnlyValidationMethod]
        public bool DynamicReadOnlyValidationMethod(string propertyName)
        {
            var isGeometryBased = data.CrossSectionType == CrossSectionType.GeometryBased;

            //for geobased CS do not allow editing of compu chainage
            if (propertyName == "CompuChainage")
            {
                return isGeometryBased;
            }

            // Marks properties marked with DynamicReadOnlyAttribute as ReadOnly for Proxy definitions
            if (data.Definition.IsProxy)
            {
                return true;
            }
            
            if (propertyName == "Thalweg")
            {
                return isGeometryBased;
            }

            return true;
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