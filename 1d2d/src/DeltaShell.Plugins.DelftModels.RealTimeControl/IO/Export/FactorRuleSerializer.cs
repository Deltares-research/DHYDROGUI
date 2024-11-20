using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export
{
    /// <summary>
    /// Serializer for a <see cref="FactorRule"/>.
    /// </summary>
    /// <seealso cref="HydraulicRuleSerializer"/>
    public class FactorRuleSerializer : HydraulicRuleSerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FactorRuleSerializer"/> class.
        /// </summary>
        /// <param name="factorRule"> The factor rule to serialize. </param>
        public FactorRuleSerializer(FactorRule factorRule) : base(factorRule) {}

        protected override string XmlTag { get; } = RtcXmlTag.FactorRule;
    }
}