using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using log4net;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public class SobekRRGreenhouseImporter: PartialSobekImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRRGreenhouseImporter));

        public override string DisplayName
        {
            get { return "Rainfall Runoff greenhouse data"; }
        }

        protected override void PartialImport()
        {
            log.DebugFormat("Importing greenhouse data ...");

            var rainfallRunoffModel = GetModel<RainfallRunoffModel>();
            var catchmentModelData = rainfallRunoffModel.GetAllModelData()
                                                        .OfType<GreenhouseData>()
                                                        .ToDictionary(rra => rra.Name);
            ReadAndAddOrUpdateGreenhouseData(GetFilePath(SobekFileNames.SobekRRGreenhouseFileName), catchmentModelData);
        }

        private void ReadAndAddOrUpdateGreenhouseData(string filePath, Dictionary<string, GreenhouseData> catchmentModelData)
        {
            var pathRf = filePath.Replace(".3B", ".RF");
            var pathSil = filePath.Replace(".3B", ".SIL");

            var dicRf = new SobekRRStorageReader().Read(pathRf).ToDictionaryWithErrorDetails(pathRf, item => item.Id, item => item);
            var dicSil = new SobekRRGreenhouseSiloDefinitionReader().Read(pathSil).ToDictionaryWithErrorDetails(pathSil, item => item.Id, item => item);

            foreach (var greenhouse in new SobekRRGreenhouseReader().Read(filePath))
            {
                if (catchmentModelData.ContainsKey(greenhouse.Id))
                {
                    var greenhouseData = catchmentModelData[greenhouse.Id];

                    greenhouseData.CalculationArea = greenhouse.Areas.Sum();

                    for (var i = 0; i < greenhouse.Areas.Length; i++)
                    {
                        greenhouseData.AreaPerGreenhouse[
                            (GreenhouseEnums.AreaPerGreenhouseType)Enum.ToObject(typeof(GreenhouseEnums.AreaPerGreenhouseType), i)] =
                            greenhouse.Areas[i];
                    }

                    greenhouseData.SurfaceLevel = greenhouse.SurfaceLevel;

                    greenhouseData.MeteoStationName = greenhouse.MeteoStationId;
                    greenhouseData.AreaAdjustmentFactor = greenhouse.AreaAjustmentFactor;

                    // Roof storage
                    if (dicRf.ContainsKey(greenhouse.StorageOnRoofsId))
                    {
                        var storage = dicRf[greenhouse.StorageOnRoofsId];
                        greenhouseData.InitialRoofStorage = storage.InitialRoofStorage;
                        greenhouseData.MaximumRoofStorage = storage.MaxRoofStorage;
                    }
                    else
                    {
                        log.ErrorFormat("Storage definition {0} has not been found.", greenhouse.StorageOnRoofsId);
                    }

                    // Silo definition
                    if (dicSil.ContainsKey(greenhouse.SiloId))
                    {
                        var silo = dicSil[greenhouse.SiloId];
                        greenhouseData.SiloCapacity = silo.SiloCapacity;
                        greenhouseData.PumpCapacity = silo.SiloPumpCapacity;
                    }
                    else
                    {
                        log.ErrorFormat("Silo definition {0} has not been found.", greenhouse.SiloId);
                    }


                    greenhouseData.SubSoilStorageArea = greenhouse.SiloArea;
                    greenhouseData.UseSubsoilStorage = greenhouse.SiloArea > 0;
                }
                else
                {
                    log.WarnFormat("Rainfall runoff area with id {0} has not been found. Item has been skipped...", greenhouse.Id);
                }
            }
        }
    }
}