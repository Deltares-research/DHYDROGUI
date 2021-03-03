using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;
using DeltaShell.NGHS.Common.Properties;
using log4net;

namespace DeltaShell.NGHS.Common.IO
{
    /// <summary>
    /// Base class definition for importing model data.
    /// </summary>
    public abstract class ModelFileImporterBase : IFileImporter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ModelFileImporterBase));

        public abstract string Name { get; }
        public abstract string Category { get; }
        public abstract string Description { get; }
        public abstract Bitmap Image { get; }
        public abstract IEnumerable<Type> SupportedItemTypes { get; }
        public abstract bool CanImportOnRootLevel { get; }
        public abstract string FileFilter { get; }
        public abstract string TargetDataDirectory { get; set; }
        public abstract bool ShouldCancel { get; set; }
        public abstract ImportProgressChangedDelegate ProgressChanged { get; set; }
        public abstract bool OpenViewAfterImport { get; }

        public abstract bool CanImportOn(object targetObject);

        public object ImportItem(string path, object target = null)
        {
            log.Info(Resources.ModelFileImporterBase_ImportItem_Start_importing_model_data);

            try
            {
                object importedObject = OnImportItem(path, target);
                
                if (importedObject == null)
                {
                    LogImportFailed();
                    return null;
                }
                
                log.Info(Resources.ModelFileImporterBase_ImportItem_Stop_importing_model_data);
                return importedObject;
            }
            catch
            {
                LogImportFailed();
                throw;
            }
        }

        private static void LogImportFailed()
        {
            log.Error(Resources.ModelFileImporterBase_ImportItem_Importing_model_data_failed);
        }

        /// <summary>
        /// Imports the data from <paramref name="path"/> to (optionally) the <paramref name="target"/>.
        /// </summary>
        /// <param name="path">The file path to import the data from.</param>
        /// <param name="target">The optional target to import the data to.</param>
        /// <returns>The imported object, or <c>null</c> when the import has failed.</returns>
        /// <remarks>This method can throw any exceptions.</remarks>
        protected abstract object OnImportItem(string path, object target = null);
    }
}