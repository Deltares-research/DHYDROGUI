using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.FMSuite.Common.IO.ImportExport.Exporters;
using DeltaShell.Plugins.FMSuite.Wave.IO.Exporters;
using DeltaShell.Plugins.FMSuite.Wave.IO.Importers;
using Mono.Addins;

namespace DeltaShell.Plugins.FMSuite.Wave
{
    [Extension(typeof(IPlugin))]
    public class WaveApplicationPlugin : ApplicationPlugin, IDataAccessListenersProvider
    {
        public override string Name => "Delft3D Wave";

        public override string DisplayName => "D-Waves Plugin";

        public override string Description => "A 2D/3D Wave module";

        public override string Version => GetType().Assembly.GetName().Version.ToString();

        public override string FileFormatVersion => "1.1.0.0";

        public override IEnumerable<ModelInfo> GetModelInfos()
        {
            yield return new ModelInfo
            {
                Name = "Waves Model",
                Category = "1D / 2D / 3D Standalone Models",
                Image = Properties.Resources.wave,
                GetParentProjectItem = owner =>
                {
                    Folder rootFolder = Application?.Project?.RootFolder;
                    return ApplicationPluginHelper.FindParentProjectItemInsideProject(rootFolder, owner) ?? rootFolder;
                },
                AdditionalOwnerCheck = owner =>
                    !(owner is ICompositeActivity) // Allow "standalone" wave models
                    || !((ICompositeActivity) owner).Activities.OfType<WaveModel>().Any() &&
                    owner is IHydroModel, // Don't allow multiple wave models in one composite activity
                CreateModel = t => new WaveModel()
            };
        }

        public override IEnumerable<IFileImporter> GetFileImporters()
        {
            yield return new WaveModelFileImporter();
            yield return new WaveGridFileImporter(Name, GetModels);
            yield return new WaveDepthFileImporter(Name, GetModels);
            yield return new WaveBoundaryFileImporter();
            yield return new WavmFileImporter();
        }

        public override IEnumerable<IFileExporter> GetFileExporters()
        {
            yield return new WaveModelFileExporter();
            yield return new Delft3DDepthFileExporter();
            yield return new Delft3DGridFileExporter();
        }

        public override IEnumerable<Assembly> GetPersistentAssemblies()
        {
            yield return typeof(WaveModel).Assembly;
        }

        public IEnumerable<WaveModel> GetModels()
        {
            return Application.Project.RootFolder.GetAllItemsRecursive().OfType<WaveModel>();
        }

        public IEnumerable<IDataAccessListener> CreateDataAccessListeners()
        {
            yield return new WaveDataAccessListener();
        }
    }
}