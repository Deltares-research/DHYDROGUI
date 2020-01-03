using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Wave.IO.Importers
{
    public class WaveModelFileImporter : IFileImporter
    {
        private readonly ILog log = LogManager.GetLogger(typeof(WaveModelFileImporter));

        public string Name => "Waves Model";

        public string Category => "D-Flow FM 2D/3D";

        public string Description => string.Empty;

        public Bitmap Image => Resources.wave;

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(IHydroModel);
            }
        }

        public bool OpenViewAfterImport => true;

        public bool CanImportOn(object targetObject)
        {
            return targetObject is ICompositeActivity || targetObject is WaveModel;
        }

        public bool CanImportOnRootLevel => true;

        public string FileFilter => "Master Definition WAVE File|*.mdw";

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public object ImportItem(string path, object target = null)
        {
            try
            {
                var importedWaveModel = new WaveModel(path);

                //replace the Wave Model
                var targetWaveModel = target as WaveModel;
                if (targetWaveModel != null)
                {
                    IProjectItem parent = targetWaveModel.Owner();

                    //add / replace the Wave Model in the project
                    var folder = parent as Folder;
                    if (folder != null)
                    {
                        folder.Items.Remove(targetWaveModel);
                        folder.Items.Add(importedWaveModel);
                    }

                    //add / replace the Wave Model in the integrated model
                    var compositeActivity = parent as ICompositeActivity;
                    if (compositeActivity != null)
                    {
                        importedWaveModel.MoveModelIntoIntegratedModel(null, compositeActivity);
                        return compositeActivity;
                    }
                    return ShouldCancel ? null : importedWaveModel;
                }

                //add / replace the Wave Model in the integrated model
                var hydroModel = target as ICompositeActivity;
                if (hydroModel != null)
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