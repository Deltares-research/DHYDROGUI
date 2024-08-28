using DelftTools.Shell.Core;
using DelftTools.Shell.Persistence;
using DeltaShell.Plugins.Persistence.NHibernate;
using Mono.Addins;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Persistence
{
    [Extension(typeof(IPlugin))]
    public class NetworkEditorGuiPersistencePlugin : PersistencePlugin, INHibernatePluginExtensions
    {
        public override string Name => "Network editor UI persistence plugin";
        public override string DisplayName => "Network editor UI persistence plugin";
        public override string Description => "Plugin for persisting the network editor UI";
        public override string Version => "0.1.0.0";
        public override string FileFormatVersion => "3.5.0.0";
        public string PluginNameBeforeNHibernateMigration => "Network (UI)";
    }
}