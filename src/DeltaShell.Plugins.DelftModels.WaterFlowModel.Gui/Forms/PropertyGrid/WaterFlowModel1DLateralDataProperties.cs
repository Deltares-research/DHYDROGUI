using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DLateralDataProperties_DisplayName")]
    public class WaterFlowModel1DLateralDataProperties : ObjectProperties<WaterFlowModel1DLateralSourceData>
    {
        [PropertyOrder(1)]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Common_Name_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DLateralDataProperties_Name_Description")]
        public string Name => data.Name;

        [PropertyOrder(2)]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DLateralDataProperties_Type_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DLateralDataProperties_Type_Description")]
        public WaterFlowModel1DLateralDataType LateralType
        {
            get => data.DataType;
            set
            {
                if (data.DataType != value)
                {
                    data.DataType = value;
                }
            }
        }

        [PropertyOrder(3)]
        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DLateralDataProperties_InterpolationType_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DLateralDataProperties_InterpolationType_Description")]
        public Flow1DInterpolationType InterpolationType
        {
            get
            {
                if (data.DataType == WaterFlowModel1DLateralDataType.FlowConstant)
                    return Flow1DInterpolationType.Linear;
                return data.Data.GetInterpolationType();
            }
            set => data.Data.SetInterpolationType(value);
        }

        [PropertyOrder(4)]
        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DLateralDataProperties_ExtrapolationType_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DLateralDataProperties_ExtrapolationType_Description")]
        public Flow1DExtrapolationType ExtrapolationType
        {
            get
            {
                if (data.DataType == WaterFlowModel1DLateralDataType.FlowConstant)
                    return Flow1DExtrapolationType.Linear;
                return data.Data.GetExtrapolationType();
            }
            set => data.Data.SetExtrapolationType(value);
        }

        [PropertyOrder(5)]
        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DLateralDataProperties_HasPeriodicity_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DLateralDataProperties_HasPeriodicity_Description")]
        public bool HasPeriodicity
        {
            get
            {
                if (data.DataType == WaterFlowModel1DLateralDataType.FlowConstant)
                    return false;
                return data.Data.HasPeriodicity();
            }
            set => data.Data.SetPeriodicity(value);
        }

        [DynamicReadOnlyValidationMethod]
        public bool DynamicReadOnlyValidationMethod(string propertyName)
        {
            switch (propertyName) {
                case nameof(InterpolationType):
                    return LateralType != WaterFlowModel1DLateralDataType.FlowTimeSeries ||
                           !data.Data.Arguments[0].AllowSetInterpolationType;
                case nameof(ExtrapolationType):
                    return LateralType != WaterFlowModel1DLateralDataType.FlowTimeSeries      ||
                           !data.Data.Arguments[0].AllowSetExtrapolationType                  ||
                           data.Data.GetInterpolationType() != Flow1DInterpolationType.Linear ||
                           data.Data.HasPeriodicity();
                case nameof(HasPeriodicity):
                    return  LateralType != WaterFlowModel1DLateralDataType.FlowTimeSeries ||
                            !data.Data.Arguments[0].AllowSetExtrapolationType;
                default:
                    return true;
            }
        }
    }
}
