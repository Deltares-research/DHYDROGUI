using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Validators;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.TestUtils.TestReferenceHelper;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Export;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.CoverageViews;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap;
using SharpMap.Api;
using SharpTestsEx;
using Control = System.Windows.Controls.Control;

namespace DeltaShell.Plugins.NetworkEditor.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class NetworkEditorGuiPluginTest
    {
        private static readonly MockRepository Mocks = new MockRepository();

        [Test]
        public void GetFileExporters_ContainsExpectedExporterForShapes()
        {
            // Arrange
            var plugin = new NetworkEditorGuiPlugin();
            
            // Act
            IEnumerable<IFileExporter> exporters = plugin.GetFileExporters();

            // Assert
            Assert.That(exporters, Has.Exactly(1).TypeOf<HydroRegionShapeFileExporter>());
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void PluginGuiUpdatesCoverageViewViewContextsOnNetworkCoverageNetworkPropertyChanged()
        {
            using (var clipboardMock = new ClipboardMock())
            using (CultureUtils.SwitchToCulture("nl-NL"))
            {
                clipboardMock.GetText_Returns_SetText();
                
                var pluginsToAdd = new List<IPlugin>()
                {
                    new NetworkEditorApplicationPlugin(),
                    new SharpMapGisApplicationPlugin(),
                    new ProjectExplorerGuiPlugin(),
                    new SharpMapGisGuiPlugin(),
                    new NetworkEditorGuiPlugin(),
                };
                using (var gui = new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build())
                {
                    var app = gui.Application;

                    gui.Run();

                    app.CreateNewProject();
                    
                    // Create a network coverage and add it to the root folder
                    var networkCoverage = new NetworkCoverage {Network = new HydroNetwork {Name = "HydroNetwork 1"}};
                    app.Project.RootFolder.Add(networkCoverage);

                    // Create two view contexts and add them to the gui context manager
                    var coverageViewViewContext1 = new CoverageViewViewContext
                    {
                        Coverage = networkCoverage,
                        Map = new Map {Layers = {MapLayerProviderHelper.CreateLayersRecursive(networkCoverage.Network, null, new List<IMapLayerProvider> {new NetworkEditorMapLayerProvider()})}}
                    };

                    var coverageViewViewContext2 = new CoverageViewViewContext
                    {
                        Coverage = networkCoverage,
                        Map = new Map {Layers = {MapLayerProviderHelper.CreateLayersRecursive(networkCoverage.Network, null, new List<IMapLayerProvider> {new NetworkEditorMapLayerProvider()})}}
                    };

                    ((GuiContextManager) gui.ViewContextManager).ProjectViewContexts.Add(coverageViewViewContext1);
                    ((GuiContextManager) gui.ViewContextManager).ProjectViewContexts.Add(coverageViewViewContext2);

                    // Change the network of the coverage layer
                    networkCoverage.Network = new HydroNetwork {Name = "HydroNetwork 2"};

                    // Check if the network of both coverage view view contexts is correctly updated
                    var hydroNetworkMapLayer = coverageViewViewContext1.Map.Layers.OfType<HydroRegionMapLayer>().First(l => l.Region is IHydroNetwork);
                    Assert.AreEqual("HydroNetwork 2", hydroNetworkMapLayer.Region.Name);

                    hydroNetworkMapLayer = coverageViewViewContext2.Map.Layers.OfType<HydroRegionMapLayer>().First(l => l.Region is IHydroNetwork);
                    Assert.AreEqual("HydroNetwork 2", hydroNetworkMapLayer.Region.Name);
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RenamingNetworkCoverageNodesWrappedWithDataItems()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                new NetworkEditorApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new ProjectExplorerGuiPlugin(),
                new CommonToolsGuiPlugin(),
                new SharpMapGisGuiPlugin(),
                new NetworkEditorGuiPlugin(),

            };
            using (var gui = new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build())
            {
                var app = gui.Application;
                
                gui.Run();

                app.CreateNewProject();
                
                app.UserSettings["autosaveWindowLayout"] = false; // skip damagin of window layout

                var networkCoverage = new NetworkCoverage { Name = "coverage1" };
                app.Project.RootFolder.Add(networkCoverage);

                Action afterShow = () =>
                                       {
                                           var networkCoverageDataItem = app.Project.RootFolder.DataItems.Last();

                                           var treeView = ProjectExplorerGuiPlugin.Instance.ProjectExplorer.TreeView;
                                           treeView.Refresh();
                                           var nodePresenter = treeView.GetTreeViewNodePresenter(networkCoverageDataItem, null);

                                           treeView.WaitUntilAllEventsAreProcessed();
                                           var node = treeView.GetNodeByTag(networkCoverageDataItem);

                                           nodePresenter.CanRenameNode(node).Should("Renaming network coverages in project explorer is off by default").Be.False();

                                       };

                WpfTestHelper.ShowModal((Control) gui.MainWindow, afterShow);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void SelectingSubElementOfNetworkWithNoNetworkViewOpenDoesNotCauseException()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                new NetworkEditorApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),                
                new ProjectExplorerGuiPlugin(),
                new SharpMapGisGuiPlugin(),
                new NetworkEditorGuiPlugin(),
            };
            using (var gui = new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build())
            {
                var app = gui.Application;
                
                gui.Run();

                app.CreateNewProject();

                app.UserSettings["autosaveWindowLayout"] = false; // skip damagin of window layout

                var network = new HydroNetwork();
                app.Project.RootFolder.Add(network);

                var treeView = gui.ToolWindowViews.AllViews.OfType<HydroRegionTreeView>().First().TreeView;

                WpfTestHelper.ShowModal((Control) gui.MainWindow,
                    () =>
                        {
                            gui.Selection = app.Project.RootFolder.DataItems.First(di => di.Value == network);
                            Assert.AreEqual(network, treeView.Data);
                            treeView.SelectedNode = treeView.AllLoadedNodes.First(n => n.Text == "Shared Cross Section Definitions");
                            Assert.AreEqual(network, treeView.Data);
                        });
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ReleaseCopiedBranchFeatureOnProjectClosing()
        {
            using (var clipboardMock = new ClipboardMock())
            using (CultureUtils.SwitchToCulture("nl-NL"))
            {
                clipboardMock.GetData_Returns_SetData();

                var gui = Mocks.DynamicMock<IGui>();
                var documentViews = Mocks.DynamicMock<IViewList>();

                using (var mapView = new MapView())
                {
                    var application = Mocks.DynamicMock<IApplication>();
                    var projectService = Mocks.DynamicMock<IProjectService>();
                    application.Stub(a => a.FileExporters).Return(new List<IFileExporter>());
                    application.Stub(a => a.ProjectService).Return(projectService);

                    var project = new Project(); // Project is pretty lightweight don't need to mock here

                    var activeView = Mocks.DynamicMock<ICompositeView>();
                    Expect.Call(activeView.ChildViews).Return(new EventedList<IView>() {mapView}).Repeat.Any();
                    Expect.Call(documentViews.ActiveView).Return(activeView);

                    Expect.Call(gui.DocumentViews).Return(documentViews).Repeat.Any();
                    Expect.Call(gui.ToolWindowViews).Return(documentViews).Repeat.Any();
                    Expect.Call(gui.Application).Return(application).Repeat.Any();
                    Expect.Call(application.Project).Return(project).Repeat.Any();

                    projectService.ProjectClosing += null;
                    var projectClosingRaiser = LastCall.IgnoreArguments().GetEventRaiser();

                    Mocks.ReplayAll();

                    using (var pluginGui = new NetworkEditorGuiPlugin {Gui = gui})
                    {
                        pluginGui.Activate();

                        HydroNetworkCopyAndPasteHelper.SetNetworkFeatureToClipBoard(new Bridge());
                        Assert.IsTrue(HydroNetworkCopyAndPasteHelper.IsBranchFeatureSetToClipBoard());

                        projectClosingRaiser.Raise(this, new EventArgs<Project>(project));
                        Assert.IsFalse(HydroNetworkCopyAndPasteHelper.IsBranchFeatureSetToClipBoard());
                    }
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void SelectingAnotherCrossSectionInNetworkTreeCleansViewCorrectly_Tools7425()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                new NetworkEditorApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new ProjectExplorerGuiPlugin(),
                new SharpMapGisGuiPlugin(),
                new NetworkEditorGuiPlugin(),
            };
            using (var gui = new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build())
            {
                var app = gui.Application;
                
                gui.Run();

                app.CreateNewProject();
                
                app.UserSettings["autosaveWindowLayout"] = false; // skip damagin of window layout

                var network = new HydroNetwork();
                app.Project.RootFolder.Add(network);
                var branch1 = new Branch
                                  {
                                      Geometry = new LineString(new [] {new Coordinate(0, 0), new Coordinate(100, 0)})
                                  };
                NetworkHelper.AddChannelToHydroNetwork(network, branch1);

                var cs1 = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch1, CrossSectionDefinitionYZ.CreateDefault("cs1"), 15);
                var cs2 = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch1, CrossSectionDefinitionYZ.CreateDefault("cs2"), 30);

                var main = new CrossSectionSectionType {Name = "Main"};
                var fp1 = new CrossSectionSectionType {Name = "FloodPlain1"};

                cs1.Definition.Sections.Add(new CrossSectionSection {MinY = 0, MaxY = 25, SectionType = fp1});
                cs1.Definition.Sections.Add(new CrossSectionSection {MinY = 25, MaxY = 75, SectionType = main});
                cs1.Definition.Sections.Add(new CrossSectionSection {MinY = 75, MaxY = 100, SectionType = fp1});
                cs1.Name = "cs1";

                cs2.Definition.Sections.Add(new CrossSectionSection { MinY = 0, MaxY = 20, SectionType = fp1 });
                cs2.Definition.Sections.Add(new CrossSectionSection { MinY = 20, MaxY = 80, SectionType = main });
                cs2.Definition.Sections.Add(new CrossSectionSection {MinY = 80, MaxY = 100, SectionType = fp1});
                cs2.Name = "cs2";

                var treeView = gui.ToolWindowViews.AllViews.OfType<HydroRegionTreeView>().First().TreeView;

                WpfTestHelper.ShowModal(
                    (Control) gui.MainWindow,
                    () =>
                    {
                        gui.CommandHandler.OpenView(cs1);
                        var csView = (CrossSectionView) gui.DocumentViews.ActiveView;
                        var cs1Node = treeView.GetNodeByTag(cs1);
                        var cs2Node = treeView.GetNodeByTag(cs2);
                        
                        var propBefore = TestReferenceHelper.FindEventSubscriptions(cs2);
                        var chartView = (ChartView)csView.Controls.Find("chartView", true)[0];

                        var innerChart = TypeUtils.GetField(chartView.Chart, "chart");
                        var innerTools = TypeUtils.GetPropertyValue(innerChart, "Tools");
                        var toolsBefore = (int) TypeUtils.GetPropertyValue(innerTools, "Count");
                        
                        treeView.SelectedNode = cs2Node;

                        Assert.AreEqual(cs2, csView.Data);

                        treeView.SelectedNode = cs1Node;
                        treeView.SelectedNode = cs2Node;
                        treeView.SelectedNode = cs1Node;

                        // give delayed event handler some time
                        Thread.Sleep(100);
                        Application.DoEvents();
                        Thread.Sleep(100);

                        var propAfter = TestReferenceHelper.FindEventSubscriptions(cs2);
                        var toolsAfter = (int)TypeUtils.GetPropertyValue(innerTools, "Count");

                        Assert.AreEqual(propBefore, propAfter, "#event leaks");
                        Assert.AreEqual(toolsBefore, toolsAfter, "#tools leaks");
                    });
            }
        }
        [Test]
        [Category(TestCategory.Integration)]
        public void ShowPipeViewWithSharedCrossSection()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                new NetworkEditorApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new ProjectExplorerGuiPlugin(),
                new SharpMapGisGuiPlugin(),
                new NetworkEditorGuiPlugin(),
            };
            using (var gui = new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build())
            {
                var app = gui.Application;

                gui.Run();

                app.CreateNewProject();
                
                app.UserSettings["autosaveWindowLayout"] = false; // skip damagin of window layout
                var mocks = new MockRepository();
                var model = mocks.DynamicMultiMock<IModelWithRoughnessSections>(typeof(IModelWithNetwork), typeof(IItemContainer));
                
                var network = new HydroNetwork() { CrossSectionSectionTypes = new EventedList<CrossSectionSectionType>(new []{new CrossSectionSectionType(){Name = RoughnessDataSet.SewerSectionTypeName }, })};
                ((IModelWithNetwork) model).Expect(m => m.Network).Return(network).Repeat.Any();
                Expect.Call(model.GetDirectChildren())
                    .Return(Enumerable.Range(0, 1).Select(i => network).Cast<object>().AsEnumerable());

                
                var pipe = new Pipe()
                {
                    Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(100, 0) })
                };
                SewerFactory.AddDefaultPipeToNetwork(pipe, network);
                var RoughnessSections = new EventedList<RoughnessSection>
                {
                    new RoughnessSection(new CrossSectionSectionType {Name = RoughnessDataSet.SewerSectionTypeName},
                        network)
                };
                model.Expect(m => m.RoughnessSections).Return(RoughnessSections).Repeat.Any();
                mocks.ReplayAll();
                app.Project.RootFolder.Add(model);

                WpfTestHelper.ShowModal(
                    (Control)gui.MainWindow,
                    () =>
                    {
                        gui.CommandHandler.OpenView(pipe);
                        
                    });
                mocks.VerifyAll();

            }
        }

        [Test]
        public void GetViewInfoObjects_ContainsCorrectViewInfos()
        {
            // Setup
            var plugin = new NetworkEditorGuiPlugin();

            // Call
            IEnumerable<ViewInfo> viewInfos = plugin.GetViewInfoObjects();

            // Assert
            Assert.That(viewInfos, Has.One.Matches<ViewInfo>(IsValidatedFeatureViewInfo));
        }

        private static bool IsValidatedFeatureViewInfo(ViewInfo viewInfo)
        {
            // Cannot just use TypeOf<ValidatedFeatureViewInfo>, since generic ViewInfos are implicitly converted to a ViewInfo
            return viewInfo.DataType == typeof(ValidatedFeatures) &&
                   viewInfo.ViewDataType == typeof(IMap) &&
                   viewInfo.ViewType == typeof(MapView);
        }
    }
}