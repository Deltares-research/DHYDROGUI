using System.ComponentModel;
using DelftTools.Functions;

namespace DeltaShell.Plugins.FMSuite.Common.FeatureData
{
    public enum WindComponent
    {
        X,
        Y,
        Pressure,
        Magnitude,
        Angle
    };

    public enum WindQuantity
    {
        [Description("X-component")]
        VelocityX,

        [Description("Y-component")]
        VelocityY,

        [Description("Velocity vector")]
        VelocityVector,

        [Description("Air pressure")]
        AirPressure,

        [Description("Wind vector and air pressure")]
        VelocityVectorAirPressure
    }

    public interface IWindField
    {
        WindQuantity Quantity { get; }

        IFunction Data { get; }

        string Name { get; }
    }
}