using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using Deltares.Infrastructure.API.DependencyInjection;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extensions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.NHibernate;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using Mono.Addins;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel
{
    [Extension(typeof(IPlugin))]
    public class WaterQualityModelApplicationPlugin : ApplicationPlugin
    {
        private IEnumerable<IFileImporter> _getFileImporters;

        public override string Name => "Water quality model";

        public override string DisplayName => "D-Water Quality Plugin";

        public override string Description => "Allows to simulate water quality in rivers and channels.";

        public override string Version => AssemblyUtils.GetAssemblyInfo(GetType().Assembly).Version;

        public override string FileFormatVersion => "3.6.0.0";

        public override IApplication Application
        {
            get => base.Application;
            set
            {
                if (Application != null)
                {
                    Application.ActivityRunner.ActivityStatusChanged -= ActivityRunner_OnActivityStatusChanged;
                    Application.ProjectService.ProjectOpened -= Application_OnProjectOpened;
                    Application.ProjectService.ProjectCreated -= Application_OnProjectOpened;
                    Application.ProjectService.ProjectClosing -= Application_OnProjectClosing;
                    Application.ProjectService.ProjectSaving -= Application_ProjectSaving;
                    Application.ProjectService.ProjectSaved -= Application_ProjectSaveFinished;
                }

                base.Application = value;

                if (Application != null)
                {
                    // list to activities. Especially the importers, so you can set extra information on them
                    Application.ActivityRunner.ActivityStatusChanged += ActivityRunner_OnActivityStatusChanged;
                    Application.ProjectService.ProjectOpened += Application_OnProjectOpened;
                    Application.ProjectService.ProjectCreated += Application_OnProjectOpened;
                    Application.ProjectService.ProjectClosing += Application_OnProjectClosing;
                    Application.ProjectService.ProjectSaving += Application_ProjectSaving;
                    Application.ProjectService.ProjectSaved += Application_ProjectSaveFinished;
                }
            }
        }

        /// <summary>
        /// Function that can be called if the hyd file could not be found.
        /// Can be used to throw a modal popup.
        /// </summary>
        public Action<WaterQualityModel, string> HydFileNotFoundGuiHandler { get; set; }

        /// <summary>
        /// Function that can be called if the process definition file could not be found.
        /// Can be used to throw a modal popup.
        /// </summary>
        public Action<WaterQualityModel, string> ProcessDefinitionFilesNotFoundGuiHandler { get; set; }

        /// <summary>
        /// Excecutes all spatial operations available in <see cref="WaterQualityModel"/>
        /// instances available in the <see cref="Project"/>.
        /// </summary>
        public static void ExecuteAllWaterQualitySpatialOperations(Project project)
        {
            foreach (IDataItem dataItem in project.GetAllItemsRecursive().OfType<WaterQualityModel>()
                                                  .SelectMany(waq => waq.AllDataItems))
            {
                var spatialOperationSetValueConverter = dataItem.ValueConverter as SpatialOperationSetValueConverter;

                if (spatialOperationSetValueConverter != null)
                {
                    spatialOperationSetValueConverter.LoadConvertedValue(dataItem.ComposedValue);

                    // this execute is required to get the result of the spatial operation set in the value converter.
                    // The value converter listens to events, but no events are sent when the project is loading.
                    // See TOOLS-22124 for more info
                    spatialOperationSetValueConverter.SpatialOperationSet.Execute();
                }
            }
        }

        public override IEnumerable<ModelInfo> GetModelInfos()
        {
            yield return new ModelInfo
            {
                Name = "Water Quality Model",
                Category = "1D / 2D / 3D Standalone Models",
                GetParentProjectItem = owner =>
                {
                    Folder rootFolder = Application?.ProjectService.Project?.RootFolder;
                    return ApplicationPluginHelper.FindParentProjectItemInsideProject(rootFolder, owner) ?? rootFolder;
                },
                AdditionalOwnerCheck =
                    owner =>
                        !(owner is ICompositeActivity), // Don't allow water quality models to be added to composite activity
                CreateModel = owner =>
                {
                    var model = new WaterQualityModel();
                    model.SetWorkingDirectoryInModelSettings(() => Application.WorkDirectory);
                    return model;
                }
            };
        }

        public override IEnumerable<IFileImporter> GetFileImporters()
        {
            yield return new SubFileImporter();
            yield return new HydFileImporter(() => Application.WorkDirectory);
            yield return new LoadsImporter();
            yield return new ObservationPointImporter();
            yield return new BoundaryDataTableImporter();
            yield return new LoadsDataTableImporter();
            yield return new WaterQualityObservationAreaImporter();
        }

        public override IEnumerable<IFileExporter> GetFileExporters()
        {
            yield return new InputFileExporter();
        }

        private void Application_ProjectSaveFinished(object sender, EventArgs<Project> e)
        {
            Project project = e.Value;
            if (project == null)
            {
                return;
            }

            IEnumerable<WaterQualityModel> allWaqModels = Application.ProjectService.Project.RootFolder
                                                                     .GetAllModelsRecursive()
                                                                     .OfType<WaterQualityModel>();
            allWaqModels.ForEach(m =>
            {
                m.SetupModelDataFolderStructure(Application.ProjectService);
                m.SetEnableMarkOutputOutOfSync(true);
                RemoveDisconnectedOutputFiles(m);
            });
        }

        private void RemoveDisconnectedOutputFiles(WaterQualityModel model)
        {
            if (model.OutputFolder?.Path == null)
            {
                FileUtils.DeleteIfExists(model.ModelSettings.OutputDirectory);
            }
        }

        private void Application_ProjectSaving(object sender, EventArgs<Project> e)
        {
            Project project = e.Value;
            if (project == null)
            {
                return;
            }

            IEnumerable<WaterQualityModel> allWaqModels =
                Application.ProjectService.Project.RootFolder.GetAllModelsRecursive().OfType<WaterQualityModel>();
            allWaqModels.ForEach(m => m.SetEnableMarkOutputOutOfSync(false));
        }

        private void Application_OnProjectClosing(object sender, EventArgs<Project> e)
        {
            Project project = e.Value;
            ((INotifyCollectionChange) project).CollectionChanged -= Project_OnCollectionChanged;
            ((INotifyPropertyChanged) project).PropertyChanged -= Project_OnPropertyChanged;
        }

        private void OnHydroDataNotFound(WaterQualityModel model, string filePath)
        {
            if (HydFileNotFoundGuiHandler != null)
            {
                HydFileNotFoundGuiHandler(model, filePath);
            }
        }

        private void OnProcessDefinitionFilesNotFound(WaterQualityModel model, string filePath)
        {
            if (ProcessDefinitionFilesNotFoundGuiHandler != null)
            {
                ProcessDefinitionFilesNotFoundGuiHandler(model, filePath);
            }
        }

        private void ActivityRunner_OnActivityStatusChanged(object sender,
                                                            ActivityStatusChangedEventArgs
                                                                activityStatusChangedEventArgs)
        {
            if (activityStatusChangedEventArgs.NewStatus != ActivityStatus.Initializing)
            {
                return;
            }

            var importActivity = sender as FileImportActivity;
            if (importActivity != null)
            {
                SetupLoadsImporter(importActivity);
                SetupObservationPointImporter(importActivity);
                SetupObservationAreaImporter(importActivity);
            }
        }

        private void SetupLoadsImporter(FileImportActivity importActivity)
        {
            var loadsImporter = importActivity.FileImporter as LoadsImporter;
            if (loadsImporter == null)
            {
                return;
            }

            var loadList = (IEventedList<WaterQualityLoad>) importActivity.Target;
            WaterQualityModel model = Application.ProjectService.Project.RootFolder.GetAllModelsRecursive().OfType<WaterQualityModel>()
                                                 .First(m => Equals(m.Loads, loadList));

            loadsImporter.ModelCoordinateSystem = model.CoordinateSystem;
            loadsImporter.GetDefaultZValue = model.GetDefaultZ;
        }

        private void SetupObservationPointImporter(FileImportActivity importActivity)
        {
            var observationPointsImporter = importActivity.FileImporter as ObservationPointImporter;
            if (observationPointsImporter == null)
            {
                return;
            }

            var obsList = (IEventedList<WaterQualityObservationPoint>) importActivity.Target;
            WaterQualityModel model = Application.ProjectService.Project.RootFolder.GetAllModelsRecursive().OfType<WaterQualityModel>()
                                                 .First(m => Equals(m.ObservationPoints, obsList));

            observationPointsImporter.ModelCoordinateSystem = model.CoordinateSystem;
            observationPointsImporter.GetDefaultZValue = model.GetDefaultZ;
        }

        private void SetupObservationAreaImporter(FileImportActivity importActivity)
        {
            var observationAreaImporter = importActivity.FileImporter as WaterQualityObservationAreaImporter;
            if (observationAreaImporter != null)
            {
                var oberservationAreas = (WaterQualityObservationAreaCoverage) importActivity.Target;
                WaterQualityModel model =
                    Application.ProjectService.Project.RootFolder.Models.OfType<WaterQualityModel>()
                               .First(m => Equals(m.ObservationAreas, oberservationAreas));
                IDataItem observationAreasDataItem =
                    model.GetDataItemByTag(WaterQualityModel.ObservationAreasDataItemMetaData.Tag);

                observationAreaImporter.GetDataItemForTarget = coverage => observationAreasDataItem;
                observationAreaImporter.ModelCoordinateSystem = model.CoordinateSystem;
            }
        }

        /// <summary>
        /// Project is not null anymore in the application.
        /// Subscribe to the event so we can check for new waq models.
        /// </summary>
        /// <param name="project"> </param>
        private void Application_OnProjectOpened(object sender, EventArgs<Project> e)
        {
            Project project = e.Value;
            IEnumerable<WaterQualityModel> allWaqModels =
                project.RootFolder.GetAllModelsRecursive().OfType<WaterQualityModel>().ToList();

            allWaqModels.ForEach(m => m.SetWorkingDirectoryInModelSettings(() => Application.WorkDirectory));
            allWaqModels.ForEach(ReimportHydFileForWaterQualityModel);
            allWaqModels.ForEach(RelinkToProcessDefinitionFiles);

            ((INotifyCollectionChange) project).CollectionChanged += Project_OnCollectionChanged;
            ((INotifyPropertyChanged) project).PropertyChanged += Project_OnPropertyChanged;

            ExecuteAllWaterQualitySpatialOperations(project);
        }

        private void ReimportHydFileForWaterQualityModel(WaterQualityModel waqModel)
        {
            var importedHydFile = waqModel.HydroData as HydFileData;
            if (importedHydFile == null)
            {
                return;
            }

            if (importedHydFile.Path.Exists)
            {
                new HydFileImporter
                {
                    MarkModelOutputOutOfSync = false,
                    SkipImportTimers = true /*We don't reimport the timers from the hyd file when opening the project */
                }.ImportItem(importedHydFile.FilePath, waqModel);
            }
            else
            {
                OnHydroDataNotFound(waqModel, importedHydFile.FilePath);
            }
        }

        private void RelinkToProcessDefinitionFiles(WaterQualityModel waqModel)
        {
            if (waqModel.SubstanceProcessLibrary == null)
            {
                return;
            }

            string processDefinitionFileName = waqModel.SubstanceProcessLibrary.ProcessDefinitionFilesPath;
            if (!File.Exists(processDefinitionFileName + ".def"))
            {
                OnProcessDefinitionFilesNotFound(waqModel, processDefinitionFileName);
            }
        }

        /// <summary>
        /// Listen to the creation of new waq models.
        /// Set the project data directory in the models.
        /// </summary>
        private void Project_OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var model = e.GetRemovedOrAddedItem() as WaterQualityModel;
                if (model != null)
                {
                    model.SetupModelDataFolderStructure(Application.ProjectService);
                }
            }
        }

        /// <summary>
        /// Listens to a change of a waq model's name property.
        /// </summary>
        /// <param name="sender"> </param>
        /// <param name="e"> </param>
        private void Project_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name" && sender is WaterQualityModel model)
            {
                model.SetupModelDataFolderStructure(Application.ProjectService);
            }
        }

        /// <inheritdoc/>
        public override void AddRegistrations(IDependencyInjectionContainer container)
        {
            container.Register<IDataAccessListenersProvider, WaterQualityDataAccessListenersProvider>(LifeCycle.Transient);
        }
    }
}