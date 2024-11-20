using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;
using SharpMap;
using SharpMap.Api.Enums;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.IntegrationTests.NHibernate
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    public class HibernateHydroNetworkIntegrationTest : NHibernateHydroRegionTestBase
    {
        [Test]
        public void RemoveMapWithNetworkLayerFromProjectAndSave()
        {
            //issue 1144

            string path =
                TestHelper.GetCurrentMethodName() + ".dsproj";
            ProjectRepository.Create(path);

            //create a project with a map
            var project = ProjectRepository.GetProject();
            project.RootFolder.Add(new Map());
            var mapDataItem = project.RootFolder.Items[0] as DataItem;
            Map map = (Map)mapDataItem.Value;

            //add a network to this map
            HydroNetwork network = new HydroNetwork();
            project.RootFolder.Add(network);
            var networkMapLayer = MapLayerProviderHelper.CreateLayersRecursive(network, null, new List<IMapLayerProvider> { new NetworkEditorMapLayerProvider() });
            map.Layers.Add(networkMapLayer);

            //save
            ProjectRepository.SaveOrUpdate(project);

            //remove the map
            project.RootFolder.Items.Remove(mapDataItem);
            ProjectRepository.SaveOrUpdate(project);
        }

        [Test]
        public void SaveHydroNetworkWithStandardCrossSectionDefinition()
        {
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";
            ProjectRepository.Create(path);
            var project = new Project();

            var network = new HydroNetwork();
            var node1 = new HydroNode("Node1") { Geometry = new Point(0, 0), LongName = "LongName" };
            var node2 = new HydroNode("Node2") { Geometry = new Point(10, 0) };
            var channel1 = new Channel(node1, node2)
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(10, 0) }),
                LongName = "Channel"
            };
            var crossSection = new CrossSection(new CrossSectionDefinitionStandard())
                                   {
                                       Network = network,
                                       Branch = channel1,
                                       Geometry = new Point(5, 0),
                                       Chainage = 5.0,
                                   };
            channel1.BranchFeatures.Add(crossSection);
            network.Nodes.AddRange(new[] { node1, node2 });
            network.Branches.AddRange(new[] { channel1 });

            var dataItem = new DataItem(network);
            project.RootFolder.Add(dataItem);
            ProjectRepository.SaveOrUpdate(project);
        }

        [Test]
        public void SaveGeneratedMapLayerInfo()
        {
            var layerInfo = new GeneratedMapLayerInfo
                                {
                                    Name = "Test",
                                    Visible = true,
                                    AutoUpdateThemeOnDataSourceChanged = false,
                                    MaxVisible = 1000,
                                    MinVisible = 1,
                                    RenderOrder = 2,
                                    Selectable = true,
                                    ShowInLegend = false,
                                    ShowAttributeTable = true,
                                    ShowLabels = false,
                                    LabelColumn = "labels",
                                    LabelStyle = new LabelStyle{HorizontalAlignment = HorizontalAlignmentEnum.Right},
                                    LabelShowInTreeView = true,
                                    Theme = new CategorialTheme("abc", new VectorStyle()),
                                    VectorStyle = new VectorStyle
                                                      {
                                                          Shape = ShapeType.Ellipse
                                                      }
                                };

            layerInfo.Theme.ThemeItems.AddRange(new[]
                                                    {
                                                        new CategorialThemeItem("a", new VectorStyle {Shape = ShapeType.Diamond}, null),
                                                        new CategorialThemeItem("b", new VectorStyle {Shape = ShapeType.Rectangle}, null),
                                                        new CategorialThemeItem("c", new VectorStyle {Shape = ShapeType.Triangle}, null),
                                                    });

            var savedLayerInfo = SaveAndRetrieveObject(layerInfo);

            Assert.AreEqual("Test", savedLayerInfo.Name);
            Assert.AreEqual(true, savedLayerInfo.Visible);
            Assert.AreEqual(false, savedLayerInfo.AutoUpdateThemeOnDataSourceChanged);
            Assert.AreEqual(1000, savedLayerInfo.MaxVisible);
            Assert.AreEqual(1, savedLayerInfo.MinVisible);
            Assert.AreEqual(2, savedLayerInfo.RenderOrder);
            Assert.AreEqual(true, savedLayerInfo.Selectable);
            Assert.AreEqual(false, savedLayerInfo.ShowInLegend);
            Assert.AreEqual(true, savedLayerInfo.ShowAttributeTable);
            Assert.AreEqual(false, savedLayerInfo.ShowLabels);
            Assert.AreEqual("labels", savedLayerInfo.LabelColumn);
            Assert.AreEqual(HorizontalAlignmentEnum.Right, savedLayerInfo.LabelStyle.HorizontalAlignment);
            Assert.AreEqual(true, savedLayerInfo.LabelShowInTreeView);
            Assert.AreEqual(ShapeType.Ellipse, savedLayerInfo.VectorStyle.Shape);

            var savedTheme = savedLayerInfo.Theme;
            Assert.AreEqual("abc", savedTheme.AttributeName);
            Assert.AreEqual(3, savedTheme.ThemeItems.Count);
            Assert.AreEqual(ShapeType.Diamond, ((VectorStyle)savedTheme.ThemeItems[0].Style).Shape);
            Assert.AreEqual(ShapeType.Rectangle, ((VectorStyle)savedTheme.ThemeItems[1].Style).Shape);
            Assert.AreEqual(ShapeType.Triangle, ((VectorStyle)savedTheme.ThemeItems[2].Style).Shape);
        }

        [Test]
        public void SaveNetworkAndCheckCollectionChangedForBranchFeatures()
        {
            //create a network
            var network = new HydroNetwork();

            INode fromNode = new HydroNode { Name = "From", Network = network, Geometry = new Point(1000, 1000) };
            INode toNode = new HydroNode { Name = "To", Network = network, Geometry = new Point(1000, 1500) };
            network.Nodes.Add(fromNode);
            network.Nodes.Add(toNode);

            var branch = CreateChannel(fromNode, toNode);
            network.Branches.Add(branch);
            var networkLocation = new NetworkLocation(branch, 10);

            networkLocation.Geometry = new WKTReader().Read("LINESTRING(20 20,20 30,30 30,30 20,40 20)");

            branch.BranchFeatures.Add(networkLocation);
            var crossSection = CrossSectionHelper.CreateNewCrossSectionXYZ(new List<Coordinate>
                                                            {
                                                                new Coordinate(1.0, 1.0, 0.0),
                                                                new Coordinate(2.0, 1.0, 0.1),
                                                                new Coordinate(3.0, 1.0, 0.1),
                                                                new Coordinate(4.0, 1.0, 0.1)
                                                            });

            //register to collectionchanged of network
            int callCount = 0;
            ((INotifyCollectionChange)(network)).CollectionChanged +=
                delegate(object sender, NotifyCollectionChangedEventArgs e)
                {
                    callCount++;
                    Debug.WriteLine(String.Format("{0} sent a {1} for {2}", sender, e.Action, e.GetRemovedOrAddedItem()));
                };
            //add a cross section results in only one call!
            branch.BranchFeatures.Add(crossSection);

            Assert.AreEqual(1, callCount);
            branch.BranchFeatures.Remove(crossSection);
            Assert.AreEqual(2, callCount);
            branch.BranchFeatures.Add(crossSection);
            Assert.AreEqual(3, callCount);


            //save it to a project.
            var project = new Project();
            project.RootFolder.Add(new DataItem(network));

            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            ProjectRepository.Create(path);
            ProjectRepository.SaveOrUpdate(project);

            //remove a brachfeature should result in a single changed event;
            Assert.AreEqual(3, callCount);
            branch.BranchFeatures.Remove(crossSection);
            Assert.AreEqual(4, callCount);

        }
    }
}