using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using DelftTools.Utils;
using Deltares.Infrastructure.IO.Ini;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRRIniSettingsReader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRRIniSettingsReader));

        private string path;
        private IniData iniData;
        private SobekRRIniSettings settings;

        public SobekRRIniSettings GetSobekRRIniSettings(string argPath)
        {
            path = argPath;

            using (CultureUtils.SwitchToInvariantCulture())
            using (FileStream stream = File.OpenRead(path))
            {
                settings = new SobekRRIniSettings { PeriodFromEvent = true };
                
                var iniParser = new IniParser { Configuration = { AllowPropertyKeysWithSpaces = true } };
                iniData = iniParser.Parse(stream);

                GetGeneralSettings();
                GetOutputSettings();

                return settings;
            }
        }

        private void GetGeneralSettings()
        {
            IniSection outputOptions = iniData.FindSection("OutputOptions");
            settings.OutputTimestepMultiplier = outputOptions?.GetPropertyValue("OutputAtTimestep", 1.0d) ?? 1.0d;

            IniSection optionSettings = iniData.FindSection("Options");
            if (optionSettings != null)
            {
                settings.UnsaturatedZone = optionSettings.GetPropertyValue<int>("UnsaturatedZone");
                settings.GreenhouseYear = optionSettings.GetPropertyValue<short>("GreenhouseYear");
                settings.InitCapsimOption = optionSettings.GetPropertyValue<int>("InitCapsimOption");

                if (optionSettings.ContainsProperty("CapsimPerCropArea"))
                {
                    settings.CapsimPerCropArea = optionSettings.GetPropertyValue<int>("CapsimPerCropArea");
                    settings.CapsimPerCropAreaIsDefined = true;
                }
            }

            IniSection timeSettings = iniData.FindSection("TimeSettings");
            if (timeSettings != null)
            {
                settings.PeriodFromEvent = timeSettings.GetPropertyValue<bool>("PeriodFromEvent");

                string startTimeStr = timeSettings.GetPropertyValue("StartTime");
                string endTimeStr = timeSettings.GetPropertyValue("EndTime");
                string timeStepStr = timeSettings.GetPropertyValue("TimestepSize");

                if (!string.IsNullOrEmpty(startTimeStr))
                {
                    if (TryConvertToDateTime(startTimeStr, out DateTime startTime))
                    {
                        settings.StartTime = startTime;
                    }
                    else
                    {
                        log.ErrorFormat("Parsing RR StartTime from {0} failed.", path);
                    }
                }

                if (!string.IsNullOrEmpty(endTimeStr))
                {
                    if (TryConvertToDateTime(endTimeStr, out DateTime endTime))
                    {
                        settings.EndTime = endTime;
                    }
                    else
                    {
                        log.ErrorFormat("Parsing RR EndTime from {0} failed.", path);
                    }
                }

                if (!string.IsNullOrEmpty(timeStepStr))
                {
                    if (int.TryParse(timeStepStr, out int timestep))
                    {
                        settings.TimestepSize = new TimeSpan(0, 0, timestep);
                    }
                    else
                    {
                        log.ErrorFormat("Parsing RR TimestepSize from {0} failed.", path);
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
                string[] parts = dateTimeStr.Replace('\'', ' ').Split(';');
                string dateStr = parts[0];
                string timeStr = parts[1];

                if (!DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                {
                    error = true;
                }

                if (!DateTime.TryParse(timeStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime time))
                {
                    error = true;
                }

                dateTime = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
            }
            catch (Exception)
            {
                error = true;
            }

            return !error;
        }

        private void GetOutputSettings()
        {
            IniSection outputOptions = iniData.FindSection("OutputOptions");
            if (outputOptions == null)
            {
                return;
            }
            
            settings.AggregationOptions = outputOptions.GetPropertyValue<int>("OutputAtTimestepOption");

            var actionDict = new Dictionary<string, Action<bool>>
            {
                { "OutputRRPaved", (bool b) => settings.OutputRRPaved = b },
                { "OutputRRUnpaved", (bool b) => settings.OutputRRUnpaved = b },
                { "OutputRRGreenhouse", (bool b) => settings.OutputRRGreenhouse = b },
                { "OutputRROpenWater", (bool b) => settings.OutputRROpenWater = b },
                { "OutputRRStructure", (bool b) => settings.OutputRRStructure = b },
                { "OutputRRBoundary", (bool b) => settings.OutputRRBoundary = b },
                { "OutputRRWWTP", (bool b) => settings.OutputRRWWTP = b },
                { "OutputRRNWRW", (bool b) => settings.OutputRRNWRW = b },
                { "OutputRRIndustry", (bool b) => settings.OutputRRIndustry = b },
                { "OutputRRSacramento", (bool b) => settings.OutputRRSacramento = b },
                { "OutputRRRunoff", (bool b) => settings.OutputRRRunoff = b },
                { "OutputRRLinkFlows", (bool b) => settings.OutputRRLinkFlows = b },
                { "OutputRRBalance", (bool b) => settings.OutputRRBalance = b }
            };

            foreach (KeyValuePair<string, Action<bool>> kvp in actionDict)
            {
                var propertyValue = outputOptions.GetPropertyValue<bool>(kvp.Key);
                
                kvp.Value(propertyValue);
            }
        }
    }
}