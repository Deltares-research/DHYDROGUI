using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.Importers
{
    public class WaveDepthFileImporter : IFileImporter
    {
        private readonly Func<IEnumerable<WaveModel>> getModels;

        public WaveDepthFileImporter(string category, Func<IEnumerable<WaveModel>> getModelsFunc)
        {
            Category = category;
            getModels = getModelsFunc;
        }

        public string Name => "Delft3D Depth File";

        public string Category { get; private set; }
        public string Description => string.Empty;

        [ExcludeFromCodeCoverage]
        public Bitmap Image { get; private set; }

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(CurvilinearCoverage);
            }
        }

        public bool CanImportOnRootLevel => false;

        public string FileFilter => "Delft3D Depth File (*.dep)|*.dep|All Files (*.*)|*.*";

        public string TargetDataDirectory { get; set; }
        public bool ShouldCancel { get; set; }
        public ImportProgressChangedDelegate ProgressChanged { get; set; }
        public bool OpenViewAfterImport { get; private set; }

        public bool CanImportOn(object targetObject)
        {
            var coverage = targetObject as CurvilinearCoverage;
            if (coverage?.Store is WavmFileFunctionStore)
            {
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Imports the Bathymetry data from the file at the path <paramref name="path"/>.
        /// </summary>
        /// <param name="path"> The path to the Delft3D Depth File. </param>
        /// <param name="target"> The target CurvilinearCoverage to which the specified file should be loaded. </param>
        /// <returns> The updated target. </returns>
        /// <exception cref="T:System.NotSupportedException"> target == null =&gt; Need a target bed level to import depth file </exception>
        public object ImportItem(string path, object target = null)
        {
            var bathymetry = target as CurvilinearCoverage;
            if (bathymetry == null)
            {
                throw new NotSupportedException("Need a target bed level to import depth file");
            }

            WaveModel model = getModels()
                .First(m => WaveDomainHelper.GetAllDomains(m.OuterDomain)
                                            .Any(d => Equals(d.Bathymetry, bathymetry)));
            IWaveDomainData domain = WaveDomainHelper.GetAllDomains(model.OuterDomain)
                                                     .First(d => Equals(d.Bathymetry, bathymetry));

            model.BeginEdit("Importing bed level");
            try
            {
                string uniqueFileName = model.ImportIntoModelDirectory(path);
                domain.BedLevelFileName = uniqueFileName;
                WaveModel.LoadBathymetry(model, Path.GetDirectoryName(model.MdwFilePath), domain);
            }
            finally
            {
                model.EndEdit();
            }

            return target;
        }
    }
}