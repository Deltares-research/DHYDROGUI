using System;
using System.IO;
using DelftTools.Functions;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo
{
    /// <summary>
    /// Interface to get correct meteo data time series based on active meteo data source.
    /// </summary>
    public interface IMeteoDataSourceSelector
    {
        /// <summary>
        /// Get the function for the specified meteo data source.
        /// </summary>
        /// <param name="meteoDataSource"> The meteo data source. </param>
        /// <param name="modelDirectory"> The model data directory. </param>
        /// <returns>
        /// The function with meteo data.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="modelDirectory"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">
        /// Thrown when <paramref name="meteoDataSource"/> is not a defined <see cref="MeteoDataSource"/>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="meteoDataSource"/> is not a supported value.
        /// </exception>
        IFunction GetMeteoTimeSeries(MeteoDataSource meteoDataSource, DirectoryInfo modelDirectory);
    }
}