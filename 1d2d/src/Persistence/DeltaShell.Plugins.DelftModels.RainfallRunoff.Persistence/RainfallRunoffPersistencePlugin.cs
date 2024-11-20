using DelftTools.Shell.Core;
using DelftTools.Shell.Persistence;
using DeltaShell.Plugins.Persistence.NHibernate;
using Mono.Addins;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Persistence
{
    [Extension(typeof(IPlugin))]
    public class RainfallRunoffPersistencePlugin : PersistencePlugin, INHibernatePluginExtensions
    {
        public override string Name => "D-Rainfall Runoff domain persistence plugin";
        public override string DisplayName => "D-Rainfall Runoff domain persistence plugin";
        public override string Description => "Plugin for persisting the D-Rainfall Runoff domain";
        public override string Version => "0.1.0.0";
        public override string FileFormatVersion => "3.7.0.0";
        public string PluginNameBeforeNHibernateMigration => "rainfall runoff model";
    }
}