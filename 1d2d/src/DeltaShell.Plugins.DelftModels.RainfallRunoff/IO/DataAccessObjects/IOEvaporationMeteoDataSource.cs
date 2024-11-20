namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.DataAccessObjects
{
    /// <summary>
    /// The meteo data source type for evaporation.
    /// </summary>
    public enum IOEvaporationMeteoDataSource
    {
        /// <summary>
        /// User defined data from a *.evp file.
        /// </summary>
        UserDefined,

        /// <summary>
        /// Long term average data from an EVAPOR.GEM file.
        /// </summary>
        LongTermAverage,

        /// <summary>
        /// Guideline sewer systems data from an EVAPOR.PLV file.
        /// </summary>
        GuidelineSewerSystems
    }
}