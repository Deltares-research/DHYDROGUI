using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI
{
    internal static class RainfallRunoffIntegrationTestHelper
    {
        public static string GetSobekImportTestDir()
        {
            return TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowFMModelImporterTest).Assembly);
        }

        public static ICompositeActivity ImportModel(string file)
        {
            var importer = new SobekHydroModelImporter(true, false);

            var compositeModel = (ICompositeActivity)importer.ImportItem(file);

            //remove non-RR
            foreach(var activity in compositeModel.Activities.Where(a => !(a is RainfallRunoffModel)).ToList())
            {
                compositeModel.Activities.Remove(activity);
            }
            return compositeModel;
        }

        public static void RunModel(IActivity model)
        {
            ActivityRunner.RunActivity(model);

            if (model.Status == ActivityStatus.Failed)
            {
                throw new Exception("Model run failed!");
            }
        }

       
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