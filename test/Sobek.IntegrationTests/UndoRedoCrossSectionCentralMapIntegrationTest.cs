using System;
using System.Linq;
using System.Windows;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    [Category(TestCategory.UndoRedo)]
    public class UndoRedoCrossSectionCentralMapIntegrationTest : UndoRedoCentralMapTestBase
    {
        [SetUp]
        public void SetUp()
        {
            gui = new DeltaShellGui();
            var app = gui.Application;

            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());

            gui.Plugins.Add(new CommonToolsGuiPlugin());
            gui.Plugins.Add(new SharpMapGisGuiPlugin());
            gui.Plugins.Add(new NetworkEditorGuiPlugin());


            gui.Run();

            project = app.Project;

            // add data
            network = new HydroNetwork();
            project.RootFolder.Add(network);

            // show gui main window
            mainWindow = (Window)gui.MainWindow;

            // wait until gui starts
            mainWindowShown = () =>
                {
                    var networkDataItem = project.RootFolder.DataItems.First();
                    gui.CommandHandler.OpenView(networkDataItem, typeof(ProjectItemMapView));

                    mapView = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();

                    gui.UndoRedoManager.TrackChanges = true;

                    onMainWindowShown();
                };
        }

        [TearDown]
        public void TearDown()
        {
            LogHelper.ResetLogging();
            gui.Dispose();
            onMainWindowShown = null;
            mainWindowShown = null;
            project = null;
            network = null;
            mainWindow = null;
            GC.Collect();
        }

        [Test]
        public void AddCrossSection()
        {
            onMainWindowShown = () =>
                {
                    AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(100, 0) });
                    var cs = AddCrossSection(new Coordinate(20, 0), CrossSection.CreateDefault(CrossSectionType.YZ, null));

                    Assert.AreEqual(2, gui.UndoRedoManager.UndoStack.Count());

                    gui.UndoRedoManager.Undo();
                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(2, gui.UndoRedoManager.RedoStack.Count());

                    gui.UndoRedoManager.Redo();
                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual(network, cs.HydroNetwork);
                    Assert.AreEqual(1, network.CrossSections.Count());
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void ChangeStandardCrossSectionType()
        {
            onMainWindowShown = () =>
                {
                    AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(100, 0) });
                    var cs = AddCrossSection(new Coordinate(20, 0), CrossSection.CreateDefault(CrossSectionType.Standard, null));

                    var csDefStandard = (CrossSectionDefinitionStandard)cs.Definition;

                    csDefStandard.ShapeType = CrossSectionStandardShapeType.Arch;

                    Assert.AreEqual(3, gui.UndoRedoManager.UndoStack.Count());

                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(1, gui.UndoRedoManager.RedoStack.Count());
                    Assert.AreEqual(CrossSectionStandardShapeType.Rectangle, csDefStandard.ShapeType);

                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual(CrossSectionStandardShapeType.Arch, csDefStandard.ShapeType);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void ModifySections()
        {
            onMainWindowShown = () =>
                {
                    gui.UndoRedoManager.TrackChanges = false;

                    AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(100, 0) });
                    var cs = AddCrossSection(new Coordinate(20, 0), CrossSection.CreateDefault(CrossSectionType.YZ, null));

                    var section1 = cs.Definition.Sections[0];
                    var origValue = section1.MaxY;

                    gui.UndoRedoManager.TrackChanges = true;

                    section1.MaxY = 15.0;

                    Assert.AreEqual(1, gui.UndoRedoManager.UndoStack.Count());

                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(origValue, section1.MaxY);

                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual(15.0, section1.MaxY);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void ModifySectionsWithForcingOn()
        {
            onMainWindowShown = () =>
                {
                    gui.UndoRedoManager.TrackChanges = false;

                    AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(100, 0) });
                    var cs = AddCrossSection(new Coordinate(20, 0), CrossSection.CreateDefault(CrossSectionType.YZ, null));
                    cs.Definition.ForceSectionsSpanFullWidth = true;

                    var section1 = cs.Definition.Sections[0];
                    var origValue = section1.MaxY;

                    gui.UndoRedoManager.TrackChanges = true;

                    section1.MaxY = 15.0;

                    Assert.AreEqual(1, gui.UndoRedoManager.UndoStack.Count());

                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(origValue, section1.MaxY);

                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual(15.0, section1.MaxY);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void ChangeOfInnerDefinitionProfileUpdatesGeometryOfCrossSection()
        {
            onMainWindowShown =
                () =>
                    {
                        gui.UndoRedoManager.TrackChanges = false;

                        AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(100, 0) });

                        // create shared definition
                        var innerDefinition = CrossSectionDefinitionZW.CreateDefault();
                        network.SharedCrossSectionDefinitions.Add(innerDefinition);
                        var proxyDefinition = new CrossSectionDefinitionProxy(innerDefinition);
                        var cs = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(
                                                                                      network.Branches.First(), proxyDefinition, 15);

                        var geometryBefore = cs.Geometry;

                        gui.UndoRedoManager.TrackChanges = true;

                        // modify shared definition
                        innerDefinition.ZWDataTable.AddCrossSectionZWRow(-5, 200, 50);

                        // make sure geometry was changed
                        Assert.AreNotEqual(geometryBefore, cs.Geometry);

                        // undo changes to shared definition
                        gui.UndoRedoManager.Undo();

                        // very the geometry is also restored
                        Assert.AreEqual(geometryBefore, cs.Geometry);
                    };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void AddRowToProfile()
        {
            onMainWindowShown = () =>
                {
                    gui.UndoRedoManager.TrackChanges = false;

                    AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(100, 0) });
                    var cs = AddCrossSection(new Coordinate(20, 0), CrossSection.CreateDefault(CrossSectionType.YZ, null));
                    ICrossSectionDefinition definition = cs.Definition;
                    gui.UndoRedoManager.TrackChanges = true;

                    definition.RawData.Add(Enumerable.Range(1, 3).Select(i => i*1.0).ToArray());
                    
                    Assert.AreEqual(1, gui.UndoRedoManager.UndoStack.Count(), "#undo");

                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(6, definition.RawData.Rows.Count, "#rows after undo");

                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual(7, definition.RawData.Rows.Count, "#rows after redo");
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void AddRowToProfileWithForceOn()
        {
            onMainWindowShown = () =>
                {
                    gui.UndoRedoManager.TrackChanges = false;

                    AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(100, 0) });
                    var cs = AddCrossSection(new Coordinate(20, 0), CrossSection.CreateDefault(CrossSectionType.YZ, null));
                    var definition = cs.Definition;
                    definition.ForceSectionsSpanFullWidth = true;

                    gui.UndoRedoManager.TrackChanges = true;

                    definition.RawData.Add(new [] { 101.0, 2.0, 3.0 });
                    
                    Assert.AreEqual(101, definition.Sections.Max(s => s.MaxY), "section width before");
                    Assert.AreEqual(1, gui.UndoRedoManager.UndoStack.Count(), "#undo");

                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(6, definition.RawData.Rows.Count, "#rows after undo");
                    Assert.AreEqual(100, definition.Sections.Max(s => s.MaxY), "section width after undo");

                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual(7, definition.RawData.Rows.Count, "#rows after redo");
                    Assert.AreEqual(101, definition.Sections.Max(s => s.MaxY), "section width after redo");
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }
    }
}