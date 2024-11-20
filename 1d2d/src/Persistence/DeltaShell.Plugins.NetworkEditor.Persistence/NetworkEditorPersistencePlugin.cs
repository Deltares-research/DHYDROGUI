using DelftTools.Shell.Core;
using DelftTools.Shell.Persistence;
using DeltaShell.Plugins.Persistence.NHibernate;
using Mono.Addins;

namespace DeltaShell.Plugins.NetworkEditor.Persistence
{
    [Extension(typeof(IPlugin))]
    public class NetworkEditorPersistencePlugin : PersistencePlugin, INHibernatePluginExtensions
    {
        public override string Name => "Network editor persistence plugin";
        public override string DisplayName => "Network editor persistence plugin";
        public override string Description => "Plugin for persisting the network editor";
        public override string Version => "0.1.0.0";
        public override string FileFormatVersion => "3.5.2.0";
        public string PluginNameBeforeNHibernateMigration => "Network";
    }
}