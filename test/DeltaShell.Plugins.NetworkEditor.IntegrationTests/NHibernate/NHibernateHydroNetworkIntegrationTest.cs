using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.Data.NHibernate.DelftTools.Shell.Core.Dao;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;
using SharpMap;
using SharpMap.Api;
using SharpMap.Api.Enums;
using SharpMap.Converters.WellKnownText;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.NetworkEditor.IntegrationTests.NHibernate
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    public class HibernateHydroNetworkIntegrationTest : NHibernateHydroRegionTestBase
    {
        [Test]
        public void SaveLoadHydroNetworkWithSharedDefinitions()
        {
            var network = new HydroNetwork();
            var node1 = new HydroNode("Node1")
            {
                Geometry = new Point(0, 0),
                LongName = "LongName"
            };
            var node2 = new HydroNode("Node2") {Geometry = new Point(10, 0)};
            var channel1 = new Channel(node1, node2)
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(10, 0)
                }),
                LongName = "Channel"
            };
            network.Nodes.AddRange(new[]
            {
                node1,
                node2
            });
            network.Branches.AddRange(new[]
            {
                channel1
            });

            var definitionName = "test";

            HydroNetwork retrievedNetwork = SaveLoadObject(network, TestHelper.GetCurrentMethodName() + ".dsproj");
        }

        [Test]
        public void SaveLoadHydroNetworkWithDefaultDefinition()
        {
            var network = new HydroNetwork();
            var node1 = new HydroNode("Node1")
            {
                Geometry = new Point(0, 0),
                LongName = "LongName"
            };
            var node2 = new HydroNode("Node2") {Geometry = new Point(10, 0)};
            var channel1 = new Channel(node1, node2)
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(10, 0)
                }),
                LongName = "Channel"
            };
            network.Nodes.AddRange(new[]
            {
                node1,
                node2
            });
            network.Branches.AddRange(new[]
            {
                channel1
            });

            var definitionName = "test";

            HydroNetwork retrievedNetwork = SaveLoadObject(network, TestHelper.GetCurrentMethodName() + ".dsproj");
        }

        [Test]
        public void SaveHydroNetworkWithStandardCrossSectionDefinition()
        {
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";
            ProjectRepository.Create(path);
            var project = new Project();

            var network = new HydroNetwork();
            var node1 = new HydroNode("Node1")
            {
                Geometry = new Point(0, 0),
                LongName = "LongName"
            };
            var node2 = new HydroNode("Node2") {Geometry = new Point(10, 0)};
            var channel1 = new Channel(node1, node2)
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(10, 0)
                }),
                LongName = "Channel"
            };
           
            network.Nodes.AddRange(new[]
            {
                node1,
                node2
            });
            network.Branches.AddRange(new[]
            {
                channel1
            });

            var dataItem = new DataItem(network);
            project.RootFolder.Add(dataItem);
            ProjectRepository.SaveOrUpdate(project);
        }

        [Test]
        public void SaveLoadHydroNetworkWithRoutes()
        {
            IHydroNetwork network = CreateDummyHydroNetwork();

            var routeName = "NewRoute";

            var route = new Route {Name = routeName};
            network.Routes.Add(route);

            route.Locations.AddValues(new[]
            {
                new NetworkLocation(network.Branches[0], 0),
                new NetworkLocation(network.Branches[1], 5)
            });

            double routeLength = RouteHelper.GetRouteLength(route);

            Assert.AreEqual(105, routeLength);

            IHydroNetwork retrievedNetwork = SaveLoadObject(network, TestHelper.GetCurrentMethodName() + ".dsproj");

            Assert.AreEqual(1, retrievedNetwork.Routes.Count);
            Route retrievedRoute = retrievedNetwork.Routes[0];
            Assert.AreEqual(routeName, retrievedRoute.Name);
            Assert.AreEqual(routeLength, RouteHelper.GetRouteLength(retrievedRoute));
        }

        [Test]
        public void SaveLoadHydroNetwork()
        {
            //node 1 --(b1)--> node2 <--(b2)- node 3 <--(b3)- node 4
            var network = new HydroNetwork();
            var node1 = new HydroNode("Node1")
            {
                Geometry = new Point(0, 0),
                LongName = "LongName"
            };
            var node2 = new HydroNode("Node2") {Geometry = new Point(10, 0)};
            var node3 = new HydroNode("Node3") {Geometry = new Point(20, 0)};
            var node4 = new HydroNode("Node4") {Geometry = new Point(30, 0)};
            var channel1 = new Channel(node1, node2)
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(10, 0)
                }),
                LongName = "Channel"
            };
            var channel2 = new Channel(node3, node2)
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(20, 0),
                    new Coordinate(10, 0)
                })
            };
            var channel3 = new Channel(node4, node3)
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(20, 0),
                    new Coordinate(10, 0)
                })
            };

            network.CoordinateSystem = Map.CoordinateSystemFactory.CreateFromEPSG(3857);
            network.Nodes.AddRange(new[]
            {
                node1,
                node2,
                node3,
                node4
            });
            network.Branches.AddRange(new[]
            {
                channel1,
                channel2,
                channel3
            });

            Assert.IsFalse(node1.IsConnectedToMultipleBranches);
            Assert.IsTrue(node2.IsConnectedToMultipleBranches);
            Assert.IsTrue(node3.IsConnectedToMultipleBranches);
            Assert.IsFalse(node4.IsConnectedToMultipleBranches);

            HydroNetwork retrievedNetwork = SaveLoadObject(network, TestHelper.GetCurrentMethodName() + ".dsproj");

            Assert.AreEqual(4, retrievedNetwork.Nodes.Count);
            Assert.AreEqual(3, retrievedNetwork.Branches.Count);

            IHydroNode retrievedNode1 = retrievedNetwork.HydroNodes.First();
            INode retrievedNode2 = retrievedNetwork.Nodes[1];
            INode retrievedNode3 = retrievedNetwork.Nodes[2];
            INode retrievedNode4 = retrievedNetwork.Nodes[3];
            IChannel retrievedBranch1 = retrievedNetwork.Channels.First();
            IBranch retrievedBranch2 = retrievedNetwork.Branches[1];

            // Check nodes
            Assert.AreEqual("LongName", retrievedNode1.LongName);

            // Check boundary stuff
            Assert.IsFalse(retrievedNode1.IsConnectedToMultipleBranches);
            Assert.IsTrue(retrievedNode2.IsConnectedToMultipleBranches);
            Assert.IsTrue(retrievedNode3.IsConnectedToMultipleBranches);
            Assert.IsFalse(retrievedNode4.IsConnectedToMultipleBranches);

            // Check in/out branches
            Assert.AreEqual(retrievedBranch1.Source, retrievedNode1);
            Assert.AreEqual(retrievedBranch1.Target, retrievedNode2);
            Assert.AreEqual(retrievedBranch1, retrievedNode1.OutgoingBranches[0]);
            Assert.AreEqual("Channel", retrievedBranch1.LongName);
            Assert.AreEqual(retrievedBranch1, retrievedNode2.IncomingBranches[0]);
            Assert.AreEqual(retrievedBranch2, retrievedNode2.IncomingBranches[1]);

            Assert.NotNull(retrievedNetwork.CoordinateSystem);
            Assert.AreEqual(3857, retrievedNetwork.CoordinateSystem.AuthorityCode);
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
                LabelStyle = new LabelStyle {HorizontalAlignment = HorizontalAlignmentEnum.Right},
                LabelShowInTreeView = true,
                Theme = new CategorialTheme("abc", new VectorStyle()),
                VectorStyle = new VectorStyle {Shape = ShapeType.Ellipse}
            };

            layerInfo.Theme.ThemeItems.AddRange(new[]
            {
                new CategorialThemeItem("a", new VectorStyle {Shape = ShapeType.Diamond}, null),
                new CategorialThemeItem("b", new VectorStyle {Shape = ShapeType.Rectangle}, null),
                new CategorialThemeItem("c", new VectorStyle {Shape = ShapeType.Triangle}, null)
            });

            GeneratedMapLayerInfo savedLayerInfo = SaveAndRetrieveObject(layerInfo);

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

            ITheme savedTheme = savedLayerInfo.Theme;
            Assert.AreEqual("abc", savedTheme.AttributeName);
            Assert.AreEqual(3, savedTheme.ThemeItems.Count);
            Assert.AreEqual(ShapeType.Diamond, ((VectorStyle) savedTheme.ThemeItems[0].Style).Shape);
            Assert.AreEqual(ShapeType.Rectangle, ((VectorStyle) savedTheme.ThemeItems[1].Style).Shape);
            Assert.AreEqual(ShapeType.Triangle, ((VectorStyle) savedTheme.ThemeItems[2].Style).Shape);
        }

