using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "RoughnessSectionProperties_RoughnessSection_DisplayName")]
    public class RoughnessSectionPropertiesBase<T> : ObjectProperties<T> where T : RoughnessSection
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RoughnessSectionPropertiesBase<T>));

        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Common_Name_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RoughnessSectionProperties_Name_Description")]
        public string Name
        {
            get { return data.Name; }
        }

        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "RoughnessSectionProperties_DefaultRoughness_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RoughnessSectionProperties_DefaultRoughness_Description")]
        public double DefaultRoughness
        {
            get { return data.GetDefaultRoughnessValue(); }
            set { data.RoughnessNetworkCoverage.DefaultValue = value; }
        }

        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "RoughnessSectionProperties_DefaultRoughnessType_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RoughnessSectionProperties_DefaultRoughnessType_Description")]
        public RoughnessType DefaultRoughnessType
        {
            get { return data.GetDefaultRoughnessType(); }
            set
            {
                if (data.RoughnessNetworkCoverage.DefaultRoughnessType != value)
                {
                    data.RoughnessNetworkCoverage.DefaultRoughnessType = value;
                    data.RoughnessNetworkCoverage.DefaultValue = RoughnessHelper.GetDefault(value);
                }
            }
        }

        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "RoughnessSectionProperties_Extrapolation_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RoughnessSectionProperties_Extrapolation_Description")]
        public ExtrapolationType ExtrapolationType
        {
            get { return data.RoughnessNetworkCoverage.Locations.ExtrapolationType; }
        }

        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "RoughnessSectionProperties_Interpolation_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RoughnessSectionProperties_Interpolation_Description")]
        public InterpolationType InterpolationType
        {
            get { return data.RoughnessNetworkCoverage.Locations.InterpolationType; }
            set
            {
                if (data.RoughnessNetworkCoverage.Locations.AllowSetInterpolationType)
                {
                    data.RoughnessNetworkCoverage.Locations.InterpolationType = value;
                }
                else
                {
                    Log.ErrorFormat("Unable to set interpolation-type for locations, it is not allowed.");
                }
            }
        }

        [DynamicReadOnlyValidationMethod]
        public bool DynamicReadOnlyValidation(string propertyName)
        {
            if (propertyName.Equals(nameof(DefaultRoughness)) || propertyName.Equals(nameof(DefaultRoughnessType)))
            {
                return data.Reversed;
            }

            return false;
        }
    }
}
