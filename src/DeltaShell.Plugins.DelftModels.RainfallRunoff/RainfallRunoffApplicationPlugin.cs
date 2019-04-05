using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers;
using Mono.Addins;
using log4net;

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
                        Name = "Rainfall Runoff"
                    }
                };
        }

        public override IEnumerable<IFileImporter> GetFileImporters()
        {
            yield return new MeteoDataImporter();
            yield return new PolderFromGisImporter();
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