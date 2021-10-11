using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.Utils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    /// <summary>
    /// Sobek importer for the greenhouse data.
    /// </summary>
    public class SobekRRGreenhouseImporter : PartialSobekImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRRGreenhouseImporter));

        public override string DisplayName => "Rainfall Runoff greenhouse data";

        public override SobekImporterCategories Category => SobekImporterCategories.RainfallRunoff;

        protected override void PartialImport()
        {
            log.DebugFormat("Importing greenhouse data ...");

            var rainfallRunoffModel = GetModel<RainfallRunoffModel>();
            Dictionary<string, GreenhouseData> catchmentModelData = rainfallRunoffModel.GetAllModelData()
                                                                                       .OfType<GreenhouseData>()
                                                                                       .ToDictionary(rra => rra.Name);
            ImportGreenhouseData(GetFilePath(SobekFileNames.SobekRRGreenhouseFileName), catchmentModelData);
        }

        private static void ImportGreenhouseData(string filePath, Dictionary<string, GreenhouseData> catchmentModelData)
        {
            string storageFilePath = filePath.Replace(".3B", ".RF");
            string siloFilePath = filePath.Replace(".3B", ".SIL");

            Dictionary<string, SobekRRStorage> storageData = new SobekRRStorageReader().Read(storageFilePath).ToDictionaryWithErrorDetails(storageFilePath, item => item.Id, item => item);
            Dictionary<string, SobekRRGreenhouseSiloDefinition> siloData = new SobekRRGreenhouseSiloDefinitionReader().Read(siloFilePath).ToDictionaryWithErrorDetails(siloFilePath, item => item.Id, item => item);

            foreach (SobekRRGreenhouse greenhouse in new SobekRRGreenhouseReader().Read(filePath))
            {
                if (!catchmentModelData.TryGetValue(greenhouse.Id, out GreenhouseData greenhouseData))
                {
                    log.WarnFormat("Rainfall runoff area with id {0} has not been found. Item has been skipped...", greenhouse.Id);
                    continue;
                }

                greenhouseData.CalculationArea = greenhouse.Areas.Sum();

                for (var i = 0; i < greenhouse.Areas.Length; i++)
                {
                    greenhouseData.AreaPerGreenhouse[(GreenhouseEnums.AreaPerGreenhouseType)i] = greenhouse.Areas[i];
                }

                greenhouseData.SurfaceLevel = greenhouse.SurfaceLevel;
                greenhouseData.MeteoStationName = greenhouse.MeteoStationId;
                greenhouseData.AreaAdjustmentFactor = greenhouse.AreaAjustmentFactor;

                ImportStorageDefinition(greenhouseData, greenhouse, storageData, storageFilePath);
                ImportSiloDefinition(greenhouseData, greenhouse, siloData, siloFilePath);
            }
        }

        private static void ImportStorageDefinition(GreenhouseData greenhouseData, SobekRRGreenhouse greenhouse, Dictionary<string, SobekRRStorage> data, string filePath)
        {
            if (data.TryGetValue(greenhouse.StorageOnRoofsId, out SobekRRStorage storage))
            {
                greenhouseData.InitialRoofStorage = storage.InitialRoofStorage;
                greenhouseData.MaximumRoofStorage = storage.MaxRoofStorage;
            }
            else
            {
                log.ErrorFormat($"Storage definition with id '{greenhouse.StorageOnRoofsId}' has not been found: {filePath}");
            }
        }

        private static void ImportSiloDefinition(GreenhouseData greenhouseData, SobekRRGreenhouse greenhouse, Dictionary<string, SobekRRGreenhouseSiloDefinition> data, string filePath)
        {
            greenhouseData.SubSoilStorageArea = greenhouse.SiloArea;
            greenhouseData.UseSubsoilStorage = greenhouse.SiloArea > 0;

            if (data.TryGetValue(greenhouse.SiloId, out SobekRRGreenhouseSiloDefinition silo))
            {
                greenhouseData.SiloCapacity = silo.SiloCapacity;
                greenhouseData.PumpCapacity = silo.SiloPumpCapacity;
            }
            else if (greenhouseData.UseSubsoilStorage)
            {
                log.ErrorFormat($"Silo definition with id '{greenhouse.SiloId}' has not been found: {filePath}");
            }
        }
    }
}