using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Design;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "HydraulicRuleProperties_DisplayName")]
    public class HydraulicRuleProperties : ObjectProperties<HydraulicRule>
    {
        [ExcludeFromCodeCoverage]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Common_Name_DisplayName")]
        [ResourcesDescription(typeof(Resources), "Rule_Name_Description")]
        [PropertyOrder(1)]
        public string Name
        {
            get => data.Name;
            set => data.Name = value;
        }

        [ExcludeFromCodeCoverage]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Common_LongName_DisplayName")]
        [ResourcesDescription(typeof(Resources), "Rule_LongName_Description")]
        [PropertyOrder(2)]
        public string LongName
        {
            get => data.LongName;
            set => data.LongName = value;
        }

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
        
        /// <summary>
        /// Update the column name for better user readability
        /// add an extra [i] to avoid dupliclate names: crestlevel may set crestlevel
        /// todo refactor UpdateFunctionArgumentName and UpdateFunctionComponentName
        /// </summary>
        private void UpdateFunctionArgumentName()
        {
            if (data.Inputs.Count == 1)
            {
                var input = data.Inputs[0];

                if (input.IsConnected)
                {
                    SetVariableName(data.Function.Arguments[0], input.ParameterName + " [i]");
                }

                return;
            }

            SetVariableName(data.Function.Arguments[0], "<input undefined>");
        }

        private static void SetVariableName(IVariable target, string value)
        {
            if (target.Name != value)
            {
                target.Name = value;
            }
        }

        /// <summary>
        /// Update the column name for better user readability
        /// add an extra [o] to avoid dupliclate names: crestlevel may set crestlevel
        /// </summary>
        private void UpdateFunctionComponentName()
        {
            if (data.Outputs.Count == 1)
            {
                var output = data.Outputs[0];

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
