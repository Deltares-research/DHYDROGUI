using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using log4net;


namespace DeltaShell.Plugins.FMSuite.Wave.IO.Importers
{
    public class WaveModelFileImporter : IFileImporter
    {
        private readonly ILog log = LogManager.GetLogger(typeof(WaveModelFileImporter));

        public string Name
        {
            get { return "Waves Model"; }
        }

        public string Category
        {
            get { return "D-Flow FM 2D/3D"; }
        }

        public Bitmap Image
        {
            get { return Properties.Resources.wave; }
        }

        public IEnumerable<Type> SupportedItemTypes
        {
            get { yield return typeof(IHydroModel); }
        }
        
        public bool OpenViewAfterImport
        {
            get { return true; }
        }

        public bool CanImportOn(object targetObject)
        {
            return targetObject is ICompositeActivity || targetObject is WaveModel;
        }

        public bool CanImportOnRootLevel
        {
            get { return true; }
        }

        public string FileFilter
        {
            get { return "Master Definition WAVE File|*.mdw"; }
        }

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
                    importedWaveModel.IsCoupledToFlow = targetWaveModel.IsCoupledToFlow;
                    var parent = targetWaveModel.Owner();

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
                        importedWaveModel.IsCoupledToFlow = true;
                        importedWaveModel.MoveModelIntoIntegratedModel(null, compositeActivity);
                        return compositeActivity;
                    }
                    return ShouldCancel ? null : importedWaveModel;
                }

                //add / replace the Wave Model in the integrated model
                var hydroModel = target as ICompositeActivity;
                if (hydroModel != null)
                {
                    importedWaveModel.IsCoupledToFlow = true;
                    importedWaveModel.MoveModelIntoIntegratedModel(null, hydroModel);
                    return hydroModel;
                }
                importedWaveModel.IsCoupledToFlow = false;
                return ShouldCancel ? null : importedWaveModel;
            }
            catch (Exception e)
            {
                if (e is ArgumentException || e is PathTooLongException || e is FormatException ||
                    e is OutOfMemoryException || e is IOException || e is InvalidOperationException)
                {
                    log.Error(String.Format("An error occurred while trying to import a {0}; Cause: ",
                        Name), e);
                    return null;
                }

                // !!Unexpected type of exception (like NotSupportedException or NotImplementedException), so fail fast!!
                throw;
            }
        }
    }
}
