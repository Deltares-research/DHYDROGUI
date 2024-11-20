using System;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class MeasurementLocationIdGenerator
    {
        public static string GetMeasurementLocationId(string branchId, double chainage)
        {
            return branchId + "_" + Math.Round(chainage, 0);
        }
    }
}