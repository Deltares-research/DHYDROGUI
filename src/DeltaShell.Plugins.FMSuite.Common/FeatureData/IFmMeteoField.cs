using System.ComponentModel;
using DelftTools.Functions;
using GeoAPI.Extensions.Feature;

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
    /// <summary>
    /// Interface which contains the quantity, data and name of an FmMeteoField
    /// </summary>
    public interface IFmMeteoField
    {
        FmMeteoQuantity Quantity { get; }

        IFunction Data { get; }

        string Name { get; }

        IFeatureData FeatureData { get; set; }
    }
}