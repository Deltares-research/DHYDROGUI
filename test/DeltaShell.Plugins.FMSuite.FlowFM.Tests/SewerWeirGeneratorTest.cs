using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class SewerWeirGeneratorTest : SewerFeatureFactoryTestHelper
    {
        [Test]
        public void GenerateWeirSewerConnectionFromGwswElement()
        {
            var weirName = "myWeir";
            var startNode = "node001";
            var endNode = "node002";
            var connectionTypeString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionMapping.ConnectionType.Weir);

            var gwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.UniqueId, weirName, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, connectionTypeString, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode, string.Empty)
                }
            };

            Weir weirOnConnection;
            var network = new HydroNetwork();
            GenerateValidWeirSewerConnection(gwswElement, network, weirName, out weirOnConnection);
        }

        [Test]
        public void GivenGeneratedWeirSewerConnection_WhenAddingItToNetworkBranches_ThenWeirSewerConnectionIsPresentInNetworkWeirs()
        {
            var weirName = "myWeir";
            var startNode = "node001";
            var endNode = "node002";
            var connectionTypeString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionMapping.ConnectionType.Weir);

            var gwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.UniqueId, weirName, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, connectionTypeString, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode, string.Empty)
                }
            };

            Weir weirOnConnection;
            var network = new HydroNetwork();
            var sewerConnection = GenerateValidWeirSewerConnection(gwswElement, network, weirName, out weirOnConnection);

            network.Branches.Add(sewerConnection);

            Assert.IsTrue(network.Branches.Contains(sewerConnection));
            Assert.IsTrue(network.SewerConnections.Contains(sewerConnection));

            // Check the weirs in the network
            Assert.IsTrue(network.Weirs.Any());
            Assert.That(network.Weirs.Count(), Is.EqualTo(1));

            var networkWeir = network.Weirs.FirstOrDefault();
            Assert.NotNull(networkWeir);
            Assert.That(networkWeir, Is.EqualTo(weirOnConnection));
        }

        private static SewerConnection GenerateValidWeirSewerConnection(GwswElement gwswElement, IHydroNetwork network,
            string expectedWeirName, out Weir weirOnConnection)
        {
            //var createdElement = SewerFeatureFactory.CreateNetWorkFeature(gwswElement, network);
            var createdElement = SewerFeatureFactory.CreateInstance(gwswElement, network);
            Assert.NotNull(createdElement);

            // A sewer connection is created
            var sewerConnection = createdElement as SewerConnection;
            Assert.NotNull(sewerConnection);
            Assert.That(sewerConnection.Name, Is.EqualTo(expectedWeirName));

            // A Weir has been added to the sewer connection
            weirOnConnection = sewerConnection.GetStructuresFromBranchFeatures<Weir>().FirstOrDefault();
            Assert.NotNull(weirOnConnection);
            Assert.That(weirOnConnection.GetType(), Is.EqualTo(typeof(Weir)));
            Assert.That(weirOnConnection.Name, Is.EqualTo(expectedWeirName));

            // Weirs should contain the above definition if the branch is added to the network.
            Assert.IsFalse(network.Weirs.Any());
            Assert.IsFalse(network.Branches.Contains(sewerConnection));
            Assert.IsFalse(network.SewerConnections.Contains(sewerConnection));
            return sewerConnection;
        }
    }
}