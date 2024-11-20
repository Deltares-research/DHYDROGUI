namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo
{
    /// <summary>
    /// Interface for evaporation meteo data.
    /// </summary>
    public interface IEvaporationMeteoData : IMeteoData
    {
        /// <summary>
        /// Selected meteo data source.
        /// </summary>
        MeteoDataSource SelectedMeteoDataSource { get; set; }
    }
}