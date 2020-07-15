using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using DelftTools.Shell.Core;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    /// <summary>This class is responsible for importing an RTC Model.</summary>
    public class RealTimeControlModelImporter : IDimrModelFileImporter
    {
        public string Name => "RTC-Tools xml files";

        public string Category => "Xml files";

        public string Description
        {
            get
            {
                return string.Empty;
            }
        }

        [ExcludeFromCodeCoverage]
        public Bitmap Image => Resources.brick_add;

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield break;
            }
        }

        public bool CanImportOnRootLevel => false;

        public string FileFilter => "xml files|*.xml";

        [ExcludeFromCodeCoverage]
        public string TargetDataDirectory { get; set; }

        [ExcludeFromCodeCoverage]
        public bool ShouldCancel { get; set; }

        [ExcludeFromCodeCoverage]
        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool OpenViewAfterImport => false;

        public string MasterFileExtension => "json";

        [ExcludeFromCodeCoverage]
        public bool CanImportOn(object targetObject)
        {
            return false;
        }

        /// <inheritdoc/>
        /// <summary>Imports the RTC Model.</summary>
        /// <param name="path">The directory path of the directory of the RTC files.</param>
        /// <param name="target">target, currently unused</param>
        /// <returns>A RealTimeControlModel as object</returns>
        /// <remarks>
        /// <paramref name="target"/> is unused.
        /// </remarks>
        public object ImportItem(string path, object target = null)
        {
            return RealTimeControlModelXmlReader.Read(path);
        }
    }
}