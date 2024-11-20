using System;
using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Files.Evaporation
{
    /// <summary>
    /// Represents a long term average evaporation file (EVAPOR.GEM).
    /// </summary>
    public sealed class LongTermAverageEvaporationFile : EvaporationFile
    {
        public override IReadOnlyCollection<string> Header =>
            new List<string>
            {
                "Longtime average",
                "year column is dummy, year 'value' should be fixed 0000"
            };

        protected override void SetEvaporationValues(DateTime date, double[] evaporationValues)
        {
            SortedEvaporations[new EvaporationDate(date.Month, date.Day)] = evaporationValues;
        }
    }
}