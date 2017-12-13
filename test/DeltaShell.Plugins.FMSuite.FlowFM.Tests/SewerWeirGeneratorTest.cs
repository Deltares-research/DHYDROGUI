using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class SewerWeirGeneratorTest : SewerFeatureFactoryTestHelper
    {
        private const string WeirName = "myWeir";

        [TestCase(true)]
        [TestCase(false)]
        public void GenerateWeirSewerConnectionFromGwswElement(bool createWithFactory)
        {
            var startNode = "node001";
            var endNode = "node002";
            var connectionTypeString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionMapping.ConnectionType.Crest);

            var gwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.UniqueId, WeirName, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, connectionTypeString, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode, string.Empty)
                }
            };

            Weir weirOnConnection;
            var network = new HydroNetwork();
            GenerateValidWeirSewerConnection(gwswElement, network, WeirName, createWithFactory, out weirOnConnection);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GivenGeneratedWeirSewerConnection_WhenAddingItToNetworkBranches_ThenWeirSewerConnectionIsPresentInNetworkWeirs(bool createWithFactory)
        {
            var startNode = "node001";
            var endNode = "node002";
            var connectionTypeString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionMapping.ConnectionType.Crest);

            var gwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.UniqueId, WeirName, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, connectionTypeString, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode, string.Empty)
                }
            };

            Weir weirOnConnection;
            var network = new HydroNetwork();
            var sewerConnection = GenerateValidWeirSewerConnection(gwswElement, network, WeirName, createWithFactory, out weirOnConnection);

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

        [TestCase(true)]
        [TestCase(false)]
        public void WhenCreatingWeirWithAuxiliaryManholes_ThenWeirHasDefaultGeometry(bool createWithFactory)
        {
            var startNode = "node001";
            var endNode = "node002";
            var connectionTypeString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionMapping.ConnectionType.Crest);

            var gwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.UniqueId, WeirName, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, connectionTypeString, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode, string.Empty)
                }
            };

            var network = new HydroNetwork();
            var sewerConnection = createWithFactory
                ? SewerFeatureFactory.CreateInstance(gwswElement, network) as SewerConnection
                : new SewerWeirGenerator().Generate(gwswElement, network) as SewerConnection;
            Assert.IsNotNull(sewerConnection);

            var createdWeir = sewerConnection.GetStructuresFromBranchFeatures<Weir>().FirstOrDefault();
            Assert.IsNotNull(createdWeir);

            Assert.IsNotNull(createdWeir.Geometry, "Default geometry not given to weir.");
            Assert.IsNotNull(createdWeir.Geometry.Coordinates);
            Assert.IsTrue(createdWeir.Geometry.Coordinates.Any());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GivenWeirGwswElement_WhenGeneratingWeir_ThenWeirIsAddedToNetwork(bool createWithFactory)
        {
            var structureTypeString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerStructureMapping.StructureType.Crest);

            var gwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.UniqueId, WeirName, string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StructureType, structureTypeString, string.Empty)
                }
            };

            var network = new HydroNetwork();
            var structureElement = createWithFactory
                ? SewerFeatureFactory.CreateInstance(gwswElement, network)
                : new SewerWeirGenerator().Generate(gwswElement, network);

            var createdWeir = structureElement as Weir;
            Assert.IsNotNull(createdWeir);
            Assert.IsTrue(network.SewerConnections.Any(p => p.Name.Equals(WeirName)));
            Assert.IsTrue(network.Weirs.Any(p => p.Name.Equals(WeirName)));

            var networkWeir = network.Weirs.FirstOrDefault();
            Assert.That(createdWeir, Is.EqualTo(networkWeir));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GivenWeirGwswElement_WhenGeneratingWeir_ThenWeirPropertiesArePresentOnWeir(bool createWithFactory)
        {
            var structureTypeString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerStructureMapping.StructureType.Crest);
            var crestWidth = "3.0";
            var crestLevel = "2.7";
            var dischargeCoefficient = "0.9";

            var gwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.UniqueId, WeirName, string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StructureType, structureTypeString, string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.CrestWidth, crestWidth, string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.CrestLevel, crestLevel, string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.DischargeCoefficient, dischargeCoefficient, string.Empty, TypeDouble)
                }
            };

            var network = new HydroNetwork();
            var createdWeir = createWithFactory
                ? SewerFeatureFactory.CreateInstance(gwswElement, network) as Weir
                : new SewerWeirGenerator().Generate(gwswElement, network) as Weir;
            Assert.IsNotNull(createdWeir);

            var weirFormula = createdWeir.WeirFormula as SimpleWeirFormula;
            Assert.IsNotNull(weirFormula);
            Assert.That(weirFormula.DischargeCoefficient, Is.EqualTo(0.9));
            Assert.That(createdWeir.CrestWidth, Is.EqualTo(3.0));
            Assert.That(createdWeir.CrestLevel, Is.EqualTo(2.7));

            Assert.IsNotEmpty(network.Branches);
            Assert.IsNotEmpty(network.SewerConnections);
            Assert.IsNotEmpty(network.BranchFeatures.OfType<Weir>());
            Assert.IsNotEmpty(network.Weirs);

            var networkWeir = network.Weirs.FirstOrDefault();
            Assert.That(createdWeir, Is.EqualTo(networkWeir));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void WhenGeneratingWeirWithoutNetwork_ThenNullIsReturned(bool createWithFactory)
        {
            var structureTypeString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerStructureMapping.StructureType.Crest);
            var gwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StructureType, structureTypeString, string.Empty)
                }
            };

            var createdWeir = createWithFactory 
                ? SewerFeatureFactory.CreateInstance(gwswElement) 
                : new SewerWeirGenerator().Generate(gwswElement, null);
            Assert.IsNull(createdWeir);
        }

        [Test]
        public void WhenGeneratingWeirWithoutNetwork_ThenLogMessageIsReturned()
        {
            var structureTypeString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerStructureMapping.StructureType.Crest);
            var gwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StructureType, structureTypeString, string.Empty)
                }
            };

            var expectedMessage = Resources.SewerWeirGenerator_CreateWeirFromGwswStructure_Weir_s__cannot_be_created_without_a_network_defined_;
            TestHelper.AssertAtLeastOneLogMessagesContains(() => SewerFeatureFactory.CreateInstance(gwswElement), expectedMessage);
            TestHelper.AssertAtLeastOneLogMessagesContains(() => new SewerWeirGenerator().Generate(gwswElement, null), expectedMessage);
        }

        [Test]
        public void GivenGwswStructureElementWithoutStructureType_WhenGeneratingFeatureWithFactory_ThenNullIsReturned()
        {
            var gwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.UniqueId, WeirName, string.Empty)
                }
            };

            var network = new HydroNetwork();
            var feature = SewerFeatureFactory.CreateInstance(gwswElement, network);
            Assert.IsNull(feature);
            Assert.IsEmpty(network.Branches);
        }

        [Test]
        public void WhenCreatingWeirAndThenCreatingSewerConnection_Then()
        {
            //Create network
            var network = new HydroNetwork();

            #region GwswElements
            var structureTypeString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerStructureMapping.StructureType.Crest);
            var crestWidth = "3.0";
            var crestLevel = "2.7";
            var dischargeCoefficient = "0.9";

            var structureGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.UniqueId, WeirName, string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StructureType, structureTypeString, string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.CrestWidth, crestWidth, string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.CrestLevel, crestLevel, string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.DischargeCoefficient, dischargeCoefficient, string.Empty, TypeDouble)
                }
            };

            var startNode = "node001";
            var endNode = "node002";
            var sewerConnectionGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.UniqueId, WeirName, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionMapping.ConnectionType.Crest), string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode, string.Empty)
                }
            };

            #endregion

            //Instance the Weir AS STRUCTURE
            var structureElement = SewerFeatureFactory.CreateInstance(structureGwswElement, network);
            Assert.IsNotNull(structureElement);

            var weirPh = network.Weirs.FirstOrDefault();
            Assert.NotNull(weirPh);

            //Instance the Weir AS SEWER CONNECTION
            var connectionElement = SewerFeatureFactory.CreateInstance(sewerConnectionGwswElement, network);
            Assert.IsNotNull(connectionElement);
            Assert.IsTrue(network.Weirs.Any(p => p.Name.Equals(WeirName)));
            Assert.IsTrue(network.SewerConnections.Any(p => p.Name.Equals(WeirName)));

            var replacedStructure = network.Weirs.FirstOrDefault(s => s.Name.Equals(WeirName));
            Assert.AreEqual(weirPh, replacedStructure, "the attributes from the element do not match");
        }

        [Test]
        public void AfterAddingAWeirYouCanExtendItsDefinition()
        {
            #region GwswElement

            var structureTypeString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerStructureMapping.StructureType.Crest);
            var crestWidth = "3.0";
            var crestLevel = "2.7";
            var dischargeCoefficient = "0.9";

            var structureGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.UniqueId, WeirName, string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StructureType, structureTypeString, string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.CrestWidth, crestWidth, string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.CrestLevel, crestLevel, string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.DischargeCoefficient, dischargeCoefficient, string.Empty, TypeDouble)
                }
            };

            #endregion

            //Create the weir, we know it works because of the previous tests.
            var network = new HydroNetwork();
            var sewerConnection = new SewerConnection();
            var weir = new Weir(WeirName);
            sewerConnection.BranchFeatures.Add(weir);
            network.Branches.Add(sewerConnection);
            Assert.IsTrue(network.Weirs.Contains(weir));

            // Check weir properties
            var networkWeirBefore = network.Weirs.FirstOrDefault(w => w.Name == WeirName) as Weir;
            Assert.IsNotNull(networkWeirBefore);
            Assert.That(networkWeirBefore.CrestWidth, Is.EqualTo(5.0));
            Assert.That(networkWeirBefore.CrestLevel, Is.EqualTo(1.0));
            Assert.That(networkWeirBefore.FlowDirection, Is.EqualTo(FlowDirection.Both));

            var weirFormulaBefore = networkWeirBefore.WeirFormula as SimpleWeirFormula;
            Assert.IsNotNull(weirFormulaBefore);
            Assert.That(weirFormulaBefore.DischargeCoefficient, Is.EqualTo(1.0));

            //Now createInstance for the weir definition.
            var createdWeir = new SewerWeirGenerator().Generate(structureGwswElement, network) as Weir;
            Assert.IsNotNull(createdWeir);
            Assert.That(network.Weirs.Count(), Is.EqualTo(1));

            var networkWeirAfter = network.Weirs.FirstOrDefault(w => w.Name == WeirName) as Weir;
            Assert.IsNotNull(networkWeirAfter);
            Assert.That(networkWeirAfter.CrestWidth, Is.EqualTo(3.0));
            Assert.That(networkWeirAfter.CrestLevel, Is.EqualTo(2.7));

            var weirFormulaAfter = (SimpleWeirFormula) networkWeirBefore.WeirFormula;
            Assert.IsNotNull(weirFormulaAfter);
            Assert.That(weirFormulaAfter.DischargeCoefficient, Is.EqualTo(0.9));
        }

        [TestCase(SewerConnectionMapping.FlowDirection.Open, FlowDirection.Both)]
        [TestCase(SewerConnectionMapping.FlowDirection.Closed, FlowDirection.None)]
        [TestCase(SewerConnectionMapping.FlowDirection.FromStartToEnd, FlowDirection.Positive)]
        [TestCase(SewerConnectionMapping.FlowDirection.FromEndToStart, FlowDirection.Negative)]
        public void GivenConnectionGwswElementWithCrestType_WhenGeneratingCrest_ThenFlowDirectionOnWeirIsCorrect(SewerConnectionMapping.FlowDirection flowDirection, FlowDirection expectedFlowDirection)
        {
            var startNode = "node001";
            var endNode = "node002";
            var flowDirectionId = EnumDescriptionAttributeTypeConverter.GetEnumDescription(flowDirection);
            var sewerConnectionGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.UniqueId, WeirName, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionMapping.ConnectionType.Crest), string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.FlowDirection, flowDirectionId, string.Empty)
                }
            };

            var network = new HydroNetwork();
            var structureElement = SewerFeatureFactory.CreateInstance(sewerConnectionGwswElement, network) as SewerConnection;
            Assert.IsNotNull(structureElement);

            var createdWeir = structureElement.BranchFeatures.FirstOrDefault(bf => bf.Name == WeirName) as Weir;
            Assert.IsNotNull(createdWeir);
            Assert.That(createdWeir.FlowDirection, Is.EqualTo(expectedFlowDirection));
        }

        #region Test helpers

        private static SewerConnection GenerateValidWeirSewerConnection(GwswElement gwswElement, IHydroNetwork network,
            string expectedWeirName, bool createWithFactory, out Weir weirOnConnection)
        {
            var createdElement = createWithFactory
                ? SewerFeatureFactory.CreateInstance(gwswElement, network)
                : new SewerWeirGenerator().Generate(gwswElement, network);
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

        #endregion
    }
}