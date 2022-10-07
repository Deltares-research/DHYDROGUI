using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Extensions;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.NGHS.Utils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers.fnm;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Converters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.DataAccessObjects;
using DeltaShell.Plugins.ImportExport.Sobek.Properties;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using DHYDRO.Common.Logging;
using log4net;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public class SobekRRMeteoDataImporter : PartialSobekImporterBase
    {
        private readonly ILogHandler logHandler = new LogHandler("importing meteo data", log);
        private static readonly EvaporationFileNameConverter evaporationFileNameConverter = new EvaporationFileNameConverter();
        private static readonly IOEvaporationMeteoDataSourceConverter evaporationMeteoDataSourceConverter = new IOEvaporationMeteoDataSourceConverter();
        private static readonly SobekRREvaporationReader evaporationReader = new SobekRREvaporationReader();

        private sealed class MeteoImportData
        {
            public MeteoImportData(DateTime[] times, string[] stationNames, IDictionary<DateTime, double[]> values)
            {
                Times = times;
                StationNames = stationNames;
                Values = values;
            }

            public DateTime[] Times { get; }

            public string[] StationNames { get; }

            public IDictionary<DateTime, double[]> Values { get; }
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRRMeteoDataImporter));

        public override string DisplayName => Resources.SobekRRMeteoDataImporter_DisplayName_Rainfall_Runoff_meteo_data;

        public override SobekImporterCategories Category => SobekImporterCategories.RainfallRunoff;

        protected override void PartialImport()
        {
            log.DebugFormat("Importing meteo data...");

            (string precipitation, string evaporation, string temperature) = GetFilePaths();
            ImportMeteoData(GetModel<IRainfallRunoffModel>(), precipitation, evaporation, temperature);
        }

        private (string precipitation, string evaporation, string temperature) GetFilePaths()
        {
            if (CaseData.IsEmpty)
            {
                FnmData fnmData = ReadFnmData(PathSobek);
                return (GetFilePath(fnmData.BuiFile), GetFilePath(fnmData.VerdampingsFile), GetFilePath(fnmData.TimeSeriesTemperature));
            }

            string filePathPrecipitation = CaseData.PrecipitationFile?.FullName;
            string rksFile = CaseData.RksFile?.FullName;
            
            if (rksFile != null)
            {
                filePathPrecipitation = rksFile;
                logHandler.ReportWarningFormat(Resources.SobekRRMeteoDataImporter_GetFilePaths__rks_files_are_not_supported__The_first_event_will_be_imported_as_precipitation_data_);
            }

            return (filePathPrecipitation, CaseData.EvaporationFile?.FullName, CaseData.TemperatureFile?.FullName);
        }

        private void ImportMeteoData(IRainfallRunoffModel model, string filePathPrecipitation, string filePathEvaporation, string filePathTemperature)
        {
            var basinCatchments = model.Basin.Catchments;

            log.DebugFormat("Importing precipitation data...");
            if (GetBuiFileMeteoImportData(filePathPrecipitation, model.Precipitation, out MeteoImportData precipitationData))
            {
                AddMeteoData(model.Precipitation, precipitationData, basinCatchments, model.MeteoStations);
            }

            log.DebugFormat("Importing evaporation data...");
            if (GetSobekRREvaporationMeteoImportData(filePathEvaporation, model.Evaporation, model, out MeteoImportData evaporationData))
            {
                AddMeteoData(model.Evaporation, evaporationData, basinCatchments, model.MeteoStations);
            }

            log.DebugFormat("Importing temperature data...");
            bool fileCanBeEmpty = !basinCatchments.Any(c => Equals(c.CatchmentType, CatchmentType.Hbv));
            if (GetBuiFileMeteoImportData(filePathTemperature, model.Temperature, out MeteoImportData temperatureData, fileCanBeEmpty))
            {
                AddMeteoData(model.Temperature, temperatureData, basinCatchments, model.TemperatureStations);
            }
        }

        private static FnmData ReadFnmData(string path)
        {
            using (FileStream fileStream = File.OpenRead(path)) 
            using (var reader = new StreamReader(fileStream)) 
            { 
                return FnmDataParser.Parse(reader);
            }
        }

        private bool GetBuiFileMeteoImportData(string path, MeteoData meteoData, out MeteoImportData importData, bool fileCanBeEmpty = false)
        {
            importData = null;

            if (fileCanBeEmpty && File.Exists(path) && new FileInfo(path).Length == 0)
            {
                return false;
            }

            var buiFileReader = new SobekRRBuiFileReader();
            if (!buiFileReader.ReadBuiHeaderData(path))
            {
                logHandler.ReportError(string.Format(Resources.SobekRRMeteoDataImporter_GetBuiFileMeteoImportData__0__import_failed__could_not_read_header_data_from__1__file_, meteoData?.Name, path));
                return false;
            }
            
            var measurements = buiFileReader.ReadMeasurementData(path).ToDictionary(m => m.TimeOfMeasurement, m => m.MeasuredValues.ToArray());
            importData = new MeteoImportData(buiFileReader.MeasurementTimes.ToArray(), buiFileReader.StationNames.ToArray(), measurements);
            return true;
        }

        private bool GetSobekRREvaporationMeteoImportData(string path, MeteoData meteoData, IRainfallRunoffModel model, out MeteoImportData importData)
        {
            importData = null;

            if (!CanReadEvaporation(path, meteoData, model.Precipitation))
            {
                return false;
            }

            SobekRREvaporation sobekRREvaporation = ReadSobekRREvaporation(path);
            if (meteoData.DataDistributionType == MeteoDataDistributionType.Global
                && sobekRREvaporation.NumberOfLocations > 1)
            {
                meteoData.DataDistributionType = MeteoDataDistributionType.PerStation;
            }

            if (meteoData.DataDistributionType == MeteoDataDistributionType.PerStation
                && sobekRREvaporation.NumberOfLocations != model.MeteoStations.Count)
            {
                logHandler.ReportError(Resources.SobekRRMeteoDataImporter_GetSobekRREvaporationMeteoImportData_Number_of_evaporation_stations_does_not_match_the_number_of_precipitation_stations__Not_supported);
                return false;
            }

            var meteoDataSource = GetEvaporationMeteoDataSource(path);
            model.Evaporation.SelectedMeteoDataSource = meteoDataSource;

            if (meteoDataSource != MeteoDataSource.UserDefined)
            {
                meteoData.Data.Arguments[0].ExtrapolationType = ExtrapolationType.Periodic;
            }
  
            var meteoStations = GetEvaporationMeteoStations(meteoData, model).ToArray();

            importData = new MeteoImportData(sobekRREvaporation.Dates.ToArray(), meteoStations, sobekRREvaporation.Data);
            return true;
        }

        private static IEnumerable<string> GetEvaporationMeteoStations(MeteoData meteoData, IRainfallRunoffModel model)
        {
            switch (meteoData.DataDistributionType)
            {
                case MeteoDataDistributionType.Global:
                    return new []{ MeteoData.GlobalMeteoName };
                case MeteoDataDistributionType.PerFeature:
                    return model.Basin.Catchments.Select(c => c.Name);
                case MeteoDataDistributionType.PerStation:
                    return model.MeteoStations;
                default:
                    var message = Resources.SobekRRMeteoDataImporter_GetEvaporationMeteoStations_Unsupported_meteo_distribution_type_for_evaporation_;
                    throw new ArgumentOutOfRangeException(nameof(meteoData), meteoData.DataDistributionType, message);
            }
        }

        private bool CanReadEvaporation(string path, MeteoData meteoData, MeteoData modelPrecipitation)
        {
            if (!File.Exists(path))
            {
                logHandler.ReportWarning(string.Format(Resources.SobekRRMeteoDataImporter_CanReadEvaporation__0__file_does_not_exist___1_, meteoData?.Name, path));
                return false;
            }

            if (modelPrecipitation.DataDistributionType == meteoData.DataDistributionType 
                || meteoData.DataDistributionType == MeteoDataDistributionType.Global)
            {
                return true;
            }

            logHandler.ReportError(Resources.SobekRRMeteoDataImporter_CanReadEvaporation_Evaporation_has_multiple_stations_while_precipitation_has_not__Not_supported);
            return false;
        }

        private static SobekRREvaporation ReadSobekRREvaporation(string path)
        {
            SobekRREvaporation sobekRREvaporation;
            using (var stream = new FileStream(path, FileMode.Open))
            {
                sobekRREvaporation = evaporationReader.Read(stream);
            }

            return sobekRREvaporation;
        }

        private void AddMeteoData(MeteoData meteoData, MeteoImportData importData, IEnumerable<Catchment> basinCatchments, IList<string> stationList)
        {
            try
            {
                CorrectDataDistributionTypeBasedOnData(meteoData, importData);

                meteoData.Data.Clear();
                meteoData.Data.Arguments[0].SetValues(importData.Times);

                switch (meteoData.DataDistributionType)
                {
                    case MeteoDataDistributionType.Global:
                        meteoData.Data.Components[0].SetValues(importData.Values.Values.Select(a => a[0]));
                        return;
                    case MeteoDataDistributionType.PerFeature:
                        AddMissingFeatures(meteoData.Data.Arguments[1], importData, basinCatchments);
                        break;
                    case MeteoDataDistributionType.PerStation:
                        AddMissingStations(meteoData.Data.Arguments[1], importData, stationList);
                        break;
                    default:
                        var message = string.Format(Resources.SobekRRMeteoDataImporter_AddMeteoData_Meteo_data_with_DataDistributionType__0__is_not_supported, meteoData.DataDistributionType);
                        throw new ArgumentOutOfRangeException(nameof(meteoData), message);
                }

                foreach (var measurement in importData.Values)
                {
                    meteoData.Data.SetValues(measurement.Value, new VariableValueFilter<DateTime>(meteoData.Data.Arguments[0], measurement.Key));
                }
            }
            catch (Exception ex) when (ex is ArgumentException || 
                                       ex is InvalidDataException ||
                                       ex is IndexOutOfRangeException)
            {
                log.Error(string.Format(Resources.SobekRRMeteoDataImporter_AddMeteoData__0__import_failed___1_, meteoData.Name, ex.Message), ex);
            }
        }

        private static void CorrectDataDistributionTypeBasedOnData(MeteoData meteoData, MeteoImportData importData)
        {
            if (IsGlobalData(importData))
            {
                meteoData.DataDistributionType = MeteoDataDistributionType.Global;
                return;
            }

            if (meteoData.DataDistributionType != MeteoDataDistributionType.PerFeature)
            {
                meteoData.DataDistributionType = MeteoDataDistributionType.PerStation;
            }
        }

        private static bool IsGlobalData(MeteoImportData importData)
        {
            return importData.StationNames.Length == 1
                   && importData.StationNames[0].Equals(MeteoData.GlobalMeteoName, StringComparison.InvariantCultureIgnoreCase);
        }

        private static void AddMissingStations(IVariable stationVariable, MeteoImportData importData, IList<string> stationList)
        {
            if (stationVariable.Values.Count != 0)
            {
                return;
            }

            stationVariable.SetValues(importData.StationNames);

            var stationsToAdd = importData.StationNames.Except(stationList).ToArray();
            stationList.AddRange(stationsToAdd);
        }

        private static void AddMissingFeatures(IVariable stationVariable, MeteoImportData importData, IEnumerable<Catchment> basinCatchments)
        {
            if (stationVariable.Values.Count != 0)
            {
                return;
            }

            var catchmentLookup = basinCatchments.ToDictionaryWithErrorDetails("Basin catchments", c => c.Name);

            var missingFeatures = new List<string>();
            var features = new List<Catchment>();

            foreach (var stationName in importData.StationNames)
            {
                if (!catchmentLookup.TryGetValue(stationName, out Catchment catchment))
                {
                    missingFeatures.Add(stationName);
                    continue;
                }

                features.Add(catchment);
            }

            if (missingFeatures.Any())
            {
                throw new InvalidDataException(string.Format(Resources.SobekRRMeteoDataImporter_AddMissingFeatures_Could_not_find_a_catchment_for_the_following_stations__0__, string.Join(",", missingFeatures)));
            }

            stationVariable.SetValues(features);
        }
        
        private MeteoDataSource GetEvaporationMeteoDataSource(string evaporationFilePath)
        {
            string fileName = Path.GetFileName(evaporationFilePath);
            IOEvaporationMeteoDataSource ioEvaporationMeteoDataSource = evaporationFileNameConverter.FromFileName(fileName, logHandler);
            MeteoDataSource evaporationMeteoDataSource = evaporationMeteoDataSourceConverter.FromIOMeteoDataSource(ioEvaporationMeteoDataSource);

            return evaporationMeteoDataSource;
        }
    }
}
