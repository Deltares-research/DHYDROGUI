using System.ComponentModel;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using GeoAPI.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using SharpMap.UI.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "FeatureCoverageProperties_DisplayName")]
    public class FeatureCoverageProperties : ObjectProperties<IFeatureCoverage>
    {
        [Category("General")]
        [Description("Name of the feature coverage")]
        [DynamicReadOnly]
        public string Name
        {
            get => data.Name;
            set => data.Name = value;
        }

        [Category("General")]
        [Description("Is the coverage time dependent")]
        [DisplayName("Time Dependent")]
        public bool TimeDepedent => data.IsTimeDependent;

        [Category("General")]
        [Description("Default value when no data is available")]
        public double DefaultValue
        {
            get => (double)data.Components[0].DefaultValue;
            set => data.Components[0].DefaultValue = value;
        }

        [Category("General")]
        [DisplayName("Unit")]
        public string Unit => data.Components[0].Unit == null ? "-" : data.Components[0].Unit.Symbol;

        [Category("General")]
        [DisplayName("Number of features")]
        public int FeatureCount => data.FeatureVariable?.Values.Count ?? 0;

        [TypeConverter(typeof(CoordinateSystemStringTypeConverter))]
        public ICoordinateSystem CoordinateSystem => data.CoordinateSystem;

        [DynamicReadOnlyValidationMethod]
        public bool ValidateDynamicAttributes(string propertyName)
        {
            if (propertyName.Equals("Name"))
            {
                return !data.IsEditable;
            }

            return false;
        }
    }
}