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
using DHYDRO.Common.Extensions;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.Importers
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
        
        public bool CanImportDimrFile(string path) => Path.GetExtension(path).EqualsCaseInsensitive(".mdw");

        public override bool CanImportOnRootLevel => true;

        public override string FileFilter => "Master Definition WAVE File|*.mdw";

        public override string TargetDataDirectory { get; set; }

        public override bool ShouldCancel { get; set; }

        public override ImportProgressChangedDelegate ProgressChanged { get; set; }

        public override bool CanImportOn(object targetObject)
        {
            return targetObject is ICompositeActivity || targetObject is WaveModel;
        }

        protected override object OnImportItem(string path, object target = null)
        {
            try
            {
                return ImportWaveModel(path, target);
            }
            catch (Exception e) when (e is ArgumentException    || 
                                      e is PathTooLongException || 
                                      e is FormatException      ||
                                      e is OutOfMemoryException || 
                                      e is IOException          || 
                                      e is InvalidOperationException)
            {
                log.Error($"An error occurred while trying to import a {Name}; Cause: ", e);
                return null;
            }
        }

        private object ImportWaveModel(string path, object target)
        {
            var importedWaveModel = new WaveModel(path, connectToOutput: false)
            {
                WorkingDirectoryPathFunc = getWorkingDirectoryPathFunc
            };

            if (TryGetParentHydroModel(target, out ICompositeActivity parentModel))
            {
                importedWaveModel.MoveModelIntoIntegratedModel(null, parentModel);
                return parentModel;
            }

            //replace the Wave Model
            if (target is WaveModel targetWaveModel && 
                targetWaveModel.Owner() is Folder folder)
            {
                folder.Items.Remove(targetWaveModel);
                folder.Items.Add(importedWaveModel);
            }

            return ShouldCancel ? null : importedWaveModel;
        }

        private static bool TryGetParentHydroModel(object target, out ICompositeActivity hydroModel)
        {
            hydroModel = null;

            switch (target)
            {
                case ICompositeActivity compositeActivity:
                    hydroModel = compositeActivity;
                    return true;
                case WaveModel targetWaveModel when targetWaveModel.Owner() is ICompositeActivity parentCompositeActivity:
                    hydroModel = parentCompositeActivity;
                    return true;
                default:
                    return false;
            }
        }
    }
}