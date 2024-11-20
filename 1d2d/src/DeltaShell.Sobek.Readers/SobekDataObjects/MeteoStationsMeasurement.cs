using System;
using System.Collections.Generic;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class MeteoStationsMeasurement
    {
        public DateTime TimeOfMeasurement { get; set; }
        public IList<double> MeasuredValues { get; set; } 
    }
}