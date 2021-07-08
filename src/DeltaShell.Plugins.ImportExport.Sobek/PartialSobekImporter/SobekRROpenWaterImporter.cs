using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    /// <summary>
    /// Partial importer for <see cref="OpenWaterData"/>.
    /// </summary>
    public sealed class SobekRROpenWaterImporter : PartialSobekImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRROpenWaterImporter));
        
        /// <summary>
        /// The display name.
        /// </summary>
        public override string DisplayName => "Rainfall Runoff open water data";
        
        /// <summary>
        /// The importer category.
        /// </summary>
        public override SobekImporterCategories Category => SobekImporterCategories.RainfallRunoff;

        /// <summary>
        /// Partial import of the Open Water Data. 
        /// </summary>
        protected override void PartialImport()
        {
            SetOpenWaterData();
        }

        private string FilePath => GetFilePath(SobekFileNames.SobekRROpenWaterFileName);

        private Dictionary<string, OpenWaterData> GetOpenWaterData() =>
            GetModel<RainfallRunoffModel>().GetAllModelData().OfType<OpenWaterData>().ToDictionary(d => d.Name);

        private void SetOpenWaterData()
        {
            IReadOnlyDictionary<string, OpenWaterData> modelData = GetOpenWaterData();

            foreach (SobekRROpenWater sobekOpenWater in new SobekRROpenWaterReader().Read(FilePath))
            {
                if (!modelData.TryGetValue(sobekOpenWater.Id, out OpenWaterData openWaterData))
                {
                    LogError(sobekOpenWater.Id);
                    continue;
                }

                openWaterData.CalculationArea = sobekOpenWater.Area;
                openWaterData.AreaAdjustmentFactor = sobekOpenWater.AreaAjustmentFactor;
                openWaterData.MeteoStationName = sobekOpenWater.MeteoStationId;
            }
        }

        private static void LogError(string id)
        {
            log.ErrorFormat("No open paved data to be present in model for catchment with id {0}", id);
        }
    }
}