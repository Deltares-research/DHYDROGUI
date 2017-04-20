using System;
using System.IO;

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

        public static string QuantityTypeToString(QuantityType quantityType)
        {
            var quantityNames = new string[]
                                    {
                                        "Rainfall",
                                        "Seepage",
                                        "Pumpstop",
                                        "Openwater level",
                                        "Groundwater level",
                                        "Groundwater recharge",
                                        "Unsaturated zone content",
                                        "Storage coefficient",
                                        "Flow",
                                        "Boundary levels",
                                        "Boundary depths",
                                        "Boundary areas",
                                        "Boundary salt concentrations",
                                        "Sacr. UpperZoneTensionWaterContent",
                                        "Sacr. UpperZoneFreeWaterContent",
                                        "Sacr. LowerZoneTensionWaterContent",
                                        "Sacr. LowerZoneFreePrimaryWaterContent",
                                        "Sacr. LowerZoneFreeSupplementaryWaterContent",
                                        "Sacr. BaseFlow",
                                        "Sacr. SurfaceFlow",
                                        "Sacr. Total Runoff"
                                    };
            return quantityNames[((int)quantityType - 1)];
        }

        public static string ElementSetToString(ElementSet elSet)
        {
            switch (elSet)
            {
                case ElementSet.RainfallElmSet:
                    return "Rainfall stations";
                case ElementSet.UnpavedElmSet:
                    return "RR-Unpaved nodes";
                case ElementSet.OpenWaterElmSet:
                    return "RR-Openwater Nodes";
                case ElementSet.StructureElmSet:
                    return "RR-Structures";
                case ElementSet.BoundaryElmSet:
                    return "RR-Boundaries";
                case ElementSet.NWRWElmSet:
                    return "NWRW nodes";
                case ElementSet.SacramentoElmSet:
                    return "RR-Sacramento Nodes";
            }
            throw new NotSupportedException("Unknown element set");
        }

        public static string TryGetCrashReasonFromLogs()
        {
            var crashReason = "";
            const string logFile = "sobek_3b.log";

            if (File.Exists(logFile))
            {
                var logFileLog = TryReadAllText(logFile);

                //get crash reason in one line
                var errorIndex = logFileLog.IndexOf("Error:");
                if (errorIndex == -1)
                {
                    errorIndex = logFileLog.IndexOf("Error ");
                }
                if (errorIndex >= 0)
                {
                    var eol = logFileLog.IndexOf('\n', errorIndex);
                    eol = eol >= 0 ? eol : logFileLog.Length;
                    crashReason = logFileLog.Substring(errorIndex, eol - errorIndex);
                }
                //end get
            }
            return crashReason;
        }

        public static string GetSobekLogFiles()
        {
            var crashLog = "Model Crash Log: ";
            const string initFile = "Sobek_3bInit.log";
            const string logFile = "sobek_3b.log";
            const string debugFile = "sobek_3b.dbg";
            const string delimiter = "======================\n\r";
            if (File.Exists(initFile))
            {
                crashLog += delimiter + "Initialization log:\n\r" + delimiter;
                crashLog += TryReadAllText(initFile);
            }
            if (File.Exists(logFile))
            {
                crashLog += delimiter + "Run log:\n\r" + delimiter;
                var logFileLog = TryReadAllText(logFile);
                crashLog += logFileLog;
            }
            if (File.Exists(debugFile))
            {
                crashLog += delimiter + "Debug log:\n\r" + delimiter;
                crashLog += TryReadAllText(debugFile);
            }
            return crashLog;
        }

        private static string TryReadAllText(string logPath)
        {
            try
            {
                return File.ReadAllText(logPath);
            }
            catch (Exception e)
            {
                return String.Format(">> Unable to open log file: {0}", e.Message);
            }
        }
    }
}