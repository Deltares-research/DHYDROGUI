using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using Deltares.Infrastructure.API.DependencyInjection;
using DeltaShell.Dimr.Export;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Converters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Exporters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Files.Evaporation;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.FileWriters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.NHibernate;
using log4net;
using Mono.Addins;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    [Extension(typeof(IPlugin))]
    public class RainfallRunoffApplicationPlugin : ApplicationPlugin
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RainfallRunoffApplicationPlugin));

        private static readonly IEvaporationExporter evaporationExporter = new EvaporationExporter(new EvaporationFileWriter(),
                                                                                                   new EvaporationFileCreator(),
                                                                                                   new EvaporationFileNameConverter(),
                                                                                                   new IOEvaporationMeteoDataSourceConverter());

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

        public override string FileFormatVersion => "3.7.0.0";

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
                    base.Application.ProjectService.ProjectSaving -= SynchronizeDataBeforeSaving;
                    base.Application.ProjectService.ProjectSaved -= SaveToFile;
                    base.Application.ProjectService.ProjectOpened -= Application_ProjectOpened;
                    base.Application.ProjectService.ProjectCreated -= Application_ProjectOpened;
                    base.Application.ProjectService.ProjectClosing -= Application_ProjectClosing;
                }

                base.Application = value;

                if (base.Application != null)
                {
                    base.Application.ActivityRunner.ActivityStatusChanged += ActivityRunnerOnActivityStatusChanged;
                    base.Application.ProjectService.ProjectSaving += SynchronizeDataBeforeSaving;
                    base.Application.ProjectService.ProjectSaved += SaveToFile;
                    base.Application.ProjectService.ProjectOpened += Application_ProjectOpened;
                    base.Application.ProjectService.ProjectCreated += Application_ProjectOpened;
                    base.Application.ProjectService.ProjectClosing += Application_ProjectClosing;
                }
            }
        }
        public override void Activate()
        {
            base.Activate();
            DimrConfigModelCouplerFactory.CouplerProviders.Add(new RRDimrConfigModelCouplerProvider());
        }

        private void Application_ProjectOpened(object sender, EventArgs<Project> e)
        {
            Project project = e.Value;
            foreach (var rainfallRunoffModel in GetModels(e.Value))
            {
                rainfallRunoffModel.WorkingDirectoryPathFunc = () => Application?.WorkDirectory;
                rainfallRunoffModel.DimrRunner.FileExportService = Application.FileExportService;

                UpdateDataAfterOpened(rainfallRunoffModel);
            }
            
            project.CollectionChanging += OnProjectCollectionChanging;
        }
        
        private void Application_ProjectClosing(object sender, EventArgs<Project> e)
        {
            e.Value.CollectionChanging -= OnProjectCollectionChanging;
        }
        
        private void OnProjectCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            if (e.Action == NotifyCollectionChangeAction.Add && e.Item is RainfallRunoffModel model)
            {
                model.DimrRunner.FileExportService = Application.FileExportService;
            }
        }
        
        private void SynchronizeDataBeforeSaving(object sender, EventArgs<Project> e)
        {
            var rrModels = GetModels(e.Value);
            foreach (var rainfallRunoffModel in rrModels)
            {
                rainfallRunoffModel.UpdateUnpavedDataExtended();
            }
        }
        
        private void UpdateDataAfterOpened(RainfallRunoffModel rainfallRunoffModel)
        {
            rainfallRunoffModel.UpdateUnpavedDataWithExtendedData();
        }

        private void SaveToFile(object sender, EventArgs<Project> e)
        {
            Project project = e.Value;
            if (project == null || project.RootFolder == null) return;

            var projectDataFolderDirectory = Application.ProjectService.ProjectFilePath + "_data";
            var rrModels = GetModels(project);
            var exporter = new RainfallRunoffModelExporter(new BasinGeometryShapeFileSerializer(), evaporationExporter);

            foreach (var rainfallRunoffModel in rrModels)
            {
                try
                {
                    var savePath = Path.Combine(projectDataFolderDirectory, rainfallRunoffModel.Name);

                    exporter.Export(rainfallRunoffModel, savePath);

                    if (rainfallRunoffModel.OutputIsEmpty && Directory.Exists(savePath))
                    {
                        rainfallRunoffModel.OutputFiles.DeleteOutputFiles(savePath);
                    }

                    MoveOutputFromWorkingDirectory(rainfallRunoffModel, savePath);
                }
                catch (IOException exception)
                {
                    Log.Error(string.Format(Properties.Resources.RainfallRunoffApplicationPluging_Could_not_save_RR_model,
                                                rainfallRunoffModel.Name, exception.Message));
                    return;
                }
            }
        }

        private static void MoveOutputFromWorkingDirectory(RainfallRunoffModel rainfallRunoffModel, string savePath)
        {
            rainfallRunoffModel.OutputFiles.CopyTo(savePath);
            rainfallRunoffModel.DisconnectOutput();
            rainfallRunoffModel.ConnectOutput(savePath);
        }

        private void ActivityRunnerOnActivityStatusChanged(object sender, ActivityStatusChangedEventArgs activityStatusChangedEventArgs)
        {
            if (activityStatusChangedEventArgs.NewStatus != ActivityStatus.Initializing || sender as RainfallRunoffModel == null)
            {
                return;
            }

            Log.Info("DeltaShell version: " + Application.Version);
            Log.Info(Application.PluginVersions);
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
            yield return new MeteoDataImporter(new PrecipitationDataImporter(),
                                               new EvaporationDataImporter(),
                                               new TemperatureDataImporter());
            yield return new NWRWCatchmentFrom3BImporter();
        }

        public override IEnumerable<IFileExporter> GetFileExporters()
        {
            yield return new MeteoDataExporter(evaporationExporter); 
            yield return new RainfallRunoffModelExporter(new BasinGeometryShapeFileSerializer(), evaporationExporter); 
        }

        private static IEnumerable<RainfallRunoffModel> GetModels(Project project)
        {
            if (project != null)
            {
                return project.RootFolder.GetAllModelsRecursive().OfType<RainfallRunoffModel>();
            }
            return Enumerable.Empty<RainfallRunoffModel>();
        }

        /// <inheritdoc/>
        public override void AddRegistrations(IDependencyInjectionContainer container)
        {
            container.Register<IDataAccessListenersProvider, RainfallRunoffDataAccessListenersProvider>(LifeCycle.Transient);
        }
    }
}