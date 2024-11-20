using System.Diagnostics.CodeAnalysis;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.Properties
{
    public abstract class RuleProperties<TRuleBase> : ObjectProperties<TRuleBase>
        where TRuleBase : RuleBase
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
    }
}