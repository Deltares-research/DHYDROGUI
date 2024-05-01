using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.IntegrationTestUtils.Builders;
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
    [TestFixture, Apartment(ApartmentState.STA)]
    [Category(TestCategory.WindowsForms)]
    public class HydroRegionEditorGuiIntegrationTest
    {
        protected IGui gui;
        protected Project project;
        protected IHydroRegion region;
        protected IDrainageBasin basin;
        protected IHydroNetwork network;
        protected Window mainWindow;
        protected Action onMainWindowShown;
        protected ProjectItemMapView regionEditor;
        private MouseEventArgs args = new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0);

        [SetUp]
        public void SetUp()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new SharpMapGisGuiPlugin(),
                new NetworkEditorGuiPlugin(),
                new CommonToolsGuiPlugin(),
            };
            gui = new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build();
            var app = gui.Application;
            
            gui.Run();

            app.CreateNewProject();
            
            project = app.Project;

            // add data

            network = new HydroNetwork();
            basin = new DrainageBasin();
            region = new HydroRegion { Name = "hr", SubRegions = { network, basin } };

            project.RootFolder.Add(region);

            // show gui main window
            mainWindow = (Window)gui.MainWindow;

            // wait until gui starts
            mainWindow.Loaded += delegate
            {
                var regionDataItem = project.RootFolder.DataItems.First();
                gui.CommandHandler.OpenView(regionDataItem, typeof(ProjectItemMapView));

                regionEditor = gui.DocumentViews.OfType<ProjectItemMapView>().First();

                onMainWindowShown();
            };
        }

        [TearDown]
        public void TearDown()
        {
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
        public void LinkCatchmentToLateral()
        {
            onMainWindowShown = () =>
                {
                    var b1 = AddBranch(new[] { new Coordinate(0, 0, 0), new Coordinate(1000, 0, 0) });
                    var lateral = new LateralSource() { Name = "lateral1", Branch = b1, Chainage = 5.0, Geometry = new NetTopologySuite.Geometries.Point(new Coordinate(500, 0, 0))  };
                    b1.BranchFeatures.Add(lateral);
                    var c1 = AddCatchment(new Coordinate(500, 1000, 0), CatchmentType.Unpaved);

                    var link = AddLink(c1.InteriorPoint.Coordinate, new Coordinate(500, 0, 0));

                    Assert.AreEqual(1, c1.Links.Count);
                    Assert.AreEqual(c1, link.Source);
                    Assert.AreEqual(lateral, link.Target);
                };

            WpfTestHelper.ShowModal(mainWindow);
        }

        [Test]
        public void LinkCatchmentToLateralCreatesValidGeometry()
        {
            onMainWindowShown = () =>
            {
                var c1 = AddCatchment(new Coordinate(500, 1000, 0), CatchmentType.Unpaved);
                var b1 = AddBranch(new[] { new Coordinate(0, 0, 0), new Coordinate(1000, 0, 0) });
                var lateral = new LateralSource() { Name = "lateral1", Branch = b1, Chainage = 5.0, Geometry = new NetTopologySuite.Geometries.Point(new Coordinate(500, 0, 0)) };
                b1.BranchFeatures.Add(lateral);
                var link = AddLink(c1.InteriorPoint.Coordinate, new Coordinate(500, 0, 0));

                Assert.IsTrue(link.Geometry.Coordinates.All(c => c.Z == 0.0));
            };

            WpfTestHelper.ShowModal(mainWindow);
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
                var ribbon = (Fluent.Ribbon)TypeUtils.GetField(mainWindow, "MainWindowRibbon");
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
                AddBranch(new[] { new Coordinate(0, 0, 0), new Coordinate(1000, 0, 0) });
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
            
            WpfTestHelper.ShowModal(mainWindow);
        }
        
        [Test]
        public void SelectedRouteGetsRemovedWhenRemoveSelectedRouteIsExecuted()
        {
            onMainWindowShown = () =>
            {
                /* get the coverages combo box from te ribbon */
                var ribbon = (Fluent.Ribbon)TypeUtils.GetField(mainWindow, "MainWindowRibbon");
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
                IChannel addedBranch = AddBranch(new[] { new Coordinate(0, 0, 0), new Coordinate(1000, 0, 0) });
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

                Assert.That(addedBranch.HydroNetwork.Routes, Is.Not.Empty);
                var removeCommand = new RemoveSelectedRouteCommand();
                removeCommand.Gui = gui;
                removeCommand.Execute();
                Assert.That(addedBranch.HydroNetwork.Routes, Is.Empty);
            };
            
            WpfTestHelper.ShowModal(mainWindow);
        }


        private HydroLink AddLink(Coordinate start, Coordinate end)
        {
            var newLinkTool = regionEditor.MapView.MapControl.Tools.OfType<AddHydroLinkMapTool>().First();

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

            var addBranch = (IChannel)branchLayer.DataSource.Add(branchGeom);
            return addBranch;
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