/*
        [Test]
        [Category(TestCategory.DataAccess)]
        public void SaveLoadCentralMapViewContext()
        {
            var layerInfo1 = new GeneratedMapLayerInfo { Name = "Test1" };
            var layerInfo2 = new GeneratedMapLayerInfo { Name = "Test2" };

            var map = new Map();
            map.Layers.Add(new VectorLayer("new layer"));

            var centralMapViewContext = new ProjectItemMapViewContext
                                            {
                                                Data = new HydroRegion { Name = "Test region" },
                                                DataLayerIndex = 0,
                                                GeneratedMapLayerInfoList = new List<GeneratedMapLayerInfo>
                                                                                {
                                                                                    layerInfo1, layerInfo2
                                                                                },
                                                Map = map
                                            };

            var savedContext = SaveAndRetrieveObject(centralMapViewContext);

            Assert.IsTrue(centralMapViewContext.Data is IHydroRegion);
            Assert.AreEqual("Test region", ((IHydroRegion)savedContext.Data).Name);
            Assert.AreEqual(0, savedContext.DataLayerIndex);
            Assert.AreEqual(2, savedContext.GeneratedMapLayerInfoList.Count);
        }
*/

        [Test]
        public void SaveLoadPump()
        {
            // add pump
            var pump = new Pump
            {
                OffsetY = 33,
                Chainage = 22,
                Geometry = new Point(5, 0),
                DirectionIsPositive = true,
                StartDelivery = 75,
                StartSuction = 3,
                StopDelivery = 2,
                StopSuction = 22,
                ControlDirection = PumpControlDirection.SuctionAndDeliverySideControl,
                LongName = "LongName",
                Name = "Name"
            };
            IPump retrievedPump = SaveLoadStructure(pump, TestHelper.GetCurrentMethodName() + ".dsproj");
            Assert.AreEqual(pump.OffsetY, retrievedPump.OffsetY);
            Assert.AreEqual(pump.Chainage, retrievedPump.Chainage);
            Assert.AreEqual(pump.DirectionIsPositive, retrievedPump.DirectionIsPositive);
            Assert.AreEqual(pump.StartDelivery, retrievedPump.StartDelivery);
            Assert.AreEqual(pump.StartSuction, retrievedPump.StartSuction);
            Assert.AreEqual(pump.StopDelivery, retrievedPump.StopDelivery);
            Assert.AreEqual(pump.StopSuction, retrievedPump.StopSuction);
            Assert.AreEqual(pump.ControlDirection, retrievedPump.ControlDirection);
            Assert.AreEqual(pump.DirectionIsPositive, retrievedPump.DirectionIsPositive);
            Assert.AreEqual(pump.Name, retrievedPump.Name);
            Assert.AreEqual(pump.LongName, retrievedPump.LongName);
        }

        

        

        

        [Test]
        public void SaveLoadWeir()
        {
            var weir = new Weir
            {
                Geometry = new Point(5, 0),
                OffsetY = 150,
                CrestWidth = 75,
                CrestLevel = -3,
                CrestShape = CrestShape.Triangular
            };
            Weir retrievedWeir = SaveLoadStructure(weir, TestHelper.GetCurrentMethodName() + ".dsproj");

            Assert.AreEqual(150, retrievedWeir.OffsetY);
            Assert.AreEqual(75, retrievedWeir.CrestWidth);
            Assert.AreEqual(-3, retrievedWeir.CrestLevel);
            Assert.AreEqual(CrestShape.Triangular, retrievedWeir.CrestShape);
        }

        [Test]
        public void SaveLoadGate()
        {
            var gate = new Gate()
            {
                Geometry = new Point(5, 0),
                OffsetY = 150,
                OpeningWidth = 75,
                SillLevel = -3
            };
            Gate retrievedGate = SaveLoadStructure(gate, TestHelper.GetCurrentMethodName() + ".dsproj");

            Assert.AreEqual(150, retrievedGate.OffsetY);
            Assert.AreEqual(75, retrievedGate.OpeningWidth);
            Assert.AreEqual(-3, retrievedGate.SillLevel);
        }

        [Test]
        public void SaveLoadLateralSource()
        {
            var lateralSource = new LateralSource
            {
                Name = "Source1",
                Chainage = 50,
                Geometry = new Point(0, 0)
            };
            LateralSource retrievedLateralSource = SaveLoadBranchFeature(lateralSource, TestHelper.GetCurrentMethodName());

            Assert.AreEqual(lateralSource.Name, retrievedLateralSource.Name);
            Assert.AreEqual(lateralSource.Chainage, retrievedLateralSource.Chainage);
        }

        [Test]
        public void SaveLoadLateralSourceGeometry()
        {
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";
            using (NHibernateProjectRepository repository = factory.CreateNew())
            {
                repository.Create(path);

                Project project = repository.GetProject();
                IHydroNetwork network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
                project.RootFolder.Add(network);

                var lateral = new LateralSource
                {
                    Name = "Source1",
                    Chainage = 50,
                    Geometry = new Point(0, 0)
                };
                IBranch branch0 = network.Branches[0];
                branch0.BranchFeatures.Add(lateral);
                lateral.Branch = branch0;

                repository.SaveAs(project, path);
                repository.Close();
            }

            using (NHibernateProjectRepository repository = factory.CreateNew())
            {
                repository.Open(path);

                Project project = repository.GetProject();
                var network = (HydroNetwork) project.RootFolder.DataItems.First(di => di.Value is HydroNetwork).Value;
                ILateralSource lateral = network.LateralSources.First();
                lateral.Geometry = GeometryHelper.SetCoordinate(lateral.Geometry, 0, new Coordinate(50, 0));

                repository.SaveOrUpdate(project);
            }

            using (NHibernateProjectRepository repository = factory.CreateNew())
            {
                repository.Open(path);

                Project project = repository.GetProject();
                var network = (HydroNetwork) project.RootFolder.DataItems.First(di => di.Value is HydroNetwork).Value;
                ILateralSource lateral = network.LateralSources.First();

                Assert.AreEqual(new Point(50, 0), lateral.Geometry);
            }
        }

        [Test]
        public void SaveLoadDiffuseLateralSource()
        {
            IHydroNetwork hydroNetwork = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(200, 0), new Point(200, 200));
            IBranch branch1 = hydroNetwork.Branches[0];

            var lateralSource = new LateralSource
            {
                Name = "Source1",
                Chainage = 10
            };
            branch1.BranchFeatures.Add(lateralSource);
            lateralSource.Branch = branch1;
            HydroRegionEditorHelper.UpdateBranchFeatureGeometry(lateralSource, 40);

            LateralSource retrievedLateralSource = SaveLoadBranchFeature(lateralSource, TestHelper.GetCurrentMethodName());

            Assert.AreEqual(lateralSource.Name, retrievedLateralSource.Name);
            Assert.AreEqual(lateralSource.Chainage, retrievedLateralSource.Chainage);
            Assert.AreEqual(lateralSource.IsDiffuse, retrievedLateralSource.IsDiffuse);
            Assert.AreEqual(lateralSource.Length, retrievedLateralSource.Length);
        }

        [Test]
        public void SaveLoadRetention()
        {
            var retention = new Retention()
            {
                LongName = "Retention_BovenMaas",
                BedLevel = 1,
                LevelBL = 2,
                StorageArea = 3,
                StreetStorageArea = 4,
                StreetLevel = 5,
                UseTable = true,
                Type = RetentionType.Loss,
                Name = "321",
                Chainage = 50,
                Geometry = new Point(0, 0)
            };
            Retention retrievedRetention = SaveLoadBranchFeature(retention, TestHelper.GetCurrentMethodName());

            Assert.AreEqual(retention.BedLevel, retrievedRetention.BedLevel);
            Assert.AreEqual(retention.LevelBL, retrievedRetention.LevelBL);
            Assert.AreEqual(retention.StorageArea, retrievedRetention.StorageArea);
            Assert.AreEqual(retention.StreetStorageArea, retrievedRetention.StreetStorageArea);
            Assert.AreEqual(retention.StreetLevel, retrievedRetention.StreetLevel);
            Assert.AreEqual(retention.UseTable, retrievedRetention.UseTable);
            Assert.AreEqual(retention.Type, retrievedRetention.Type);
            Assert.AreEqual(retention.LongName, retrievedRetention.LongName);
            Assert.AreEqual(retention.Name, retrievedRetention.Name);
            Assert.AreEqual(retention.Chainage, retrievedRetention.Chainage);
        }

        [Test]
        public void SaveLoadObservationPoint()
        {
            var observationPoint = new ObservationPoint()
            {
                LongName = "ObservationPoint_Maas",
                Name = "123",
                Chainage = 50,
                Geometry = new Point(0, 0)
            };
            ObservationPoint retrievedObservationPoint = SaveLoadBranchFeature(observationPoint, TestHelper.GetCurrentMethodName());

            Assert.AreEqual(observationPoint.LongName, retrievedObservationPoint.LongName);
            Assert.AreEqual(observationPoint.Name, retrievedObservationPoint.Name);
            Assert.AreEqual(observationPoint.Chainage, retrievedObservationPoint.Chainage);
        }

        [Test]
        public void ReloadNetworkDoesWorkWithNotifyCollectionChanged()
        {
            var network = new HydroNetwork();

            INode fromNode = new HydroNode
            {
                Name = "From",
                Network = network,
                Geometry = new Point(1000, 1000)
            };
            INode toNode = new HydroNode
            {
                Name = "To",
                Network = network,
                Geometry = new Point(1000, 1500)
            };
            network.Nodes.Add(fromNode);
            network.Nodes.Add(toNode);

            IChannel branch = CreateChannel(fromNode, toNode);
            network.Branches.Add(branch);

           
            var callCount = 0;
            ((INotifyCollectionChange) network.Branches[0]).CollectionChanged += delegate { callCount++; };
            network.Branches[0].BranchFeatures.RemoveAt(0);
            Assert.AreEqual(1, callCount);

            //save
            var project = new Project();
            project.RootFolder.Add(new DataItem(network));

            string path = TestHelper.GetCurrentMethodName() + ".dsproj";
            ProjectRepository.Create(path);
            ProjectRepository.SaveOrUpdate(project);
            ProjectRepository.Close();

            //reopen
            Project retrievedProject = ProjectRepository.Open(path);
            var retrievedNetwork = (IHydroNetwork) retrievedProject.RootFolder.DataItems.FirstOrDefault().Value;

            //remove a crossSection and get Notified at the network level.
            callCount = 0;
            IBranch retrievedBranch = retrievedNetwork.Branches[0];
            retrievedBranch.BranchFeatures.CollectionChanged += delegate { callCount++; };
            //crossSections is just a filtered view of branchfeatures.
            ((IList) retrievedBranch.BranchFeatures).RemoveAt(0);

            Assert.AreEqual(0, retrievedBranch.BranchFeatures.Count);
            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void WriteAndReadProjectNetworkContainingAllSubItems()
        {
            var network = new HydroNetwork();

            INode fromNode = new HydroNode
            {
                Name = "From",
                Network = network,
                Geometry = new Point(1000, 1000)
            };
            INode toNode = new HydroNode
            {
                Name = "To",
                Network = network,
                Geometry = new Point(1000, 1500)
            };
            network.Nodes.Add(fromNode);
            network.Nodes.Add(toNode);

            IChannel branch = CreateChannel(fromNode, toNode);
            network.Branches.Add(branch);
            
            network.Name = "NetworkWithAllSubItems";
            var project = new Project();
            project.RootFolder.Add(new DataItem(network));

            string path = TestHelper.GetCurrentMethodName() + ".dsproj";
            ProjectRepository.Create(path);
            ProjectRepository.SaveOrUpdate(project);
            ProjectRepository.Close();

            Project retrievedProject = ProjectRepository.Open(path);

            Assert.AreEqual(1, retrievedProject.RootFolder.DataItems.Count());
            var network2 = (IHydroNetwork) retrievedProject.RootFolder.DataItems.FirstOrDefault().Value;
            Assert.AreEqual(network.Name, network2.Name);

            Assert.AreEqual(2, network2.Nodes.Count);
            Assert.AreEqual(1, network2.Branches.Count);
        }

        [Test]
        public void SaveNetworkAndCheckCollectionChangedForBranchFeatures()
        {
            //create a network
            var network = new HydroNetwork();

            INode fromNode = new HydroNode
            {
                Name = "From",
                Network = network,
                Geometry = new Point(1000, 1000)
            };
            INode toNode = new HydroNode
            {
                Name = "To",
                Network = network,
                Geometry = new Point(1000, 1500)
            };
            network.Nodes.Add(fromNode);
            network.Nodes.Add(toNode);

            IChannel branch = CreateChannel(fromNode, toNode);
            network.Branches.Add(branch);
            var networkLocation = new NetworkLocation(branch, 10);

            networkLocation.Geometry = new WKTReader().Read("LINESTRING(20 20,20 30,30 30,30 20,40 20)");

            branch.BranchFeatures.Add(networkLocation);
            
            //register to collectionchanged of network
            var callCount = 0;
            ((INotifyCollectionChange) network).CollectionChanged +=
                delegate(object sender, NotifyCollectionChangedEventArgs e)
                {
                    callCount++;
                    Debug.WriteLine(string.Format("{0} sent a {1} for {2}", sender, e.Action, e.GetRemovedOrAddedItem()));
                };
           

            //save it to a project.
            var project = new Project();
            project.RootFolder.Add(new DataItem(network));

            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            ProjectRepository.Create(path);
            ProjectRepository.SaveOrUpdate(project);

            //remove a brachfeature should result in a single changed event;
            Assert.AreEqual(3, callCount);
            
        }

        [Test]
        public void SaveLoadChannel()
        {
            //save it to a project.
            var project = new Project();
            IHydroNetwork network = CreateDummyHydroNetwork();
            project.RootFolder.Add(new DataItem(network));

            IChannel firstChannel = network.Channels.First();
            var orderNumber = 22;
            firstChannel.OrderNumber = orderNumber;

            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            ProjectRepository.Create(path);
            ProjectRepository.SaveOrUpdate(project);
            ProjectRepository.Close();

            Project retrievedProject = ProjectRepository.Open(path);
            var retrievedNetwork = (IHydroNetwork) retrievedProject.RootFolder.DataItems.FirstOrDefault().Value;
            IChannel firstChannelOfRetrievedNetwork = retrievedNetwork.Channels.First();

            Assert.AreEqual(orderNumber, firstChannelOfRetrievedNetwork.OrderNumber);
        }

        private static IHydroNetwork CreateDummyHydroNetwork()
        {
            // create network
            var network = new HydroNetwork();

            var node1 = new HydroNode("node1");
            var node2 = new HydroNode("node2");
            var node3 = new HydroNode("node3");

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Nodes.Add(node3);

            var branch1 = new Channel("branch1", node1, node2) {Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)")};
            var branch2 = new Channel("branch2", node2, node3) {Geometry = GeometryFromWKT.Parse("LINESTRING (100 0, 300 0)")};

            node1.Geometry = new Point(0, 0);
            node2.Geometry = new Point(100, 0);
            node3.Geometry = new Point(300, 0);

            network.Branches.Add(branch1);
            network.Branches.Add(branch2);
            return network;
        }

        private T SaveLoadStructure<T>(T structure, string path) where T : IStructure1D
        {
            var compositeStructure = new CompositeBranchStructure
            {
                Geometry = new Point(5, 0),
                Chainage = 5,
                Structures = {structure}
            };

            CompositeBranchStructure retrievedCompositeStructure = SaveLoadBranchFeature(compositeStructure, path);
            return (T) retrievedCompositeStructure.Structures[0];
        }
    }
}
