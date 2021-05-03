using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers;
using log4net;
using Mono.Addins;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    [Extension(typeof(IPlugin))]
    public class RainfallRunoffApplicationPlugin : ApplicationPlugin, IDataAccessListenersProvider
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RainfallRunoffApplicationPlugin));

        public override string Name
        {
            get { return "rainfall runoff model"; }
        }

        public override string DisplayName
        {
            get { return "D-Rainfall Runoff Plugin"; }
        }

        public override string Description
        {
            get { return Properties.Resources.RainfallRunoffApplicationPlugin_Description; }
        }

        public override string Version
        {
            get { return GetType().Assembly.GetName().Version.ToString(); }
        }

        public override string FileFormatVersion
        {
            get { return "3.5.0.0"; }
        }

        public override IApplication Application
        {
            get
            {
                return base.Application;
            }
            set
            {
                if (base.Application != null)
                {
                    base.Application.ActivityRunner.ActivityStatusChanged -= ActivityRunnerOnActivityStatusChanged;
                    base.Application.ProjectSaved -= SaveToFile;
                    base.Application.ProjectOpened -= ApplicationOnProjectOpened;
                }

                base.Application = value;

                if (base.Application != null)
                {
                    base.Application.ActivityRunner.ActivityStatusChanged += ActivityRunnerOnActivityStatusChanged;
                    base.Application.ProjectSaved += SaveToFile;
                    base.Application.ProjectOpened += ApplicationOnProjectOpened;
                }
            }
        }

        public override void Activate()
        {
            base.Activate();
            DimrConfigModelCouplerFactory.CouplerProviders.Add(new RRDimrConfigModelCouplerProvider());
        }

        private void ApplicationOnProjectOpened(Project project)
        {
            foreach (var rainfallRunoffModel in project.RootFolder.GetAllModelsRecursive().OfType<RainfallRunoffModel>())
            {
                rainfallRunoffModel.WorkingDirectoryPathFunc = () => Application?.WorkDirectory;
            }
        }

        private void SaveToFile(Project project)
        {
            if (project == null || project.RootFolder == null) return;

            var projectDataFolderDirectory = Application.ProjectDataDirectory;
            var rrModels = project.RootFolder.GetAllModelsRecursive().OfType<RainfallRunoffModel>();
            var exporter = new RainfallRunoffModelExporter();

            foreach (var rainfallRunoffModel in rrModels)
            {
                var savePath = Path.Combine(projectDataFolderDirectory, rainfallRunoffModel.Name);
                
                exporter.Export(rainfallRunoffModel, savePath);
                MoveOutputFromWorkingDirectory(rainfallRunoffModel, savePath);
            }
        }

        private static void MoveOutputFromWorkingDirectory(RainfallRunoffModel rainfallRunoffModel, string savePath)
        {
            var paths = rainfallRunoffModel.OutputFunctions
                                           .Select(f => f.Store)
                                           .OfType<IFileBased>()
                                           .Select(f => f.Path)
                                           .Where(p => p != null)
                                           .Distinct()
                                           .ToArray();

            if (paths.Length == 0)
            {
                return;
            }

            rainfallRunoffModel.DisconnectOutput();
            foreach (string path in paths)
            {
                if (!path.StartsWith(savePath))
                {
                    File.Copy(path, Path.Combine(savePath, Path.GetFileName(path)));
                }
            }

            rainfallRunoffModel.ConnectOutput(savePath);
        }

        private void ActivityRunnerOnActivityStatusChanged(object sender, ActivityStatusChangedEventArgs activityStatusChangedEventArgs)
        {
            if (activityStatusChangedEventArgs.NewStatus == ActivityStatus.Initializing && sender as RainfallRunoffModel != null)
            {
                Log.Info("DeltaShell version: " + Application.Version);
                Log.Info(Application.PluginVersions);
            }
        }

        public override IEnumerable<ModelInfo> GetModelInfos()
        {
            yield return new ModelInfo
                {
                    Name = "Rainfall Runoff Model",
                    Category = "1D / 2D / 3D Standalone Models",
                    AdditionalOwnerCheck = owner => !(owner is ICompositeActivity) // Allow "standalone" rainfall runoff models
                            || (!((ICompositeActivity)owner).Activities.OfType<RainfallRunoffModel>().Any() && owner is IHydroModel), // Don't allow multiple rainfall runoff models in one composite activity
                    CreateModel = owner => new RainfallRunoffModel
                    {
                        Name = "Rainfall Runoff",
                        WorkingDirectoryPathFunc = ()=> Application?.WorkDirectory
                    }
                };
        }

        public override IEnumerable<IFileImporter> GetFileImporters()
        {
            yield return new RainfallRunoffModelImporter();
            yield return new MeteoDataImporter();
            yield return new PolderFromGisImporter();
            yield return new NWRWCatchmentFrom3BImporter();
        }

        public override IEnumerable<IFileExporter> GetFileExporters()
        {
            yield return new MeteoDataExporter(); 
            yield return new RainfallRunoffModelExporter(); 
        }

        public override IEnumerable<Assembly> GetPersistentAssemblies()
        {
            yield return GetType().Assembly;
        }

        public IEnumerable<IDataAccessListener> CreateDataAccessListeners()
        {
            yield return new RainfallRunoffDataAccessListener();
        }
    }
}