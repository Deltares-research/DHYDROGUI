using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport.Export
{
    public class FactorRuleSerializer : HydraulicRuleSerializer
    {
        public FactorRuleSerializer(FactorRule factorRule) : base(factorRule) {}

        protected override string XmlTag { get; } = RtcXmlTag.FactorRule;
    }
}