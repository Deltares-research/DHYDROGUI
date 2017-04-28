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
    [ResourcesDisplayName(typeof(Resources), "PIDRuleProperties_DisplayName")]
    public class PIDRuleProperties : ObjectProperties<PIDRule>
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

        [ResourcesCategory(typeof(Resources), "PIDRuleProperties_Category_GainFactor")]
        [ResourcesDisplayName(typeof(Resources), "PIDRuleProperties_Kp_DisplayName")]
        [ResourcesDescription(typeof(Resources), "PIDRuleProperties_Kp_Description")]
        public double Kp
        {
            get { return data.Kp; }
            set { data.Kp = value; }
        }

        [ResourcesCategory(typeof(Resources), "PIDRuleProperties_Category_GainFactor")]
        [ResourcesDisplayName(typeof(Resources), "PIDRuleProperties_Ki_DisplayName")]
        [ResourcesDescription(typeof(Resources), "PIDRuleProperties_Ki_Descirption")]
        public double Kl
        {
            get { return data.Ki; }
            set { data.Ki = value; }
        }

        [ResourcesCategory(typeof(Resources), "PIDRuleProperties_Category_GainFactor")]
        [ResourcesDisplayName(typeof(Resources), "PIDRuleProperties_Kd_DisplayName")]
        [ResourcesDescription(typeof(Resources), "PIDRuleProperties_Kd_Description")]
        public double Kd
        {
            get { return data.Kd; }
            set { data.Kd = value; }
        }

        [ResourcesCategory(typeof(Resources), "Category_Limits")]
        [ResourcesDisplayName(typeof(Resources), "PIDRuleProperties_Minimum_DisplayName")]
        [ResourcesDescription(typeof(Resources), "PIDRuleProperties_Minimum_Description")]
        public double Minimum
        {
            get { return data.Setting.Min; }
            set { data.Setting.Min = value; }
        }

        [ResourcesCategory(typeof(Resources), "Category_Limits")]
        [ResourcesDisplayName(typeof(Resources), "PIDRuleProperties_Maximum_DisplayName")]
        [ResourcesDescription(typeof(Resources), "PIDRuleProperties_Maximum_Description")]
        public double Maximum
        {
            get { return data.Setting.Max; }
            set { data.Setting.Max = value; }
        }

        [ResourcesCategory(typeof(Resources), "Category_Limits")]
        [ResourcesDisplayName(typeof(Resources), "PIDRuleProperties_MaxSpeed_DisplayName")]
        [ResourcesDescription(typeof(Resources), "PIDRuleProperties_MaxSpeed_Description")] // TODO: Unit!
        public double MaxSpeed
        {
            get { return data.Setting.MaxSpeed; }
            set { data.Setting.MaxSpeed = value; }
        }

        [ResourcesCategory(typeof(Resources), "Category_Data")]
        [ResourcesDisplayName(typeof(Resources), "ConstantSetpoint_DisplayName")]
        [ResourcesDescription(typeof(Resources), "ConstantSetpoint_Description")]
        [DynamicReadOnly]
        public double ConstantSetpoint
        {
            get { return data.ConstantValue; }
            set { data.ConstantValue = value; }
        }

        [DynamicReadOnly]
        [Editor(typeof(ViewPropertyEditor), typeof(UITypeEditor))]
        [ResourcesCategory(typeof(Resources), "Category_Data")]
        [ResourcesDisplayName(typeof(Resources), "RTC_Table_DisplayName")]
        [ResourcesDescription(typeof(Resources), "PIDRuleProperties_Table_Description")]
        public TimeSeries Table
        {
            get { return data.TimeSeries; }
            set { data.TimeSeries = value; }
        }

        private TimeSeries cachedTimeSeries;

        [ResourcesCategory(typeof(Resources), "Category_Data")]
        [ResourcesDisplayName(typeof(Resources), "SetpointMode")]
        [ResourcesDescription(typeof(Resources), "SetpointMode_Description")]
        public PIDRule.PIDRuleSetpointType SetpointMode
        {
            get { return data.PidRuleSetpointType; }
            set
            {
                if (value != data.PidRuleSetpointType)
                {
                    if (value != PIDRule.PIDRuleSetpointType.TimeSeries)
                    {
                        // set to constant save time series in cache
                        cachedTimeSeries = (TimeSeries)data.TimeSeries.Clone();
                    }

                    data.PidRuleSetpointType = value;
                    
                    if ((value == PIDRule.PIDRuleSetpointType.TimeSeries) && (cachedTimeSeries != null))
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
            get { return (InterpolationHydraulicType)data.InterpolationOptionsTime; }
            set { data.InterpolationOptionsTime = (InterpolationType)value; }
        }

        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "RTC_Category_InterpolationExtrapolation")]
        [ResourcesDisplayName(typeof(Resources), "RTC_Extrapolation_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RTC_Time_Extrapolation_Description")]
        public ExtrapolationTimeSeriesType Extrapolation
        {
            get { return (ExtrapolationTimeSeriesType) data.ExtrapolationOptionsTime; }
            set { data.ExtrapolationOptionsTime = (ExtrapolationType) value; }
        }

        [DynamicReadOnlyValidationMethod]
        public bool DynamicReadOnlyValidationMethod(string propertyName)
        {
            if (propertyName == "ConstantSetpoint")
            {
                return SetpointMode != PIDRule.PIDRuleSetpointType.Constant;
            }

            if (propertyName == "Table")
            {
                return SetpointMode != PIDRule.PIDRuleSetpointType.TimeSeries;
            }

            if (propertyName == "Interpolation")
            {
                return SetpointMode != PIDRule.PIDRuleSetpointType.TimeSeries;
            }

            if (propertyName == "Extrapolation")
            {
                return SetpointMode != PIDRule.PIDRuleSetpointType.TimeSeries;
            }

            return true;
        }
    }
}
