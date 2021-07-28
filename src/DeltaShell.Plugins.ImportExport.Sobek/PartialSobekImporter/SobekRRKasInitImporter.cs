using System.IO;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    /// <summary>
    /// Importer for the KASINIT file.
    /// </summary>
    public class SobekRRKasInitImporter : PartialSobekImporterBase
    {
        private const string kasInitFileName = "KASINIT";
        private const string greenhouseStorageFileTag = "GreenhouseStorageFile";

        /// <summary>
        /// The display name.
        /// </summary>
        public override string DisplayName => "Rainfall Runoff KASINIT data";

        /// <summary>
        /// The importer category.
        /// </summary>
        public override SobekImporterCategories Category => SobekImporterCategories.RainfallRunoff;

        /// <summary>
        /// If it exists, reads the content of the KASINIT file and adds it to the <see cref="RainfallRunoffModel"/>.
        /// </summary>
        protected override void PartialImport()
        {
            string file = GetFilePath(kasInitFileName);
            if (!File.Exists(file))
            {
                return;
            }

            var model = GetModel<RainfallRunoffModel>();
            var document = (TextDocument) model.GetDataItemByTag(greenhouseStorageFileTag).Value;
            document.Content = File.ReadAllText(file);
        }
    }
}