using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using log4net;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public class SobekRRNwrwImporter : PartialSobekImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRRNwrwImporter));

        public override string DisplayName
        {
            get { return "Rainfall Runoff NWRW data"; }
        }

        protected override void PartialImport()
        {
            log.DebugFormat("Importing nwrw data ...");

            var rainfallRunoffModel = GetModel<RainfallRunoffModel>();
            var catchmentModelData = rainfallRunoffModel.GetAllModelData()
                                                        .OfType<NwrwData>()
                                                        .ToDictionary(rra => rra.Name);
            ReadAndAddOrUpdateNWRWData(GetFilePath(SobekFileNames.SobekRRNwrwFileName), catchmentModelData);
            ReadAndAddOrUpdateSettings(GetFilePath(SobekFileNames.SobekRRNwrwSettingsFileName), rainfallRunoffModel);
        }

        private void ReadAndAddOrUpdateNWRWData(string filePath, Dictionary<string, NwrwData> catchmentModelData)
        {
            var formatPumpCapacityTable = new DataTable();
            formatPumpCapacityTable.Columns.Add(new DataColumn("Time", typeof(DateTime)));
            formatPumpCapacityTable.Columns.Add(new DataColumn("ValueMixed", typeof(double)));
            formatPumpCapacityTable.Columns.Add(new DataColumn("ValueDwf", typeof(double)));

            var pathDwa = GetFilePath(SobekFileNames.SobekRRNwrwDwaFileName);
            var pathTbl = GetFilePath(SobekFileNames.SobekRRNwrwTableFileName);

            var dicDwa = new SobekRRDryWeatherFlowReader().Read(pathDwa).ToDictionaryWithErrorDetails(pathDwa, item => item.Id, item => item);
            var dicTbl = new SobekRRTableReader("QC_T", formatPumpCapacityTable).Read(pathTbl).ToDictionaryWithErrorDetails(pathTbl,
                item => item.TableName, item => item);

            foreach (var sobekNWRW in new SobekRRNwrwReader().Read(filePath))
            {
                if (catchmentModelData.ContainsKey(sobekNWRW.Id))
                {
                    var modelData = catchmentModelData[sobekNWRW.Id];

                    var nwrwData = modelData as NwrwData;

                    if (nwrwData == null)
                    {
                        log.ErrorFormat("Expected nwrw data to be present in model for catchment with id {0}",
                            sobekNWRW.Id);
                        continue;
                    }

                    nwrwData.CalculationArea = sobekNWRW.Area;
                    nwrwData.MeteoStationId = sobekNWRW.DwaId;

                    var specialArea = new NwrwSpecialArea();
                    var dryWeatherFlow = new NwrwDryWeatherFlowDefinition();

                    //nwrwData.SurfaceLevel = sobekNWRW.StreetLevel;

                    //Storage
                    // if (dicSto.ContainsKey(sobekNWRW.StorageId))
                    // {
                    //     var storage = dicSto[sobekNWRW.StorageId];
                    //     // nwrwData.StorageUnit = RainfallRunoffEnums.StorageUnit.mm;
                    //     // nwrwData.MaximumStreetStorage = storage.MaxStreetStorage;
                    //     // nwrwData.InitialStreetStorage = storage.InitialStreetStorage;
                    //     // nwrwData.MaximumSewerMixedAndOrRainfallStorage = storage.MaxSewerStorageMixedRainfall;
                    //     // nwrwData.InitialSewerMixedAndOrRainfallStorage = storage.InitialSewerStorageMixedRainfall;
                    //     // nwrwData.MaximumSewerDryWeatherFlowStorage = storage.MaxSewerStorageDWA;
                    //     // nwrwData.InitialSewerDryWeatherFlowStorage = storage.InitialSewerStorageDWA;
                    // }
                    // else
                    // {
                    //     log.ErrorFormat("Storage table {0} has not been found.", sobekNWRW.StorageId);
                    // }
                }

            }
        }

        private void ReadAndAddOrUpdateSettings(string filePath, RainfallRunoffModel rrModel)
        {
            var dicSettings = new SobekRRNwrwSettingsReader().Read(filePath);
            foreach (var settings in dicSettings)
            {
                var nwrwDefinition = new NwrwDefinition(){Name = settings.Id};

                var existingSettings = rrModel.NwrwDefinitions.FirstOrDefault(def => def.Name == settings.Id);

                if (existingSettings != null)
                {
                    nwrwDefinition = existingSettings;
                }
                else
                {
                    rrModel.NwrwDefinitions.Add(nwrwDefinition);
                }

                //nwrwDefinition.InfiltrationCapacityMax =
            }
        }

    }
}