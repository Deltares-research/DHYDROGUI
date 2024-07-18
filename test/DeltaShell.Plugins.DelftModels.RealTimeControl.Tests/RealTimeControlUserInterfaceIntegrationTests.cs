using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Import;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using DeltaShell.Plugins.DelftModels.RTCShapes.Tests;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    [TestFixture]
    public class RealTimeControlUserInterfaceIntegrationTests
    {
        [Test]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.Wpf)]
        public void SaveAndLoadModelResultsInValidModel()
        {
            using (var tempDir = new TemporaryDirectory())
            using (IGui gui = CreateGuiWithPlugins())
            {
                gui.Run();
                IProjectService projectService = gui.Application.ProjectService;
                Project project = projectService.CreateProject();

                project.RootFolder.Add(RealTimeControlTestHelper.GenerateTestModel(false));

                string projectPath = Path.Combine(tempDir.Path, "SaveAndLoad.dsproj");
                projectService.SaveProjectAs(projectPath);
                projectService.CloseProject();

                project = projectService.OpenProject(projectPath);

                IModel model = project.RootFolder.Models.First();

                var retrievedModel = (RealTimeControlModel)model;
                ControlGroup retrievedControlGroup = retrievedModel.ControlGroups.First();
                ControlGroup resultControlGroup =
                    RealTimeControlTestHelper.GenerateTestModel(false).ControlGroups.First();

                Assert.AreEqual(RealTimeControlTestHelper.GenerateTestModel(false).Name, retrievedModel.Name);
                Assert.AreEqual(resultControlGroup.Name, retrievedControlGroup.Name);
                Assert.AreEqual(resultControlGroup.Inputs.First().Name,
                                retrievedControlGroup.Inputs.First().Name);
                Assert.AreEqual(resultControlGroup.Outputs.First().Name,
                                retrievedControlGroup.Outputs.First().Name);
                Assert.AreEqual(resultControlGroup.Rules.First().Name,
                                retrievedControlGroup.Rules.First().Name);
                Assert.AreEqual(resultControlGroup.Conditions.First().Name,
                                retrievedControlGroup.Conditions.First().Name);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.Wpf)]
        public void SaveAndLoadRestoresShapeLocationsAndSizes()
        {
            using (var tempDir = new TemporaryDirectory())
            using (IGui gui = CreateGuiWithPlugins())
            {
                gui.Run();
                IProjectService projectService = gui.Application.ProjectService;
                Project project = projectService.CreateProject();

                // Create model
                RealTimeControlModel model = CreateModelWithControlGroupAndRtcObjects();
                project.RootFolder.Add(model);

                // Open graph view
                gui.DocumentViewsResolver.OpenViewForData(model.ControlGroups[0]);
                ControlGroupGraphView view = gui.DocumentViews.OfType<ControlGroupGraphView>().First();

                // Move and resize shapes
                IEnumerable<ShapeBase> shapes = view.ControlGroupEditor.GraphControl.GetShapes<ShapeBase>().ToArray();
                RandomizeShapeSizesAndLocations(shapes);

                // Save/load project
                string projectPath = Path.Combine(tempDir.Path, "SaveAndLoad.dsproj");
                projectService.SaveProjectAs(projectPath);
                projectService.CloseProject();
                project = projectService.OpenProject(projectPath);

                var loadedModel = (RealTimeControlModel)project.RootFolder.Models.First();

                // Open restored graph view
                gui.DocumentViewsResolver.OpenViewForData(loadedModel.ControlGroups[0]);
                ControlGroupGraphView restoredView = gui.DocumentViews.OfType<ControlGroupGraphView>().First();
                IEnumerable<ShapeBase> restoredShapes = restoredView.ControlGroupEditor.GraphControl.GetShapes<ShapeBase>();

                // Assert
                Assert.That(shapes, Is.EquivalentTo(restoredShapes).Using(new ShapeGeometryComparer()));
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.Wpf)]
        public void ExportAndImportRestoresShapeLocationsAndSizes()
        {
            using (var tempDir = new TemporaryDirectory())
            using (IGui gui = CreateGuiWithPlugins())
            {
                gui.Run();
                IProjectService projectService = gui.Application.ProjectService;
                Project project = projectService.CreateProject();

                // Create model
                RealTimeControlModel model = CreateModelWithControlGroupAndRtcObjects();
                project.RootFolder.Add(model);

                // Open graph view
                gui.DocumentViewsResolver.OpenViewForData(model.ControlGroups[0]);
                ControlGroupGraphView view = gui.DocumentViews.OfType<ControlGroupGraphView>().First();

                // Move and resize shapes
                IEnumerable<ShapeBase> shapes = view.ControlGroupEditor.GraphControl.GetShapes<ShapeBase>().ToArray();
                RandomizeShapeSizesAndLocations(shapes);

                // Export model
                RealTimeControlModelExporter exporter = gui.Application.FileExporters.OfType<RealTimeControlModelExporter>().First();
                exporter.Export(model, tempDir.Path);

                // Import model into new project
                projectService.CloseProject();
                project = projectService.CreateProject();

                RealTimeControlModelImporter importer = gui.Application.FileImporters.OfType<RealTimeControlModelImporter>().First();
                var newModel = (RealTimeControlModel)importer.ImportItem(tempDir.Path);
                project.RootFolder.Add(newModel);

                // Open restored graph view
                gui.DocumentViewsResolver.OpenViewForData(newModel.ControlGroups[0]);
                ControlGroupGraphView restoredView = gui.DocumentViews.OfType<ControlGroupGraphView>().First();
                IEnumerable<ShapeBase> restoredShapes = restoredView.ControlGroupEditor.GraphControl.GetShapes<ShapeBase>();

                // Assert
                Assert.That(shapes, Is.EquivalentTo(restoredShapes).Using(new ShapeGeometryComparer()));
            }
        }

        private static IGui CreateGuiWithPlugins()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                new NHibernateDaoApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new RealTimeControlApplicationPlugin(),
                new ProjectExplorerGuiPlugin(),
                new RealTimeControlGuiPlugin()
            };
            return new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build();
        }

        private static RealTimeControlModel CreateModelWithControlGroupAndRtcObjects()
        {
            ControlGroup controlGroup = RealTimeControlTestHelper.CreateControlGroupWithTwoRulesOnOneOutput();
            var realTimeControlModel = new RealTimeControlModel();
            realTimeControlModel.ControlGroups.Add(controlGroup);

            return realTimeControlModel;
        }

        private static void RandomizeShapeSizesAndLocations(IEnumerable<ShapeBase> shapes)
        {
            var random = new Random();

            foreach (ShapeBase shape in shapes)
            {
                shape.X = random.Next(0, 100);
                shape.Y = random.Next(0, 100);
                shape.Width = random.Next(50, 150);
                shape.Height = random.Next(50, 150);
            }
        }
    }
}