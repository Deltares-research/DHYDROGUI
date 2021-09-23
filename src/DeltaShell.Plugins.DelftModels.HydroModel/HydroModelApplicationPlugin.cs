using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Reflection;
using DeltaShell.Dimr;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using log4net;
using Mono.Addins;

namespace DeltaShell.Plugins.DelftModels.HydroModel
{
    [Extension(typeof(IPlugin))]
    public class HydroModelApplicationPlugin : ApplicationPlugin, IDataAccessListenersProvider
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HydroModelApplicationPlugin));

        public HydroModelApplicationPlugin()
        {
            MainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        public override string Name
        {
            get
            {
                return "Hydro Model";
            }
        }

        public override string DisplayName
        {
            get
            {
                return DelftTools.Shell.Core.Properties.Resources.HydroModelApplicationPlugin_DisplayName_Hydro_Model_Plugin;
            }
        }

        public override string Description
        {
            get
            {
                return Properties.Resources.HydroModelApplicationPlugin_Description;
            }
        }

        public override string Version
        {
            get
            {
                return AssemblyUtils.GetAssemblyInfo(GetType().Assembly).Version;
            }
        }

        public override string FileFormatVersion => "1.3.0.0";

        public override IApplication Application
        {
            get
            {
                return base.Application;
            }
            set
            {
                if (Application != null)
                {
                    Application.ActivityRunner.ActivityStatusChanged -= ActivityRunnerOnActivityStatusChanged;
                    Application.ProjectSaving -= ApplicationProjectSaving;
                    Application.ProjectSaved -= ApplicationProjectSavedOrFailed;
                    Application.ProjectSaveFailed -= ApplicationProjectSavedOrFailed;
                    Application.ProjectOpened -= ApplicationProjectOpened;
                    Application.ProjectClosing -= ApplicationProjectClosing;
                    Application.AfterRun -= ApplicationRemoveProjectExporter;
                }

                base.Application = value;

                if (Application != null)
                {
                    Application.ActivityRunner.ActivityStatusChanged += ActivityRunnerOnActivityStatusChanged;
                    Application.ProjectSaving += ApplicationProjectSaving;
                    Application.ProjectSaveFailed += ApplicationProjectSavedOrFailed;
                    Application.ProjectSaved += ApplicationProjectSavedOrFailed;
                    Application.ProjectOpened += ApplicationProjectOpened;
                    Application.ProjectClosing += ApplicationProjectClosing;
                    Application.AfterRun += ApplicationRemoveProjectExporter;
                }
            }
        }

        public static int MainThreadId { get; set; }

        public override IEnumerable<ModelInfo> GetModelInfos()
        {
            var modelGroupNameLookUp = new Dictionary<ModelGroup, string>
            {
                {ModelGroup.Empty, DelftTools.Shell.Core.Properties.Resources.HydroModelApplicationPlugin_GetModelInfos_Empty_Integrated_Model},
                {ModelGroup.FMWaveRtcModels, DelftTools.Shell.Core.Properties.Resources.HydroModelApplicationPlugin_GetModelInfos__2D_3D_Integrated_Model}
            };

            foreach (ModelGroup modelGroup in Enum.GetValues(typeof(ModelGroup)))
            {
                if (!HydroModel.CanBuildModel(modelGroup) || modelGroup == ModelGroup.All)
                {
                    continue;
                }

                yield return new ModelInfo
                {
                    Name = modelGroupNameLookUp[modelGroup],
                    Category = DelftTools.Shell.Core.Properties.Resources.HydroModelApplicationPlugin_GetModelInfos__1D___2D___3D_Integrated_Models,
                    GetParentProjectItem = owner =>
                    {
                        Folder rootFolder = Application?.Project?.RootFolder;
                        return ApplicationPluginHelper.FindParentProjectItemInsideProject(rootFolder, owner) ?? rootFolder;
                    },
                    AdditionalOwnerCheck = owner => !(owner is ICompositeActivity), // Don't allow creation of sub-hydro models
                    CreateModel = owner =>
                    {
                        var hydroModel = HydroModel.BuildModel(modelGroup);
                        hydroModel.WorkingDirectoryPathFunc = () => Application.WorkDirectory;
                        return hydroModel;
                    }
                };
            }
        }

        public override IEnumerable<Assembly> GetPersistentAssemblies()
        {
            yield return GetType().Assembly;
        }

        public override void Activate()
        {
            var initializeThread = new Thread(InitializeModelBuilder) {Priority = ThreadPriority.BelowNormal};
            initializeThread.Start();

            base.Activate();
        }

        public override IEnumerable<IFileExporter> GetFileExporters()
        {
            yield return new DHydroConfigXmlExporter();
        }

        public override IEnumerable<IFileImporter> GetFileImporters()
        {
            yield return new DHydroConfigXmlImporter(() => Application.FileImporters.OfType<IDimrModelFileImporter>().ToList(),
                                                     () => Application.WorkDirectory);
        }

        public IEnumerable<IDataAccessListener> CreateDataAccessListeners()
        {
            return Enumerable.Empty<IDataAccessListener>();
        }

        private void ApplicationRemoveProjectExporter()
        {
            var exporters = (List<IFileExporter>) Application.FileExporters;
            exporters.RemoveAll(e => e is IProjectItemExporter);
        }

        private void ApplicationProjectClosing(Project project)
        {
            Application.Project.CollectionChanging -= OnProjectCollectionChanging;
            Application.Project.PropertyChanged -= OnProjectPropertyChanged;
        }

        private void ApplicationProjectOpened(Project project)
        {
            // relink all dataitems (between rtc and flowFM) for all hydromodels
            Application.GetAllModelsInProject().OfType<HydroModel>().ForEach(hm =>
            {
                hm.RelinkDataItems();
                hm.WorkingDirectoryPathFunc =
                    () => Application.WorkDirectory;
            });

            Application.Project.CollectionChanging += OnProjectCollectionChanging;
            Application.Project.PropertyChanged += OnProjectPropertyChanged;
        }

        private void OnProjectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(INameable.Name) && sender is IModel renamedModel)
            {
                TrimModelName(renamedModel);
                MakeModelNameUnique(renamedModel);
            }
        }

        private void OnProjectCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            if (e.Action == NotifyCollectionChangeAction.Add && e.Item is IModel addedModel)
            {
                TrimAllModelNames(addedModel);
                MakeAllModelNamesUnique(addedModel);
            }
        }

        private static void TrimAllModelNames(IModel model)
        {
            TrimModelName(model);

            foreach (IModel subModel in model.GetDirectChildren().OfType<IModel>())
            {
                TrimAllModelNames(subModel);
            }
        }

        private static void TrimModelName(IModel renamedModel)
        {
            string modelName = renamedModel.Name;
            string trimmedName = modelName.Trim();
            if (modelName != trimmedName)
            {
                renamedModel.Name = trimmedName;
            }
        }

        private void MakeAllModelNamesUnique(IModel model)
        {
            MakeModelNameUnique(model);

            foreach (IModel subModel in model.GetDirectChildren().OfType<IModel>())
            {
                MakeAllModelNamesUnique(subModel);
            }
        }

        private void MakeModelNameUnique(IModel model)
        {
            IModel[] allModels = Application.Project.GetAllItemsRecursive()
                                            .OfType<IModel>()
                                            .Where(m => m != model)
                                            .ToArray();

            if (allModels.Any(m => m.Name == model.Name))
            {
                model.Name = NamingHelper.GetUniqueName(model.Name + " ({0})", allModels);
            }
        }

        private void ApplicationProjectSaving(Project project)
        {
            if (project == null || project.RootFolder == null)
            {
                return;
            }

            // go through all hydro models and unlink all objects that link between rtc and flowFM, 
            // because flow is not saved in the database.
            foreach (HydroModel hydroModel in project.RootFolder.GetAllItemsRecursive().OfType<HydroModel>())
            {
                hydroModel.SaveLinks();
                hydroModel.UnlinkAndRememberDataItems();
            }
        }

        private void ApplicationProjectSavedOrFailed(Project project)
        {
            if (project == null || project.RootFolder == null)
            {
                return;
            }

            foreach (HydroModel hydroModel in project.RootFolder.GetAllItemsRecursive().OfType<HydroModel>())
            {
                hydroModel.RelinkDataItems();
            }
        }

        private void ActivityRunnerOnActivityStatusChanged(object sender, ActivityStatusChangedEventArgs activityStatusChangedEventArgs)
        {
            var hydroModel = sender as HydroModel;
            if (activityStatusChangedEventArgs.NewStatus == ActivityStatus.Initializing && hydroModel != null)
            {
                Log.Info(string.Format(DelftTools.Shell.Core.Properties.Resources.HydroModelApplicationPlugin_ActivityRunnerOnActivityStatusChanged_DeltaShell_version___0_, Application.Version));
                Log.Info(Application.PluginVersions);
            }
        }

        /// <summary>
        /// Initialize the model builder, and thus the IronPython scripting engine.
        /// </summary>
        private void InitializeModelBuilder()
        {
            // The HydroModelBuilder is constructed in order to trigger the
            // initialization of the IronPython scripting engine. Otherwise it
            // would need to start this once scripting is opened. It is more 
            // acceptable to have this slowdown during the start of the program.
            new HydroModelBuilder();
        }
    }
}