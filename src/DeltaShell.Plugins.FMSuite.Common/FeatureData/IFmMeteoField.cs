using System.ComponentModel;
using DelftTools.Functions;

namespace DeltaShell.Plugins.FMSuite.Common.FeatureData
{
    public enum FmMeteoComponent
    {
        Precipitation
    };
    public enum FmMeteoQuantity
    {
        [Description("Precipication rainfall")]
        Precipitation,
    }
    public interface IFmMeteoField
    {
        FmMeteoQuantity Quantity { get; }

        IFunction Data { get; }

        string Name { get; }
    }
}