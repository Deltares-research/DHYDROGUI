using System;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
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
        /// <returns>
        /// The time series.
        /// </returns>
        public TimeSeries CreateGlobalTimeSeries()
        {
            var timeSeries = new TimeSeries
            {
                Components =
                {
                    new Variable<double>(Resources.MeteoDataDistributionType_Global)
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