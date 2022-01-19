using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers.fnm;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
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
        public override string DisplayName => displayName;

        public override SobekImporterCategories Category => SobekImporterCategories.RainfallRunoff;

        protected override void PartialImport()
        {
            rainfallRunoffModel = GetModel<RainfallRunoffModel>();
            precipitation = rainfallRunoffModel.Precipitation;
            evaporation = rainfallRunoffModel.Evaporation;
            temperature = rainfallRunoffModel.Temperature;


            log.DebugFormat("Importing meteo data...");

            if (!CaseData.IsEmpty)
            {
                filePathPrecipitation = CaseData.PrecipitationFile?.FullName;

                string rksFile = CaseData.RksFile?.FullName;
                if (rksFile != null)
                {
                    filePathPrecipitation = rksFile;
                    log.WarnFormat(".rks files are not supported. The first event will be imported as precipitation data.");
                }

                filePathEvaporation = CaseData.EvaporationFile?.FullName;
                filePathTemperature = CaseData.TemperatureFile?.FullName;
                
                log.DebugFormat("Importing precipitation data...");
                ReadAndSetPrecipitation();
                
                SetModelTimesBasedOnEvent(rainfallRunoffModel, GetFilePath(SobekFileNames.SobekRRIniFileName));

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
                FnmData fnmData = ReadFnmData();

                string buiFilePath = GetFilePath(fnmData.BuiFile);
                if (File.Exists(buiFilePath))
                {
                    filePathPrecipitation = buiFilePath;
                    log.DebugFormat("Importing precipitation data...");
                    ReadAndSetPrecipitation();
                }
                
                SetModelTimesBasedOnEvent(rainfallRunoffModel, GetFilePath(SobekFileNames.SobekRRIniFileName));

                string evpFilePath = GetFilePath(fnmData.VerdampingsFile);
                if (File.Exists(evpFilePath))
                {
                    filePathEvaporation = evpFilePath;
                    log.DebugFormat("Importing precipitation data...");
                    ReadAndSetEvaporation();
                }

                string tmpFilePath = GetFilePath(fnmData.TimeSeriesTemperature);
                if (rainfallRunoffModel.GetAllModelData().OfType<HbvData>().Any() && 
                    File.Exists(tmpFilePath))
                {
                    filePathTemperature = tmpFilePath;
                    log.DebugFormat("Importing precipitation data...");
                    ReadAndSetTemperature();
                }
            }
        }

        private FnmData ReadFnmData()
        {
            using (FileStream fileStream = File.OpenRead(PathSobek)) 
            using (var reader = new StreamReader(fileStream)) 
            { 
                return FnmDataParser.Parse(reader);
            }
        }

        private static void SetModelTimesBasedOnEvent(RainfallRunoffModel model, string path)
        {
            if (!File.Exists(path))
            {
                log.ErrorFormat("Could not find ini file {0}.", path);
                return;
            }

            SobekRRIniSettings settings = new SobekRRIniSettingsReader().GetSobekRRIniSettings(path);

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

            if (!File.Exists(filePathEvaporation))
            {
                log.Warn($"Evaporation file does not exist: {filePathEvaporation}");
                return;
            }

            SobekRREvaporation sobekRREvaporation;
            using (var stream = new FileStream(filePathEvaporation, FileMode.Open))
            {
                sobekRREvaporation = SobekRREvaporationReader.Read(stream);
            }

            if (sobekRREvaporation.IsLongTimeAverage)
            {
                sobekRREvaporation.ToLongTimeAverage(model.StartTime, model.StopTime);
            }

            var precipitationIsDefinedPerStation = precipitation.DataDistributionType ==
                                                   MeteoDataDistributionType.PerStation;

            if (sobekRREvaporation.NumberOfLocations == 1)
            {
                if (precipitationIsDefinedPerStation)
                {
                    SetEvaporationPerStation(sobekRREvaporation);
                }
                else
                {
                    SetGlobalEvaporation(sobekRREvaporation);
                }
            }
            else
            {
                if (!precipitationIsDefinedPerStation)
                {
                    log.Error("Evaporation has multiple stations while precipitation has not! Not supported");
                    SetGlobalEvaporation(sobekRREvaporation);
                }
                else if (sobekRREvaporation.NumberOfLocations != rainfallRunoffModel.MeteoStations.Count)
                {
                    log.Error("Number of evaporation stations does not match the number of precipitation stations! Not supported");
                    SetGlobalEvaporation(sobekRREvaporation);
                }
                else
                {
                    SetEvaporationPerStation(sobekRREvaporation);
                }
            }
            
            if (sobekRREvaporation.IsPeriodic)
            {
                evaporation.Data.Arguments[0].ExtrapolationType = ExtrapolationType.Periodic;
            }
        }

        private void SetEvaporationPerStation(SobekRREvaporation sobekRREvaporation)
        {
            evaporation.Data.Arguments[0].Clear();
            evaporation.Data.Arguments[0].SetValues(sobekRREvaporation.Dates);

            IVariable stationArgument = evaporation.Data.Arguments[1];
            for (var i = 0; i < sobekRREvaporation.NumberOfLocations; i++)
            {
                string stationName = rainfallRunoffModel.MeteoStations[i];
                IEnumerable<double> evaporationValues = sobekRREvaporation.GetValuesByStationIndex(i);

                evaporation.Data.SetValues(evaporationValues,
                                           new VariableValueFilter<string>(stationArgument, stationName));
            }
        }
        
        private void SetGlobalEvaporation(SobekRREvaporation sobekRREvaporation)
        {
            // just do it global
            evaporation.DataDistributionType = MeteoDataDistributionType.Global;
            evaporation.Data.Arguments[0].Clear();
            evaporation.Data.Arguments[0].SetValues(sobekRREvaporation.Dates);
            evaporation.Data.SetValues(sobekRREvaporation.GetValuesByStationIndex(0));
        }
    }
}
