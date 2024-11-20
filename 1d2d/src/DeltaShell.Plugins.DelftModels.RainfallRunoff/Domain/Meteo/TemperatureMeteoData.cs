using DelftTools.Units;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo
{
    /// <summary>
    /// Temperature Meteo Data, <seealso cref="MeteoData"/>
    /// </summary>
    public class TemperatureMeteoData : MeteoData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemperatureMeteoData"/> class.
        /// </summary>
        public TemperatureMeteoData() : base(MeteoDataAggregationType.NonCumulative, new Unit(RainfallRunoffModelDataSet.TemperatureName, "°C"))
        {
            Name = RainfallRunoffModelDataSet.TemperatureName;
        }

        /// <summary>
        /// Clones <see cref="TemperatureMeteoData"/>.
        /// </summary>
        /// <returns>
        ///     <see cref="TemperatureMeteoData"/>
        /// </returns>
        public override object Clone()
        {
            var clone = new TemperatureMeteoData();

            return base.Clone(clone);
        }
    }
}