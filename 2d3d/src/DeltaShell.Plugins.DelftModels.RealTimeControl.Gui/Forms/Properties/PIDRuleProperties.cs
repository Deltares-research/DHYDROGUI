using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Design;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.Properties
{
    [ResourcesDisplayName(typeof(Resources), "PIDRuleProperties_DisplayName")]
    public class PIDRuleProperties : RuleProperties<PIDRule>
    {
        [ExcludeFromCodeCoverage]
        [ResourcesCategory(typeof(Resources), "Category_Data")]
        [ResourcesDisplayName(typeof(Resources), "ConstantSetpoint_DisplayName")]
        [ResourcesDescription(typeof(Resources), "ConstantSetpoint_Description")]
        [DynamicReadOnly]
        [PropertyOrder(3)]
        public double ConstantSetpoint
        {
            get => data.ConstantValue;
            set => data.ConstantValue = value;
        }

        [ExcludeFromCodeCoverage]
        [DynamicReadOnly]
        [Editor(typeof(ViewPropertyEditor), typeof(UITypeEditor))]
        [ResourcesCategory(typeof(Resources), "Category_Data")]
        [ResourcesDisplayName(typeof(Resources), "RTC_Table_DisplayName")]
        [ResourcesDescription(typeof(Resources), "PIDRuleProperties_Table_Description")]
        [PropertyOrder(4)]
        public TimeSeries Table
        {
            get => data.TimeSeries;
            set => data.TimeSeries = value;
        }

        [ExcludeFromCodeCoverage]
        [ResourcesCategory(typeof(Resources), "Category_Data")]
        [ResourcesDisplayName(typeof(Resources), "SetpointMode")]
        [ResourcesDescription(typeof(Resources), "SetpointMode_Description")]
        [PropertyOrder(5)]
        public PIDRule.PIDRuleSetpointTypes SetpointMode
        {
            get => data.PidRuleSetpointType;
            set => data.PidRuleSetpointType = value;
        }

        [ExcludeFromCodeCoverage]
        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "RTC_Category_InterpolationExtrapolation")]
        [ResourcesDisplayName(typeof(Resources), "RTC_Interpolation_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RTC_Interpolation_Description")]
        [PropertyOrder(6)]
        public InterpolationHydraulicType Interpolation
        {
            get => (InterpolationHydraulicType) data.InterpolationOptionsTime;
            set => data.InterpolationOptionsTime = (InterpolationType) value;
        }

        [ExcludeFromCodeCoverage]
        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "RTC_Category_InterpolationExtrapolation")]
        [ResourcesDisplayName(typeof(Resources), "RTC_Extrapolation_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RTC_Time_Extrapolation_Description")]
        [PropertyOrder(7)]
        public ExtrapolationTimeSeriesType Extrapolation
        {
            get => (ExtrapolationTimeSeriesType) data.ExtrapolationOptionsTime;
            set => data.ExtrapolationOptionsTime = (ExtrapolationType) value;
        }

        [ExcludeFromCodeCoverage]
        [ResourcesCategory(typeof(Resources), "PIDRuleProperties_Category_GainFactor")]
        [ResourcesDisplayName(typeof(Resources), "PIDRuleProperties_Kp_DisplayName")]
        [ResourcesDescription(typeof(Resources), "PIDRuleProperties_Kp_Description")]
        [PropertyOrder(8)]
        public double Kp
        {
            get => data.Kp;
            set => data.Kp = value;
        }

        [ExcludeFromCodeCoverage]
        [ResourcesCategory(typeof(Resources), "PIDRuleProperties_Category_GainFactor")]
        [ResourcesDisplayName(typeof(Resources), "PIDRuleProperties_Ki_DisplayName")]
        [ResourcesDescription(typeof(Resources), "PIDRuleProperties_Ki_Descirption")]
        [PropertyOrder(9)]
        public double Kl
        {
            get => data.Ki;
            set => data.Ki = value;
        }

        [ExcludeFromCodeCoverage]
        [ResourcesCategory(typeof(Resources), "PIDRuleProperties_Category_GainFactor")]
        [ResourcesDisplayName(typeof(Resources), "PIDRuleProperties_Kd_DisplayName")]
        [ResourcesDescription(typeof(Resources), "PIDRuleProperties_Kd_Description")]
        [PropertyOrder(10)]
        public double Kd
        {
            get => data.Kd;
            set => data.Kd = value;
        }

        [ExcludeFromCodeCoverage]
        [ResourcesCategory(typeof(Resources), "Category_Limits")]
        [ResourcesDisplayName(typeof(Resources), "PIDRuleProperties_Minimum_DisplayName")]
        [ResourcesDescription(typeof(Resources), "PIDRuleProperties_Minimum_Description")]
        [PropertyOrder(11)]
        public double Minimum
        {
            get => data.Setting.Min;
            set => data.Setting.Min = value;
        }

        [ExcludeFromCodeCoverage]
        [ResourcesCategory(typeof(Resources), "Category_Limits")]
        [ResourcesDisplayName(typeof(Resources), "PIDRuleProperties_Maximum_DisplayName")]
        [ResourcesDescription(typeof(Resources), "PIDRuleProperties_Maximum_Description")]
        [PropertyOrder(12)]
        public double Maximum
        {
            get => data.Setting.Max;
            set => data.Setting.Max = value;
        }

        [ExcludeFromCodeCoverage]
        [ResourcesCategory(typeof(Resources), "Category_Limits")]
        [ResourcesDisplayName(typeof(Resources), "PIDRuleProperties_MaxSpeed_DisplayName")]
        [ResourcesDescription(typeof(Resources), "PIDRuleProperties_MaxSpeed_Description")]
        [PropertyOrder(13)]
        public double MaxSpeed
        {
            get => data.Setting.MaxSpeed;
            set => data.Setting.MaxSpeed = value;
        }

        [DynamicReadOnlyValidationMethod]
        public bool DynamicReadOnlyValidationMethod(string propertyName)
        {
            switch (propertyName)
            {
                case "ConstantSetpoint":
                    return SetpointMode != PIDRule.PIDRuleSetpointTypes.Constant;
                case "Table":
                    return SetpointMode != PIDRule.PIDRuleSetpointTypes.TimeSeries;
                case "Interpolation":
                    return SetpointMode != PIDRule.PIDRuleSetpointTypes.TimeSeries;
                case "Extrapolation":
                    return SetpointMode != PIDRule.PIDRuleSetpointTypes.TimeSeries;
                default:
                    return true;
            }
        }
    }
}