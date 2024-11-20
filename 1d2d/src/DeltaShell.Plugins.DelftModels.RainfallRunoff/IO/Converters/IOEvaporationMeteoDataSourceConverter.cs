using System;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.DataAccessObjects;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Converters
{
    /// <summary>
    /// Converter between an <see cref="IOEvaporationMeteoDataSource"/> and a <see cref="MeteoDataSource"/>.
    /// </summary>
    public sealed class IOEvaporationMeteoDataSourceConverter
    {
        /// <summary>
        /// Converts the provided <paramref name="ioEvaporationMeteoDataSource"/> to a <see cref="MeteoDataSource"/>.
        /// </summary>
        /// <param name="ioEvaporationMeteoDataSource"> The IO evaporation meteo data source type. </param>
        /// <returns>
        /// A <see cref="MeteoDataSource"/>.
        /// </returns>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">
        /// Thrown when <paramref name="ioEvaporationMeteoDataSource"/> is not a defined <see cref="IOEvaporationMeteoDataSource"/>
        /// .
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="ioEvaporationMeteoDataSource"/> is not a supported value.
        /// </exception>
        public MeteoDataSource FromIOMeteoDataSource(IOEvaporationMeteoDataSource ioEvaporationMeteoDataSource)
        {
            Ensure.IsDefined(ioEvaporationMeteoDataSource, nameof(ioEvaporationMeteoDataSource));

            switch (ioEvaporationMeteoDataSource)
            {
                case IOEvaporationMeteoDataSource.UserDefined:
                    return MeteoDataSource.UserDefined;
                case IOEvaporationMeteoDataSource.LongTermAverage:
                    return MeteoDataSource.LongTermAverage;
                case IOEvaporationMeteoDataSource.GuidelineSewerSystems:
                    return MeteoDataSource.GuidelineSewerSystems;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ioEvaporationMeteoDataSource), ioEvaporationMeteoDataSource, null);
            }
        }

        /// <summary>
        /// Converts the provided <paramref name="evaporationMeteoDataSource"/> to an <see cref="IOEvaporationMeteoDataSource"/>.
        /// </summary>
        /// <param name="evaporationMeteoDataSource"> The evaporation meteo data source type. </param>
        /// <returns>
        /// An <see cref="IOEvaporationMeteoDataSource"/>.
        /// </returns>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">
        /// Thrown when <paramref name="evaporationMeteoDataSource"/> is not a defined <see cref="MeteoDataSource"/>
        /// .
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="evaporationMeteoDataSource"/> is not a supported value.
        /// </exception>
        public IOEvaporationMeteoDataSource ToIOMeteoDataSource(MeteoDataSource evaporationMeteoDataSource)
        {
            Ensure.IsDefined(evaporationMeteoDataSource, nameof(evaporationMeteoDataSource));

            switch (evaporationMeteoDataSource)
            {
                case MeteoDataSource.UserDefined:
                    return IOEvaporationMeteoDataSource.UserDefined;
                case MeteoDataSource.LongTermAverage:
                    return IOEvaporationMeteoDataSource.LongTermAverage;
                case MeteoDataSource.GuidelineSewerSystems:
                    return IOEvaporationMeteoDataSource.GuidelineSewerSystems;
                default:
                    throw new ArgumentOutOfRangeException(nameof(evaporationMeteoDataSource), evaporationMeteoDataSource, null);
            }
        }
    }
}