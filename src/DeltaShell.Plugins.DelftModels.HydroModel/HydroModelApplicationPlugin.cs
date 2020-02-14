using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using Mono.Addins;
using log4net;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using log4net.Appender;
using log4net.Repository.Hierarchy;

namespace DeltaShell.Plugins.DelftModels.HydroModel
{
    [Extension(typeof(IPlugin))]
    public class HydroModelApplicationPlugin : ApplicationPlugin, IDataAccessListenersProvider
    {
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
                    //Application.ProjectDataDirectory
                }
            }
        }

        public static Iterative1D2DCouplerAppender IterativeCouplerAppender { get; set; }

        private void ApplicationProjectOpened(Project project)
        {
            // relink all dataitems (between rtc and flowFM) for all hydromodels
            Application.GetAllModelsInProject().OfType<HydroModel>().ForEach(hm =>
            {
                hm.RelinkDataItems();
                hm.RelinkHydroRegionLinks();
            });
        }

        private void ApplicationProjectSaving(Project project)
        {
            if (project == null || project.RootFolder == null) return;

            // go through all hydro models and unlink all objects that link between rtc and flowFM, 
            // because flow is not saved in the database.
            foreach (var hydroModel in project.RootFolder.GetAllItemsRecursive().OfType<HydroModel>())
            {
                hydroModel.UnlinkAndRememberDataItems();
                hydroModel.UnlinkAndRememberRegionLinks();
            }
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
                    CreateModel = owner => HydroModel.BuildModel(modelGroup)
                };
            }
        }
        public IEnumerable<IDataAccessListener> CreateDataAccessListeners()
        {
            yield return new Iterative1D2DCouplerDataAccessListener();
        }

        public override IEnumerable<Assembly> GetPersistentAssemblies()
        {
            yield return GetType().Assembly;
        }

        public override void Activate()
        {
            var initializeThread = new Thread(InitializeModelBuilder) { Priority = ThreadPriority.BelowNormal };
            initializeThread.Start();

            // register Iterative1D2DCoupler log appender
            IterativeCouplerAppender = new Iterative1D2DCouplerAppender();

            var rootLogger = ((Hierarchy)LogManager.GetRepository()).Root;
            if (!rootLogger.Appenders.Cast<IAppender>().Any(a => a is Iterative1D2DCouplerAppender))
            {
                rootLogger.AddAppender(IterativeCouplerAppender);
            }

            base.Activate();
        }

        public override void Deactivate()
        {
            // unregister Iterative1D2DCoupler log appender
            var rootLogger = ((Hierarchy)LogManager.GetRepository()).Root;
            if (IterativeCouplerAppender != null)
            {
                rootLogger.RemoveAppender(IterativeCouplerAppender);
                IterativeCouplerAppender = null;
            }

            base.Deactivate();
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