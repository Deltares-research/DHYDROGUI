using System;
using System.Linq;
using System.Windows;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.Data.NHibernate.DelftTools.Shell.Core.Dao;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.Scripting;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.CoverageViews;
using GeoAPI.Extensions.Networks;
using log4net.Core;
using NUnit.Framework;
using SharpTestsEx;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    [Category(TestCategory.Slow)]
    public class NHibernateWaterFlowModel1DWinFormsTest
    {
        private NHibernateProjectRepository projectRepository;
        private NHibernateProjectRepositoryFactory factory;
        private long totalMemoryBeforeTest;
        private long maximumExpectedMemoryLeak;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            LogHelper.SetLoggingLevel(Level.Error);

            factory = new NHibernateProjectRepositoryFactory();
            factory.AddPlugin(new SharpMapGisApplicationPlugin());
            factory.AddPlugin(new CommonToolsApplicationPlugin());
            factory.AddPlugin(new NetCdfApplicationPlugin());
            factory.AddPlugin(new ScriptingApplicationPlugin());

            factory.AddPlugin(new NetworkEditorApplicationPlugin());

            factory.AddPlugin(new HydroModelApplicationPlugin());
            factory.AddPlugin(new WaterFlowModel1DApplicationPlugin());
            factory.AddPlugin(new RealTimeControlApplicationPlugin());
            factory.AddPlugin(new RainfallRunoffApplicationPlugin());
            factory.AddPlugin(new WaterQualityModelApplicationPlugin());
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            LogHelper.ResetLogging();
        }
        
        [SetUp]
        public void SetUp()
        {
            maximumExpectedMemoryLeak = 0;
            totalMemoryBeforeTest = GC.GetTotalMemory(true);

#if PROFILE_MEMORY
            MessageBox.Show("Start memory profiler");
#endif

            projectRepository = factory.CreateNew();
        }

        /// <summary>
        /// Used for memory profiling (run nunit, run empty test, take snapshot, run test, take second snapshot).
        /// </summary>
        [Test]
        public void Empty()
        {
            
        }

        [TearDown]
        public void TearDown()
        {
            projectRepository.Dispose();
            projectRepository = null;

            // aggresive collect..needed because GC is so lazy it goes out of mem (?!?)
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
#if PROFILE_MEMORY
            MessageBox.Show("Stop memory profiler");
#endif

            LogHelper.SetLoggingLevel(Level.Debug);

            var totalMemoryAfterTest = GC.GetTotalMemory(true);
            Console.WriteLine(@"Total memory leak after test: " + (totalMemoryAfterTest - totalMemoryBeforeTest)/(1024.0 * 1000.0) + @" Mb");
            (totalMemoryAfterTest - totalMemoryBeforeTest)
                .Should("memory leak should be within expectations").Be.LessThan(maximumExpectedMemoryLeak == 0 ? 40000000 : maximumExpectedMemoryLeak); // TODO: analyze it! 

            LogHelper.ResetLogging();
        }

        private static void RunModel(WaterFlowModel1D flowModel1D)
        {
            ActivityRunner.RunActivity(flowModel1D);

            if (flowModel1D.Status == ActivityStatus.Failed)
            {
                Assert.Fail("Model run has failed");
            }
        }

        public static Project GetProjectFor(IBranchFeature branchFeature)
        {
            var network = new HydroNetwork();
            var branch = new Channel();
            network.Branches.Add(branch);
            var node1 = new HydroNode { Network = network };
            var node2 = new HydroNode { Network = network };
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            branch.Source = node1;
            branch.Target = node2;

            branchFeature.Branch = branch;

            branch.BranchFeatures.Add(branchFeature);

            var project = new Project();

            project.RootFolder.Add(network);
            return project;
        }

        protected T SaveAndRetrieveObject<T>(T objectToSave) where T : class
        {
            var path = TestHelper.GetCurrentMethodName() + "1.dsproj";
            var project = new Project();
            project.RootFolder.Add(objectToSave);

            projectRepository.Create(path);
            projectRepository.SaveOrUpdate(project);
            projectRepository.Close();

            var np = projectRepository.Open(path);
            Assert.IsNotNull(np.RootFolder.Items[0]);

            var retrievedEntry = np.RootFolder.Items[0];

            if (retrievedEntry is IDataItem)
            {
                return ((IDataItem)np.RootFolder.Items[0]).Value as T;
            }

            if (retrievedEntry is IModel || retrievedEntry is Folder)
            {
                return retrievedEntry as T;
            }
            return null;
        }

        private static DeltaShellGui GetRunningGuiWithFlowPlugins()
        {
            var gui = new DeltaShellGui();

            gui.Application.Plugins.Add(new HydroModelApplicationPlugin());
            gui.Application.Plugins.Add(new NetCdfApplicationPlugin());
            gui.Application.Plugins.Add(new NHibernateDaoApplicationPlugin());
            gui.Application.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
            gui.Application.Plugins.Add(new NetworkEditorApplicationPlugin());
            gui.Application.Plugins.Add(new SharpMapGisApplicationPlugin());
            gui.Application.Plugins.Add(new CommonToolsApplicationPlugin());

            gui.Plugins.Add(new WaterFlowModel1DGuiPlugin());
            gui.Plugins.Add(new NetworkEditorGuiPlugin());
            gui.Plugins.Add(new ProjectExplorerGuiPlugin());
            gui.Plugins.Add(new SharpMapGisGuiPlugin());
            gui.Plugins.Add(new CommonToolsGuiPlugin());

            gui.Run();
            return gui;
        }

        [Test]
        public void SaveByPassModelInApplicationAfterShowingTheDiscretization()
        {
            //test reproduces issue 2560
            using (var gui = GetRunningGuiWithFlowPlugins())
            {
                string path = "SaveBypassInApp.dsproj";
                IApplication app = gui.Application;

                var modelPath =
                    TestHelper.GetTestDataPath(
                        typeof(SobekWaterFlowModel1DImporterTest).Assembly,
                        @"BYPASS.Lit\3\Network.TP");
                //what a cumbersome syntax here..
                var importedModel = (HydroModel)new SobekHydroModelImporter(false, false).ImportItem(modelPath);

                Action formShownAction =
                    delegate
                    {
                        var model = (HydroModel)importedModel.DeepClone();
                        app.Project.RootFolder.Add(model);

                        var flowModel = model.Activities.OfType<WaterFlowModel1D>().FirstOrDefault();

                        HydroNetworkHelper.GenerateDiscretization(
                            flowModel.NetworkDiscretization,
                            true,
                            false,
                            0.5, /*minimumCellLength*/
                            true, /*gridAtStructure*/
                            1.0,
                            true, /*gridAtCrossSection*/
                            false, /*gridAtLateralSource*/
                            true, /*gridAtFixedLength*/
                            100.0);

                        //show the discetization so a viewcontext will be saved (with layers etc)
                        gui.DocumentViewsResolver.OpenViewForData(flowModel.NetworkDiscretization);
                        var view = gui.DocumentViewsResolver.GetViewsForData(flowModel.NetworkDiscretization).FirstOrDefault();
                        gui.DocumentViews.Remove(view);

                        app.SaveProjectAs(path);

                        // check if it does not crash during save after new project is created via CommandHandler
                        gui.CommandHandler.CloseProject();
                        gui.CommandHandler.CreateNewProject();

                        model = (HydroModel)importedModel.DeepClone();
                        app.Project.RootFolder.Add(model);

                        flowModel = model.Activities.OfType<WaterFlowModel1D>().FirstOrDefault();

                        HydroNetworkHelper.GenerateDiscretization(
                            flowModel.NetworkDiscretization,
                            true,
                            false,
                            0.5, /*minimumCellLength*/
                            true, /*gridAtStructure*/
                            1.0,
                            true, /*gridAtCrossSection*/
                            false, /*gridAtLateralSource*/
                            true, /*gridAtFixedLength*/
                            100.0);

                        //show the discetization so a viewcontext will be saved (with layers etc)
                        gui.DocumentViewsResolver.OpenViewForData(flowModel.NetworkDiscretization);
                        view = gui.DocumentViewsResolver.GetViewsForData(flowModel.NetworkDiscretization).FirstOrDefault();
                        gui.DocumentViews.Remove(view);

                        app.SaveProjectAs(path);
                    };
                WpfTestHelper.ShowModal((System.Windows.Controls.Control)gui.MainWindow, formShownAction);
            }

            maximumExpectedMemoryLeak = 80000000; // NH is not cleaned for the first time.
        }

        [Test]
        public void SaveAsModelTwice()
        {
            //test reproduces issue 2560
            using (var gui = GetRunningGuiWithFlowPlugins())
            {
                string path = "SaveAsModelTwice.dsproj";
                IApplication app = gui.Application;

                Action formShownAction = delegate
                {
                    var model = new WaterFlowModel1D();
                    app.Project.RootFolder.Add(model);
                    app.SaveProjectAs(path);

                    // check if it does not crash during save after new project is created via CommandHandler
                    gui.CommandHandler.CloseProject();

                    FileUtils.DeleteIfExists(path);
                    FileUtils.DeleteIfExists(path + "_data");
                };

                WpfTestHelper.ShowModal((System.Windows.Controls.Control)gui.MainWindow, formShownAction);
            }
        }

        [Test]
        public void SaveProjectWithDeletedModelAndOpenCoverageViews()
        {
            //otherwise path is very long
            var path = "p1.dsproj";
            using (var gui = GetRunningGuiWithFlowPlugins())
            {
                var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

                gui.Application.Project.RootFolder.Add(model);
                RunModel(model);

                var mainWindow = (Window)gui.MainWindow;

                mainWindow.Loaded += delegate
                {
                    gui.DocumentViewsResolver.OpenViewForData(model.OutputFlow, typeof(CoverageTableView));

                    gui.Application.SaveProjectAs(path);

                    gui.Application.Project.RootFolder.Items.Remove(model);
                    gui.Application.SaveProject();

                };
                WpfTestHelper.ShowModal(mainWindow);
            }
        }

    }
}