using System;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Properties;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo
{
    /// <summary>
    /// Instance creator for meteo time series objects.
    /// </summary>
    public class MeteoTimeSeriesInstanceCreator
    {
        /// <summary>
        /// Creates a globally defined time series for meteo data.
        /// </summary>
        /// <param name="unit">Unit of the time series.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="unit"/> is <c>null</c>.
        /// </exception>
        /// <returns>
        /// The time series.
        /// </returns>
        public TimeSeries CreateGlobalTimeSeries(IUnit unit)
        {
            Ensure.NotNull(unit, nameof(unit));
            var timeSeries = new TimeSeries
            {
                Components =
                {
                    new Variable<double>(Resources.MeteoDataDistributionType_Global, unit)
                    {
                        DefaultValue = 0.0
                    }
                },
                Name = Resources.MeteoDataDistributionType_Global,
                Time = 
                {
                    DefaultValue = new DateTime(2000, 1, 1),
                    InterpolationType = InterpolationType.Constant,
                    AllowSetInterpolationType = false,
                    ExtrapolationType = ExtrapolationType.None
                }
            };

            return timeSeries;
        }
    }
}