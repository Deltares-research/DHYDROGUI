using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers
{
    public class EvaporationDataImporter : IFileImporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (EvaporationDataImporter));
        private readonly Func<MeteoData, RainfallRunoffModel> getModelFunc;

        /// <summary>
        /// Initializes a new instance of the <see cref="EvaporationDataImporter"/> class.
        /// </summary>
        /// <param name="getModelFunc"> Optional; a function to retrieve the corresponding model. </param>
        public EvaporationDataImporter(Func<MeteoData, RainfallRunoffModel> getModelFunc = null)
        {
            this.getModelFunc = getModelFunc;
        }
        
        public string Name
        {
            get { return "Evaporation Data (EVP)"; }
        }

        public string Category { get; private set; }
        public string Description { get{ return Name; } }

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
                SobekRREvaporation sobekEvaporation;
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    sobekEvaporation = SobekRREvaporationReader.Read(stream);
                }

                RainfallRunoffModel model = getModelFunc?.Invoke(evaporation);
                if (model != null && sobekEvaporation.IsLongTimeAverage)
                {
                    sobekEvaporation.ToLongTimeAverage(model.StartTime, model.StopTime);
                }
                
                SetEvaporationValues(sobekEvaporation, evaporation);
            }
            catch (Exception e)
            {
                Log.Error("Evaporation import failed: ", e);
            }

            return target;
        }

        [InvokeRequired]
        private static void SetEvaporationValues(SobekRREvaporation table, MeteoData meteo)
        {
            if (!table.Data.Any())
            {
                Log.Warn("No data values found to import to evaporation.");
                return;
            }
            meteo.Data.Arguments[0].Clear();

            switch (meteo.DataDistributionType)
            {
                case MeteoDataDistributionType.Global:
                    SetGlobalMeteoData(table, meteo);
                    break;
                case MeteoDataDistributionType.PerStation:
                    SetMeteoDataPerStation(table, meteo);
                    break;
                case MeteoDataDistributionType.PerFeature:
                    SetMeteoDataPerFeature(table, meteo);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (table.IsPeriodic)
            {
               meteo.Data.Arguments[0].ExtrapolationType = ExtrapolationType.Periodic;
            }
        }

        private static void SetMeteoDataPerFeature(SobekRREvaporation table, MeteoData meteo)
        {
            Log.Warn("Importing first evaporation series to all catchments");
            var features = ((FeatureCoverage)meteo.Data).Features;

            foreach (KeyValuePair<DateTime, double[]> data in table.Data)
            {
                DateTime dateTime = data.Key;
                double value = data.Value[0];
                foreach (var feature in features)
                {
                    meteo.Data[dateTime, feature] = value;
                }
            }
        }

        private static void SetMeteoDataPerStation(SobekRREvaporation table, MeteoData meteo)
        {
            var importedStations = Math.Min(table.NumberOfLocations, meteo.Data.Arguments[1].Values.Count);
            if (importedStations < table.NumberOfLocations)
            {
                Log.WarnFormat("Importing only {0} evaporation series of {1} defined in the file", importedStations,
                               table.NumberOfLocations);
            }

            foreach (KeyValuePair<DateTime, double[]> data in table.Data)
            {
                DateTime dateTime = data.Key;
                double[] values = data.Value;
                for (var i = 0; i < importedStations; ++i)
                {
                    var station = meteo.Data.Arguments[1].Values[i];
                    meteo.Data[dateTime, station] = values[i];
                }
            }
        }

        private static void SetGlobalMeteoData(SobekRREvaporation table, MeteoData meteo)
        {
            if (table.NumberOfLocations != 1)
            {
                Log.Warn(
                    "Importing station-dependent evaporation data to global evaporation: restricting to first column");
            }

            foreach (KeyValuePair<DateTime, double[]> data in table.Data)
            {
                DateTime dateTime = data.Key;
                double value = data.Value[0];
                meteo.Data[dateTime] = value;
            }
        }
    }
}
