using DelftTools.Shell.Core;
using DelftTools.Shell.Persistence;
using DeltaShell.Plugins.Persistence.NHibernate;
using Mono.Addins;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Persistence
{
    [Extension(typeof(IPlugin))]
    public class FlowFMPersistencePlugin : PersistencePlugin, INHibernatePluginExtensions
    {
        public override string Name => "D-Flow FM domain persistence plugin";
        public override string DisplayName => "D-Flow FM domain persistence plugin";
        public override string Description => "Plugin for persisting the D-Flow FM domain";
        public override string Version => "0.1.0.0";
        public override string FileFormatVersion => "1.4.0.0";
        public string PluginNameBeforeNHibernateMigration => "Delft3D FM";
    }
}