using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core;
using DelftTools.Utils.Aop;
using Deltares.Infrastructure.API.Logging;
using Deltares.Infrastructure.Logging;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Converters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.DataAccessObjects;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Properties;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers
{
    public class EvaporationDataImporter : IFileImporter
    {
        private readonly ILogHandler logHandler = new LogHandler("importing evaporation data", typeof (EvaporationDataImporter));
        private static readonly EvaporationFileNameConverter evaporationFileNameConverter = new EvaporationFileNameConverter();
        private static readonly IOEvaporationMeteoDataSourceConverter ioEvaporationMeteoDataSourceConverter = new IOEvaporationMeteoDataSourceConverter();
        private static readonly SobekRREvaporationReader evaporationReader = new SobekRREvaporationReader();

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
            var evaporation = target as EvaporationMeteoData;
            if (evaporation == null)
            {
                logHandler.ReportError("Evaporation data: importing on invalid item.");

                return target;
            }
            try
            {
                SobekRREvaporation sobekEvaporation;
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    sobekEvaporation = evaporationReader.Read(stream);
                }

                MeteoDataSource evaporationMeteoDataSource = GetEvaporationMeteoDataSource(path);

                SetEvaporationValues(sobekEvaporation, evaporation, evaporationMeteoDataSource);
            }
            catch (Exception e)
            {
                logHandler.ReportError($"Evaporation import failed: {e.Message}");
            }

            return target;
        }

        [InvokeRequired]
        private void SetEvaporationValues(SobekRREvaporation table, EvaporationMeteoData meteo, MeteoDataSource meteoDataSource)
        {
            if (!table.Data.Any())
            {
                logHandler.ReportWarning("No data values found to import to evaporation.");
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
                    throw new ArgumentOutOfRangeException(nameof(meteo), string.Format(Resources.Exception_UnsupportedDataDistributionType,
                                                                                       meteo.DataAggregationType));
            }

            meteo.SelectedMeteoDataSource = meteoDataSource;

            if (meteoDataSource != MeteoDataSource.UserDefined)
            {
               meteo.Data.Arguments[0].ExtrapolationType = ExtrapolationType.Periodic;
            }
        }

        private void SetMeteoDataPerFeature(SobekRREvaporation table, MeteoData meteo)
        {
            logHandler.ReportWarning("Importing first evaporation series to all catchments");
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

        private void SetMeteoDataPerStation(SobekRREvaporation table, MeteoData meteo)
        {
            var importedStations = Math.Min(table.NumberOfLocations, meteo.Data.Arguments[1].Values.Count);
            if (importedStations < table.NumberOfLocations)
            {
                logHandler.ReportWarningFormat("Importing only {0} evaporation series of {1} defined in the file", importedStations,
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

        private void SetGlobalMeteoData(SobekRREvaporation table, MeteoData meteo)
        {
            if (table.NumberOfLocations != 1)
            {
                logHandler.ReportWarning(
                    "Importing station-dependent evaporation data to global evaporation: restricting to first column");
            }

            foreach (KeyValuePair<DateTime, double[]> data in table.Data)
            {
                DateTime dateTime = data.Key;
                double value = data.Value[0];
                meteo.Data[dateTime] = value;
            }
        }

        private MeteoDataSource GetEvaporationMeteoDataSource(string evaporationFilePath)
        {
            string fileName = Path.GetFileName(evaporationFilePath);
            IOEvaporationMeteoDataSource ioEvaporationMeteoDataSource = evaporationFileNameConverter.FromFileName(fileName, logHandler);
            MeteoDataSource evaporationMeteoDataSource = ioEvaporationMeteoDataSourceConverter.FromIOMeteoDataSource(ioEvaporationMeteoDataSource);

            return evaporationMeteoDataSource;
        }
    }
}
