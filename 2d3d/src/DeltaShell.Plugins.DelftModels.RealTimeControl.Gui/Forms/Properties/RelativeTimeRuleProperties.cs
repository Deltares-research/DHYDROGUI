using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Design;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.Properties
{
    [ResourcesDisplayName(typeof(Resources), "RelativeTimeRuleProperties_DisplayName")]
    public class RelativeTimeRuleProperties : RuleProperties<RelativeTimeRule>
    {
        [ExcludeFromCodeCoverage]
        [ResourcesCategory(typeof(Resources), "Category_Data")]
        [ResourcesDisplayName(typeof(Resources), "RelativeTimeRuleProperties_FromValue_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RelativeTimeRuleProperties_FromValue_Description")]
        [PropertyOrder(3)]
        public bool FromValue
        {
            get => data.FromValue;
            set => data.FromValue = value;
        }

        [ExcludeFromCodeCoverage]
        [ResourcesCategory(typeof(Resources), "Category_Data")]
        [ResourcesDisplayName(typeof(Resources), "RelativeTimeRuleProperties_MinimumPeriod_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RelativeTimeRuleProperties_MinimumPeriod_Description")]
        [PropertyOrder(4)]
        public int MinimumPeriod
        {
            get => data.MinimumPeriod;
            set => data.MinimumPeriod = value;
        }

        [Editor(typeof(ViewPropertyEditor), typeof(UITypeEditor))]
        [ResourcesCategory(typeof(Resources), "Category_Data")]
        [ResourcesDisplayName(typeof(Resources), "RTC_Timeseries_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RTC_TimeSeries_Description")]
        [PropertyOrder(5)]
        public Function Table
        {
            get
            {
                UpdateFunctionArgumentName();
                UpdateFunctionComponentName();

                return data.Function;
            }
            set => data.Function = value;
        }

        [ExcludeFromCodeCoverage]
        [ResourcesCategory(typeof(Resources), "RelativeTimeRuleProperties_Category_Interpolation")]
        [ResourcesDisplayName(typeof(Resources), "RelativeTimeRuleProperties_Interpolation_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RTC_Interpolation_Description")]
        [PropertyOrder(6)]
        public InterpolationHydraulicType Interpolation
        {
            get => (InterpolationHydraulicType) data.Interpolation;
            set => data.Interpolation = (InterpolationType) value;
        }

        private void UpdateFunctionArgumentName()
        {
            IInput ruleInput = data.Inputs.FirstOrDefault();
            IVariable variable = data.Function.Arguments[0];
            switch (ruleInput)
            {
                case Input input when input.IsConnected:
                    SetVariableName(variable, input.ParameterName + " [i]");
                    break;
                case MathematicalExpression expression:
                    SetVariableName(variable, expression.Name + " [i]");
                    break;
                default:
                    SetVariableName(variable, "seconds");
                    break;
            }
        }

        private static void SetVariableName(IVariable target, string value)
        {
            if (target.Name != value)
            {
                target.Name = value;
            }
        }

        private void UpdateFunctionComponentName()
        {
            if (data.Outputs.Count == 1)
            {
                Output output = data.Outputs[0];

                if (!string.IsNullOrEmpty(output.ParameterName) && data.Function.Components[0].Name != output.ParameterName)
                {
                    data.Function.Components[0].Name = output.ParameterName;
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