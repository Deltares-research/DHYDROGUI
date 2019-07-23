using System.ComponentModel;
using System.Drawing.Design;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "IntervalRuleProperties_DisplayName")]
    public class IntervalRuleProperties : ObjectProperties<IntervalRule>
    {
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Common_Name_DisplayName")]
        [ResourcesDescription(typeof(Resources), "Rule_Name_Description")]
        [PropertyOrder(1)]
        public string Name
        {
            get => data.Name;
            set => data.Name = value;
        }

        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Common_LongName_DisplayName")]
        [ResourcesDescription(typeof(Resources), "Rule_LongName_Description")]
        [PropertyOrder(2)]
        public string LongName
        {
            get => data.LongName;
            set => data.LongName = value;
        }

        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "Category_Data")]
        [ResourcesDisplayName(typeof(Resources), "ConstantSetpoint_DisplayName")]
        [ResourcesDescription(typeof(Resources), "ConstantSetpoint_Description")]
        [PropertyOrder(3)]
        public double ConstantSetpoint
        {
            get => data.ConstantValue;
            set => data.ConstantValue = value;
        }

        [Editor(typeof(ViewPropertyEditor), typeof(UITypeEditor))]
        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "Category_Data")]
        [ResourcesDisplayName(typeof(Resources), "RTC_Table_DisplayName")]
        [ResourcesDescription(typeof(Resources), "IntervalRuleProperties_TimeSeries_Description")]
        [PropertyOrder(4)]
        public TimeSeries TimeSeries
        {
            get => data.TimeSeries;
            set => data.TimeSeries = value;
        }

        [ResourcesCategory(typeof(Resources), "Category_Data")]
        [ResourcesDisplayName(typeof(Resources), "SetpointMode")]
        [ResourcesDescription(typeof(Resources), "SetpointMode_Description")]
        [PropertyOrder(5)]
        public IntervalRule.IntervalRuleIntervalType IntervalType
        {
            get => data.IntervalType;
            set => data.IntervalType = value;
        }

        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "RTC_Category_InterpolationExtrapolation")]
        [ResourcesDisplayName(typeof(Resources), "RTC_Interpolation_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RTC_Interpolation_Description")]
        [PropertyOrder(6)]
        public InterpolationHydraulicType Interpolation
        {
            get => (InterpolationHydraulicType)data.InterpolationOptionsTime;
            set => data.InterpolationOptionsTime = (InterpolationType)value;
        }

        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "RTC_Category_InterpolationExtrapolation")]
        [ResourcesDisplayName(typeof(Resources), "RTC_Extrapolation_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RTC_Time_Extrapolation_Description")]
        [PropertyOrder(7)]
        public ExtrapolationTimeSeriesType Extrapolation
        {
            get => (ExtrapolationTimeSeriesType)data.Extrapolation;
            set => data.Extrapolation = (ExtrapolationType)value;
        }

        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "Category_Limits")]
        [ResourcesDisplayName(typeof(Resources), "IntervalRuleProperties_MaxSpeed_DisplayName")]
        [ResourcesDescription(typeof(Resources), "IntervalRuleProperties_MaxSpeed_Description")]
        [PropertyOrder(8)]
        public double MaxSpeed
        {
            get => data.Setting.MaxSpeed;
            set => data.Setting.MaxSpeed = value;
        }

        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "Category_Limits")]
        [ResourcesDisplayName(typeof(Resources), "IntervalRuleProperties_FixedInterval_DisplayName")]
        [ResourcesDescription(typeof(Resources), "IntervalRuleProperties_FixedInterval_Description")]
        [PropertyOrder(9)]
        public double FixedInterval
        {
            get => data.FixedInterval;
            set => data.FixedInterval = value;
        }

        [ResourcesCategory(typeof(Resources), "Category_Limits")]
        [ResourcesDisplayName(typeof(Resources), "IntervalRuleProperties_AboutOutput_DisplayName")]
        [ResourcesDescription(typeof(Resources), "IntervalRuleProperties_Above_Description")]
        [PropertyOrder(10)]
        public double Above
        {
            get => data.Setting.Above;
            set => data.Setting.Above = value;
        }

        [ResourcesCategory(typeof(Resources), "Category_Limits")]
        [ResourcesDisplayName(typeof(Resources), "IntervalRuleProperties_Below_DisplayName")]
        [ResourcesDescription(typeof(Resources), "IntervalRuleProperties_Below_Description")]
        [PropertyOrder(11)]
        public double Below
        {
            get => data.Setting.Below;
            set => data.Setting.Below = value;
        }

        [ResourcesCategory(typeof(Resources), "Category_Limits")]
        [ResourcesDisplayName(typeof(Resources), "IntervalRuleProperties_DeadbankAroundSetpoint_DisplayName")]
        [ResourcesDescription(typeof(Resources), "IntervalRuleProperties_DeadbankAroundSetpoint_Description")]
        [PropertyOrder(12)]
        public double DeadbandAroundSetpoint
        {
            get => data.DeadbandAroundSetpoint;
            set => data.DeadbandAroundSetpoint = value;
        }

        [ResourcesCategory(typeof(Resources), "Category_Limits")]
        [ResourcesDisplayName(typeof(Resources), "IntervalRuleProperties_DeadbandType_DisplayName")]
        [ResourcesDescription(typeof(Resources), "IntervalRuleProperties_DeadbandType_Description")]
        [PropertyOrder(13)]
        public IntervalRule.IntervalRuleDeadBandType DeadBandType
        {
            get => data.DeadBandType;
            set => data.DeadBandType = value;
        }

        [DynamicReadOnlyValidationMethod]
        public bool DynamicReadOnlyValidationMethod(string propertyName)
        {
            switch (propertyName)
            {
                case "ConstantSetpoint":
                    return IntervalType != IntervalRule.IntervalRuleIntervalType.Fixed;
                case "TimeSeries":
                    return IntervalType != IntervalRule.IntervalRuleIntervalType.Variable;
                case "Interpolation":
                    return IntervalType != IntervalRule.IntervalRuleIntervalType.Variable;
                case "Extrapolation":
                    return IntervalType != IntervalRule.IntervalRuleIntervalType.Variable;
                case "FixedInterval":
                    return IntervalType != IntervalRule.IntervalRuleIntervalType.Fixed;
                case "MaxSpeed":
                    return IntervalType == IntervalRule.IntervalRuleIntervalType.Fixed;
                default:
                    return true;
            }
        }

    }
}
