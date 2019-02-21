using DelftTools.Functions;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "MeteoDataFunctionProperties_DisplayName")]
    public class MeteoDataFunctionProperties : ObjectProperties<Function>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MeteoDataFunctionProperties));

        [PropertyOrder(1)]
        [ResourcesCategory(typeof(Resources),    "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Common_Name_DisplayName")]
        [ResourcesDescription(typeof(Resources), "MeteoDataFunctionProperties_Name_Description")]
        public string Name => data.Name;

        [PropertyOrder(2)]
        [ResourcesCategory(typeof(Resources),    "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "MeteoDataFunctionProperties_InterpolationType_DisplayName")]
        [ResourcesDescription(typeof(Resources), "MeteoDataFunctionProperties_InterpolationType_Description")]
        public Flow1DInterpolationType InterpolationType
        {
            get => data.GetInterpolationType();
            set => data.SetInterpolationType(value);
        }

        [PropertyOrder(3)]
        [ResourcesCategory(typeof(Resources),    "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "MeteoDataFunctionProperties_Extrapolation_DisplayName")]
        [ResourcesDescription(typeof(Resources), "MeteoDataFunctionProperties_Extrapolation_Description")]
        [DynamicReadOnly]
        public Flow1DExtrapolationType ExtrapolationType
        {
            get => InterpolationType == Flow1DInterpolationType.Linear 
                ? data.GetExtrapolationType()
                : Flow1DExtrapolationType.Constant;
            set => data.SetExtrapolationType(value);
        }

        [PropertyOrder(4)]
        [ResourcesCategory(typeof(Resources),    "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "MeteoDataFunctionProperties_HasPeriodicity_DisplayName")]
        [ResourcesDescription(typeof(Resources), "MeteoDataFunctionProperties_HasPeriodicity_Description")]
        [DynamicReadOnly]
        public bool HasPeriodicity
        {
            get => data.HasPeriodicity();
            set => data.SetPeriodicity(value);
        }

        [DynamicReadOnlyValidationMethod]
        public bool IsReadonly(string propertyName)
        {
            switch (propertyName) {
                case nameof(ExtrapolationType):
                    return (!data.Arguments[0].AllowSetExtrapolationType                  ||
                            data.GetInterpolationType() != Flow1DInterpolationType.Linear ||
                            data.HasPeriodicity());
                case nameof(InterpolationType):
                    return (!data.Arguments[0].AllowSetInterpolationType);
                case nameof(HasPeriodicity):
                    return (!data.Arguments[0].AllowSetExtrapolationType);
                default:
                    return false;
            }
        }
    }
}
