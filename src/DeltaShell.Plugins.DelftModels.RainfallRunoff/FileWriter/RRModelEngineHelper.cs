using System;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter
{
    public static class RRModelEngineHelper
    {
        public static int DateToInt(DateTime date)
        {
            return 10000 * date.Year + 100 * date.Month + date.Day;
        }

        public static int TimeToInt(DateTime time)
        {
            return 10000 * time.Hour + 100 * time.Minute + time.Second;
        }
    }
}