using System;
using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Files.Evaporation
{
    /// <summary>
    /// Represents a guideline sewer systems evaporation file (EVAPOR.PLV).
    /// </summary>
    public sealed class GuidelineSewerSystemsEvaporationFile : EvaporationFile
    {
        public override IReadOnlyCollection<string> Header =>
            new List<string>
            {
                "Verdampingsfile",
                "Meteo data: Evaporation stations; for each station: evaporation intensity in mm",
                "First record: start date, data in mm/day",
                "Datum (year month day), verdamping (mm/dag) voor elk weerstation",
                "jaar maand dag verdamping[mm]"
            };

        protected override void SetEvaporationValues(DateTime date, double[] evaporationValues)
        {
            SortedEvaporations[new EvaporationDate(0, date.Month, date.Day)] = evaporationValues;
        }
    }
}