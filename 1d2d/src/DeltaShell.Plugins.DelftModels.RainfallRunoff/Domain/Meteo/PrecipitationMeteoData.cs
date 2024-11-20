using DelftTools.Units;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo
{
    /// <summary>
    /// Precipitation Meteo Data, <seealso cref="MeteoData"/>
    /// </summary>
    public class PrecipitationMeteoData : MeteoData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrecipitationMeteoData"/> class.
        /// </summary>
        public PrecipitationMeteoData() : base(MeteoDataAggregationType.Cumulative, new Unit(RainfallRunoffModelDataSet.PrecipitationName, "mm"))
        {
            Name = RainfallRunoffModelDataSet.PrecipitationName;
        }

        /// <summary>
        /// Clones <see cref="PrecipitationMeteoData"/>.
        /// </summary>
        /// <returns>
        ///     <see cref="PrecipitationMeteoData"/>
        /// </returns>
        public override object Clone()
        {
            var clone = new PrecipitationMeteoData();

            return base.Clone(clone);
        }
    }
}