using DelftTools.Functions;

namespace DeltaShell.Plugins.FMSuite.Common.FeatureData
{
    public class FMPrecipitationField : IFmMeteoField
    {
        public FmMeteoQuantity Quantity { get; }
        public IFunction Data { get; }
        public string Name { get; }
    }
}