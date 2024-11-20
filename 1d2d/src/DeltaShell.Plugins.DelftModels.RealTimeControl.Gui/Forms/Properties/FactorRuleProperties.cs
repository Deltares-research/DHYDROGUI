using System.Diagnostics.CodeAnalysis;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.Properties
{
    [ExcludeFromCodeCoverage]
    [ResourcesDisplayName(typeof(Resources), "FactorRuleProperties_DisplayName")]
    public class FactorRuleProperties : RuleProperties<FactorRule>
    {
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