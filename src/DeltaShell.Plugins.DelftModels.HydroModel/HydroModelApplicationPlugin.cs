using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Reflection;
using DeltaShell.Dimr;
using DeltaShell.NGHS.Common;
using DeltaShell.NGHS.Common.Extensions;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using log4net;
using Mono.Addins;

namespace DeltaShell.Plugins.DelftModels.HydroModel
{
    [Extension(typeof(IPlugin))]
    public class HydroModelApplicationPlugin : ApplicationPlugin
    {
        public const string RHUINTEGRATEDMODEL_TEMPLATE_ID = "RHUIntegratedModel";
        public const string DimrProjectTemplateId = "DimrProjectTemplateId";

        private static readonly ILog Log = LogManager.GetLogger(typeof(HydroModelApplicationPlugin));

        public override string Name => "Hydro Model";

        public override string DisplayName => "Hydro Model Plugin";

        public override string Description => Properties.Resources.HydroModelApplicationPlugin_Description;

        public override string Version => GetType().Assembly.GetName().Version.ToString();

        public override string FileFormatVersion => "1.1.1.0";

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

        public override IEnumerable<ModelInfo> GetModelInfos()
        {
            var modelGroupNameLookUp = new Dictionary<ModelGroup, string>
                {
                    {ModelGroup.Empty, DelftTools.Hydro.Properties.Resources.HydroModelApplicationPlugin_GetModelInfos_Empty_Integrated_Model},
                    {ModelGroup.RHUModels, DelftTools.Hydro.Properties.Resources.HydroModelGuiGraphicsProvider_CanProvideDrawingGroupFor_1D_2D_Integrated_Model_RHU},
                };

            foreach (ModelGroup modelGroup in modelGroupNameLookUp.Keys)
            {
                if (!HydroModel.CanBuildModel(modelGroup)) continue;

                yield return new ModelInfo
                {
                    Name = modelGroupNameLookUp[modelGroup],
                    Category = DelftTools.Hydro.Properties.Resources.HydroModelApplicationPlugin_GetModelInfos_1D_2D_3D_Integrated_Models,
                    AdditionalOwnerCheck = owner => (Application.ProjectService.Project != null && !Application.ProjectService.Project.RootFolder.GetAllModelsRecursive().Any()) &&
                        !(owner is ICompositeActivity), // Don't allow creation of sub-hydro models
                    CreateModel = owner =>
                    {
                        HydroModel hydroModel = new HydroModelBuilder().BuildModel(modelGroup);
                        hydroModel.WorkingDirectoryPathFunc = () => Application.WorkDirectory;
                        hydroModel.TimeStep = new TimeSpan(0, 5, 0);
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
            var initializeThread = new Thread(InitializeModelBuilder) { Priority = ThreadPriority.BelowNormal };
            initializeThread.Start();

            base.Activate();
        }

        public override IEnumerable<ProjectTemplate> ProjectTemplates()
        {
            yield return new ProjectTemplate
            {
                Id = RHUINTEGRATEDMODEL_TEMPLATE_ID,
                Category = ProductCategories.NewTemplateCategory,
                Name = "Integrated model",
                Description = "Creates a new Integrated Hydro model with RHU D-HYDRO models",
                ExecuteTemplateOpenView = (project, settings) =>
                {
                    var hydroModel = new HydroModelBuilder().BuildModel(ModelGroup.RHUModels);
                    if (settings is HydroModelProjectTemplateSettings modelSettings)
                    {
                        hydroModel.Name = modelSettings.ModelName;
                        hydroModel.CoordinateSystem = modelSettings.CoordinateSystem;
                        if (!modelSettings.UseRR)
                        {
                            hydroModel.Models.OfType<IHydroModel>().Where(hm => hm is IDimrModel dimrModel && dimrModel.IsActivityOfEnumType(ModelType.DRR)).ForEach(m => hydroModel.Region.SubRegions.RemoveAllWhere(r => m.Region.AllRegions.Any(sr => sr.GetType().Implements(r.GetType()))));
                            hydroModel.Activities.RemoveAllWhere(a =>a is IDimrModel dimrModel && dimrModel.IsActivityOfEnumType(ModelType.DRR));
                        }
                        if (!modelSettings.UseFlowFM)
                        {
                            hydroModel.Models.OfType<IHydroModel>().Where(hm => hm is IDimrModel dimrModel && dimrModel.IsActivityOfEnumType(ModelType.DFlowFM)).ForEach(m => hydroModel.Region.SubRegions.RemoveAllWhere(r => m.Region.AllRegions.Any(sr => sr.GetType().Implements(r.GetType()))));
                            hydroModel.Activities.RemoveAllWhere(a => a is IDimrModel dimrModel && dimrModel.IsActivityOfEnumType(ModelType.DFlowFM));
                        }

                        if (!modelSettings.UseRTC || !modelSettings.UseFlowFM && !modelSettings.UseRR)
                        {
                            hydroModel.Activities.RemoveAllWhere(a => a is IDimrModel dimrModel && dimrModel.IsActivityOfEnumType(ModelType.DFBC));
                        }

                        if (modelSettings.UseModelNameForProject)
                        {
                            project.Name = hydroModel.Name;
                        }
                    }

                    hydroModel.TimeStep = new TimeSpan(0, 5, 0);
                    project.RootFolder.Items.Add(hydroModel);
                    return hydroModel;
                }
            };
            yield return new ProjectTemplate
            {
                Id = DimrProjectTemplateId,
                Category = ProductCategories.ImportTemplateCategory,
                Description = "Import DIMR .xml as model",
                Name = "Dimr import",
                ExecuteTemplateOpenView = (project, o) =>
                {
                    if (!(o is string path) || !File.Exists(path))
                    {
                        return null;
                    }

                    var importer = Application.FileImporters.OfType<DHydroConfigXmlImporter>().First();
                    var fileImportActivity = new FileImportActivity(importer, project) { Files = new[] { path } };

                    fileImportActivity.OnImportFinished += (activity, importedObject, fileImporter) =>
                    {
                        project.RootFolder.Add(importedObject);
                    };

                    Application.ActivityRunner.Enqueue(fileImportActivity);
                    return fileImportActivity;
                }
            };
        }

        public override IEnumerable<IFileExporter> GetFileExporters()
        {
            yield return new DHydroConfigXmlExporter(Application.FileExportService);
        }
        
        public override IEnumerable<IFileImporter> GetFileImporters()
        {
            yield return new DHydroConfigXmlImporter(Application.FileImportService, new HydroModelReader(Application.FileImportService), () => Application.WorkDirectory);
        }
        private void InitializeModelBuilder()
        {
            new HydroModelBuilder();
        }

        private void ApplicationProjectSaving(object sender, EventArgs<Project> e)
        {
            // go through all hydro models and unlink all objects that link between rtc and flowFM, 
            // because flow is not saved in the database.
            DoWithHydroModels(e.Value, "Unlinking items for saving", m =>
            {
                m.UnlinkAndRememberDataItems();
                m.UnlinkAndRememberRegionLinks();
            });
        }

        private void ApplicationProjectSaved(object sender, EventArgs<Project> e)
        {
            DoWithHydroModels(e.Value, Properties.Resources.Linking_items_in_the_integrated_model_after_saving_the_project, RelinkHydroModelItems);
        }

        private void ApplicationProjectOpened(object sender, EventArgs<Project> e)
        {
            Project project = e.Value;
            project.RootFolder.GetAllModelsRecursive().OfType<HydroModel>().ForEach(hm =>
            {
                hm.WorkingDirectoryPathFunc = () => Application.WorkDirectory;
                hm.HydroModelExporter.FileExportService = Application.FileExportService;
            });

            project.CollectionChanging += OnProjectCollectionChanging;

            DoWithHydroModels(e.Value, Properties.Resources.Linking_items_in_the_integrated_model_after_loading_the_project, RelinkHydroModelItems);
        }
        
        private void ApplicationProjectClosing(object sender, EventArgs<Project> e)
        {
            e.Value.CollectionChanging -= OnProjectCollectionChanging;
        }
        
        private void OnProjectCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            if (e.Action == NotifyCollectionChangeAction.Add && e.Item is HydroModel hydroModel)
            {
                hydroModel.HydroModelExporter.FileExportService = Application.FileExportService;
            }
        }

        private static void RelinkHydroModelItems(HydroModel hydroModel)
        {
            hydroModel.RelinkDataItems();
            hydroModel.RelinkHydroRegionLinks();
        }

        private void ActivityRunnerOnActivityStatusChanged(object sender, ActivityStatusChangedEventArgs activityStatusChangedEventArgs)
        {
            if (activityStatusChangedEventArgs.NewStatus != ActivityStatus.Initializing || !(sender is HydroModel))
            {
                return;
            }

            Log.Info($"DeltaShell version: {Application.Version}");
            Log.Info(Application.PluginVersions);
        }

        private static void DoWithHydroModels(Project project, string actionName, Action<HydroModel> modelAction)
        {
            var models = project?.RootFolder?.GetAllModelsRecursive().ToArray() ?? new IModel[0];
            var hydroModels = models.OfType<HydroModel>().ToArray();
            if (!hydroModels.Any()) return;

            models.ForEach(m => m.SuspendClearOutputOnInputChange = true);

            var projectChangedState = project?.IsChanged;

            using (project.InEditMode())
            {
                foreach (var hydroModel in hydroModels)
                {
                    using (hydroModel.InEditMode(actionName))
                    {
                        modelAction?.Invoke(hydroModel);
                    }
                }
            }

            models.ForEach(m => m.SuspendClearOutputOnInputChange = false);

            if (projectChangedState.HasValue)
            {
                project.IsChanged = projectChangedState.Value;
            }
        }
    }
}