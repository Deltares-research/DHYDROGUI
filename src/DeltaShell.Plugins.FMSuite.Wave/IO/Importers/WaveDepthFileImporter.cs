using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Utils.Editing;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.Wave.IO.Importers
{
    public class WaveDepthFileImporter : IFileImporter
    {
        private readonly Func<IEnumerable<WaveModel>> getModels;
        public WaveDepthFileImporter(string category, Func<IEnumerable<WaveModel>> getModelsFunc)
        {
            Category = category;
            getModels = getModelsFunc;
        }

        public string Name 
        {
            get { return "Delft3D Depth File"; }
        }

        public string Category { get; private set; }

        public Bitmap Image { get; private set; }

        public IEnumerable<Type> SupportedItemTypes
        {
            get { yield return typeof (CurvilinearCoverage); }
        }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public bool CanImportOnRootLevel
        {
            get { return false; }
        }

        public string FileFilter
        {
            get { return "Delft3D Depth File (*.dep)|*.dep|All Files (*.*)|*.*"; }
        }

        public object ImportItem(string path, object target = null)
        {
            var bathymetry = target as CurvilinearCoverage;
            if (bathymetry == null)
                throw new NotSupportedException("Need a target bed level to import depth file");

            var model = getModels()
                .First(m => WaveDomainHelper.GetAllDomains(m.OuterDomain)
                                            .Any(d => Equals(d.Bathymetry, bathymetry)));
            var domain = WaveDomainHelper.GetAllDomains(model.OuterDomain)
                                         .First(d => Equals(d.Bathymetry, bathymetry));
            
            model.BeginEdit(new DefaultEditAction("Importing bed level"));
            try
            {
                var uniqueFileName = model.ImportIntoModelDirectory(path);
                domain.BedLevelFileName = uniqueFileName;
                WaveModel.LoadBathymetry(model, Path.GetDirectoryName(path), domain);
            }
            finally
            {
                model.EndEdit();
            }

            return target;
        }

        public string TargetDataDirectory { get; set; }
        public bool ShouldCancel { get; set; }
        public ImportProgressChangedDelegate ProgressChanged { get; set; }
        public bool OpenViewAfterImport { get; private set; }
    }
}