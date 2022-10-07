using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers
{
    /// <summary>
    /// Meteo data importer for precipitation, evaporation and temperature.
    /// </summary>
    public class MeteoDataImporter : IFileImporter
    {
        private readonly PrecipitationDataImporter precipitationImporter;
        private readonly EvaporationDataImporter evaporationImporter;
        private readonly TemperatureDataImporter temperatureImporter;
        private IFileImporter importer = new PrecipitationDataImporter();

        /// <summary>
        /// Initializes a new instance of the <see cref="MeteoDataImporter"/> class.
        /// </summary>
        /// <param name="precipitationImporter"> The precipitation importer. </param>
        /// <param name="evaporationImporter"> The evaporation importer. </param>
        /// <param name="temperatureImporter"> The temperature importer. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="precipitationImporter"/>, <paramref name="evaporationImporter"/> or
        /// <paramref name="temperatureImporter"/> is <c>null</c>.
        /// </exception>
        public MeteoDataImporter(PrecipitationDataImporter precipitationImporter, 
                                 EvaporationDataImporter evaporationImporter,
                                 TemperatureDataImporter temperatureImporter)
        {
            Ensure.NotNull(precipitationImporter, nameof(precipitationImporter));
            Ensure.NotNull(evaporationImporter, nameof(evaporationImporter));
            Ensure.NotNull(temperatureImporter, nameof(temperatureImporter));

            this.precipitationImporter = precipitationImporter;
            this.evaporationImporter = evaporationImporter;
            this.temperatureImporter = temperatureImporter;
        }
        
        public string Name
        {
            get { return importer.Name; }
        }

        public string Category
        {
            get { return importer.Category; }
        }

        public string Description { get{ return Name; } }

        public Bitmap Image
        {
            get { return importer.Image; }
        }

        public IEnumerable<Type> SupportedItemTypes
        {
            get { return importer.SupportedItemTypes; }
        }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public bool CanImportOnRootLevel
        {
            get { return importer.CanImportOnRootLevel; }
        }

        public string FileFilter
        {
            get { return "SOBEK BUI/Evaporation/TMP files (*.BUI;*.EVP;*.GEM;*.PLV;*.TMP)|*.bui;*.evp;*.gem;*.plv;*.tmp"; }
        }
        public bool OpenViewAfterImport { get { return false; } }
        public string TargetDataDirectory
        {
            get { return importer.TargetDataDirectory; }
            set { importer.TargetDataDirectory = value; }
        }

        public bool ShouldCancel
        {
            get { return importer.ShouldCancel; }
            set { importer.ShouldCancel = value; }
        }

        public ImportProgressChangedDelegate ProgressChanged
        {
            get { return importer.ProgressChanged; }
            set { importer.ProgressChanged = value; }
        }

        public object ImportItem(string path, object target)
        {
            var meteoData = target as MeteoData;
            if (meteoData != null)
            {
                if (meteoData.Name == RainfallRunoffModelDataSet.PrecipitationName)
                {
                    importer = precipitationImporter;
                }

                if (meteoData.Name == RainfallRunoffModelDataSet.EvaporationName)
                {
                    importer = evaporationImporter;
                }

                if (meteoData.Name == RainfallRunoffModelDataSet.TemperatureName)
                {
                    importer = temperatureImporter;
                }
            }

            return importer.ImportItem(path, target);
        }
    }
}
