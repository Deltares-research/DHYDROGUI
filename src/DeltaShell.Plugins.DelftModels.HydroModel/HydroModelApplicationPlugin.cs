using System.Collections.Generic;
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
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using log4net;
using Mono.Addins;

namespace DeltaShell.Plugins.DelftModels.HydroModel
{
    [Extension(typeof(IPlugin))]
    public class HydroModelApplicationPlugin : ApplicationPlugin
    {
        public const string RHUINTEGRATEDMODEL_TEMPLATE_ID = "RHUIntegratedModel";
        public static int MainThreadId;
        private static readonly ILog Log = LogManager.GetLogger(typeof(HydroModelApplicationPlugin));

        public HydroModelApplicationPlugin()
        {
            MainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        public override string Name
        {
            get { return DelftTools.Shell.Core.Properties.Resources.HydroModelApplicationPlugin_Name_Hydro_Model; }
        }

        public override string DisplayName
        {
            get { return DelftTools.Shell.Core.Properties.Resources.HydroModelApplicationPlugin_DisplayName_Hydro_Model_Plugin; }
        }

        public override string Description
        {
            get { return Properties.Resources.HydroModelApplicationPlugin_Description; }
        }

        public override string Version
        {
            get { return GetType().Assembly.GetName().Version.ToString(); }
        }

        public override string FileFormatVersion
        {
            get { return "1.1.1.0"; }
        }
        
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

        private void ApplicationProjectSaving(Project project)
        {
            if (project == null || project.RootFolder == null) return;

            project.RootFolder.GetAllModelsRecursive().ForEach(m => m.SuspendClearOutputOnInputChange = true);
            // go through all hydro models and unlink all objects that link between rtc and flowFM, 
            // because flow is not saved in the database.
            foreach (var hydroModel in project.RootFolder.GetAllItemsRecursive().OfType<HydroModel>())
            {
                hydroModel.UnlinkAndRememberDataItems();
                hydroModel.UnlinkAndRememberRegionLinks();
            }
            project.RootFolder.GetAllModelsRecursive().ForEach(m => m.SuspendClearOutputOnInputChange = false);
        }

        private void ApplicationProjectSavedOrFailed(Project project)
        {
            if (project == null || project.RootFolder == null) return;

            foreach (var hydroModel in project.RootFolder.GetAllItemsRecursive().OfType<HydroModel>())
            {
                hydroModel.RelinkDataItems();
                hydroModel.RelinkHydroRegionLinks();
            }
        }
        private void ApplicationProjectOpened(Project project)
        {
            // relink all dataitems (between rtc and flowFM) for all hydromodels
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
                Log.Info(string.Format(DelftTools.Shell.Core.Properties.Resources.HydroModelApplicationPlugin_ActivityRunnerOnActivityStatusChanged_DeltaShell_version___0_, Application.Version));
                Log.Info(Application.PluginVersions);
            }
        }

        public override IEnumerable<ModelInfo> GetModelInfos()
        {
            var modelGroupNameLookUp = new Dictionary<ModelGroup, string>
                {
                    {ModelGroup.Empty, DelftTools.Shell.Core.Properties.Resources.HydroModelApplicationPlugin_GetModelInfos_Empty_Integrated_Model},
                    {ModelGroup.RHUModels, DelftTools.Shell.Core.Properties.Resources.HydroModelApplicationPlugin_GetModelInfos__1D_2D_Integrated_Model + " (RHU)"},
                };

            foreach (ModelGroup modelGroup in modelGroupNameLookUp.Keys)
            {
                if (!HydroModel.CanBuildModel(modelGroup)) continue;

                yield return new ModelInfo
                {
                    Name = modelGroupNameLookUp[modelGroup],
                    Category = DelftTools.Shell.Core.Properties.Resources.HydroModelApplicationPlugin_GetModelInfos__1D___2D___3D_Integrated_Models,
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
                Category = "RHU Templates",
                Name = "RHU model",
                Description = "Creates a new Integrated Hydro model with RHU D-HYDRO models",
                ExecuteTemplate = (p, settings) =>
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

                    p.RootFolder.Items.Add(model);

                }
            };
        }
        public override IEnumerable<IFileExporter> GetFileExporters()
        {
            yield return new DHydroConfigXmlExporter();
        }

        private void InitializeModelBuilder()
        {
            new HydroModelBuilder();
        }
    }
}