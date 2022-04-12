using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
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
    public class HydroModelApplicationPlugin : ApplicationPlugin, IDataAccessListenersProvider
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
                    Application.ProjectSaving -= ApplicationProjectSaving;
                    Application.ProjectSaved -= ApplicationProjectSavedOrFailed;
                    Application.ProjectSaveFailed -= ApplicationProjectSavedOrFailed;
                    Application.ProjectOpened -= ApplicationProjectOpened;
                }
                
                base.Application = value;

                if (Application != null)
                {
                    Application.ActivityRunner.ActivityStatusChanged += ActivityRunnerOnActivityStatusChanged;
                    Application.ProjectSaving += ApplicationProjectSaving;
                    Application.ProjectSaveFailed += ApplicationProjectSavedOrFailed;
                    Application.ProjectSaved += ApplicationProjectSavedOrFailed;
                    Application.ProjectOpened += ApplicationProjectOpened;
                }
            }
        }

        public IEnumerable<IDataAccessListener> CreateDataAccessListeners()
        {
            yield return new HydroModelDataAccessListener();
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
                    AdditionalOwnerCheck = owner => (Application.Project != null && !Application.GetAllModelsInProject().Any()) &&
                        !(owner is ICompositeActivity), // Don't allow creation of sub-hydro models
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
                    var model = new HydroModelBuilder().BuildModel(ModelGroup.RHUModels);
                    if (settings is HydroModelProjectTemplateSettings modelSettings)
                    {
                        model.Name = modelSettings.ModelName;
                        model.CoordinateSystem = modelSettings.CoordinateSystem;
                        if (!modelSettings.UseRR)
                        {
                            model.Models.OfType<IHydroModel>().Where(hm => hm is IDimrModel dimrModel && dimrModel.IsActivityOfEnumType(ModelType.DRR)).ForEach(m => model.Region.SubRegions.RemoveAllWhere(r => m.Region.AllRegions.Any(sr => sr.GetType().Implements(r.GetType()))));
                            model.Activities.RemoveAllWhere(a =>a is IDimrModel dimrModel && dimrModel.IsActivityOfEnumType(ModelType.DRR));
                        }
                        if (!modelSettings.UseFlowFM)
                        {
                            model.Models.OfType<IHydroModel>().Where(hm => hm is IDimrModel dimrModel && dimrModel.IsActivityOfEnumType(ModelType.DFlowFM)).ForEach(m => model.Region.SubRegions.RemoveAllWhere(r => m.Region.AllRegions.Any(sr => sr.GetType().Implements(r.GetType()))));
                            model.Activities.RemoveAllWhere(a => a is IDimrModel dimrModel && dimrModel.IsActivityOfEnumType(ModelType.DFlowFM));
                        }

                        if (!modelSettings.UseRTC || !modelSettings.UseFlowFM && !modelSettings.UseRR)
                        {
                            model.Activities.RemoveAllWhere(a => a is IDimrModel dimrModel && dimrModel.IsActivityOfEnumType(ModelType.DFBC));
                        }
                    }

                    project.RootFolder.Items.Add(model);
                    return model;
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

                    var importer = new DHydroConfigXmlImporter(() => Application.FileImporters.OfType<IDimrModelFileImporter>().ToList(),
                                                               () => Application.WorkDirectory);

                    var fileImportActivity = new FileImportActivity(importer, project)
                    {
                        Files = new[]
                        {
                            path
                        }
                    };

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
            yield return new DHydroConfigXmlExporter();
        }
        public override IEnumerable<IFileImporter> GetFileImporters()
        {
            yield return new DHydroConfigXmlImporter(() => Application.FileImporters.OfType<IDimrModelFileImporter>().ToList(),
                                                     () => Application.WorkDirectory);
        }
        private void InitializeModelBuilder()
        {
            new HydroModelBuilder();
        }

        private void ApplicationProjectSaving(Project project)
        {
            // go through all hydro models and unlink all objects that link between rtc and flowFM, 
            // because flow is not saved in the database.
            DoWithHydroModels(project, "Unlinking items for saving", m =>
            {
                m.UnlinkAndRememberDataItems();
                m.UnlinkAndRememberRegionLinks();
            });
        }

        private void ApplicationProjectSavedOrFailed(Project project)
        {
            DoWithHydroModels(project, "Linking items after for saving", m =>
            {
                m.RelinkDataItems();
                m.RelinkHydroRegionLinks();
            });
        }

        private void ApplicationProjectOpened(Project project)
        {
            Application.GetAllModelsInProject().OfType<HydroModel>().ForEach(hm =>
            {
                hm.WorkingDirectoryPathFunc = () => Application.WorkDirectory;
            });
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