using System;
using DelftTools.Functions.Generic;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo
{
    public interface IMeteoTimeAggregator
    {
        double[] GetTimeSeriesForPeriod(Variable<double> valueVariable, Variable<DateTime> timeVariable, DateTime startDate,
                                        DateTime endDate, TimeSpan timeStep);
    }
}
