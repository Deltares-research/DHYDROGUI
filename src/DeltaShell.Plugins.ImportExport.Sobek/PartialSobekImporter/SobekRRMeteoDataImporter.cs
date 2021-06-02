using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using log4net;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public class SobekRRMeteoDataImporter : PartialSobekImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRRMeteoDataImporter));
        private string filePathPrecipitation = "";
        private string filePathEvaporation = "";
        private string filePathTemperature = "";

        private MeteoData precipitation;
        private MeteoData evaporation;
        private MeteoData temperature;
        private RainfallRunoffModel rainfallRunoffModel;

        private const string displayName = "Rainfall Runoff meteo data";
        public override string DisplayName
        {
            get { return displayName; }
        }

        public override SobekImporterCategories Category { get; } = SobekImporterCategories.RainfallRunoff;

        protected override void PartialImport()
        {
            rainfallRunoffModel = GetModel<RainfallRunoffModel>();
            precipitation = rainfallRunoffModel.Precipitation;
            evaporation = rainfallRunoffModel.Evaporation;
            temperature = rainfallRunoffModel.Temperature;

            log.DebugFormat("Importing meteo data...");

            if (SetFilePath(GetFilePath(SobekFileNames.SobekCaseDescriptionFile)))
            {
                log.DebugFormat("Importing precipitation data...");
                ReadAndSetPrecipitation();

                log.DebugFormat("Importing evaporation data...");
                ReadAndSetEvaporation();

                if (rainfallRunoffModel.GetAllModelData().OfType<HbvData>().Any())
                {
                    log.DebugFormat("Importing temperature data...");
                    ReadAndSetTemperature();
                }
            }
            else
            {
                if (File.Exists(GetFilePath("default.bui")))
                {
                    filePathPrecipitation = GetFilePath("default.bui");
                    log.DebugFormat("Importing precipitation data...");
                    ReadAndSetPrecipitation();
                }

                if (File.Exists(GetFilePath("default.evp")))
                {
                    filePathEvaporation = GetFilePath("default.evp");
                    log.DebugFormat("Importing precipitation data...");
                    ReadAndSetEvaporation();
                }

                if (rainfallRunoffModel.GetAllModelData().OfType<HbvData>().Any())
                {
                    if (File.Exists(GetFilePath("default.tmp")))
                    {
                        filePathTemperature = GetFilePath("default.tmp");
                        log.DebugFormat("Importing precipitation data...");
                        ReadAndSetTemperature();
                    }
                }
            }

            SetModelTimesBasedOnEvent(rainfallRunoffModel, GetFilePath(SobekFileNames.SobekRRIniFileName));
        }

        private void SetModelTimesBasedOnEvent(RainfallRunoffModel model, string path)
        {
            if (!File.Exists(path))
            {
                log.ErrorFormat("Could not find ini file {0}.", path);
                return;
            }

            var settings = new SobekRRIniSettingsReader().GetSobekRRIniSettings(path);

            if (settings.PeriodFromEvent)
            {
                DateTime startTime;
                DateTime stopTime;
                SobekMeteoDataImporterHelper.ReadTimersFromMeteo(
                    model.Precipitation.Data.Arguments[0].GetValues<DateTime>(), settings.StartTime, settings.EndTime,
                    out startTime, out stopTime);
                model.StartTime = startTime;
                model.StopTime = stopTime;

                model.SaveStateStartTime = model.StartTime;
                model.SaveStateStopTime = model.StopTime;
            }
        }

        private void ReadAndSetTemperature()
        {
            var buiFileReader = new SobekRRBuiFileReader();
            if (!buiFileReader.ReadBuiHeaderData(filePathTemperature))
            {
                log.Error("Temperature data import failed, could not read header data from TMP file.");
                return;
            }

            var measurements = buiFileReader.ReadMeasurementData(filePathTemperature);

            try
            {
                if (buiFileReader.StationNames.Count > 1)
                {
                    rainfallRunoffModel.TemperatureStations.AddRange(buiFileReader.StationNames);
                    temperature.DataDistributionType = MeteoDataDistributionType.PerStation;
                    temperature.Data.Arguments[0].SetValues(buiFileReader.MeasurementTimes);
                    if (temperature.Data.Arguments[1].Values.Count == 0) // this can happen during load not during sobek2 import. During load we don't have eventing enabled to speed up the loading.
                        temperature.Data.Arguments[1].SetValues(buiFileReader.StationNames);

                    foreach (var measurement in measurements)
                    {
                        temperature.Data.SetValues(measurement.MeasuredValues,
                                                     new VariableValueFilter<DateTime>(temperature.Data.Arguments[0],
                                                                                       measurement.TimeOfMeasurement));
                    }
                }
                else if (buiFileReader.StationNames.Count == 1)
                {
                    temperature.DataDistributionType = MeteoDataDistributionType.Global;
                    temperature.Data.Arguments[0].SetValues(buiFileReader.MeasurementTimes);
                    foreach (var measurement in measurements)
                    {
                        temperature.Data.SetValues(measurement.MeasuredValues,
                                                     new VariableValueFilter<DateTime>(temperature.Data.Arguments[0],
                                                                                       measurement.TimeOfMeasurement));
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("precipitation import failed", e);
            }

        }

        private void ReadAndSetPrecipitation()
        {
            var buiFileReader = new SobekRRBuiFileReader();

            if (!buiFileReader.ReadBuiHeaderData(filePathPrecipitation))
            {
                log.Error("Precipitation import failed, could not read header data from BUI file.");
                return;
            }

            var measurements = buiFileReader.ReadMeasurementData(filePathPrecipitation);

            try
            {
                if (buiFileReader.StationNames.Count > 1)
                {
                    rainfallRunoffModel.MeteoStations.AddRange(buiFileReader.StationNames);
                    precipitation.DataDistributionType = MeteoDataDistributionType.PerStation; //via eventing fills the second argument with station names
                    precipitation.Data.Arguments[0].SetValues(buiFileReader.MeasurementTimes);
                    if(precipitation.Data.Arguments[1].Values.Count == 0) // this can happen during load not during sobek2 import. During load we don't have eventing enabled to speed up the loading.
                        precipitation.Data.Arguments[1].SetValues(buiFileReader.StationNames);

                    foreach (var measurement in measurements)
                    {
                        precipitation.Data.SetValues(measurement.MeasuredValues,
                                                     new VariableValueFilter<DateTime>(precipitation.Data.Arguments[0],
                                                                                       measurement.TimeOfMeasurement));
                    }
                }
                else if (buiFileReader.StationNames.Count == 1)
                {
                    precipitation.DataDistributionType = MeteoDataDistributionType.Global;
                    precipitation.Data.Arguments[0].SetValues(buiFileReader.MeasurementTimes);
                    foreach (var measurement in measurements)
                    {
                        precipitation.Data.SetValues(measurement.MeasuredValues,
                                                     new VariableValueFilter<DateTime>(precipitation.Data.Arguments[0],
                                                                                       measurement.TimeOfMeasurement));
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("precipitation import failed", e);
            }
        }

        private void ReadAndSetEvaporation()
        {
            var model = GetModel<RainfallRunoffModel>();
            var hydroModel = TryGetModel<DelftModels.HydroModel.HydroModel>();

            bool isPeriodic;
            using (var evaporationTable = hydroModel != null ? SobekRREvaporationReader.ReadEvaporationData(filePathEvaporation, hydroModel.StartTime, hydroModel.StopTime).FirstOrDefault() : SobekRREvaporationReader.ReadEvaporationData(filePathEvaporation, model.StartTime, model.StopTime).FirstOrDefault())
            {
                if (evaporationTable == null)
                {
                    log.ErrorFormat("Error importing evaporation data.");
                    return;
                }

                var precipitationIsDefinedPerStation = precipitation.DataDistributionType ==
                                                       MeteoDataDistributionType.PerStation;

                var numStations = evaporationTable.Columns.Count - 3; //first 3 rows are the date
                var evapDict = ParseEvaporationTableToDictionary(evaporationTable, out isPeriodic);

                if (numStations == 1) //one station, typical
                {
                    if (!precipitationIsDefinedPerStation)
                    {
                        SetGlobalEvaporation(evapDict.Keys, evapDict.Values);
                    }
                    else
                    {
                        evaporation.Data.Arguments[0].Clear();
                        evaporation.Data.Arguments[0].SetValues(evapDict.Keys);

                        // stations already filled in (shared with precipitation)
                        foreach (var stationName in rainfallRunoffModel.MeteoStations)
                        {
                            evaporation.Data.SetValues(evapDict.Values, new VariableValueFilter<string>(
                                evaporation.Data.Arguments[1], stationName));
                        }
                    }
                }
                else //multiple stations
                {
                    if (!precipitationIsDefinedPerStation)
                    {
                        log.Error("Evaporation has multiple stations while precipitation has not! Not supported");
                        SetGlobalEvaporation(evapDict.Keys, evapDict.Values);
                    } 
                    else if (numStations != rainfallRunoffModel.MeteoStations.Count)
                    {
                        log.Error("Number of evaporation stations does not match the number of precipitation stations! Not supported");
                        SetGlobalEvaporation(evapDict.Keys, evapDict.Values);
                    }
                    else
                    {
                        evaporation.Data.Arguments[0].SetValues(evapDict.Keys);

                        // stations already filled in (shared with precipitation)
                        var stationIndex = 0;
                        foreach (var stationName in rainfallRunoffModel.MeteoStations)
                        {
                            var values = evaporationTable.Rows.OfType<DataRow>()
                                .Select(row => Convert.ToDouble(row[3 + stationIndex]))
                                .ToList();

                            evaporation.Data.SetValues(values, new VariableValueFilter<string>(
                                evaporation.Data.Arguments[1], stationName));
                            stationIndex++;
                        }
                    }
                }
            }

            if (isPeriodic)
            {
                evaporation.Data.Arguments[0].ExtrapolationType = ExtrapolationType.Periodic;
            }
        }

        private static IDictionary<DateTime, double> ParseEvaporationTableToDictionary(DataTable dataTable,
                                                                                       out bool periodic)
        {
            periodic = false;
            var result = new Dictionary<DateTime, double>();
            foreach (DataRow row in dataTable.Rows)
            {
                DateTime dateTime;
                if (SobekRREvaporationReader.TryReadDateTime(row, out dateTime, out periodic))
                {
                    result[dateTime] = Convert.ToDouble(row[3]);
                }
            }
            return result;
        }

        private void SetGlobalEvaporation(IEnumerable<DateTime> times, IEnumerable<double> values)
        {
            // just do it global
            evaporation.DataDistributionType = MeteoDataDistributionType.Global;
            evaporation.Data.Arguments[0].Clear();
            evaporation.Data.Arguments[0].SetValues(times);
            evaporation.Data.SetValues(values);
        }

        private bool SetFilePath(string caseDescriptionFile)
        {
            if (!File.Exists(caseDescriptionFile))
            {
                return false;
            }

            string caseDescriptionFileText = File.ReadAllText(caseDescriptionFile, Encoding.Default);

            const string group = "filepath";
            const string buiPattern = @"I\s*(?<" + group + ">" + RegularExpression.FileName + @"\.BUI)\s*";
            const string rksPattern = @"I\s*(?<" + group + ">" + RegularExpression.FileName + @"\.RKS)\s*";
            const string evapPattern = @"I\s*(?<" + group + ">" + RegularExpression.FileName + @"\.(EVP|GEM|PLV))\s*";
            const string evaporPattern = @"I\s*(?<" + group + ">" + RegularExpression.FileName + @"EVAPOR)\s*";
            const string tmpPattern = @"I\s*(?<" + group + ">" + RegularExpression.FileName + @"\.TMP)\s*";
            
            //Precipitation
            var matches = RegularExpression.GetMatches(buiPattern, caseDescriptionFileText);
            if (matches.Count > 0)
            {
                filePathPrecipitation = GetFilePath(ResolveFixedPaths(matches[0].Groups[group].Value));
            }

            matches = RegularExpression.GetMatches(rksPattern, caseDescriptionFileText);
            if (matches.Count > 0)
            {
                filePathPrecipitation = GetFilePath(ResolveFixedPaths(matches[0].Groups[group].Value));
                log.WarnFormat(".rks files are not supported. The first event will be imported as precipitation data.");
            }

            //Evaporation           
            matches = RegularExpression.GetMatches(evapPattern, caseDescriptionFileText);
            if (matches.Count > 0)
            {
                filePathEvaporation = GetFilePath(ResolveFixedPaths(matches[0].Groups[group].Value));
            }
            else
            {
                matches = RegularExpression.GetMatches(evaporPattern, caseDescriptionFileText);
                if (matches.Count > 0)
                {
                    filePathEvaporation = GetFilePath(ResolveFixedPaths(matches[0].Groups[group].Value));
                }
            }

            //Temperatures
            matches = RegularExpression.GetMatches(tmpPattern, caseDescriptionFileText);
            if (matches.Count > 0)
            {
                filePathTemperature = GetFilePath(ResolveFixedPaths(matches[0].Groups[group].Value));
            }

            return true;
        }

        private static string ResolveFixedPaths(string inputPath)
        {
            const string find1 = @"(\\)?" + RegularExpression.AnyNonGreedy + @"\\FIXED\\";
            const string replace = @"..\..\FIXED\";

            var res = Regex.Replace(inputPath, find1, replace);
            return res;
        }

    }
}
