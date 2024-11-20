using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelMerge;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.ModelMerge
{
    [TestFixture]
    public class ModelMergerTest
    {
        [Test]
        public void Merge_OriginalModelNull_ThrowsArgumentNullException()
        {
            // Call
            TestDelegate action = () => ModelMerger.Merge(null, new WaterFlowFMModel());

            // Assert
            Assert.That(action, Throws.TypeOf<ArgumentNullException>());
        }
        
        [Test]
        public void Merge_newModelNull_ThrowsArgumentNullException()
        {
            // Call
            TestDelegate action = () => ModelMerger.Merge(new WaterFlowFMModel(), null);

            // Assert
            Assert.That(action, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Merge_CorrectlyMergesNetworks()
        {
            // Setup
            var originalModel = new WaterFlowFMModel();
            AddFeaturesToNetwork(originalModel.Network, "original-");
            
            var newModel = new WaterFlowFMModel();
            AddFeaturesToNetwork(newModel.Network, "new-");

            // Call
            ModelMerger.Merge(originalModel, newModel);

            // Assert
            IHydroNetwork mergedNetwork = originalModel.Network;
            Assert.That(mergedNetwork.Branches.Count(), Is.EqualTo(2));
            Assert.That(mergedNetwork.Nodes.Count(), Is.EqualTo(4)); // 2 hydro nodes + 2 manholes
            Assert.That(mergedNetwork.Compartments.Count(), Is.EqualTo(4)); // 2 compartments + 2 outlet compartments
            
            Assert.That(mergedNetwork.Bridges.Count(), Is.EqualTo(2));
            Assert.That(mergedNetwork.CompositeBranchStructures.Count(), Is.EqualTo(2));
            Assert.That(mergedNetwork.Culverts.Count(), Is.EqualTo(2));
            Assert.That(mergedNetwork.Gates.Count(), Is.EqualTo(2));
            Assert.That(mergedNetwork.LateralSources.Count(), Is.EqualTo(2));
            Assert.That(mergedNetwork.ObservationPoints.Count(), Is.EqualTo(2));
            Assert.That(mergedNetwork.Orifices.Count(), Is.EqualTo(2));
            Assert.That(mergedNetwork.Pumps.Count(), Is.EqualTo(2));
            Assert.That(mergedNetwork.Retentions.Count(), Is.EqualTo(2));
            Assert.That(mergedNetwork.Weirs.Count(), Is.EqualTo(4)); // 2 orifices + 2 weirs
            
            Assert.That(mergedNetwork.SharedCrossSectionDefinitions.Count(), Is.EqualTo(2));
            Assert.That(mergedNetwork.CrossSectionSectionTypes.Count(), Is.EqualTo(4)); // Main, Sewer, Main-imported, Sewer-imported
            
            Assert.That(originalModel.ChannelFrictionDefinitions.Count, Is.EqualTo(2));
            Assert.That(originalModel.ChannelInitialConditionDefinitions.Count, Is.EqualTo(2));
        }

        private static void AddFeaturesToNetwork(IHydroNetwork network, string namePrefix)
        {
            var branch = new Channel() {Name = $"{namePrefix}Channel"};
            network.Branches.Add(branch);
            
            branch.BranchFeatures.Add(new Bridge($"{namePrefix}Bridge"));
            branch.BranchFeatures.Add(new CompositeBranchStructure(){Name = $"{namePrefix}CompositeBranchStructure"});
            branch.BranchFeatures.Add(new Culvert(){Name = $"{namePrefix}Culvert"});
            branch.BranchFeatures.Add(new Gate(){Name = $"{namePrefix}Gate"});
            branch.BranchFeatures.Add(new LateralSource(){Name = $"{namePrefix}LateralSource"});
            branch.BranchFeatures.Add(new ObservationPoint(){Name = $"{namePrefix}ObservationPoint"});
            branch.BranchFeatures.Add(new Orifice(){Name = $"{namePrefix}Orifice"});
            branch.BranchFeatures.Add(new Pump(){Name = $"{namePrefix}Pump"});
            branch.BranchFeatures.Add(new Retention(){Name = $"{namePrefix}Retention"});
            branch.BranchFeatures.Add(new Weir(){Name = $"{namePrefix}Weir"});
            branch.Links.Add(new HydroLink(Substitute.For<IHydroObject>(), Substitute.For<IHydroObject>()){Name = $"{namePrefix}HydroLink"});
            
            network.Nodes.Add(new HydroNode(){Name = $"{namePrefix}HydroNode"});

            var manhole = new Manhole() {Name = $"{namePrefix}Manhole"};
            manhole.Compartments.Add(new Compartment($"{namePrefix}Compartment"));
            manhole.Compartments.Add(new OutletCompartment($"{namePrefix}OutletCompartment"));
            network.Nodes.Add(manhole);

            var crossSectionDefinition = new CrossSectionDefinitionYZ($"{namePrefix}CrossSectionDefinition");
            network.SharedCrossSectionDefinitions.Add(crossSectionDefinition);
        }

        [Test]
        public void Merge_CorrectlyMergesChannelFrictionDefinitions()
        {
            // Setup
            var originalModel = new WaterFlowFMModel();

            const string originalBranchName = "originalBranch";
            var branch = new Channel() { Name = originalBranchName};
            originalModel.Network.Branches.Add(branch);

            Assert.That(originalModel.ChannelFrictionDefinitions.Count, Is.EqualTo(1));
            ChannelFrictionDefinition originalDefinition = originalModel.ChannelFrictionDefinitions.First();

            var newModel = new WaterFlowFMModel();

            var newBranchName = "newBranch";
            var newBranch = new Channel() { Name = newBranchName};
            newModel.Network.Branches.Add(newBranch);

            Assert.That(newModel.ChannelFrictionDefinitions.Count, Is.EqualTo(1));
            ChannelFrictionDefinition newDefinition = newModel.ChannelFrictionDefinitions.First();
            
            // Call
            ModelMerger.Merge(originalModel, newModel);

            // Assert
            WaterFlowFMModel mergedModel = originalModel;
            Assert.That(mergedModel.ChannelFrictionDefinitions.Count, Is.EqualTo(2));
            
            Assert.That(mergedModel.ChannelFrictionDefinitions.FirstOrDefault(def => def.Channel.Name.Equals(originalBranchName)), Is.EqualTo(originalDefinition));
            Assert.That(mergedModel.ChannelFrictionDefinitions.FirstOrDefault(def => def.Channel.Name.Equals(newBranchName)), Is.EqualTo(newDefinition));
        }
        
        [Test]
        public void Merge_CorrectlyMergesChannelInitialConditionDefinitions()
        {
            // Setup
            var originalModel = new WaterFlowFMModel();

            const string originalBranchName = "originalBranch";
            var branch = new Channel() { Name = originalBranchName};
            originalModel.Network.Branches.Add(branch);

            Assert.That(originalModel.ChannelInitialConditionDefinitions.Count, Is.EqualTo(1));
            ChannelInitialConditionDefinition originalDefinition = originalModel.ChannelInitialConditionDefinitions.First();

            var newModel = new WaterFlowFMModel();

            var newBranchName = "newBranch";
            var newBranch = new Channel() { Name = newBranchName};
            newModel.Network.Branches.Add(newBranch);

            Assert.That(newModel.ChannelInitialConditionDefinitions.Count, Is.EqualTo(1));
            ChannelInitialConditionDefinition newDefinition = newModel.ChannelInitialConditionDefinitions.First();
            
            // Call
            ModelMerger.Merge(originalModel, newModel);

            // Assert
            WaterFlowFMModel mergedModel = originalModel;
            Assert.That(mergedModel.ChannelInitialConditionDefinitions.Count, Is.EqualTo(2));
            
            Assert.That(mergedModel.ChannelInitialConditionDefinitions.FirstOrDefault(def => def.Channel.Name.Equals(originalBranchName)), Is.EqualTo(originalDefinition));
            Assert.That(mergedModel.ChannelInitialConditionDefinitions.FirstOrDefault(def => def.Channel.Name.Equals(newBranchName)), Is.EqualTo(newDefinition));
        }
        
        [Test]
        public void Merge_CorrectlyMergesBoundaryConditions1D()
        {
            // Setup
            const string originalFromNodeName = "originalFromNode"; 
            const string originalToNodeName = "originalToNode"; 
            const string originalBranchName = "originalBranch";

            var originalFromNode = new HydroNode(originalFromNodeName);
            var originalToNode = new HydroNode(originalToNodeName);
            
            var branch = new Channel(originalBranchName, originalFromNode, originalToNode);
            var originalModel = new WaterFlowFMModel();
            originalModel.Network.Branches.Add(branch);
            
            Model1DBoundaryNodeData originalBoundaryCondition1 = new Model1DBoundaryNodeData() {Feature = originalFromNode};
            Model1DBoundaryNodeData originalBoundaryCondition2 = new Model1DBoundaryNodeData() {Feature = originalToNode};
            originalModel.BoundaryConditions1D.Add(originalBoundaryCondition1);
            originalModel.BoundaryConditions1D.Add(originalBoundaryCondition2);
            Assert.That(originalModel.BoundaryConditions1D.Count, Is.EqualTo(2));
            
            const string newFromNodeName = "newFromNode"; 
            const string newToNodeName = "newToNode"; 
            const string newBranchName = "newBranch";
            
            var newFromNode = new HydroNode(newFromNodeName);
            var newToNode = new HydroNode(newToNodeName);

            var newBranch = new Channel(newBranchName, newFromNode, newToNode);
            var newModel = new WaterFlowFMModel();
            newModel.Network.Branches.Add(newBranch);

            Model1DBoundaryNodeData newBoundaryCondition1 = new Model1DBoundaryNodeData() {Feature = newFromNode};
            Model1DBoundaryNodeData newBoundaryCondition2 = new Model1DBoundaryNodeData() {Feature = newToNode};
            newModel.BoundaryConditions1D.Add(newBoundaryCondition1);
            newModel.BoundaryConditions1D.Add(newBoundaryCondition2);
            Assert.That(newModel.BoundaryConditions1D.Count, Is.EqualTo(2));

            // Call
            ModelMerger.Merge(originalModel, newModel);

            // Assert
            WaterFlowFMModel mergedModel = originalModel;
            Assert.That(mergedModel.BoundaryConditions1D.Count, Is.EqualTo(4)); // 2 + 2
            
            Assert.That(mergedModel.BoundaryConditions1D.FirstOrDefault(bc => bc.Node.Name.Equals(originalFromNodeName)), Is.EqualTo(originalBoundaryCondition1));
            Assert.That(mergedModel.BoundaryConditions1D.FirstOrDefault(bc => bc.Node.Name.Equals(originalToNodeName)), Is.EqualTo(originalBoundaryCondition2));
            Assert.That(mergedModel.BoundaryConditions1D.FirstOrDefault(bc => bc.Node.Name.Equals(newFromNodeName)), Is.EqualTo(newBoundaryCondition1));
            Assert.That(mergedModel.BoundaryConditions1D.FirstOrDefault(bc => bc.Node.Name.Equals(newToNodeName)), Is.EqualTo(newBoundaryCondition2));
        }

        [Test]
        public void Merge_CorrectlyMergesNetworkDiscretizations()
        {
            // Setup
            WaterFlowFMModel originalModel = CreateOriginalFmModelWithNetworkAndDiscretization();
            WaterFlowFMModel newModel = CreateNewFmModelWithNetworkAndDiscretization();

            // Call
            ModelMerger.Merge(originalModel, newModel);

            // Assert
            WaterFlowFMModel mergedModel = originalModel;
            Assert.That(mergedModel.NetworkDiscretization.Locations.Values.Count, Is.EqualTo(12));
            
        }
        
        private static WaterFlowFMModel CreateOriginalFmModelWithNetworkAndDiscretization()
        {
            var originalModel = new WaterFlowFMModel();
            IHydroNetwork originalNetwork = originalModel.Network;

            var fromNode = new HydroNode("fromNode") {Geometry = new Point(100, 1)};
            originalNetwork.Nodes.Add(fromNode);
            var toNode = new HydroNode("toNode") {Geometry = new Point(1000, 1)};
            originalNetwork.Nodes.Add(toNode);
            var branch = new Channel("branch1", fromNode, toNode)
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(100, 1),
                    new Coordinate(1000, 1)
                })
            };
            originalNetwork.Branches.Add(branch);

            var networkDiscretisation = new Discretization
            {
                Name = "mesh1d",
                Network = originalNetwork
            };

            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch, 0) {Name = "point_1"});
            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch, 100) {Name = "point_2"});
            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch, 300) {Name = "point_3"});
            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch, 500) {Name = "point_4"});
            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch, 700) {Name = "point_5"});
            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch, 1000) {Name = "point_6"});

            originalModel.NetworkDiscretization = networkDiscretisation;
            return originalModel;
        }
        
        private static WaterFlowFMModel CreateNewFmModelWithNetworkAndDiscretization()
        {
            var newModel = new WaterFlowFMModel();
            IHydroNetwork newNetwork = newModel.Network;

            var newFromNode = new HydroNode("newFromNode") {Geometry = new Point(1, 100)};
            newNetwork.Nodes.Add(newFromNode);
            var newToNode = new HydroNode("newToNode") {Geometry = new Point(1, 1000)};
            newNetwork.Nodes.Add(newToNode);
            var newBranch = new Channel("newBranch1", newFromNode, newToNode)
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(1, 100),
                    new Coordinate(1, 1000)
                })
            };
            newNetwork.Branches.Add(newBranch);

            var newNetworkDiscretisation = new Discretization
            {
                Name = "mesh1d",
                Network = newNetwork
            };

            newNetworkDiscretisation.Locations.Values.Add(new NetworkLocation(newBranch, 0) {Name = "new_point_1"});
            newNetworkDiscretisation.Locations.Values.Add(new NetworkLocation(newBranch, 200) {Name = "new_point_2"});
            newNetworkDiscretisation.Locations.Values.Add(new NetworkLocation(newBranch, 400) {Name = "new_point_3"});
            newNetworkDiscretisation.Locations.Values.Add(new NetworkLocation(newBranch, 600) {Name = "new_point_4"});
            newNetworkDiscretisation.Locations.Values.Add(new NetworkLocation(newBranch, 800) {Name = "new_point_5"});
            newNetworkDiscretisation.Locations.Values.Add(new NetworkLocation(newBranch, 1000) {Name = "new_point_6"});

            newModel.NetworkDiscretization = newNetworkDiscretisation;
            return newModel;
        }
    }
}