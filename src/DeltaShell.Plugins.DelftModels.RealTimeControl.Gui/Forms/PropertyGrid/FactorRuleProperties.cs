using System.Diagnostics.CodeAnalysis;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.PropertyGrid
{
    [ExcludeFromCodeCoverage]
    [ResourcesDisplayName(typeof(Resources), "FactorRuleProperties_DisplayName")]
    public class FactorRuleProperties : ObjectProperties<FactorRule>
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

        [ResourcesCategory(typeof(Resources), "Category_Data")]
        [ResourcesDisplayName(typeof(Resources), "Rule_Factor_DisplayName")]
        [ResourcesDescription(typeof(Resources), "Rule_Factor_Description")]
        [PropertyOrder(3)]
        public double Factor
        {
            get => data.Factor;
            set => data.Factor = value;
        }
    }
}
