using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.PhysicalParameters;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Boundary;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Validation;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.Validation
{
    [TestFixture]
    public class WaterFlowModel1DModelMergeHelperTest
    {
        private WaterFlowModel1D destinationModel1D;
        private WaterFlowModel1D sourceModel1D;
        
        [SetUp]
        public void SetupModels()
        {
            destinationModel1D = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(0, 100);
            destinationModel1D.Name = "Destination";
            sourceModel1D = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(100, 120);
            sourceModel1D.Name = "Source";
        }

        #region Get Connected Nodes
        [Test]
        public void Given2ModelsWith1SourceNodedConnectingTo1DestinationNodesWhenGetConnectedNodeListThenHave1ConnectedNode()
        {
            var connectedNodes = WaterFlowModel1DModelMergeHelper.ConnectedNodesList(destinationModel1D, sourceModel1D);
            Assert.That(connectedNodes.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Given2ModelsWith2SourceNodedConnectingTo2DestinationNodesWhenGetConnectedNodeListThenHave2ConnectedNodes()
        {
            var extraSourceNode1 = new HydroNode("extraSourceNode1") { Geometry = new Point(5, 0) };
            sourceModel1D.Network.Nodes.Add(extraSourceNode1);

            var connectedNodes = WaterFlowModel1DModelMergeHelper.ConnectedNodesList(destinationModel1D, sourceModel1D);
            Assert.That(connectedNodes.Count(), Is.EqualTo(2));
        }

        [Test]
        public void Given2ModelsWith2SourceNodedAnd1SpareSourceNodeConnectingTo2DestinationNodesWhenGetConnectedNodeListThenHave2ConnectedNodes()
        {
            var extraSourceNode1 = new HydroNode("extraSourceNode1") { Geometry = new Point(5, 0) };
            sourceModel1D.Network.Nodes.Add(extraSourceNode1);
            
            var extraSourceNode2 = new HydroNode("extraSourceNode2") { Geometry = new Point(200, 0) };
            sourceModel1D.Network.Nodes.Add(extraSourceNode2);

            var connectedNodes = WaterFlowModel1DModelMergeHelper.ConnectedNodesList(destinationModel1D, sourceModel1D);
            Assert.That(connectedNodes.Count(), Is.EqualTo(2));
        }

        [Test]
        public void Given2ModelsWith2SourceNodedConnectingTo2DestinationNodesAnd1SpareDestinationNodeWhenGetConnectedNodeListThenHave2ConnectedNodes()
        {
            var extraSourceNode1 = new HydroNode("extraSourceNode1") { Geometry = new Point(5, 0) };
            sourceModel1D.Network.Nodes.Add(extraSourceNode1);

            var extraDestinationNode1 = new HydroNode("extraDestinationNode1") { Geometry = new Point(200, 0) };
            destinationModel1D.Network.Nodes.Add(extraDestinationNode1);

            var connectedNodes = WaterFlowModel1DModelMergeHelper.ConnectedNodesList(destinationModel1D, sourceModel1D);
            Assert.That(connectedNodes.Count(), Is.EqualTo(2));
        }

        [Test]
        public void Given2ModelsWith2SourceNodedConnectingTo1DestinationNodesWhenGetConnectedNodeListThenHave1ConnectedDestinationNodeWith2Source()
        {
            var extraDestinationNode1 = new HydroNode("extraDestinationNode1") { Geometry = new Point(101, 0) };// can also be connected with sourcenode node1
            destinationModel1D.Network.Nodes.Add(extraDestinationNode1);

            var connectedNodes = WaterFlowModel1DModelMergeHelper.ConnectedNodesList(destinationModel1D, sourceModel1D);
            Assert.That(connectedNodes.Count(), Is.EqualTo(1));
            var connectedNode = connectedNodes.FirstOrDefault();
            Assert.That(connectedNode, Is.Not.Null);

            Assert.That(connectedNode.Key.Name, Is.EqualTo("node2"));//destination node : node2 is closer than extraDestinationNode1 
        }

        [Test]
        public void Given2ModelsWith1SourceNodedConnectingTo2DestinationNodesWhenGetConnectedNodeListThenHave1ConnectedNearsestNode()
        {
            var extraSourceNode1 = new HydroNode("extraSourceNode1") { Geometry = new Point(101, 0) };
            sourceModel1D.Network.Nodes.Add(extraSourceNode1);

            var connectedNodes = WaterFlowModel1DModelMergeHelper.ConnectedNodesList(destinationModel1D, sourceModel1D);
            Assert.That(connectedNodes.Count(), Is.EqualTo(1));
            var connectedNode = connectedNodes.FirstOrDefault();
            Assert.That(connectedNode, Is.Not.Null);
            Assert.That(connectedNode.Value.Count, Is.EqualTo(2));

            var connectingDestinationNode =
                destinationModel1D.Network.Nodes.FirstOrDefault(n => n.Name == connectedNode.Key.Name);
            Assert.That(connectingDestinationNode, Is.Not.Null);

            var connectingSourceNode1 = sourceModel1D.Network.Nodes.FirstOrDefault(n => n.Name == connectedNode.Value.ElementAt(0).Name);
            Assert.That(connectingSourceNode1, Is.Not.Null);

            var connectingSourceNode2 = sourceModel1D.Network.Nodes.FirstOrDefault(n => n.Name == connectedNode.Value.ElementAt(1).Name);
            Assert.That(connectingSourceNode2, Is.Not.Null);
        }
        #endregion

        #region rename elements
        [Test]
        public void Given2ModelsWithNodesWhenRenameNetworkElementNodesThenNodesRenamedInSourceModel()
        {
            WaterFlowModel1DModelMergeHelper.RenameNetworkElement(typeof (HydroNode), destinationModel1D, sourceModel1D);
            Assert.That(sourceModel1D.Network.Nodes.Count, Is.EqualTo(2));
            Assert.That(sourceModel1D.Network.Nodes.ElementAt(0).Name, Is.EqualTo("Source0_node1"));
            Assert.That(sourceModel1D.Network.Nodes.ElementAt(1).Name, Is.EqualTo("Source0_node2"));
        }
        [Test]
        public void Given2ModelsWithChannelWhenRenameNetworkElementChannelThenChannelRenamedInSourceModel()
        {
            WaterFlowModel1DModelMergeHelper.RenameNetworkElement(typeof(Channel), destinationModel1D, sourceModel1D);
            Assert.That(sourceModel1D.Network.Channels.Count(), Is.EqualTo(1));
            Assert.That(sourceModel1D.Network.Channels.ElementAt(0).Name, Is.EqualTo("Source0_channel"));
        }

        [Test]
        public void Given2ModelsWithChannelWhenRenameAllNetworkElementsThenChannelRenamedInSourceModel()
        {
            const string uniquenodename = "UniqueNodeName";
            sourceModel1D.Network.Nodes.ElementAt(1).Name = uniquenodename;
            WaterFlowModel1DModelMergeHelper.RenameAllNetworkElements(destinationModel1D, sourceModel1D);
            Assert.That(sourceModel1D.Network.Channels.Count(), Is.EqualTo(1));
            Assert.That(sourceModel1D.Network.Channels.ElementAt(0).Name, Is.EqualTo("Source0_channel"));
            
            Assert.That(sourceModel1D.Network.Nodes.Count, Is.EqualTo(2));
            Assert.That(sourceModel1D.Network.Nodes.ElementAt(0).Name, Is.EqualTo("Source0_node1"));
            Assert.That(sourceModel1D.Network.Nodes.ElementAt(1).Name, Is.EqualTo(uniquenodename));
        }
        [Test]
        public void Given2ModelsWithNodesWithMergedModelNamesWhenRenameNetworkElementNodesThenNodesRenamedInSourceModel()
        {
            destinationModel1D.Network.Nodes[0].Name = "Source0_node1";
            destinationModel1D.Network.Nodes[1].Name = "Source0_node2";
            
            sourceModel1D.Network.Nodes[0].Name = "Source0_node1";
            sourceModel1D.Network.Nodes[1].Name = "Source0_node2";


            WaterFlowModel1DModelMergeHelper.RenameNetworkElement(typeof (HydroNode), destinationModel1D, sourceModel1D);
            Assert.That(sourceModel1D.Network.Nodes.Count, Is.EqualTo(2));
            Assert.That(sourceModel1D.Network.Nodes.ElementAt(0).Name, Is.EqualTo("Source1_node1"));
            Assert.That(sourceModel1D.Network.Nodes.ElementAt(1).Name, Is.EqualTo("Source1_node2"));
        }
        [Test]
        public void Given2ModelsWithMoreNodesWithMergedModelNamesWhenRenameNetworkElementNodesThenNodesRenamedInSourceModel()
        {
            destinationModel1D.Network.Nodes[0].Name = "Source0_node1";
            destinationModel1D.Network.Nodes[1].Name = "Source0_node2";
            destinationModel1D.Network.Nodes.Add(new HydroNode("Source1_node1"));


            sourceModel1D.Network.Nodes[0].Name = "Source0_node1";
            sourceModel1D.Network.Nodes[1].Name = "Source0_node2";
            sourceModel1D.Network.Nodes.Add(new HydroNode("Source1_node1"));


            WaterFlowModel1DModelMergeHelper.RenameNetworkElement(typeof(HydroNode), destinationModel1D, sourceModel1D);
            Assert.That(sourceModel1D.Network.Nodes.Count, Is.EqualTo(3));
            Assert.That(sourceModel1D.Network.Nodes.ElementAt(0).Name, Is.EqualTo("Source2_node1"));
            Assert.That(sourceModel1D.Network.Nodes.ElementAt(1).Name, Is.EqualTo("Source1_node2"));
            Assert.That(sourceModel1D.Network.Nodes.ElementAt(2).Name, Is.EqualTo("Source3_node1"));
        }
        [Test]
        public void Given2ModelsWithNodesWithMergedModelNamesWithoutSequenceNumberWhenRenameNetworkElementNodesThenNodesRenamedInSourceModel()
        {
            destinationModel1D.Network.Nodes[0].Name = "Source_node1";
            destinationModel1D.Network.Nodes[1].Name = "Source_node2";

            sourceModel1D.Network.Nodes[0].Name = "Source_node1";
            sourceModel1D.Network.Nodes[1].Name = "Source_node2";


            WaterFlowModel1DModelMergeHelper.RenameNetworkElement(typeof(HydroNode), destinationModel1D, sourceModel1D);
            Assert.That(sourceModel1D.Network.Nodes.Count, Is.EqualTo(2));
            Assert.That(sourceModel1D.Network.Nodes.ElementAt(0).Name, Is.EqualTo("Source0_node1"));
            Assert.That(sourceModel1D.Network.Nodes.ElementAt(1).Name, Is.EqualTo("Source0_node2"));
        }
        #endregion

        #region fit source channels on destination nodes
        [Test]
        public void Given2ModelsWithNotExtactConnectingNodesWhenFitChannelsThenSourceChannelIsFitted()
        {
            var sourceModelNode1 = sourceModel1D.Network.Nodes.FirstOrDefault(node => node.Name == "node1");
            Assert.That(sourceModelNode1, Is.Not.Null);
            sourceModelNode1.Geometry = new Point(101, 0);

            var sourceModelChannel = sourceModel1D.Network.Channels.FirstOrDefault(Channel => Channel.Source == sourceModelNode1);
            Assert.That(sourceModelChannel, Is.Not.Null);
            NetworkEditor.Import.ChannelFromGisImporter.UpdateGeometry(sourceModelChannel, new LineString(new[] { new Coordinate(101, 0), new Coordinate(120, 0) }));
            sourceModelChannel.Length = sourceModelChannel.Geometry.Length; // manually set new length!

            Assert.That(sourceModelChannel.IsLengthCustom, Is.Not.True);
            var sourceChannelLengthBeforeFitting = sourceModelChannel.Length;
            var sourceChannelGeometryLengthBeforeFitting = sourceModelChannel.Geometry.Length;
            Assert.That(sourceChannelLengthBeforeFitting, Is.EqualTo(sourceChannelGeometryLengthBeforeFitting));
            
            var connectedNodes = WaterFlowModel1DModelMergeHelper.ConnectedNodesList(destinationModel1D, sourceModel1D);
            
            foreach (var connectedNode in connectedNodes)
            {
                foreach (var sourceNode in connectedNode.Value)
                {
                    WaterFlowModel1DModelMergeHelper.FitSourceChannelsOnDestinationNode(connectedNode.Key, sourceNode, sourceModel1D);
                }
            }
            Assert.That(sourceModelChannel.IsLengthCustom, Is.True);
            Assert.That(sourceModelChannel.Length, Is.Not.EqualTo(sourceModelChannel.Geometry.Length));
            Assert.That(sourceModelChannel.Length, Is.EqualTo(sourceChannelLengthBeforeFitting));
            Assert.That(sourceModelChannel.Geometry.Length, Is.Not.EqualTo(sourceChannelGeometryLengthBeforeFitting));
            
        }
        [Test]
        public void Given2ModelsWithNotExtactConnectingNodes2WhenFitChannelsThenSourceChannelIsFitted()
        {
            var destinationNode2 = destinationModel1D.Network.Nodes.FirstOrDefault(node => node.Name == "node2");
            Assert.That(destinationNode2, Is.Not.Null);
            destinationNode2.Geometry = new Point(125, 0);

            var destinationModelChannel = destinationModel1D.Network.Channels.FirstOrDefault(Channel => Channel.Target == destinationNode2);
            Assert.That(destinationModelChannel, Is.Not.Null);
            NetworkEditor.Import.ChannelFromGisImporter.UpdateGeometry(destinationModelChannel, new LineString(new[] { new Coordinate(0, 0), new Coordinate(125, 0) }));
            
            var sourceModelNode2 = sourceModel1D.Network.Nodes.FirstOrDefault(node => node.Name == "node2");
            Assert.That(sourceModelNode2, Is.Not.Null);
            var sourceModelChannel = sourceModel1D.Network.Channels.FirstOrDefault(Channel => Channel.Target == sourceModelNode2);
            Assert.That(sourceModelChannel, Is.Not.Null);

            Assert.That(sourceModelChannel.IsLengthCustom, Is.Not.True);
            var sourceChannelLengthBeforeFitting = sourceModelChannel.Length;
            var sourceChannelGeometryLengthBeforeFitting = sourceModelChannel.Geometry.Length;
            Assert.That(sourceChannelLengthBeforeFitting, Is.EqualTo(sourceChannelGeometryLengthBeforeFitting));

            var connectedNodes = WaterFlowModel1DModelMergeHelper.ConnectedNodesList(destinationModel1D, sourceModel1D);

            foreach (var connectedNode in connectedNodes)
            {
                foreach (var sourceNode in connectedNode.Value)
                {
                    WaterFlowModel1DModelMergeHelper.FitSourceChannelsOnDestinationNode(connectedNode.Key, sourceNode, sourceModel1D);
                }
            }
            Assert.That(sourceModelChannel.IsLengthCustom, Is.True);
            Assert.That(sourceModelChannel.Length, Is.Not.EqualTo(sourceModelChannel.Geometry.Length));
            Assert.That(sourceModelChannel.Length, Is.EqualTo(sourceChannelLengthBeforeFitting));
            Assert.That(sourceModelChannel.Geometry.Length, Is.Not.EqualTo(sourceChannelGeometryLengthBeforeFitting));

        }
        #endregion

        #region merge

        [Test]
        public void TestRoughnessCoveragesThatAreOutOfOrderAreMergedCorrectly() // Issue#: SOBEK3-711
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(3);
            var branchA = network.Branches[0];
            var branchB = network.Branches[1];
            var branchC = network.Branches[2];

            var sourceModel = new WaterFlowModel1D() { Network = network };
            var sourceMainRoughnessCoverage = sourceModel.RoughnessSections[0].RoughnessNetworkCoverage;

            // this will add roughness locations that are not in order of branches in the network (as can be the case with real-world models)
            WaterFlowModel1DModelMergeTestHelper.SetupCoverageLocationsInSpecificBranchOrder(sourceMainRoughnessCoverage, 2, new List<int> { 1, 2, 0 });

            var sourceMainRoughnessValues = sourceMainRoughnessCoverage.Components[0].Values;
            sourceMainRoughnessValues[0] = 11.11;
            sourceMainRoughnessValues[1] = 22.22;
            sourceMainRoughnessValues[2] = 33.33;
            sourceMainRoughnessValues[3] = 44.44;
            sourceMainRoughnessValues[4] = 55.55;
            sourceMainRoughnessValues[5] = 66.66;

            var destinationModel = new WaterFlowModel1D();
            WaterFlowModel1DModelMergeHelper.Merge(destinationModel, sourceModel);

            var destinationMainRoughnessCoverage = destinationModel.RoughnessSections[0].RoughnessNetworkCoverage;

            // make sure the arguments are in the correct order
            var destinationRoughnessLocations = (IList<INetworkLocation>)destinationMainRoughnessCoverage.Arguments[0].Values;
            
            Assert.AreEqual(branchB, destinationRoughnessLocations[0].Branch);
            Assert.AreEqual(branchB, destinationRoughnessLocations[1].Branch);
            Assert.AreEqual(branchC, destinationRoughnessLocations[2].Branch);
            Assert.AreEqual(branchC, destinationRoughnessLocations[3].Branch);
            Assert.AreEqual(branchA, destinationRoughnessLocations[4].Branch);
            Assert.AreEqual(branchA, destinationRoughnessLocations[5].Branch);

            // make sure the components are in the correct order
            var destinationMainRoughnessValues = destinationMainRoughnessCoverage.Components[0].Values;
            for (var i = 0; i < sourceMainRoughnessValues.Count; i++)
            {
                Assert.AreEqual(sourceMainRoughnessValues[i], destinationMainRoughnessValues[i]);
            }
        }

        [Test]
        public void Given2ModelsWithDifferentNotConnectingNetworksWhenMergeThenSourceNetworkIsMergedInDestinationModel()
        {
            var destinationModel = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(0, 100);
            var sourceModel = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(200, 300);
            Assert.That(destinationModel.Network.Nodes.Count, Is.EqualTo(2));
            Assert.That(destinationModel.Network.Channels.Count(), Is.EqualTo(1));
            WaterFlowModel1DModelMergeHelper.RenameAllNetworkElements(destinationModel, sourceModel);
            WaterFlowModel1DModelMergeHelper.Merge(destinationModel, sourceModel);
            Assert.That(destinationModel.Network.Nodes.Count, Is.EqualTo(4));
            Assert.That(destinationModel.Network.Channels.Count(), Is.EqualTo(2));
        }

        [Test]
        public void Given2ModelsWithDifferentConnectingNetworksOnSourceNodeWhenMergeThenSourceNetworkIsMergedInDestinationModel()
        {
            Assert.That(destinationModel1D.Network.Nodes.Count, Is.EqualTo(2));
            Assert.That(destinationModel1D.Network.Channels.Count(), Is.EqualTo(1));
            WaterFlowModel1DModelMergeHelper.RenameAllNetworkElements(destinationModel1D, sourceModel1D);
            WaterFlowModel1DModelMergeHelper.Merge(destinationModel1D, sourceModel1D);
            Assert.That(destinationModel1D.Network.Nodes.Count, Is.EqualTo(3));
            Assert.That(destinationModel1D.Network.Channels.Count(), Is.EqualTo(2));
            var destinationNode2 = destinationModel1D.Network.Nodes.FirstOrDefault(n => n.Name == "node2");
            Assert.That(destinationNode2, Is.Not.Null);
            var sourceChannelName = sourceModel1D.Network.Channels.Select(c => c.Name).FirstOrDefault();
            Assert.That(sourceChannelName, Is.Not.Null);
            var newChannelFromSourceNetwork = destinationModel1D.Network.Channels.FirstOrDefault(c => c.Name == sourceChannelName);
            Assert.That(newChannelFromSourceNetwork, Is.Not.Null);
            Assert.That(newChannelFromSourceNetwork.Source, Is.EqualTo(destinationNode2)); // We are connected!
        }

        [Test]
        public void Given2ModelsWithDifferentConnectingNetworksOnTargetNodeWhenMergeThenSourceNetworkIsMergedInDestinationModel()
        {
            var destinationModel = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(100, 200);
            var sourceModel = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(0, 100);
            
            Assert.That(destinationModel.Network.Nodes.Count, Is.EqualTo(2));
            Assert.That(destinationModel.Network.Channels.Count(), Is.EqualTo(1));
            
            WaterFlowModel1DModelMergeHelper.RenameAllNetworkElements(destinationModel, sourceModel); 
            WaterFlowModel1DModelMergeHelper.Merge(destinationModel, sourceModel);

            Assert.That(destinationModel.Network.Nodes.Count, Is.EqualTo(3));
            Assert.That(destinationModel.Network.Channels.Count(), Is.EqualTo(2));
            var destinationNode1 = destinationModel.Network.Nodes.FirstOrDefault(n => n.Name == "node1");
            Assert.That(destinationNode1, Is.Not.Null);
            var sourceChannelName = sourceModel.Network.Channels.Select(c => c.Name).FirstOrDefault();
            Assert.That(sourceChannelName, Is.Not.Null);
            var newChannelFromSourceNetwork = destinationModel.Network.Channels.FirstOrDefault(c => c.Name == sourceChannelName);
            Assert.That(newChannelFromSourceNetwork, Is.Not.Null);
            Assert.That(newChannelFromSourceNetwork.Target, Is.EqualTo(destinationNode1)); // We are connected!
        }

        [Test]
        public void Given2ModelsWithDifferentConnectingNetworksOnTargetNodeInRangeWhenMergeThenSourceNetworkIsMergedInDestinationModel()
        {
            var destinationModel = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(100, 200);
            var sourceModel = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(0, 95);

            Assert.That(destinationModel.Network.Nodes.Count, Is.EqualTo(2));
            Assert.That(destinationModel.Network.Channels.Count(), Is.EqualTo(1));
            var sourceChannelIsLenghtCustom = sourceModel.Network.Channels.Select(c => c.IsLengthCustom).FirstOrDefault();
            Assert.That(sourceChannelIsLenghtCustom, Is.Not.Null);
            Assert.That(sourceChannelIsLenghtCustom, Is.EqualTo(false));
            
            var sourceChannelGeometry = sourceModel.Network.Channels.Select(c => c.Geometry.ToString()).FirstOrDefault();
            Assert.That(sourceChannelGeometry, Is.Not.Null);

            var sourceChannelLenght = sourceModel.Network.Channels.Select(c => c.Length).FirstOrDefault();
            Assert.That(sourceChannelLenght, Is.Not.Null);
            
            var sourceChannelGeometryLenght = sourceModel.Network.Channels.Select(c => c.Geometry.Length).FirstOrDefault();
            Assert.That(sourceChannelGeometryLenght, Is.Not.Null);
            
            Assert.That(destinationModel.Network.CrossSections.Count(), Is.EqualTo(1));
            var sourceCrossSectionChainage = sourceModel.Network.CrossSections.Select(cs => cs.Chainage).FirstOrDefault();
            Assert.That(sourceCrossSectionChainage, Is.Not.Null);

            WaterFlowModel1DModelMergeHelper.RenameAllNetworkElements(destinationModel, sourceModel);
            WaterFlowModel1DModelMergeHelper.Merge(destinationModel, sourceModel);

            Assert.That(destinationModel.Network.Nodes.Count, Is.EqualTo(3));
            Assert.That(destinationModel.Network.Channels.Count(), Is.EqualTo(2));
            Assert.That(destinationModel.Network.CrossSections.Count(), Is.EqualTo(2));
            
            var destinationNode1 = destinationModel.Network.Nodes.FirstOrDefault(n => n.Name == "node1");
            Assert.That(destinationNode1, Is.Not.Null);
            
            var sourceChannelName = sourceModel.Network.Channels.Select(c => c.Name).FirstOrDefault();
            Assert.That(sourceChannelName, Is.Not.Null);
            var newChannelFromSourceNetwork = destinationModel.Network.Channels.FirstOrDefault(c => c.Name == sourceChannelName);
            Assert.That(newChannelFromSourceNetwork, Is.Not.Null);
            Assert.That(newChannelFromSourceNetwork.Target, Is.EqualTo(destinationNode1)); // We are connected!
            Assert.That(newChannelFromSourceNetwork.IsLengthCustom, Is.EqualTo(true));
            Assert.That(newChannelFromSourceNetwork.Length, Is.EqualTo(sourceChannelLenght)); 
            Assert.That(newChannelFromSourceNetwork.Geometry.Length, Is.Not.EqualTo(sourceChannelGeometryLenght)); 
            Assert.That(newChannelFromSourceNetwork.Geometry.ToString(), Is.Not.EqualTo(sourceChannelGeometry));

            var sourceCrossSectionName = sourceModel.Network.CrossSections.Select(c => c.Name).FirstOrDefault();
            Assert.That(sourceCrossSectionName, Is.Not.Null); 
            var newCrossSectionFromSourceNetwork = destinationModel.Network.CrossSections.FirstOrDefault(cs => cs.Name == sourceCrossSectionName);
            Assert.That(newCrossSectionFromSourceNetwork, Is.Not.Null);
            Assert.That(newCrossSectionFromSourceNetwork.Branch, Is.EqualTo(newChannelFromSourceNetwork));
            Assert.That(newCrossSectionFromSourceNetwork.Chainage, Is.EqualTo(sourceCrossSectionChainage));
        }

        [Test]
        public void Given2ModelsWithTemperatureOnlyInSourceIsMergedInDestinationModel()
        {
            var destinationModel = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(0, 100);
            var sourceModel = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(200, 300);

            sourceModel.UseTemperature = true;
            sourceModel.TemperatureModelType = TemperatureModelType.Composite;
            sourceModel.BackgroundTemperature = 36.00;
            sourceModel.DaltonNumber = 0.8;
            sourceModel.StantonNumber = 0.75;
            sourceModel.SurfaceArea = 85.35;

            var startTime = new DateTime(2000, 1, 1);
            sourceModel.StartTime = startTime;
            var meteoDataArguments = new[] { startTime + TimeSpan.FromMinutes(BoundaryFileWriterTestHelper.MeteoDataTimeSeriesArgument1), startTime + TimeSpan.FromMinutes(BoundaryFileWriterTestHelper.MeteoDataTimeSeriesArgument2) };
            sourceModel.MeteoData.Clear();
            sourceModel.MeteoData.Arguments[0].SetValues(meteoDataArguments);

            var valuesAirTemp = new[] { 1.0, 51.0 };
            var valuesHumidity = new[] { 1.0, 10.0 };
            var valuesCloudinesss = new[] { 10.0, 10.1 };
            sourceModel.MeteoData.AirTemperature.SetValues(valuesAirTemp);
            sourceModel.MeteoData.Cloudiness.SetValues(valuesHumidity);
            sourceModel.MeteoData.RelativeHumidity.SetValues(valuesCloudinesss);

            Assert.That(destinationModel.Network.Nodes.Count, Is.EqualTo(2));
            Assert.That(destinationModel.Network.Channels.Count(), Is.EqualTo(1));
            Assert.That(destinationModel.UseTemperature, Is.False);
            Assert.That(destinationModel.MeteoData.AirTemperature.Values, Is.Empty);
            Assert.That(destinationModel.MeteoData.Cloudiness.Values, Is.Empty);
            Assert.That(destinationModel.MeteoData.RelativeHumidity.Values, Is.Empty);

            WaterFlowModel1DModelMergeHelper.RenameAllNetworkElements(destinationModel, sourceModel);
            WaterFlowModel1DModelMergeHelper.Merge(destinationModel, sourceModel);

            Assert.That(destinationModel.Network.Nodes.Count, Is.EqualTo(4));
            Assert.That(destinationModel.Network.Channels.Count(), Is.EqualTo(2));

            /* Temp asserts */
            Assert.That(destinationModel.UseTemperature, Is.True);
            Assert.That(destinationModel.TemperatureModelType, Is.EqualTo(TemperatureModelType.Composite));
            Assert.That(destinationModel.BackgroundTemperature, Is.EqualTo(36.00));
            Assert.That(destinationModel.DaltonNumber, Is.EqualTo(0.8));
            Assert.That(destinationModel.StantonNumber, Is.EqualTo(0.75));
            Assert.That(destinationModel.SurfaceArea, Is.EqualTo(85.35));
            Assert.That(destinationModel.MeteoData.AirTemperature.GetValues(), Is.Empty);
            Assert.That(destinationModel.MeteoData.Cloudiness.GetValues(), Is.Empty);
            Assert.That(destinationModel.MeteoData.RelativeHumidity.GetValues(), Is.Empty);
        }

        [Test]
        public void Given2ModelsWithTemperatureOnlyInitialTemperatureIsMerged()
        {
            // Setup source model
            var sourceModel = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(200, 300);
            sourceModel.UseTemperature = true;
            sourceModel.TemperatureModelType = TemperatureModelType.Composite;
            sourceModel.BackgroundTemperature = 36.00;
            sourceModel.DaltonNumber = 0.8;
            sourceModel.StantonNumber = 0.75;
            sourceModel.SurfaceArea = 85.35;

            // Setup destination model
            var destinationModel = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(0, 100);
            destinationModel.UseTemperature = true;
            destinationModel.TemperatureModelType = TemperatureModelType.Composite;
            destinationModel.BackgroundTemperature = 29.00;
            destinationModel.DaltonNumber = 0.5;
            destinationModel.StantonNumber = 0.5;
            destinationModel.SurfaceArea = 12.50;

            // Setup timing
            var startTime = new DateTime(2000, 1, 1);
            sourceModel.StartTime = startTime;
            destinationModel.StartTime = startTime;

            // Setup InitialTemperature
            var destinationChannel = destinationModel.Network.Channels.First();
            destinationModel.InitialTemperature[new NetworkLocation(destinationChannel, 0.0)] = 0.1;
            destinationModel.InitialTemperature[new NetworkLocation(destinationChannel, 10.0)] = 0.3;

            var sourceChannel = sourceModel.Network.Channels.First();
            sourceModel.InitialTemperature[new NetworkLocation(sourceChannel, 0.0)] = 1.1;
            sourceModel.InitialTemperature[new NetworkLocation(sourceChannel, 10.0)] = 1.3;

            // Setup MeteoData
            var meteoDataArguments = new[] { startTime + TimeSpan.FromMinutes(BoundaryFileWriterTestHelper.MeteoDataTimeSeriesArgument1), startTime + TimeSpan.FromMinutes(BoundaryFileWriterTestHelper.MeteoDataTimeSeriesArgument2) };
            sourceModel.MeteoData.Clear();
            destinationModel.MeteoData.Clear();
            sourceModel.MeteoData.Arguments[0].SetValues(meteoDataArguments);
            destinationModel.MeteoData.Arguments[0].SetValues(meteoDataArguments);

            var valuesAirTempSource = new[] { 1.0, 51.0 };
            var valuesCloudinessSource = new[] { 10.0, 10.1 };
            var valuesHumiditySource = new[] { 1.0, 10.0 };
            sourceModel.MeteoData.AirTemperature.SetValues(valuesAirTempSource);
            sourceModel.MeteoData.Cloudiness.SetValues(valuesCloudinessSource);
            sourceModel.MeteoData.RelativeHumidity.SetValues(valuesHumiditySource);

            var valuesAirTempDest = new[] { 25.0, 59.0 };
            var valuesCloudinessDest = new[] { 25.0, 40.0 };
            var valuesHumidityDest = new[] { 15.0, 30.0 };
            destinationModel.MeteoData.AirTemperature.SetValues(valuesAirTempDest);
            destinationModel.MeteoData.Cloudiness.SetValues(valuesCloudinessDest);
            destinationModel.MeteoData.RelativeHumidity.SetValues(valuesHumidityDest);

            // Pre-merge asserts
            Assert.That(destinationModel.Network.Nodes.Count, Is.EqualTo(2));
            Assert.That(destinationModel.Network.Channels.Count(), Is.EqualTo(1));
            Assert.That(destinationModel.UseTemperature, Is.True);
            Assert.AreEqual(2, destinationModel.InitialTemperature.Arguments[0].Values.Count);
            Assert.AreEqual(2, destinationModel.InitialTemperature.Components[0].Values.Count);
            Assert.That(destinationModel.MeteoData.AirTemperature.GetValues(), Is.EqualTo(valuesAirTempDest));
            Assert.That(destinationModel.MeteoData.Cloudiness.GetValues(), Is.EqualTo(valuesCloudinessDest));
            Assert.That(destinationModel.MeteoData.RelativeHumidity.GetValues(), Is.EqualTo(valuesHumidityDest));

            // Merge
            WaterFlowModel1DModelMergeHelper.RenameAllNetworkElements(destinationModel, sourceModel);
            WaterFlowModel1DModelMergeHelper.Merge(destinationModel, sourceModel);

            // Post-merge asserts
            Assert.That(destinationModel.Network.Nodes.Count, Is.EqualTo(4));
            Assert.That(destinationModel.Network.Channels.Count(), Is.EqualTo(2));

            Assert.That(destinationModel.UseTemperature, Is.True);
            Assert.That(destinationModel.TemperatureModelType, Is.EqualTo(TemperatureModelType.Composite));
            Assert.That(destinationModel.BackgroundTemperature, Is.EqualTo(29.00));
            Assert.That(destinationModel.DaltonNumber, Is.EqualTo(0.5));
            Assert.That(destinationModel.StantonNumber, Is.EqualTo(0.5));
            Assert.That(destinationModel.SurfaceArea, Is.EqualTo(12.50));

            Assert.AreEqual(4, destinationModel.InitialTemperature.Arguments[0].Values.Count);
            Assert.AreEqual(4, destinationModel.InitialTemperature.Components[0].Values.Count);

            var mergedChannel = destinationModel.Network.Channels.First(c => c.Name == sourceChannel.Name);
            var mergedInitialTemperatureLocations = destinationModel.InitialTemperature.Arguments[0].Values;
            var mergedInitialTemperatureValues = destinationModel.InitialTemperature.Components[0].Values;

            var location1 = (NetworkLocation)mergedInitialTemperatureLocations[0];
            Assert.AreEqual(destinationChannel, location1.Branch);
            var value1 = (double)mergedInitialTemperatureValues[0];
            Assert.AreEqual(0.1, value1, 0.01);

            var location2 = (NetworkLocation)mergedInitialTemperatureLocations[1];
            Assert.AreEqual(destinationChannel, location2.Branch);
            var value2 = (double)mergedInitialTemperatureValues[1];
            Assert.AreEqual(0.3, value2, 0.01);

            var location3 = (NetworkLocation)mergedInitialTemperatureLocations[2];
            Assert.AreEqual(mergedChannel, location3.Branch);
            var value3 = (double)mergedInitialTemperatureValues[2];
            Assert.AreEqual(1.1, value3, 0.01);
            
            var location4 = (NetworkLocation)mergedInitialTemperatureLocations[3];
            Assert.AreEqual(mergedChannel, location4.Branch);
            var value4 = (double)mergedInitialTemperatureValues[3];
            Assert.AreEqual(1.3, value4, 0.01);
            
            Assert.That(destinationModel.MeteoData.AirTemperature.GetValues(), Is.EqualTo(valuesAirTempDest));
            Assert.That(destinationModel.MeteoData.Cloudiness.GetValues(), Is.EqualTo(valuesCloudinessDest));
            Assert.That(destinationModel.MeteoData.RelativeHumidity.GetValues(), Is.EqualTo(valuesHumidityDest));
        }

        #endregion
    }
}