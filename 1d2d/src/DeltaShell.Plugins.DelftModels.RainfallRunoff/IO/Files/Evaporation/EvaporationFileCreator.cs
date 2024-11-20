using System;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.DataAccessObjects;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Files.Evaporation
{
    /// <summary>
    /// Creator of <see cref="IEvaporationFile"/>.
    /// </summary>
    public sealed class EvaporationFileCreator
    {
        /// <summary>
        /// Creates an evaporation file for the specified meteo data source.
        /// </summary>
        /// <param name="ioEvaporationMeteoDataSource"> The evaporation meteo data source. </param>
        /// <returns> The evaporation file. </returns>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">
        /// Thrown when <paramref name="ioEvaporationMeteoDataSource"/> is not a defined <see cref="IOEvaporationMeteoDataSource"/>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="ioEvaporationMeteoDataSource"/> is not a supported value.
        /// </exception>
        public IEvaporationFile CreateFor(IOEvaporationMeteoDataSource ioEvaporationMeteoDataSource)
        {
            Ensure.IsDefined(ioEvaporationMeteoDataSource, nameof(ioEvaporationMeteoDataSource));

            switch (ioEvaporationMeteoDataSource)
            {
                case IOEvaporationMeteoDataSource.UserDefined:
                    return new UserDefinedEvaporationFile();
                case IOEvaporationMeteoDataSource.LongTermAverage:
                    return new LongTermAverageEvaporationFile();
                case IOEvaporationMeteoDataSource.GuidelineSewerSystems:
                    return new GuidelineSewerSystemsEvaporationFile();
                default:
                    throw new ArgumentOutOfRangeException(nameof(ioEvaporationMeteoDataSource), ioEvaporationMeteoDataSource, null);
            }
        }
    }
}