using System;
using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.Plugins.ImportExport.SobekNetwork.Importers
{
    public static class SobekMeteoDataImporterHelper
    {
        public static void ReadTimersFromMeteo(IList<DateTime> times, DateTime simulationStartTime, DateTime simulationStopTime, out DateTime startTime, out DateTime stopTime)
        {
            if (times.Count >= 2)
            {
                startTime = times[0];
                stopTime = times.Last();
            }
            else
            {
                startTime = simulationStartTime;
                stopTime = simulationStopTime;
            }
        }
    }
}
