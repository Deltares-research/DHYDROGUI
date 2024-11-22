﻿using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Functions;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Reflection;
using DeltaShell.Gui.Forms.ViewManager;
using DeltaShell.NGHS.TestUtils.Builders;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.SharpMapGis.Gui;
using NUnit.Framework;
using Rhino.Mocks;
using Is = Rhino.Mocks.Constraints.Is;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class WaterQualityModelPluginGuiTest
    {
        [Test]
        public void OpenedMonitoringOutputViewsAreClosedAfterRemovingMonitoringOutput()
        {
            using (IGui gui = CreateDeltaShellGuiWithMonitoringOutputView())
            {
                WaterQualityModel waterQualityModel1D = gui.Application.ProjectService.Project.RootFolder.Items.OfType<WaterQualityModel>().First();

                // Remove the monitoring output data item
                waterQualityModel1D.MonitoringOutputDataItemSet.DataItems.Clear();

                // The dummy MultipleFunctionView should be removed from the document views
                Assert.AreEqual(0, gui.DocumentViews.Count);
            }
        }

        [Test]
        public void OpenedMonitoringOutputViewsAreClosedAfterRemovingMonitoringOutputDataItemSet()
        {
            using (IGui gui = CreateDeltaShellGuiWithMonitoringOutputView())
            {
                WaterQualityModel waterQualityModel1D = gui.Application.ProjectService.Project.RootFolder.Items.OfType<WaterQualityModel>().First();

                // Remove the monitoring output data item set
                waterQualityModel1D.DataItems.Remove(waterQualityModel1D.MonitoringOutputDataItemSet);

                // The dummy MultipleFunctionView should be removed from the document views
                Assert.AreEqual(0, gui.DocumentViews.Count);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void OpenedMonitoringOutputViewsAreClosedAfterRemovingModel()
        {
            using (IGui gui = CreateDeltaShellGuiWithMonitoringOutputView())
            {
                // Remove the model
                gui.Application.ProjectService.Project.RootFolder.Items.Clear();

                // The dummy MultipleFunctionView should be removed from the document views
                Assert.AreEqual(0, gui.DocumentViews.Count);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void OpenedMonitoringOutputViewsAreClosedAfterClosingProject()
        {
            using (IGui gui = CreateDeltaShellGuiWithMonitoringOutputView())
            {
                // Close the current project by creating a new one
                gui.Application.ProjectService.CreateProject();

                // The dummy MultipleFunctionView should be removed from the document views
                Assert.AreEqual(0, gui.DocumentViews.Count);
            }
        }

        [Test]
        public void SubstanceProcessLibraryViewTextsAreUpdatedAfterSubFileImport()
        {
            var mocks = new MockRepository();

            // Create a gui stub
            var gui = mocks.Stub<IGui>();
            var documentViews = new ViewList(mocks.Stub<IDockingManager>(), ViewLocation.Top);
            var application = mocks.Stub<IApplication>();
            var project = mocks.Stub<Project>();
            var activityRunner = mocks.Stub<ActivityRunner>();

            gui.Application = application;
            application.Stub(a => a.ActivityRunner).Return(activityRunner);
            application.Stub(a => a.Plugins).Return(new List<ApplicationPlugin>());

            var projectService = mocks.Stub<IProjectService>();
            application.Stub(a => a.ProjectService).Return(projectService);
            projectService.Stub(a => a.Project).Repeat.Any().Return(project);
            
            projectService.ProjectOpened += null;
            LastCall.Constraints(Is.NotNull()).Repeat.Any();

            projectService.ProjectCreated += null;
            LastCall.Constraints(Is.NotNull()).Repeat.Any();

            projectService.ProjectClosing += null;
            LastCall.Constraints(Is.NotNull()).Repeat.Any();
            
            Expect.Call(gui.Plugins).Return(new List<GuiPlugin>()).Repeat.Any();
            Expect.Call(gui.DocumentViews).Return(documentViews);

           projectService.Expect(a => a.ProjectClosing -= Arg<EventHandler<EventArgs<Project>>>.Is.Anything);
           projectService.Expect(a => a.ProjectOpened -= Arg<EventHandler<EventArgs<Project>>>.Is.Anything);
           projectService.Expect(a => a.ProjectCreated -= Arg<EventHandler<EventArgs<Project>>>.Is.Anything);

            // Create some other stubs
            var waterQualityModel1D = mocks.Stub<WaterQualityModel>();
            var substanceProcessLibrary = mocks.Stub<SubstanceProcessLibrary>();
            var subFileImporter = mocks.Stub<SubFileImporter>();
            var fileImportActivity = mocks.Stub<FileImportActivity>(subFileImporter, substanceProcessLibrary);

            gui.Stub(g => g.SelectedModel).Return(waterQualityModel1D);
            waterQualityModel1D.Name = "Water quality model";
            substanceProcessLibrary.Name = "Substance process library";
            fileImportActivity.ImportedItemOwner = new DataItem(substanceProcessLibrary);

            mocks.ReplayAll();

            var substanceProcessLibraryView = new SubstanceProcessLibraryView
            {
                Text = "Test",
                Data = substanceProcessLibrary
            };

            documentViews.Add(substanceProcessLibraryView);

            Assert.AreEqual("Test", substanceProcessLibraryView.Text);

            // Create a plugin gui
            using (new SharpMapGisGuiPlugin() {Gui = gui})
            using (var waterQualityModel1DPluginGui = new WaterQualityModelGuiPlugin {Gui = gui})
            {
                // Fake an import activity finished changed event
                TypeUtils.CallPrivateMethod(waterQualityModel1DPluginGui, "ActivityRunnerOnActivityCompleted",
                                            new object[]
                                            {
                                                null,
                                                new ActivityEventArgs(fileImportActivity)
                                            });

                Assert.AreEqual("Water quality model:Substance process library", substanceProcessLibraryView.Text);
            }

            mocks.VerifyAll();
        }

        private static IGui CreateDeltaShellGuiWithMonitoringOutputView()
        {
            var gui = new DHYDROGuiBuilder().WithWaterQuality().Build();

            gui.Run();
            WaterQualityModelGuiPlugin waterQualityModel1DGuiPlugin = gui.Plugins.OfType<WaterQualityModelGuiPlugin>().Single();
            IProjectService projectService = gui.Application.ProjectService;
            Project project = projectService.CreateProject();
            
            // Create a WaterQualityModel1D with dummy WaterQualityObservationVariableOutput and add it to the project root folder
            var waterQualityModel1D = new WaterQualityModel {ModelSettings = {MonitoringOutputLevel = MonitoringOutputLevel.Points}};
            var observationVariableOutput = new WaterQualityObservationVariableOutput(new List<DelftTools.Utils.Tuple<string, string>>
            {
                new DelftTools.Utils.Tuple<string, string>("Substance 1", ""),
                new DelftTools.Utils.Tuple<string, string>("Substance 2", "")
            });

            waterQualityModel1D.MonitoringOutputDataItemSet.DataItems.Add(new DataItem(observationVariableOutput));
            project.RootFolder.Add(waterQualityModel1D);

            // Create a dummy MultipleFunctionView and add it to the document views
            IEnumerable<IFunction> functions = new List<IFunction>(new[]
            {
                observationVariableOutput.TimeSeriesList.First()
            }).AsEnumerable();
            ViewInfo viewInfo = waterQualityModel1DGuiPlugin.GetViewInfoObjects().FirstOrDefault(vi => vi.DataType == typeof(WaterQualityObservationVariableOutput));

            viewInfo.GetViewData = d => functions;

            var multipleFunctionView = new MultipleFunctionView
            {
                Data = functions,
                Text = "Multiple function view",
                ViewInfo = viewInfo
            };

            gui.DocumentViews.Add(multipleFunctionView);

            return gui;
        }
    }
}