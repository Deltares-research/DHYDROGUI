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
    [ResourcesDisplayName(typeof(Resources), "HydraulicRuleProperties_DisplayName")]
    public class HydraulicRuleProperties : RuleProperties<HydraulicRule>
    {
        [ExcludeFromCodeCoverage]
        [ResourcesCategory(typeof(Resources), "Category_Data")]
        [ResourcesDisplayName(typeof(Resources), "HydraulicRuleProperties_TimeLag_DisplayName")]
        [ResourcesDescription(typeof(Resources), "HydraulicRuleProperties_TimeLag_Description")]
        [PropertyOrder(3)]
        public int TimeLag
        {
            get => data.TimeLag;
            set => data.TimeLag = value;
        }

        [Editor(typeof(ViewPropertyEditor), typeof(UITypeEditor))]
        [ResourcesCategory(typeof(Resources), "Category_Data")]
        [ResourcesDisplayName(typeof(Resources), "Table_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RTC_Table_Description")]
        [PropertyOrder(4)]
        public Function Table
        {
            get
            {
                UpdateFunctionArgumentName(); //OMFG!!
                UpdateFunctionComponentName();

                return data.Function;
            }
            set => data.Function = value;
        }

        [ResourcesCategory(typeof(Resources), "RTC_Category_Interpolation")]
        [ResourcesDisplayName(typeof(Resources), "RTC_Interpolation_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RTC_Interpolation_Description")]
        [PropertyOrder(5)]
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
                    SetVariableName(variable, "<input undefined>");
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

                if (output.IsConnected)
                {
                    SetVariableName(data.Function.Components[0], output.ParameterName + " [o]");
                }

                return;
            }

            SetVariableName(data.Function.Components[0], "<output undefined>");
        }
    }
}