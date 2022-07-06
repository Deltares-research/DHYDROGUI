using System.ComponentModel;
using System.Drawing.Design;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.Properties
{
    [ResourcesDisplayName(typeof(Resources), "LookupSignalProperties_DisplayName")]
    public class LookupSignalProperties : ObjectProperties<LookupSignal>
    {
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Common_Name_DisplayName")]
        [ResourcesDescription(typeof(Resources), "Rule_Name_Description")]
        public string Name
        {
            get
            {
                return data.Name;
            }
            set
            {
                data.Name = value;
            }
        }

        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Common_LongName_DisplayName")]
        [ResourcesDescription(typeof(Resources), "Rule_LongName_Description")]
        public string LongName
        {
            get
            {
                return data.LongName;
            }
            set
            {
                data.LongName = value;
            }
        }

        [ResourcesCategory(typeof(Resources), "Category_Table")]
        [ResourcesDisplayName(typeof(Resources), "RTC_Interpolation_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RTC_Interpolation_Description")]
        public InterpolationHydraulicType Interpolation
        {
            get
            {
                return (InterpolationHydraulicType) data.Interpolation;
            }
            set
            {
                data.Interpolation = (InterpolationType) value;
            }
        }

        [ResourcesCategory(typeof(Resources), "Category_Table")]
        [ResourcesDisplayName(typeof(Resources), "RTC_Extrapolation_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RTC_Extrapolation_Description")]
        public ExtrapolationHydraulicType Extrapolation
        {
            get
            {
                return (ExtrapolationHydraulicType) data.Extrapolation;
            }
        }

        [Editor(typeof(ViewPropertyEditor), typeof(UITypeEditor))]
        [ResourcesCategory(typeof(Resources), "Category_Table")]
        [ResourcesDisplayName(typeof(Resources), "Table_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RTC_Table_Description")]
        public Function Table
        {
            get
            {
                UpdateFunctionArgumentName();

                return data.Function;
            }
            set
            {
                data.Function = value;
            }
        }
        
        private void UpdateFunctionArgumentName()
        {
            if (data.Inputs.Count == 1)
            {
                Input input = data.Inputs[0];

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
    }
}