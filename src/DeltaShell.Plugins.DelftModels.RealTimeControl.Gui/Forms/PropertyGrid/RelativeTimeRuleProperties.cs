using System;
using System.ComponentModel;
using System.Drawing.Design;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "RelativeTimeRuleProperties_DisplayName")]
    public class RelativeTimeRuleProperties : ObjectProperties<RelativeTimeRule>
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

        // TODO: When Extrapolation is added, change the category reference to RTC_Category_InterpolationExtrapolation
        [ResourcesCategory(typeof(Resources), "RelativeTimeRuleProperties_Category_Interpolation")]
        [ResourcesDisplayName(typeof(Resources), "RelativeTimeRuleProperties_Interpolation_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RTC_Interpolation_Description")]
        public InterpolationType Interpolation
        {
            get { return data.Interpolation; }
            set { data.Interpolation = value; }
        }

        [Editor(typeof(ViewPropertyEditor), typeof(UITypeEditor))]
        [ResourcesCategory(typeof(Resources), "Category_Table")]
        [ResourcesDisplayName(typeof(Resources), "Table_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RTC_TimeSeries_Description")]
        public Function Table
        {
            get
            {
                UpdateFunctionArgumentName();
                UpdateFunctionComponentName();

                return data.Function;
            }
            set { data.Function = value; }
        }

        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "RelativeTimeRuleProperties_FromValue_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RelativeTimeRuleProperties_FromValue_Description")]
        public bool FromValue
        {
            get { return data.FromValue; }
            set { data.FromValue = value; }
        }

        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "RelativeTimeRuleProperties_MinimumPeriod_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RelativeTimeRuleProperties_MinimumPeriod_Description")]
        public int MinimumPeriod
        {
            get { return data.MinimumPeriod; }
            set { data.MinimumPeriod = value; }
        }

        /// <summary>
        /// todo refactor UpdateFunctionArgumentName and UpdateFunctionComponentName
        /// </summary>
        private void UpdateFunctionArgumentName()
        {
            if (data.Inputs.Count == 1)
            {
                var input = data.Inputs[0];

                if (!string.IsNullOrEmpty(input.ParameterName))
                {
                    // Prevent unneeded property change event
                    if(data.Function.Arguments[0].Name != input.ParameterName)
                    {
                        data.Function.Arguments[0].Name = input.ParameterName;
                    }
                }

                return;
            }

            // Prevent unneeded property change event
            if (data.Function.Arguments[0].Name != "seconds")
            {
                data.Function.Arguments[0].Name = "seconds";
            }
        }

        private void UpdateFunctionComponentName()
        {
            if (data.Outputs.Count == 1)
            {
                var output = data.Outputs[0];

                if (!string.IsNullOrEmpty(output.ParameterName))
                {
                    // Prevent unneeded property change event
                    if (data.Function.Components[0].Name != output.ParameterName)
                    {
                        data.Function.Components[0].Name = output.ParameterName;
                    }
                }

                return;
            }

            // Prevent unneeded property change event
            if (data.Function.Components[0].Name != "<output undefined>")
            {
                data.Function.Components[0].Name = "<output undefined>";
            }
        }
    }
}
