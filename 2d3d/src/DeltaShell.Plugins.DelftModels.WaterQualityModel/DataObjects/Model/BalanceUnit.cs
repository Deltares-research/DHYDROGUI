using System.ComponentModel;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model
{
    /// <summary>
    /// Balance unit enumeration
    /// </summary>
    /// <remarks>
    /// The enumeration indices correspond to the following delwaq definitions:
    /// 1 - The terms can simply be the total mass (unit: g)
    /// 2 - The terms can be divided by the total surface area (unit: g/m2)
    /// 3 - The terms can be divided by the total volume (unit: g/m3)
    /// </remarks>
    public enum BalanceUnit
    {
        [Description("Gram")]
        Gram,

        [Description("Gram per square meter")]
        GramPerSquareMeter,

        [Description("Gram per cubic meter")]
        GramPerCubicMeter
    }
}