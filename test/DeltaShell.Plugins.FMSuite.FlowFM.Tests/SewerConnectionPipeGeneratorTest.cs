using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class SewerConnectionPipeGeneratorTest: SewerFeatureFactoryTestHelper
    {
        #region Pipes

        [Test]
        public void GeneratePipeFromGwswConnectionElementReturnsValidObject()
        {
            var startNode = "node001";
            var endNode = "node002";
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionMapping.ConnectionType.InfiltrationPipe), string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode, string.Empty)
                }
            };

            var network = new HydroNetwork();
            var element = new SewerConnectionPipeGenerator().Generate(nodeGwswElement, network);

            //A sewer connection is created.
            var pipe = element as IPipe;
            Assert.IsNotNull(pipe);
        }

        [Test]
        [TestCase("GSL", true)]
        [TestCase("ITR", true)]
        [TestCase("OPL", true)]
        [TestCase("OVS", false)]
        [TestCase("DRL", false)]
        [TestCase("PMP", false)]
        public void CreatePipeWhenGivingPipeIndicatorAttributeFromFactory(string typeOfConnection, bool isPipe)
        {
            var pipeId = "123";
            var startNode = "node001";
            var endNode = "node002";
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeIndicator, isPipe ? pipeId : string.Empty, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, typeOfConnection, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode, string.Empty)
                }
            };
            var network = new HydroNetwork();

            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement, network);
            Assert.AreEqual(isPipe, element is Pipe);
            if (isPipe)
            {
                var pipe = element as Pipe;
                Assert.NotNull(pipe);
                Assert.AreEqual(pipeId, pipe.PipeId);
            }
        }

        [Test]
        [TestCase("GSL")]
        [TestCase("ITR")]
        [TestCase("OPL")]
        public void CreatePipeWhenGivingPipeIndicatorAttributeFromGenerator(string typeOfConnection)
        {
            var pipeId = "123";
            var startNode = "node001";
            var endNode = "node002";
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeIndicator, pipeId, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, typeOfConnection, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode, string.Empty)
                }
            };
            var network = new HydroNetwork();

            var element = new SewerConnectionPipeGenerator().Generate(nodeGwswElement, network);
            Assert.IsTrue(element is Pipe);
            
            var pipe = element as Pipe;
            Assert.NotNull(pipe);
            Assert.AreEqual(pipeId, pipe.PipeId);
        }

        [Test]
        public void CreatePipeFromFactoryWithKnownAttributes()
        {
            var startLevel = 30;
            var endLevel = 100;
            var length = 200;
            var crossSectionDef = "crossSectionDef001";
            var startNode = "node001";
            var endNode = "node002";
            var pipeType = SewerConnectionMapping.ConnectionType.ClosedConnection;
            var pipeTypeString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(pipeType);

            var defaultString = string.Empty;
            var defaultDouble = 0.0;

            var nodeGwswElement = GetSewerConnectionGwswElement(string.Empty, startNode, endNode, pipeTypeString, startLevel, endLevel, defaultString, length, crossSectionDef, defaultString, defaultString, defaultDouble, defaultDouble, defaultDouble, defaultDouble);

            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement);
            Assert.That(element.GetType(), Is.EqualTo(typeof(Pipe)));

            var createdPipe = element as Pipe;
            Assert.IsNotNull(createdPipe);

            Assert.IsNull(createdPipe.Source);
            Assert.IsNull(createdPipe.Target);
            Assert.IsNull(createdPipe.SewerProfileDefinition);

            //Defined
            Assert.IsNotNull(createdPipe.LevelSource);
            Assert.AreEqual(startLevel, createdPipe.LevelSource);

            Assert.IsNotNull(createdPipe.LevelTarget);
            Assert.AreEqual(endLevel, createdPipe.LevelTarget);

            Assert.IsNotNull(createdPipe.Length);
            Assert.AreEqual(length, createdPipe.Length);
        }

        [Test]
        public void CreatePipeFromFactoryCreatesDefaultCrossSectionDefinitionIfNotPresentInNetwork()
        {
            var sewerDefinitionName = "crossSectionDef001";
            var startNode = "node001";
            var endNode = "node002";
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.CrossSectionDef, sewerDefinitionName, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionMapping.ConnectionType.ClosedConnection), string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode, string.Empty)
                }
            };

            var network = new HydroNetwork();
            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement, network);
            Assert.That(element.GetType(), Is.EqualTo(typeof(Pipe)));

            var createdPipe = element as Pipe;
            Assert.IsNotNull(createdPipe);

            //Defined
            Assert.IsNotNull(createdPipe.SewerProfileDefinition);
            Assert.AreEqual(sewerDefinitionName, createdPipe.SewerProfileDefinition.Name);
            Assert.IsTrue(network.SharedCrossSectionDefinitions.Any(cs => cs.Name.Equals(sewerDefinitionName)));
        }

        [Test]
        public void CreatePipeFromFactoryAssignsExistingCrossSectionDefinitionIfPresentInNetwork()
        {
            var network = new HydroNetwork();
            Assert.IsFalse(network.SharedCrossSectionDefinitions.Any());

            var sewerDefinitionName = "crossSectionDef001";
            var auxCrossSection = CrossSectionDefinitionStandard.CreateDefault();
            auxCrossSection.Name = sewerDefinitionName;

            network.SharedCrossSectionDefinitions.Add(auxCrossSection);
            Assert.IsTrue(network.SharedCrossSectionDefinitions.Any(sp => sp.Name.Equals(sewerDefinitionName)));

            var startNode = "node001";
            var endNode = "node002";
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.CrossSectionDef, sewerDefinitionName, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionMapping.ConnectionType.ClosedConnection), string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode, string.Empty)
                }
            };

            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement, network);
            Assert.That(element.GetType(), Is.EqualTo(typeof(Pipe)));

            var createdPipe = element as Pipe;
            Assert.IsNotNull(createdPipe);

            //Defined
            Assert.IsNotNull(createdPipe.SewerProfileDefinition);
            Assert.AreEqual(sewerDefinitionName, createdPipe.SewerProfileDefinition.Name);
            Assert.IsTrue(network.SharedCrossSectionDefinitions.Any(cs => cs.Name.Equals(sewerDefinitionName)));
            Assert.AreEqual(auxCrossSection, createdPipe.SewerProfileDefinition);
        }

        #endregion
    }
}