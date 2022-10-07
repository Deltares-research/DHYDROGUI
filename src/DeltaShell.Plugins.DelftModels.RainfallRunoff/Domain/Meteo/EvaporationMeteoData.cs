using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo
{
    /// <summary>
    /// Evaporation Meteo Data, <seealso cref="MeteoData"/>
    /// </summary>
    [Entity]
    public class EvaporationMeteoData : MeteoData, IEvaporationMeteoData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EvaporationMeteoData"/> class.
        /// </summary>
        public EvaporationMeteoData() : base(MeteoDataAggregationType.Cumulative)
        {
            Name = RainfallRunoffModelDataSet.EvaporationName;
            SelectedMeteoDataSource = MeteoDataSource.UserDefined;
        }

        /// <summary>
        /// Selected Meteo Data source for Evaporation, initial value is <see cref="MeteoDataSource.UserDefined"/>
        /// </summary>
        public MeteoDataSource SelectedMeteoDataSource { get; set; }
        
        /// <summary>
        /// Clones <see cref="EvaporationMeteoData"/>.
        /// </summary>
        /// <returns><see cref="EvaporationMeteoData"/></returns>
        public override object Clone()
        {
            var clone = new EvaporationMeteoData
            {
                SelectedMeteoDataSource = SelectedMeteoDataSource
            };
            
            return base.Clone(clone);
        }
    }
}