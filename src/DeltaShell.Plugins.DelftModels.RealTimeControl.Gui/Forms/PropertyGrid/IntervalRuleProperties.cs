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
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
        }

        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Common_LongName_DisplayName")]
        [ResourcesDescription(typeof(Resources), "Rule_LongName_Description")]
        public string LongName
        {
            get { return data.LongName; }
            set { data.LongName = value; }
        }

        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "IntervalRuleProperties_IntervalType_DisplayName")]
        [ResourcesDescription(typeof(Resources), "IntervalRuleProperties_IntervalType_Descirption")]
        public IntervalRule.IntervalRuleIntervalType IntervalType
        {
            get { return data.IntervalType; }
            set { data.IntervalType = value; }
        }

        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "IntervalRuleProperties_MaxSpeed_DisplayName")]
        [ResourcesDescription(typeof(Resources), "IntervalRuleProperties_MaxSpeed_Description")]
        public double MaxSpeed
        {
            get { return data.Setting.MaxSpeed; }
            set { data.Setting.MaxSpeed = value; }
        }

        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "IntervalRuleProperties_FixedInterval_DisplayName")]
        [ResourcesDescription(typeof(Resources), "IntervalRuleProperties_FixedInterval_Description")]
        public double FixedInterval
        {
            get { return data.FixedInterval; }
            set { data.FixedInterval = value; }
        }

        [ResourcesCategory(typeof(Resources), "Category_Limits")]
        [ResourcesDisplayName(typeof(Resources), "IntervalRuleProperties_AboutOutput_DisplayName")]
        [ResourcesDescription(typeof(Resources), "IntervalRuleProperties_Above_Description")]
        public double Above
        {
            get { return data.Setting.Above; }
            set { data.Setting.Above = value; }
        }

        [ResourcesCategory(typeof(Resources), "Category_Limits")]
        [ResourcesDisplayName(typeof(Resources), "IntervalRuleProperties_Below_DisplayName")]
        [ResourcesDescription(typeof(Resources), "IntervalRuleProperties_Below_Description")]
        public double Below
        {
            get { return data.Setting.Below; }
            set { data.Setting.Below = value; }
        }

        [ResourcesCategory(typeof(Resources), "Category_Limits")]
        [ResourcesDisplayName(typeof(Resources), "IntervalRuleProperties_DeadbankAroundSetpoint_DisplayName")]
        [ResourcesDescription(typeof(Resources), "IntervalRuleProperties_DeadbankAroundSetpoint_Description")]
        public double DeadbandAroundSetpoint
        {
            get { return data.DeadbandAroundSetpoint; }
            set { data.DeadbandAroundSetpoint = value; }
        }

        [ResourcesCategory(typeof(Resources), "Category_Limits")]
        [ResourcesDisplayName(typeof(Resources), "IntervalRuleProperties_DeadbandType_DisplayName")]
        [ResourcesDescription(typeof(Resources), "IntervalRuleProperties_DeadbandType_Description")]
        public IntervalRule.IntervalRuleDeadBandType DeadBandType
        {
            get { return data.DeadBandType; }
            set { data.DeadBandType = value; }
        }

        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "Category_Data")]
        [ResourcesDisplayName(typeof(Resources), "ConstantSetpoint_DisplayName")]
        [ResourcesDescription(typeof(Resources), "ConstantSetpoint_Description")]
        public double ConstantSetpoint
        {
            get { return data.ConstantValue; }
            set { data.ConstantValue = value; }
        }

        [Editor(typeof(ViewPropertyEditor), typeof(UITypeEditor))]
        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "Category_Data")]
        [ResourcesDisplayName(typeof(Resources), "RTC_Table_DisplayName")]
        [ResourcesDescription(typeof(Resources), "IntervalRuleProperties_TimeSeries_Description")]
        public TimeSeries TimeSeries
        {
            get { return data.TimeSeries; }
            set { data.TimeSeries = value; }
        }

        private TimeSeries cachedTimeSeries;

        [ResourcesCategory(typeof(Resources), "Category_Data")]
        [ResourcesDisplayName(typeof(Resources), "SetpointMode")]
        [ResourcesDescription(typeof(Resources), "SetpointMode_Description")]
        public IntervalRule.IntervalRuleIntervalType IntervalMode
        {
            get { return data.IntervalType; }
            set
            {
                if (data.IntervalType != value)
                {
                    if (value != IntervalRule.IntervalRuleIntervalType.Variable)
                    {
                        // set to constant save time series in cache
                        cachedTimeSeries = (TimeSeries)data.TimeSeries.Clone();
                    }

                    data.IntervalType = value;

                    if (value == IntervalRule.IntervalRuleIntervalType.Variable && cachedTimeSeries != null)
                    {
                        // Restore cached time series
                        data.TimeSeries = (TimeSeries)cachedTimeSeries.Clone();
                    }
                }
            }
        }

        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "RTC_Category_InterpolationExtrapolation")]
        [ResourcesDisplayName(typeof(Resources), "RTC_Interpolation_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RTC_Interpolation_Description")]
        public InterpolationHydraulicType Interpolation
        {
            get { return (InterpolationHydraulicType) data.InterpolationOptionsTime; }
            set { data.InterpolationOptionsTime = (InterpolationType) value; }
        }

        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "RTC_Category_InterpolationExtrapolation")]
        [ResourcesDisplayName(typeof(Resources), "RTC_Extrapolation_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RTC_Time_Extrapolation_Description")]
        public ExtrapolationTimeSeriesType Extrapolation
        {
            get { return (ExtrapolationTimeSeriesType)data.Extrapolation; }
            set { data.Extrapolation = (ExtrapolationType) value; }
        }

        [DynamicReadOnlyValidationMethod]
        public bool DynamicReadOnlyValidationMethod(string propertyName)
        {
            if (propertyName == "ConstantSetpoint")
            {
                return IntervalMode != IntervalRule.IntervalRuleIntervalType.Fixed;
            }

            if (propertyName == "TimeSeries")
            {
                return IntervalMode != IntervalRule.IntervalRuleIntervalType.Variable;
            }

            if (propertyName == "Interpolation")
            {
                return IntervalMode != IntervalRule.IntervalRuleIntervalType.Variable;
            }

            if (propertyName == "Extrapolation")
            {
                return IntervalMode != IntervalRule.IntervalRuleIntervalType.Variable;
            }

            return true;
        }
    }
}
