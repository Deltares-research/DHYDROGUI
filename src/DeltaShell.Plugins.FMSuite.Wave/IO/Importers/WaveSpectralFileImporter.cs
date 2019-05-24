using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Utils.Editing;

namespace DeltaShell.Plugins.FMSuite.Wave.IO.Importers
{
    public class WaveSpectralFileImporter : IFileImporter
    {
        private readonly Func<IEnumerable<WaveModel>> getModels;

        public WaveSpectralFileImporter(Func<IEnumerable<WaveModel>> getModelsFunc)
        {
            getModels = getModelsFunc;
        }

        public string Name => "Swan Spectral File (*.sp2)";

        public string Category { get; private set; }
        public string Description => string.Empty;

        public string FileFilter => "Swan Spectral Files (*.sp2)|*.sp2";

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(IList<WaveBoundaryCondition>);
            }
        }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public bool CanImportOnRootLevel => false;

        public string SelectedFilePath { get; set; }

        public object ImportItem(string path, object target)
        {
            path = path ?? SelectedFilePath;

            var conditions = target as IList<WaveBoundaryCondition>;
            if (conditions == null)
            {
                return null;
            }

            WaveModel model = getModels().First(m => m.BoundaryConditions.Equals(conditions));
            string filePath = Path.GetFullPath(path);

            model.BeginEdit(new DefaultEditAction("Import sp2 file"));
            model.BoundaryIsDefinedBySpecFile = true;
            model.OverallSpecFile = model.ImportIntoModelDirectory(filePath);
            model.EndEdit();

            return target;
        }

        public Bitmap Image { get; private set; }
        public string TargetDataDirectory { get; set; }
        public bool ShouldCancel { get; set; }
        public ImportProgressChangedDelegate ProgressChanged { get; set; }
        public bool OpenViewAfterImport { get; private set; }
    }
}