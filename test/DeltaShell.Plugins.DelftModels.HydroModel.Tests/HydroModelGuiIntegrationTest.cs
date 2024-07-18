using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DeltaShell.Gui.Forms.MainWindow;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.FMSuite.Common.Gui.RgfGrid;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using NUnit.Framework;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    [Category(TestCategory.WindowsForms)]
    public class HydroModelGuiIntegrationTest
    {
        private IGui gui;
        private Project project;

        [SetUp]
        public void SetUp()
        {
            InitializeGui();
        }

        [TearDown]
        public void TearDown()
        {
            DisposeGui();
        }

        private void InitializeGui()
        {
            //new RunningActivityLogAppender();
            //HACK: inside this constructor singleton magic happens, this should not be required

            var pluginsToAdd = new List<IPlugin>()
            {
                new NHibernateDaoApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new HydroModelApplicationPlugin(),
                new RealTimeControlApplicationPlugin(),
                new RainfallRunoffApplicationPlugin(),
                new NetCdfApplicationPlugin(),
                new FlowFMApplicationPlugin(),
                new SobekImportApplicationPlugin(),
                new ProjectExplorerGuiPlugin(),
                new CommonToolsGuiPlugin(),
                new SharpMapGisGuiPlugin(),
                new NetworkEditorGuiPlugin(),
                new HydroModelGuiPlugin(),
                new RealTimeControlGuiPlugin(),
                new RainfallRunoffGuiPlugin(),
                new FlowFMGuiPlugin(),

            };
            gui = new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build();
            gui.Run();
            project = gui.Application.ProjectService.CreateProject();
        }

        private void DisposeGui()
        {
            gui.Dispose();

            gui = null;
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void Add2D3DIntegratedModelAddFMModelRemoveIntegratedModel()
        {
            var mainWindow = (MainWindow) gui.MainWindow;

            if (!gui.DocumentViewsResolver.DefaultViewTypes.ContainsKey(typeof(WaterFlowFMModel)))
                gui.DocumentViewsResolver.DefaultViewTypes.Add(typeof(WaterFlowFMModel), typeof(WaterFlowFMFileStructureView));

            Action mainWindowShown = delegate
            {
                var hydroModelBuilder = new HydroModelBuilder();
                using (var integratedModel2D3D = hydroModelBuilder.BuildModel(ModelGroup.FMWaveRtcModels))
                {
                    using (var waterFlowFMModel = new WaterFlowFMModel())
                    {
                        gui.CommandHandler.AddItemToProject(integratedModel2D3D);
                        gui.Selection = integratedModel2D3D;
                        gui.CommandHandler.OpenViewForSelection();
                        gui.CommandHandler.AddItemToProject(waterFlowFMModel);
                        gui.Selection = waterFlowFMModel;
                        gui.CommandHandler.OpenViewForSelection();
                        project.RootFolder.Items.Remove(integratedModel2D3D);
                        Assert.IsTrue(project.RootFolder.GetAllModelsRecursive().SequenceEqual(new[] {waterFlowFMModel}));
                    }
                }
            };
            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void GivenAnIntegratedModelWithFMModelInItWhenOpeningGridInRGFGridAndClosingItThenItShouldNotThowAnException()
        {
            Action mainWindowShown = delegate
            {
                using (var integratedModel = new HydroModelBuilder().BuildModel(ModelGroup.FMWaveRtcModels))
                {
                    gui.CommandHandler.AddItemToProject(integratedModel);
                    gui.Selection = integratedModel;
                    gui.CommandHandler.OpenViewForSelection();
                    var waterFlowFMModel = gui.Application.ProjectService.Project.RootFolder.GetAllModelsRecursive().OfType<WaterFlowFMModel>().First();
                    waterFlowFMModel.Grid = UnstructuredGridTestHelper.GenerateRegularGrid(50, 50, 20, 20);
                    waterFlowFMModel.ReloadGrid();
                    gui.Selection = waterFlowFMModel.Grid;
                    PerformActionWithCancellationThread(60000, () => gui.CommandHandler.OpenViewForSelection());
                }
            };
            WpfTestHelper.ShowModal((MainWindow)gui.MainWindow, mainWindowShown);
        }

        private static void PerformActionWithCancellationThread(int timeout, Action action)
        {
            // Action waits for rgfgrid to close, we do this manually from another thread
            var cancellationThread = new Thread(() => CloseRgfGrid(timeout));
            cancellationThread.Start();

            // Invoke action
            action.Invoke();
        }

        private static void CloseRgfGrid(int maxTimeout)
        {
            Thread.Sleep(500); // Give action time to get started
            const int millisecondsToSleep = 100;

            // Get active rgfGrid processes (there should only be one)
            var rgfGridProcesses = Process.GetProcessesByName(RgfGridEditor.MfeAppProcessName);
            while (!rgfGridProcesses.Any())
            {
                Thread.Sleep(millisecondsToSleep);
                rgfGridProcesses = Process.GetProcessesByName(RgfGridEditor.MfeAppProcessName);
            }

            foreach (var process in rgfGridProcesses)
            {
                var totalTimeWaiting = 0;
                // attempt to close rgfGrid (may not be successful straight away)
                while (!process.CloseMainWindow())
                {
                    totalTimeWaiting += millisecondsToSleep;
                    Thread.Sleep(millisecondsToSleep);

                    if (totalTimeWaiting > maxTimeout)
                    {
                        return;
                    }
                }
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void FmModelShouldBeReplacedWhenImportedInIntegratedModel()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            /* create a integrated model */
            var hydroModel = HydroModel.BuildModel(ModelGroup.FMWaveRtcModels);

            /* add it to you project */
            project.RootFolder.Add(hydroModel);

            // wait until gui starts
            Action mainWindowShown = delegate
            {
                /* get the water flow fm model */
                var waterFlowFmModel = hydroModel.Activities.OfType<WaterFlowFMModel>().FirstOrDefault();
                Assert.NotNull(waterFlowFmModel);
                Assert.IsTrue(waterFlowFmModel.Name.StartsWith("FlowFM"));

                var fmImporter = gui.Application.FileImporters.OfType<WaterFlowFMFileImporter>().FirstOrDefault();
                Assert.IsNotNull(fmImporter);
                fmImporter.ImportItem(mduPath, waterFlowFmModel);

                var targetFmModel = hydroModel.Activities.OfType<WaterFlowFMModel>().FirstOrDefault();
                Assert.IsNotNull(targetFmModel);
                Assert.IsTrue(targetFmModel.Name.StartsWith("har"));
            };
            WpfTestHelper.ShowModal((MainWindow)gui.MainWindow, mainWindowShown);
        }
    }
}