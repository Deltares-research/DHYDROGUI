using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using DelftTools.Shell.Core;
using DeltaShell.Dimr;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Import
{
    public class DHydroConfigXmlImporter: IFileImporter
    {
        public DHydroConfigXmlImporter(Func<List<IDimrModelFileImporter>> dimrFileImporters)
        {
            getDimrModelFileImporters = dimrFileImporters;
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public string Name => "DIMR Configuration File (*.xml)";

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public string Category => "DIMR Configuration File";

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public Bitmap Image { get; }

        /// <inheritdoc />
        public IEnumerable<Type> SupportedItemTypes
        {
            get { yield break; }
        }

        /// <inheritdoc />
        public bool CanImportOnRootLevel => GetDimrModelFileImporters.Any(e => e.CanImportOnRootLevel);

        /// <inheritdoc />
        public string FileFilter => "xml|*.xml";

        /// <inheritdoc />
        public string TargetDataDirectory { get; set; }

        /// <inheritdoc />
        public bool ShouldCancel { get; set; }

        /// <inheritdoc />
        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        /// <inheritdoc />
        public bool OpenViewAfterImport => true;

        /// <inheritdoc />
        public bool CanImportOn(object targetObject)
        {
            return targetObject is Project && GetDimrModelFileImporters.Any(e => e.CanImportOn(targetObject));
        }

        /// <inheritdoc />
        public object ImportItem(string path, object target = null)
        {
            var dimrModelFileImporters = GetDimrModelFileImporters;
            return HydroModelReader.Read(path, dimrModelFileImporters );
        }

        private List<IDimrModelFileImporter> GetDimrModelFileImporters =>
            getDimrModelFileImporters?.Invoke() ?? new List<IDimrModelFileImporter>();

        private readonly Func<List<IDimrModelFileImporter>> getDimrModelFileImporters;
    }
}
