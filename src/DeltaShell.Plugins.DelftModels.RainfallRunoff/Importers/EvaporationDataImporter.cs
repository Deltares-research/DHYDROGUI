using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using log4net;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers
{
    public class EvaporationDataImporter : IFileImporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (EvaporationDataImporter));

        public string Name
        {
            get { return "Evaporation Data (EVP)"; }
        }

        public string Category { get; private set; }
        
        public Bitmap Image { get; private set; }

        public IEnumerable<Type> SupportedItemTypes
        {
            get { yield return typeof (MeteoData); }
        }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public bool CanImportOnRootLevel
        {
            get { return false; }
        }

        public string FileFilter
        {
            get { return "All Supported Files|*.evp;*.gem;*.plv"; }
        }
        public bool OpenViewAfterImport { get { return false; } }
        public string TargetDataDirectory { get; set; }
        
        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public object ImportItem(string path, object target)
        {
            var evaporation = target as MeteoData;
            if (evaporation == null)
            {
                Log.Error("Evaporation data: importing on invalid item.");

                return target;
            }
            try
            {
                var evaporationTable = SobekRREvaporationReader.ReadEvaporationData(path).FirstOrDefault();
                if (evaporationTable != null)
                {
                    SetEvaporationValues(evaporationTable, evaporation);
                }
            }
            catch (Exception e)
            {
                Log.Error("Evaporation import failed: ", e);
            }

            return target;
        }

        [InvokeRequired]
        private static void SetEvaporationValues(DataTable table, MeteoData meteo)
        {
            if (table.Rows.Count == 0 || table.Columns.Count < 4)
            {
                Log.Warn("No data values found to import to evaporation.");
                return;
            }
            var stations = table.Columns.Count - 3;
            var isPeriodic = false;
            if (meteo.DataDistributionType == MeteoDataDistributionType.Global)
            {
                if (stations != 1)
                {
                    Log.Warn(
                        "Importing station-dependent evaporation data to global evaporation: restricting to first column");
                }
                foreach (DataRow row in table.Rows)
                {
                    DateTime dateTime;
                    if (SobekRREvaporationReader.TryReadDateTime(row, out dateTime, out isPeriodic))
                    {
                        meteo.Data[dateTime] = (double) row[3];
                    }
                }
            }
            if (meteo.DataDistributionType == MeteoDataDistributionType.PerStation)
            {
                var importedStations = Math.Min(stations, meteo.Data.Arguments[1].Values.Count);
                if (importedStations < stations)
                {
                    Log.WarnFormat("Importing only {0} evaporation series of {1} defined in the file", importedStations,
                                   stations);
                }
                foreach (DataRow row in table.Rows)
                {
                    DateTime dateTime;
                    if (SobekRREvaporationReader.TryReadDateTime(row, out dateTime, out isPeriodic))
                    {
                        for (var i = 0; i < importedStations; ++i)
                        {
                            var station = meteo.Data.Arguments[1].Values[i];
                            meteo.Data[dateTime, station] = (double)row[3 + i];
                        }
                    }
                }
            }
            if (meteo.DataDistributionType == MeteoDataDistributionType.PerFeature)
            {
                Log.Warn("Importing first evaporation series to all catchments");
                var features = ((FeatureCoverage) meteo.Data).Features;
                
                foreach (DataRow row in table.Rows)
                {
                    DateTime dateTime;
                    if (SobekRREvaporationReader.TryReadDateTime(row, out dateTime, out isPeriodic))
                    {
                        foreach (var feature in features)
                        {
                            meteo.Data[dateTime, feature] = (double)row[3];
                        }
                    }
                }
            }
            if (isPeriodic)
            {
               meteo.Data.Arguments[0].ExtrapolationType = ExtrapolationType.Periodic;
            }
        }
    }
}
