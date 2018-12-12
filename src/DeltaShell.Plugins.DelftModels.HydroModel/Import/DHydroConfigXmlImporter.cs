using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;
using DeltaShell.Dimr;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Import
{
    public class DHydroConfigXmlImporter: IFileImporter
    {
        private readonly Func<List<IDimrModelFileImporter>> getDimrModelFileImporters;

        public DHydroConfigXmlImporter(Func<List<IDimrModelFileImporter>> dimrFileImporters)
        {
            getDimrModelFileImporters = dimrFileImporters;
        }

        /// <inheritdoc />
        public string Name
        {
            get { return "DIMR Configuration File (*.xml)"; }
        }

        /// <inheritdoc />
        public string Category
        {
            get { return "DIMR Configuration File"; }
        }

        /// <inheritdoc />
        public Bitmap Image { get; }

        /// <inheritdoc />
        public IEnumerable<Type> SupportedItemTypes
        {
            get { yield break; }
        }

        /// <inheritdoc />
        public bool CanImportOnRootLevel
        {
            get { return true; }
        }

        /// <inheritdoc />
        public string FileFilter
        {
            get { return "xml|*.xml"; }
        }

        /// <inheritdoc />
        public string TargetDataDirectory { get; set; }

        /// <inheritdoc />
        public bool ShouldCancel { get; set; }

        /// <inheritdoc />
        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        /// <inheritdoc />
        public bool OpenViewAfterImport
        {
            get { return true; }
        }

        /// <inheritdoc />
        public bool CanImportOn(object targetObject)
        {
            return targetObject is Project;
        }

        /// <inheritdoc />
        public object ImportItem(string path, object target = null)
        {
            var dimrModelFileImporters = getDimrModelFileImporters?.Invoke() ?? new List<IDimrModelFileImporter>();
            return HydroModelReader.Read(path, dimrModelFileImporters );
        }
    }
}
