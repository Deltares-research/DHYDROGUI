using System;
using System.Collections.Generic;
using log4net;
using Nini.Config;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRRIniSettingsReader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRRIniSettingsReader));

        private string path; 
        private SobekRRIniSettings settings;
        private IniConfigSource iniReader;

        public SobekRRIniSettings GetSobekRRIniSettings(string argPath)
        {
            path = argPath; 
            settings = new SobekRRIniSettings();
            settings.PeriodFromEvent = true;

            iniReader = new IniConfigSource(path);
            iniReader.Alias.AddAlias("-1", true);
            iniReader.Alias.AddAlias("0", false);
            iniReader.Alias.AddAlias("1", true);

            getGeneralSettings();
            getOutputSettings(); 
            return settings;
        }

        private void getGeneralSettings ()
        {
            settings.OutputTimestepMultiplier = 1;

            var outputOptions = iniReader.Configs["OutputOptions"];
            if (outputOptions != null)
            {
                var multiplier = outputOptions.GetDouble("OutputAtTimestep");
                settings.OutputTimestepMultiplier = multiplier;
            }

            var optionSettings = iniReader.Configs["Options"];
            if (optionSettings != null)
            {
                if (optionSettings.Contains("UnsaturatedZone"))
                {
                    var unsaturatedZone = optionSettings.GetInt("UnsaturatedZone");
                    settings.UnsaturatedZone = unsaturatedZone;
                }

                if (optionSettings.Contains("InitCapsimOption"))
                {
                    var initCapsimOption = optionSettings.GetInt("InitCapsimOption");
                    settings.InitCapsimOption = initCapsimOption;
                }

                if (optionSettings.Contains("CapsimPerCropArea"))
                {
                    var capsimPerCropArea = optionSettings.GetInt("CapsimPerCropArea");
                    settings.CapsimPerCropArea = capsimPerCropArea;
                    settings.CapsimPerCropAreaIsDefined = true;
                }
            }

            var timeSettings = iniReader.Configs["TimeSettings"];
            if (timeSettings != null)
            {
                if (timeSettings.Contains("PeriodFromEvent"))
                {
                    var periodFromEvent = timeSettings.GetBoolean("PeriodFromEvent");
                    settings.PeriodFromEvent = periodFromEvent;
                }
                if (timeSettings.Contains("StartTime"))
                {
                    DateTime startTime;
                    var startTimeStr = timeSettings.GetString("StartTime");
                    if (!TryConvertToDateTime(startTimeStr, out startTime))
                    {
                        log.ErrorFormat("Parsing RR StartTime from {0} failed.", path);
                    }
                    else
                    {
                        settings.StartTime = startTime;
                    }
                }
                if (timeSettings.Contains("EndTime"))
                {
                    DateTime endTime;
                    var endTimeStr = timeSettings.GetString("EndTime");
                    if (!TryConvertToDateTime(endTimeStr, out endTime))
                    {
                        log.ErrorFormat("Parsing RR EndTime from {0} failed.", path);
                    }
                    else
                    {
                        settings.EndTime = endTime;
                    }
                }
                if (timeSettings.Contains("TimestepSize"))
                {
                    int timestep;
                    var timeStepStr = timeSettings.GetString("TimestepSize");
                    if (!Int32.TryParse(timeStepStr, out timestep))
                    {
                        log.ErrorFormat("Parsing RR TimestepSize from {0} failed.", path);
                    }
                    else
                    {
                        settings.TimestepSize = new TimeSpan(0, 0, timestep);
                    }
                }
            }
            else
            {
                log.ErrorFormat("TimeSettings configuration not found in {0}", path);
            }
        }

        private static bool TryConvertToDateTime(string dateTimeStr, out DateTime dateTime)
        {
            dateTime = new DateTime();
            var error = false;
            try
            {
                var parts = dateTimeStr.Split(new[] { ';' });
                var dateStr = parts[0];
                var timeStr = parts[1];

                DateTime date, time;

                if (!DateTime.TryParse(dateStr, out date))
                {
                    error = true;
                }
                if (!DateTime.TryParse(timeStr, out time))
                {
                    error = true;
                }
                dateTime = new DateTime(date.Year,date.Month,date.Day, time.Hour,time.Minute, time.Second);
            }
            catch (Exception)
            {
                error = true;
            }
            return !error;
        }

        private void getOutputSettings()
        {
            IConfig outputOptions = iniReader.Configs["OutputOptions"];
            if (outputOptions != null)
            {
                int aggregationOptions = outputOptions.GetInt("OutputAtTimestepOption");
                settings.AggregationOptions = aggregationOptions;
            }

            var actionDict = new Dictionary<string, Action<bool>>()
                {
                    {"OutputRRPaved", (bool b) => settings.OutputRRPaved = b},
                    {"OutputRRUnpaved", (bool b) => settings.OutputRRUnpaved = b},
                    {"OutputRRGreenhouse", (bool b) => settings.OutputRRGreenhouse = b},
                    {"OutputRROpenWater", (bool b) => settings.OutputRROpenWater = b},
                    {"OutputRRStructure", (bool b) => settings.OutputRRStructure = b},
                    {"OutputRRBoundary", (bool b) => settings.OutputRRBoundary = b},
                    {"OutputRRWWTP", (bool b) => settings.OutputRRWWTP = b},
                    {"OutputRRNWRW", (bool b) => settings.OutputRRNWRW = b},
                    {"OutputRRIndustry", (bool b) => settings.OutputRRIndustry = b},
                    {"OutputRRSacramento", (bool b) => settings.OutputRRSacramento = b},
                    {"OutputRRRunoff", (bool b) => settings.OutputRRRunoff = b},
                    {"OutputRRLinkFlows", (bool b) => settings.OutputRRLinkFlows = b},
                    {"OutputRRBalance", (bool b) => settings.OutputRRBalance = b}
                };
            
            foreach (var kvp in actionDict) // KeyValuePair
            {
                if (outputOptions.Contains(kvp.Key))
                {
                    kvp.Value(outputOptions.GetBoolean(kvp.Key));
                }
            }
        }
    }
}
