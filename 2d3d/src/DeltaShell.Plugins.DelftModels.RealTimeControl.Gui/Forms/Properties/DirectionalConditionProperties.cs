using System.ComponentModel;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Converters;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.Properties
{
    [ResourcesDisplayName(typeof(Resources), "DirectionalConditionProperties_DisplayName")]
    public class DirectionalConditionProperties : ObjectProperties<DirectionalCondition>
    {
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Common_Name_DisplayName")]
        [ResourcesDescription(typeof(Resources), "Condition_Name_Description")]
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
        [ResourcesDescription(typeof(Resources), "Condition_LongName_Description")]
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

        [TypeConverter(typeof(DirectionalOperationConverter))]
        [PropertyOrder(2)]
        [ResourcesCategory(typeof(Resources), "DirectionalConditionProperties_Category_Input")]
        [ResourcesDisplayName(typeof(Resources), "DirectionalConditionProperties_Operation_DisplayName")]
        [ResourcesDescription(typeof(Resources), "DirectionalConditionProperties_Operation_Description")]
        public string Operation
        {
            get
            {
                return new DirectionalOperationConverter().OperationToString(data.Operation);
            }
            set
            {
                data.Operation = new DirectionalOperationConverter().StringToOperation(value);
            }
        }
    }
}