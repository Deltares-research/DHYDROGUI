using System;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.IntegrationTestUtils;
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
            var deltaShell = DeltaShellCoreFactory.CreateGui();

            deltaShell.Application.Plugins.Add(new NetCdfApplicationPlugin());
            deltaShell.Application.Plugins.Add(new NHibernateDaoApplicationPlugin());
            deltaShell.Application.Plugins.Add(new HydroModelApplicationPlugin());
            deltaShell.Application.Plugins.Add(new NetworkEditorApplicationPlugin());
            deltaShell.Application.Plugins.Add(new SharpMapGisApplicationPlugin());
            deltaShell.Application.Plugins.Add(new CommonToolsApplicationPlugin());
            deltaShell.Application.Plugins.Add(new RainfallRunoffApplicationPlugin());

            deltaShell.Plugins.Add(new ProjectExplorerGuiPlugin());
            deltaShell.Application.Plugins.ForEach(p => p.Application = deltaShell.Application);
            deltaShell.Run();

            deltaShell.Application.CreateNewProject();

            return deltaShell;
        }

        internal static IApplication GetDeltaShellApplicationWithRRPlugins()
        {
            var app = DeltaShellCoreFactory.CreateApplication();
            
            app.Plugins.Add(new NetCdfApplicationPlugin());
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new HydroModelApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new RainfallRunoffApplicationPlugin());

            app.Run();
            app.CreateNewProject();

            return app;
        }
    }
}