using System;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DBoundaryNodeDataProperties_DisplayName")]
    public class WaterFlowModel1DBoundaryNodeDataProperties : ObjectProperties<WaterFlowModel1DBoundaryNodeData>
    {
        [PropertyOrder(1)]
        [ResourcesCategory(typeof(Resources),    "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Common_Name_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DBoundaryNodeDataProperties_Name_Description")]
        public string Name => data.Name;

        [PropertyOrder(2)]
        [ResourcesCategory(typeof(Resources),    "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DBoundaryNodeDataProperties_Type_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DBoundaryNodeDataProperties_Type_Description")]
        public WaterFlowModel1DBoundaryNodeDataType Type
        {
            get => data.DataType;
            set => data.DataType = value;
        }

        [PropertyOrder(3)]
        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DBoundaryNodeDataProperties_InterpolationType_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DBoundaryNodeDataProperties_InterpolationType_Description")]
        public Flow1DInterpolationType InterpolationType
        {
            get
            {
                switch (Type)
                {
                    case WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries:
                    case WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries:
                    case WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable:
                        return data.Data.GetInterpolationType();
                    case WaterFlowModel1DBoundaryNodeDataType.FlowConstant:
                    case WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant:
                    case WaterFlowModel1DBoundaryNodeDataType.None: // This should never happen
                        return Flow1DInterpolationType.BlockFrom;
                    default:
                        throw new NotSupportedException("The provided boundary condition type is not supported.");
                }
            }
            set => data.Data.SetInterpolationType(value);
        }

        [PropertyOrder(4)]
        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DBoundaryNodeDataProperties_ExtrapolationType_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DBoundaryNodeDataProperties_ExtrapolationType_Description")]
        public Flow1DExtrapolationType ExtrapolationType
        {
            get
            {
                switch (Type)
                {
                    case WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries:
                    case WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries:
                    case WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable:
                        return data.Data.GetExtrapolationType();
                    case WaterFlowModel1DBoundaryNodeDataType.FlowConstant:
                    case WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant:
                    case WaterFlowModel1DBoundaryNodeDataType.None: // This should never happen
                        return Flow1DExtrapolationType.Constant;
                    default:
                        throw new NotSupportedException("The provided boundary condition type is not supported.");
                }
            }
            set => data.Data.SetExtrapolationType(value);
        }

        [PropertyOrder(4)]
        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DBoundaryNodeDataProperties_HasPeriodicity_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DBoundaryNodeDataProperties_HasPeriodicity_Description")]
        public bool HasPeriodicity
        {
            get
            {
                switch (Type)
                {
                    case WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries:
                    case WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries:
                    case WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable:
                        return data.Data.HasPeriodicity();
                    case WaterFlowModel1DBoundaryNodeDataType.FlowConstant:
                    case WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant:
                    case WaterFlowModel1DBoundaryNodeDataType.None: // This should never happen
                        return false;
                    default:
                        throw new NotSupportedException("The boundary condition type is not supported.");
                }
            }
            set => data.Data.SetPeriodicity(value);
        }

        [DynamicReadOnlyValidationMethod]
        public bool DynamicReadOnlyValidationMethod(string propertyName)
        {
            switch (propertyName) {
                case nameof(InterpolationType):
                    return (Type != WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries &&
                            Type != WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries) ||
                           !data.Data.Arguments[0].AllowSetInterpolationType;
                case nameof(ExtrapolationType):
                    return (Type != WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries &&
                            Type != WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries) ||
                           !data.Data.Arguments[0].AllowSetExtrapolationType                   ||
                           data.Data.GetInterpolationType() != Flow1DInterpolationType.Linear  ||
                           data.Data.HasPeriodicity();
                case nameof(HasPeriodicity):
                    return (Type != WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries &&
                            Type != WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries) ||
                           !data.Data.Arguments[0].AllowSetExtrapolationType;
                default:
                    return true;
            }
        }
    }
}