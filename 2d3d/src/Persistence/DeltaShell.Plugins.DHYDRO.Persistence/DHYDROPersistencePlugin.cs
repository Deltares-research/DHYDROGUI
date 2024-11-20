using DelftTools.Shell.Core;
using DelftTools.Shell.Persistence;
using DeltaShell.Plugins.Persistence.NHibernate;
using Mono.Addins;

namespace DeltaShell.Plugins.DHYDRO.Persistence
{
    [Extension(typeof(IPlugin))]
    public class DHYDROPersistencePlugin : PersistencePlugin, INHibernatePluginExtensions
    {
        public override string Name => "D-HYDRO domain persistence plugin";
        public override string DisplayName => "D-HYDRO domain persistence plugin";
        public override string Description => "Plugin for persisting the D-HYDRO domain";
        public override string Version => "0.1.0.0";
        public override string FileFormatVersion => "3.5.2.0";
        public string PluginNameBeforeNHibernateMigration => "Network";
    }
}