using System.Collections.Generic;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Collections;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI
{
    internal static class RainfallRunoffIntegrationTestHelper
    {
        internal static IGui GetRunningGuiWithRRPlugins()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                new NetCdfApplicationPlugin(),
                new NHibernateDaoApplicationPlugin(),
                new HydroModelApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new RainfallRunoffApplicationPlugin(),
                new ProjectExplorerGuiPlugin(),
            };
            
            var deltaShell = new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build();
            
            deltaShell.Application.Plugins.ForEach(p => p.Application = deltaShell.Application);
            deltaShell.Run();

            deltaShell.Application.CreateNewProject();

            return deltaShell;
        }

        internal static IApplication GetDeltaShellApplicationWithRRPlugins()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                new NetCdfApplicationPlugin(),
                new NHibernateDaoApplicationPlugin(),
                new HydroModelApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new RainfallRunoffApplicationPlugin(),
            };
            var app = new DeltaShellApplicationBuilder().WithPlugins(pluginsToAdd).Build();
            
            app.Run();
            app.CreateNewProject();

            return app;
        }
    }
}