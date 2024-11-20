using DelftTools.Shell.Core;
using DelftTools.Shell.Persistence;
using DeltaShell.Plugins.Persistence.NHibernate;
using Mono.Addins;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Persistence
{
    [Extension(typeof(IPlugin))]
    public class RealTimeControlPersistencePlugin : PersistencePlugin, INHibernatePluginExtensions
    {
        public override string Name => "D-Real Time Control domain persistence plugin";
        public override string DisplayName => "D-Real Time Control domain persistence plugin";
        public override string Description => "Plugin for persisting the D-Real Time Control domain";
        public override string Version => "0.1.0.0";
        public override string FileFormatVersion => "3.8.0.0";
        public string PluginNameBeforeNHibernateMigration => "Real-Time Control";
    }
}