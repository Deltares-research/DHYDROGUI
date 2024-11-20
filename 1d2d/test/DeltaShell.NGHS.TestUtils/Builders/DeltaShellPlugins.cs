using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetCDF.Persistence;
using DeltaShell.Plugins.Persistence;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Persistence;
using DeltaShell.Plugins.SharpMapGis.Persistence;
using DeltaShell.Plugins.Toolbox;
using DeltaShell.Plugins.Toolbox.Gui;

namespace DeltaShell.NGHS.TestUtils.Builders
{
    // Scripting plugins are ignored; leads to problems run-time in tests.
    internal static class DeltaShellPlugins
    {
        public static IEnumerable<IPlugin> All =>
            Application.Concat(Persistence)
                       .Concat(Gui)
                       .Concat(GuiPersistence);

        public static IEnumerable<IPlugin> AllNonGui =>
            Application.Concat(Persistence);

        public static IEnumerable<IPlugin> AllPersistence =>
            Persistence.Concat(GuiPersistence);

        private static IEnumerable<IPlugin> Application => new List<IPlugin>
        {
            new CommonToolsApplicationPlugin(),
            new NetCdfApplicationPlugin(),
            new NHibernateDaoApplicationPlugin(),
            new SharpMapGisApplicationPlugin(),
            new ToolboxApplicationPlugin()
        };

        private static IEnumerable<IPlugin> Gui => new List<IPlugin>
        {
            new CommonToolsGuiPlugin(),
            new ProjectExplorerGuiPlugin(),
            new SharpMapGisGuiPlugin(),
            new ToolboxGuiPlugin()
        };

        private static IEnumerable<IPlugin> Persistence => new List<IPlugin>
        {
            new DeltaShellPersistencePlugin(),
            new NetCDFPersistencePlugin(),
            new SharpMapGisPersistencePlugin()
        };

        private static IEnumerable<IPlugin> GuiPersistence => new List<IPlugin> { new SharpMapGisGuiPersistencePlugin() };
    }
}