using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using Mono.Addins;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl
{
    [Extension(typeof(IPlugin))]
    public class RealTimeControlApplicationPlugin : ApplicationPlugin, IDataAccessListenersProvider
    {
        public override string Name
        {
            get { return "Real-Time Control"; }
        }

        public override string DisplayName
        {
            get { return "D-Real Time Control Plugin"; }
        }

        public override string Description
        {
            get { return Properties.Resources.RealTimeControlApplicationPlugin_Description; }
        }

        public override string Version
        {
            get
            {
                return GetType().Assembly.GetName().Version.ToString();
            }
        }

        public override string FileFormatVersion
        {
            get { return "3.5.0.0"; }
        }

        public override Image Image
        {
            get { return null; }
        }

        public override IEnumerable<ModelInfo> GetModelInfos()
        {
            yield return new ModelInfo
                {
                    Name = "Real-Time Control Model",
                    Category = "1D / 2D / 3D Standalone Models",
                    AdditionalOwnerCheck = owner => owner is ICompositeActivity // Only allow composite activities as target
                                                 && !((ICompositeActivity) owner).Activities.OfType<RealTimeControlModel>().Any(), // Don't allow multiple realtime control models in one composite activity
                    CreateModel = owner => new RealTimeControlModel("Real-Time Control")
                };
        }

        public override IEnumerable<Assembly> GetPersistentAssemblies()
        {
            yield return GetType().Assembly;
        }

        public IEnumerable<IDataAccessListener> CreateDataAccessListeners()
        {
            yield return new RtcDataAccessListener();
        }

        public override IEnumerable<IFileExporter> GetFileExporters()
        {
            yield return new RealTimeControlModelExporter();
        }

        public override IEnumerable<IFileImporter> GetFileImporters()
        {
            yield return new RealTimeControlModelImporter();
        }
    }
}