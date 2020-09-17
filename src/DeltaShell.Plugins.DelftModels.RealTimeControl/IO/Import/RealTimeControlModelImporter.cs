using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using DelftTools.Shell.Core;
using DeltaShell.Dimr;
using DeltaShell.NGHS.Common.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Import
{
    /// <summary>This class is responsible for importing an RTC Model.</summary>
    public class RealTimeControlModelImporter : ModelFileImporterBase, IDimrModelFileImporter
    {
        public override string Name => "RTC-Tools xml files";

        public override string Category => "Xml files";

        public override string Description => string.Empty;

        [ExcludeFromCodeCoverage]
        public override Bitmap Image => Resources.brick_add;

        public override IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield break;
            }
        }

        public override bool CanImportOnRootLevel => false;

        public override string FileFilter => "xml files|*.xml";

        [ExcludeFromCodeCoverage]
        public override string TargetDataDirectory { get; set; }

        [ExcludeFromCodeCoverage]
        public override bool ShouldCancel { get; set; }

        [ExcludeFromCodeCoverage]
        public override ImportProgressChangedDelegate ProgressChanged { get; set; }

        public override bool OpenViewAfterImport => false;

        public string MasterFileExtension => "json";

        [ExcludeFromCodeCoverage]
        public override bool CanImportOn(object targetObject)
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
        protected override object OnImportItem(string path, object target = null)
        {
            return RealTimeControlModelXmlReader.Read(path);
        }
    }
}