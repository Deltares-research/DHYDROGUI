using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness;
using Mono.Addins;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel
{
    [Extension(typeof(IPlugin))]
    public class WaterFlowModel1DApplicationPlugin : ApplicationPlugin, IDataAccessListenersProvider
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterFlowModel1DApplicationPlugin));

        public static string PluginVersion; 
        public static string PluginName;

        public WaterFlowModel1DApplicationPlugin()
        {
            PluginVersion = Version;
            PluginName = DisplayName;
        }


        public override string Name
        {
            get { return "1D water flow model"; }
        }

        public override string DisplayName
        {
            get { return "D-Flow1D Plugin"; }
        }

        public override string Description
        {
            get { return Properties.Resources.WaterFlowModel1DApplicationPlugin_Description; }
        }

        public override string Version
        {
            get { return GetType().Assembly.GetName().Version.ToString(); }
        }

        public override string FileFormatVersion
        {
            get { return "3.5.2.0"; }
        }

        public override Image Image
        {
            get { return null; }
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
                }

                base.Application = value;

                if (base.Application != null)
                {
                    base.Application.ActivityRunner.ActivityStatusChanged += ActivityRunnerOnActivityStatusChanged;
                }
            }
        }

        private void ActivityRunnerOnActivityStatusChanged(object sender, ActivityStatusChangedEventArgs activityStatusChangedEventArgs)
        {
            if (activityStatusChangedEventArgs.NewStatus == ActivityStatus.Initializing && sender as WaterFlowModel1D != null)
            {
                Log.Info("DeltaShell version: " + Application.Version);
                Log.Info(Application.PluginVersions);
            }
        }


        public override IEnumerable<ModelInfo> GetModelInfos()
        {
            yield return new ModelInfo
                {
                    Name = "Flow 1D Model",
                    Category = "1D / 2D / 3D Standalone Models",
                    AdditionalOwnerCheck = owner => !(owner is ICompositeActivity) // Allow "standalone" flow models
                                                 || !((ICompositeActivity) owner).Activities.OfType<WaterFlowModel1D>().Any(), // Don't allow multiple flow models in one composite activity
                    CreateModel = owner => new WaterFlowModel1D("Flow1D")
                };
        }

        public override IEnumerable<IFileImporter> GetFileImporters()
        {
            yield return new FlowDataCsvImporter();
            yield return new RoughnessFromCsvToSectionImporter();
            yield return new CalibratedRoughnessImporter();
            yield return new WaterFlowModel1DFileImporter();
        }

        public override IEnumerable<IFileExporter> GetFileExporters()
        {
            yield return new RoughnessFromCsvFileExporter();
            yield return new SobekToFMExporter();
            yield return new WaterFlowModel1DExporter();
        }

        public override IEnumerable<Assembly> GetPersistentAssemblies()
        {
            yield return GetType().Assembly;
        }

        public IEnumerable<IDataAccessListener> CreateDataAccessListeners()
        {
            yield return new WaterFlowModel1DDataAccessListener();
        }
    }
}