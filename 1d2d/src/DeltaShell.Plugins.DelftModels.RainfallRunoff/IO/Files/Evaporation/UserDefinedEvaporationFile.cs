using System;
using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Files.Evaporation
{
    /// <summary>
    /// Represents a user defined evaporation file (default.evp).
    /// </summary>
    public sealed class UserDefinedEvaporationFile : EvaporationFile
    {
        public override IReadOnlyCollection<string> Header =>
            new List<string>
            {
                "Verdampingsfile",
                "Meteo data: evaporation intensity in mm/day",
                "First record: start date, data in mm/day",
                "Datum (year month day), verdamping (mm/dag) voor elk weerstation",
                "jaar maand dag verdamping[mm]"
            };

        protected override void SetEvaporationValues(DateTime date, double[] evaporationValues)
        {
            SortedEvaporations[new EvaporationDate(date.Year, date.Month, date.Day)] = evaporationValues;
        }
    }
}