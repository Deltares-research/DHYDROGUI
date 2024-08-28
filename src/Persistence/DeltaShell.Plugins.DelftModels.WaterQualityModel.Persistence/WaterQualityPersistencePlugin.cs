using DelftTools.Shell.Core;
using DelftTools.Shell.Persistence;
using DeltaShell.Plugins.Persistence.NHibernate;
using Mono.Addins;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Persistence
{
    [Extension(typeof(IPlugin))]
    public class WaterQualityPersistencePlugin : PersistencePlugin, INHibernatePluginExtensions
    {
        public override string Name => "D-Water Quality domain persistence plugin";
        public override string DisplayName => "D-Water Quality domain persistence plugin";
        public override string Description => "Plugin for persisting the D-Water Quality domain";
        public override string Version => "0.1.0.0";
        public override string FileFormatVersion => "3.6.0.0";
        public string PluginNameBeforeNHibernateMigration => "Water quality model";
    }
}