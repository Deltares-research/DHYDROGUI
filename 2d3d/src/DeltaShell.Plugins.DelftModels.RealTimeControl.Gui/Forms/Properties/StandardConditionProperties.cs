using System.ComponentModel;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Converters;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.Properties
{
    [ResourcesDisplayName(typeof(Resources), "StandardConditionProperties_DisplayName")]
    public class StandardConditionProperties : ObjectProperties<StandardCondition>
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

        [PropertyOrder(1)]
        [ResourcesCategory(typeof(Resources), "DirectionalConditionProperties_Category_Input")]
        [ResourcesDisplayName(typeof(Resources), "DirectionalConditionProperties_Input_DisplayName")]
        [ResourcesDescription(typeof(Resources), "DirectionalConditionProperties_Input_Description")]
        public string Input
        {
            get
            {
                return null != data.Input ? data.Input.Name : "";
            }
        }

        [TypeConverter(typeof(OperationConverter))]
        [PropertyOrder(2)]
        [ResourcesCategory(typeof(Resources), "DirectionalConditionProperties_Category_Input")]
        [ResourcesDisplayName(typeof(Resources), "DirectionalConditionProperties_Operation_DisplayName")]
        [ResourcesDescription(typeof(Resources), "DirectionalConditionProperties_Operation_Description")]
        public string Operation
        {
            get
            {
                return new OperationConverter().OperationToString(data.Operation);
            }
            set
            {
                data.Operation = new OperationConverter().StringToOperation(value);
            }
        }

        [PropertyOrder(3)]
        [ResourcesCategory(typeof(Resources), "DirectionalConditionProperties_Category_Input")]
        [ResourcesDisplayName(typeof(Resources), "StandardConditionProperties_Value_DisplayName")]
        [ResourcesDescription(typeof(Resources), "StandardConditionProperties_Value_Description")]
        public double Value
        {
            get
            {
                return data.Value;
            }
            set
            {
                data.Value = value;
            }
        }
    }
}