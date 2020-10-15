using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Guards;
using DeltaShell.Dimr;
using DeltaShell.NGHS.Common.IO;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Wave.IO.Importers
{
    /// <summary>
    /// Importer for importing D-Waves models from .mdw files.
    /// </summary>
    /// <seealso cref="IDimrModelFileImporter" />
    public class WaveModelFileImporter : ModelFileImporterBase, IDimrModelFileImporter
    {
        private readonly ILog log = LogManager.GetLogger(typeof(WaveModelFileImporter));
        private readonly Func<string> getWorkingDirectoryPathFunc;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaveModelFileImporter"/> class.
        /// </summary>
        /// <param name="getWorkingDirectoryPathFunc">
        /// The function to retrieve the working directory path.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="getWorkingDirectoryPathFunc"/> is <c>null</c>.
        /// </exception>
        public WaveModelFileImporter(Func<string> getWorkingDirectoryPathFunc)
        {
            Ensure.NotNull(getWorkingDirectoryPathFunc, nameof(getWorkingDirectoryPathFunc));

            this.getWorkingDirectoryPathFunc = getWorkingDirectoryPathFunc;
        }

        public override string Name => "Waves Model";

        public override string Category => "D-Flow FM 2D/3D";

        public override string Description => string.Empty;

        public override Bitmap Image => Resources.wave;

        public override IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(IHydroModel);
            }
        }

        public override bool OpenViewAfterImport => true;

        public override bool CanImportOnRootLevel => true;

        public override string FileFilter => "Master Definition WAVE File|*.mdw";

        public override string TargetDataDirectory { get; set; }

        public override bool ShouldCancel { get; set; }

        public override ImportProgressChangedDelegate ProgressChanged { get; set; }

        /// <summary>
        /// MasterFileExtension needed for dimr xml import for
        /// retrieving the correct file importer based on the
        /// input file mentioned.
        /// </summary>
        public string MasterFileExtension => "mdw";

        public override bool CanImportOn(object targetObject)
        {
            return targetObject is ICompositeActivity || targetObject is WaveModel;
        }

        protected override object OnImportItem(string path, object target = null)
        {
            try
            {
                var importedWaveModel = new WaveModel(path) {WorkingDirectoryPathFunc = getWorkingDirectoryPathFunc};

                //replace the Wave Model
                if (target is WaveModel targetWaveModel)
                {
                    IProjectItem parent = targetWaveModel.Owner();

                    //add / replace the Wave Model in the project
                    if (parent is Folder folder)
                    {
                        folder.Items.Remove(targetWaveModel);
                        folder.Items.Add(importedWaveModel);
                    }

                    //add / replace the Wave Model in the integrated model
                    if (parent is ICompositeActivity compositeActivity)
                    {
                        importedWaveModel.MoveModelIntoIntegratedModel(null, compositeActivity);
                        return compositeActivity;
                    }

                    return ShouldCancel ? null : importedWaveModel;
                }

                //add / replace the Wave Model in the integrated model
                if (target is ICompositeActivity hydroModel)
                {
                    importedWaveModel.MoveModelIntoIntegratedModel(null, hydroModel);
                    return hydroModel;
                }

                return ShouldCancel ? null : importedWaveModel;
            }
            catch (Exception e)
            {
                if (e is ArgumentException || e is PathTooLongException || e is FormatException ||
                    e is OutOfMemoryException || e is IOException || e is InvalidOperationException)
                {
                    log.Error(string.Format("An error occurred while trying to import a {0}; Cause: ",
                                            Name), e);
                    return null;
                }

                // !!Unexpected type of exception (like NotSupportedException or NotImplementedException), so fail fast!!
                throw;
            }
        }
    }
}