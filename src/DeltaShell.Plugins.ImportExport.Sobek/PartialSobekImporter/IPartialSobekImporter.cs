using System;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public interface IPartialSobekImporter
    {
        /// <summary>
        /// Path to the SOBEK case or network file
        /// </summary>
        string PathSobek { get; set; }

        /// <summary>
        /// Name to describe the data that is imported by this importer
        /// </summary>
        string DisplayName { get;}

        /// <summary>
        /// Category of the importer
        /// </summary>
        SobekImporterCategories Category { get; }

        /// <summary>
        /// Object to import to
        /// </summary>
        object TargetObject { get; set; }

        /// <summary>
        /// Importer that should be run before this importer
        /// </summary>
        IPartialSobekImporter PartialSobekImporter { get; set; }

        /// <summary>
        /// Import action to perform for this importer
        /// </summary>
        void Import();

        /// <summary>
        /// Indicates if this importer should be used
        /// </summary>
        bool IsActive { get; set; }

        /// <summary>
        /// Indicates if this importer is visible in GUI (selection list user)
        /// </summary>
        bool IsVisible { get; set; }

        /// <summary>
        /// When this property is set to true - importer should stop importing (when in Import()) and return.
        /// </summary>
        bool ShouldCancel { get; set; }

        /// <summary>
        /// When set - called by importer after import has finished importing.
        /// </summary>
        Action<IPartialSobekImporter> AfterImport { get; set; }

        /// <summary>
        /// When set - called by importer before import has been started.
        /// </summary>
        Action<IPartialSobekImporter> BeforeImport { get; set; }
    }
}
