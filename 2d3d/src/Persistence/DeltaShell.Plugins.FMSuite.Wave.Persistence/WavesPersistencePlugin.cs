using DelftTools.Shell.Core;
using DelftTools.Shell.Persistence;
using DeltaShell.Plugins.Persistence.NHibernate;
using Mono.Addins;

namespace DeltaShell.Plugins.FMSuite.Wave.Persistence
{
    [Extension(typeof(IPlugin))]
    public class WavesPersistencePlugin : PersistencePlugin, INHibernatePluginExtensions
    {
        public override string Name => "D-Waves domain persistence plugin";
        public override string DisplayName => "D-Waves domain persistence plugin";
        public override string Description => "Plugin for persisting the D-Waves domain";
        public override string Version => "0.1.0.0";
        public override string FileFormatVersion => "1.3.0.0";
        public string PluginNameBeforeNHibernateMigration => "Delft3D Wave";
    }
}