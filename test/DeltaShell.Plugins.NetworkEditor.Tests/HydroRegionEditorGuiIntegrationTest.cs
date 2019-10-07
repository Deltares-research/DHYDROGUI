using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Commands;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.Layers;
using SharpMap.UI.Tools;
using ComboBox = System.Windows.Controls.ComboBox;

namespace DeltaShell.Plugins.NetworkEditor.Tests
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class HydroRegionEditorGuiIntegrationTest
    {
        protected DeltaShellGui gui;
        protected Project project;
        protected IHydroRegion region;
        protected DrainageBasin basin;
        protected IHydroNetwork network;
        protected Window mainWindow;
        protected Action onMainWindowShown;
        protected ProjectItemMapView regionEditor;
        private MouseEventArgs args = new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0);
        private Action mainWindowShown;

        [SetUp]
        public void SetUp()
        {
            gui = new DeltaShellGui();
            var app = gui.Application;

            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            

            gui.Plugins.Add(new SharpMapGisGuiPlugin());
            gui.Plugins.Add(new NetworkEditorGuiPlugin());
            gui.Plugins.Add(new CommonToolsGuiPlugin());
            gui.Run();

            project = app.Project;

            // add data

            network = new HydroNetwork();
            basin = new DrainageBasin();
            region = new HydroRegion { Name = "hr", SubRegions = { network, basin } };

            project.RootFolder.Add(region);

            // show gui main window
            mainWindow = (Window)gui.MainWindow;

            // wait until gui starts
            mainWindowShown = delegate
            {
                var regionDataItem = project.RootFolder.DataItems.First();
                gui.CommandHandler.OpenView(regionDataItem, typeof(ProjectItemMapView));

                regionEditor = gui.DocumentViews.OfType<ProjectItemMapView>().First();

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
            project = null;
            basin = null;
            network = null;
            region = null;
            mainWindow = null;
            GC.Collect();
        }
        
        [Test]
        public void LinkCatchmentToBoundaryNode()
        {
            onMainWindowShown = () =>
                {
                    var b1 = AddBranch(new[] { new Coordinate(0, 0, 0), new Coordinate(1000, 0, 0) });
                    var c1 = AddCatchment(new Coordinate(500, 1000, 0), CatchmentType.Unpaved);

                    var link = AddLink(c1.InteriorPoint.Coordinate, new Coordinate(0, 0, 0));

                    Assert.AreEqual(1, c1.Links.Count);
                    Assert.AreEqual(c1, link.Source);
                    Assert.AreEqual(b1.Source, link.Target);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void LinkCatchmentToBoundaryNodeCreatesValidGeometry()
        {
            onMainWindowShown = () =>
            {
                var c1 = AddCatchment(new Coordinate(500, 1000, 0), CatchmentType.Unpaved);
                var b1 = AddBranch(new[] { new Coordinate(0, 0, 0), new Coordinate(1000, 0, 0) });
                var link = AddLink(c1.InteriorPoint.Coordinate, new Coordinate(0, 0, 0));

                Assert.IsTrue(link.Geometry.Coordinates.All(c => c.Z == 0.0));
            };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        /// <summary>
        /// TOOLS-22846 dictates as issue that the combobox is not filled after a route layer is added to network
        /// this test will verify that the combo box could get updated when a branch is drawn and a route layer is added.
        /// </summary>
        [Test]
        public void EmptyCoverageDropdownBoxGetsUpdatedAfterRouteAdd()
        {
            onMainWindowShown = () =>
            {
                /* get the coverages combo box from te ribbon */
                var ribbon = (Fluent.Ribbon)TypeUtils.GetField(gui.MainWindow, "MainWindowRibbon");
                var tab = ribbon.Tabs.First(t => t.Header.Equals("Map"));
                var group = tab.Groups.First(g => g.Name.Equals("NetworkCoverage"));
                var wrapPanel = group.Items.OfType<WrapPanel>().First();
                var comboBox = wrapPanel.Children.OfType<ComboBox>().First(c => c.Name == "ComboBoxSelectNetworkCoverage");
                
                Assert.NotNull(comboBox);

                /* attach event to see how many times the selected item attibute is set */
                var count = 0;
                comboBox.SelectionChanged += (s, e) => {  count++; };

                /* check if combobox is still empty! */
                Assert.IsFalse(comboBox.HasItems);

                /* add simple branch and add a route to the hydroregion */
                var b1 = AddBranch(new[] { new Coordinate(0, 0, 0), new Coordinate(1000, 0, 0) });
                new AddNewNetworkRouteCommand().Execute();
                
                /* check if the route (route_1) is added to the combobox as ONLY one */
                Assert.AreEqual(1, count);
                Assert.IsTrue(comboBox.HasItems);
                Assert.AreEqual(1, comboBox.Items.Count);
                var routeLayer = comboBox.Items[0] as Layer;
                Assert.IsNotNull(routeLayer);
                Assert.AreEqual("route_1", routeLayer.Name);

                /* validate that the just added route is the selected item */
                var selected = comboBox.SelectedItem as Layer;
                Assert.IsNotNull(selected);
                Assert.AreEqual("route_1", selected.Name);
            };
            
            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }


        private HydroLink AddLink(Coordinate start, Coordinate end)
        {
            var newLinkTool = regionEditor.MapView.MapControl.Tools.OfType<NewArrowLineTool>().First(
                t => t.Name == HydroRegionEditorMapTool.AddHydroLinkToolName);

            newLinkTool.OnMouseDown(start, args);
            newLinkTool.OnMouseMove(start, args);
            newLinkTool.OnMouseUp(start, args);
            newLinkTool.OnMouseDown(end, args);
            newLinkTool.OnMouseMove(end, args);
            newLinkTool.OnMouseUp(end, args);

            return (HydroLink)newLinkTool.Layers.Last().DataSource.Features.Cast<IFeature>().Last();
        }

        private IChannel AddBranch(Coordinate[] coordinates)
        {
            var vectorLayers = regionEditor.MapView.Map.GetAllLayers(true).OfType<VectorLayer>();
            var branchLayer = vectorLayers.First(l => l.DataSource.FeatureType == typeof (Channel));

            var branchGeom = new LineString(coordinates);

            return (IChannel)branchLayer.DataSource.Add(branchGeom);
        }

        private Catchment AddCatchment(Coordinate center, CatchmentType catchmentType)
        {
            const int offset = 400;

            var newCatchmentTool =
                regionEditor.MapView.MapControl.Tools.OfType<NewLineTool>().First(
                    t => t.Name == HydroRegionEditorMapTool.AddCatchmentToolName);
            var catchmentLayer = newCatchmentTool.Layers.First();

            ((CatchmentFeatureEditor) catchmentLayer.FeatureEditor).NewCatchmentType = catchmentType;

            var x0 = new Coordinate(center.X - offset, center.Y - offset, 0);
            var x1 = new Coordinate(center.X + offset, center.Y - offset, 0);
            var x2 = new Coordinate(center.X + offset, center.Y + offset, 0);
            var x3 = new Coordinate(center.X - offset, center.Y + offset, 0);

            newCatchmentTool.OnMouseDown(x0, args);
            newCatchmentTool.OnMouseMove(x1, args);
            newCatchmentTool.OnMouseMove(x1, args);
            newCatchmentTool.OnMouseMove(x2, args);
            newCatchmentTool.OnMouseUp(x3, args);
            
            return (Catchment) catchmentLayer.DataSource.Features.Cast<IFeature>().Last();
        }
    }
}