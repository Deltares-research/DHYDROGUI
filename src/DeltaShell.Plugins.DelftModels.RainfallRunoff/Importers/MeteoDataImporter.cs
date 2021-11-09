using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers
{
    public class MeteoDataImporter : IFileImporter
    {
        private IFileImporter importer = new PrecipitationDataImporter();
        private readonly Func<MeteoData, RainfallRunoffModel> getModelFunc;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MeteoDataImporter"/> class.
        /// </summary>
        /// <param name="getModelFunc"> Optional; a function to retrieve the corresponding model. </param>
        public MeteoDataImporter(Func<MeteoData, RainfallRunoffModel> getModelFunc = null)
        {
            this.getModelFunc = getModelFunc;
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
                    importer = new PrecipitationDataImporter();
                }

                if (meteoData.Name == RainfallRunoffModelDataSet.EvaporationName)
                {
                    importer = new EvaporationDataImporter(getModelFunc);
                }

                if (meteoData.Name == RainfallRunoffModelDataSet.TemperatureName)
                {
                    importer = new TemperatureDataImporter();
                }
            }

            return importer.ImportItem(path, target);
        }
    }
}
