using DelftTools.Shell.Core;
using DelftTools.Shell.Persistence;
using DeltaShell.Plugins.Persistence.NHibernate;
using Mono.Addins;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Persistence
{
    [Extension(typeof(IPlugin))]
    public class HydroModelPersistencePlugin : PersistencePlugin, INHibernatePluginExtensions
    {
        public override string Name => "Integrated model domain persistence plugin";
        public override string DisplayName => "Integrated model domain persistence plugin";
        public override string Description => "Plugin for persisting the integrated model domain";
        public override string Version => "0.1.0.0";
        public override string FileFormatVersion => "1.3.0.0";
        public string PluginNameBeforeNHibernateMigration => "Hydro Model";
    }
}