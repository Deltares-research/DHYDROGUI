using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using log4net;
using Mono.Addins;

namespace DeltaShell.Plugins.DelftModels.HydroModel
{
    [Extension(typeof(IPlugin))]
    public class HydroModelApplicationPlugin : ApplicationPlugin
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HydroModelApplicationPlugin));

        public HydroModelApplicationPlugin()
        {
            MainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        public override string Name => Properties.Resources.HydroModelApplicationPlugin_Name_Hydro_Model;

        public override string DisplayName => Properties.Resources.HydroModelApplicationPlugin_DisplayName_Hydro_Model_Plugin;

        public override string Description => Properties.Resources.HydroModelApplicationPlugin_Description;

        public override string Version => AssemblyUtils.GetAssemblyInfo(GetType().Assembly).Version;

        public override string FileFormatVersion => "1.3.0.0";

        public override IApplication Application
        {
            get => base.Application;
            set
            {
                if (Application != null)
                {
                    Application.ActivityRunner.ActivityStatusChanged -= ActivityRunnerOnActivityStatusChanged;
                    Application.ProjectService.ProjectSaving -= ApplicationProjectSaving;
                    Application.ProjectService.ProjectSaved -= ApplicationProjectSaved;
                    Application.ProjectService.ProjectOpened -= ApplicationProjectOpened;
                    Application.ProjectService.ProjectCreated -= ApplicationProjectOpened;
                    Application.ProjectService.ProjectClosing -= ApplicationProjectClosing;
                }

                base.Application = value;

                if (Application != null)
                {
                    Application.ActivityRunner.ActivityStatusChanged += ActivityRunnerOnActivityStatusChanged;
                    Application.ProjectService.ProjectSaving += ApplicationProjectSaving;
                    Application.ProjectService.ProjectSaved += ApplicationProjectSaved;
                    Application.ProjectService.ProjectOpened += ApplicationProjectOpened;
                    Application.ProjectService.ProjectCreated += ApplicationProjectOpened;
                    Application.ProjectService.ProjectClosing += ApplicationProjectClosing;
                }
            }
        }

        public static int MainThreadId { get; set; }

        public override IEnumerable<ModelInfo> GetModelInfos()
        {
            var modelGroupNameLookUp = new Dictionary<ModelGroup, string>
            {
                {ModelGroup.Empty, Properties.Resources.HydroModelApplicationPlugin_GetModelInfos_Empty_Integrated_Model},
                {ModelGroup.FMWaveRtcModels, Properties.Resources.HydroModelApplicationPlugin_GetModelInfos_2D_3D_Integrated_Model}
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
                    Category = Properties.Resources.HydroModelApplicationPlugin_GetModelInfos_1D_2D_3D_Integrated_Models,
                    GetParentProjectItem = owner =>
                    {
                        Folder rootFolder = Application?.ProjectService.Project?.RootFolder;
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

        public override void Activate()
        {
            var initializeThread = new Thread(InitializeModelBuilder) {Priority = ThreadPriority.BelowNormal};
            initializeThread.Start();

            base.Activate();
        }

        public override IEnumerable<IFileExporter> GetFileExporters()
        {
            yield return new DHydroConfigXmlExporter(Application.FileExportService);
        }

        public override IEnumerable<IFileImporter> GetFileImporters()
        {
            yield return new DHydroConfigXmlImporter(Application.FileImportService, new HydroModelReader(Application.FileImportService), () => Application.WorkDirectory);
        }

        private void ApplicationProjectClosing(object sender, EventArgs<Project> e)
        {
            Project project = e.Value;
            project.CollectionChanging -= OnProjectCollectionChanging;
            project.PropertyChanged -= OnProjectPropertyChanged;
        }

        private void ApplicationProjectOpened(object sender, EventArgs<Project> e)
        {
            Project project = e.Value;
            // relink all dataitems (between rtc and flowFM) for all hydromodels
            Application.ProjectService.Project.RootFolder.GetAllModelsRecursive().OfType<HydroModel>().ForEach(hm =>
            {
                hm.RelinkDataItems();
                hm.WorkingDirectoryPathFunc = () => Application.WorkDirectory;
                hm.HydroModelExporter.FileExportService = Application.FileExportService;
            });

            project.CollectionChanging += OnProjectCollectionChanging;
            project.PropertyChanged += OnProjectPropertyChanged;
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
            if (e.Action == NotifyCollectionChangeAction.Add)
            {
                if (e.Item is IModel model)
                {
                    TrimAllModelNames(model);
                    MakeAllModelNamesUnique(model);
                }

                if (e.Item is HydroModel hydroModel)
                {
                    hydroModel.HydroModelExporter.FileExportService = Application.FileExportService;
                }
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
            IModel[] allModels = Application.ProjectService.Project.GetAllItemsRecursive()
                                            .OfType<IModel>()
                                            .Where(m => m != model)
                                            .ToArray();

            if (allModels.Any(m => m.Name == model.Name))
            {
                model.Name = NamingHelper.GetUniqueName(model.Name + " ({0})", allModels);
            }
        }

        private void ApplicationProjectSaving(object sender, EventArgs<Project> e)
        {
            Project project = e.Value;
            if (project == null || project.RootFolder == null)
            {
                return;
            }

            // go through all hydro models and unlink all objects that link between rtc and flowFM, 
            // because flow is not saved in the database.
            foreach (HydroModel hydroModel in project.RootFolder.GetAllItemsRecursive().OfType<HydroModel>())
            {
                hydroModel.UnlinkAndRememberDataItems();
            }
        }

        private void ApplicationProjectSaved(object sender, EventArgs<Project> e)
        {
            Project project = e.Value;
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
                Log.Info($"DeltaShell version: {Application.Version}");
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