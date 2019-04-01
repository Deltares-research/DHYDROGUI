using System;
using System.Linq;
using System.Windows;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DeltaShell.Core;
using DeltaShell.Gui;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.ProjectExplorer;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using DeltaShell.Plugins.ProjectExplorer;
using log4net.Core;
using NUnit.Framework;
using SharpTestsEx;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    public class ProjectExplorerIntegrationTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowProjectExporerWithModel()
        {
            var gui = new DeltaShellGui();
            var app = gui.Application;

            app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
            gui.Plugins.Add(new ProjectExplorerGuiPlugin());
            
            gui.Run();
            //add a model and grid and link them
            var model = new WaterFlowModel1D();

            gui.Application.Project.RootFolder.Add(model);
            var mainWindow = (Window)gui.MainWindow;
            WpfTestHelper.ShowModal(mainWindow);
            
            gui.Dispose();
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [SetCulture("en-us")]
        public void RunFlowModelWithOutputFolderCollapsedInProjectExplorerShouldNotResultInDuplicateRunReport()
        {
            //HACK: inside this constructor singleton magic happens, this should not be required
            new RunningActivityLogAppender();

            var appender = RunningActivityLogAppender.Instance;
            
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;

                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());

                
                gui.Run();

                var projectExplorer = gui.ToolWindowViews.OfType<ProjectExplorer>().First();

                // add a model and grid and link them
                var flowModel = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

                flowModel.StatusChanged +=
                    (s, e) =>
                        {
                            // cannot get logging to be received by appender, so we send a manual message here
                            if (flowModel.Status == ActivityStatus.Done)
                            {
                                appender.DoAppend(new LoggingEvent(new LoggingEventData {Message = "Model about to finish"}));
                            }
                        };

                gui.Application.Project.RootFolder.Add(flowModel);
                var mainWindow = (Window)gui.MainWindow;
                WpfTestHelper.ShowModal(mainWindow, 
                    () =>
                        {
                            gui.Application.RunActivity(flowModel);

                            // asserts
                            Assert.AreEqual(ActivityStatus.Cleaned, flowModel.Status);

                            var modelOutputs = flowModel.DataItems.Where(di => di.Role == DataItemRole.Output);
                            Assert.AreEqual(1, modelOutputs.Count(di => di.Tag == RunningActivityLogAppender.LastRunLogfileDataItemTag), 
                                "#Run report in object model");
                            
                            var treeView = projectExplorer.TreeView;
                            var projectNode = treeView.Nodes[0];
                            var modelNode = projectNode.Nodes[0];
                            var outputNode = modelNode.Nodes[1];
                            outputNode.Expand();

                            Assert.AreEqual(1, outputNode.Nodes.Count(n => n.Text == "Run report"), 
                                "#Run report nodes");
                        });
            }

            LogHelper.ResetLogging();
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void CustomNodePresentersShouldBeRegisteredOnCloseReopen_Tools8350()
        {
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new HydroModelApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new HydroModelGuiPlugin());

                
                gui.Run();

                var mainWindow = (Window)gui.MainWindow;

                Action onShown = delegate
                {
                    var treeView = gui.MainWindow.ProjectExplorer.TreeView;
                    ProjectExplorerGuiPlugin.Instance.InitializeProjectTreeView(); // make sure project explorer is shown

                    var hydroModel = new HydroModel();

                    // add model with child models
                    app.Project.RootFolder.Add(hydroModel);

                    treeView.WaitUntilAllEventsAreProcessed();

                    // now close and re-open project explorer
                    gui.ToolWindowViews.Remove(gui.MainWindow.ProjectExplorer);
                    ProjectExplorerGuiPlugin.Instance.InitializeProjectTreeView(); // make sure project explorer is shown

                    // asserts
                    gui.MainWindow.ProjectExplorer.TreeView.Nodes[0].Nodes[0].Presenter
                        .Should("hydro model node presenter is correctly set").Be.OfType<HydroModelTreeViewNodePresenter>();
                };

                WpfTestHelper.ShowModal(mainWindow, onShown);
            }
        }
    }
}