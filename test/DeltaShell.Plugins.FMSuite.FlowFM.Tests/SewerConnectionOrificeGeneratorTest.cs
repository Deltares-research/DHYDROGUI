using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class SewerConnectionOrificeGeneratorTest: SewerFeatureFactoryTestHelper
    {
        [Test]
        public void GenerateOrificeFromGwswConnectionElementReturnsValidObject()
        {
            var startNode = "node001";
            var endNode = "node002";
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionMapping.ConnectionType.Orifice), string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode, string.Empty)
                }
            };

            var network = new HydroNetwork();
            var element = new SewerConnectionOrificeGenerator().Generate(nodeGwswElement, network);

            //A sewer connection is created.
            var orifice = element as SewerConnectionOrifice;
            Assert.IsNotNull(orifice);
        }

        [Test]
        public void GenerateOrificeFromGwswStructureElementReturnsValidObject()
        {
            var orificeId = "orifice123";
            var bottomLevel = 30.0;
            var contractionCoef = 0.5;
            var maxDischarge = 1;
            var structureOrificeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.UniqueId, orificeId, string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StructureType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionMapping.ConnectionType.Orifice), string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.BottomLevel, bottomLevel.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.ContractionCoefficient, contractionCoef.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.MaxDischarge, maxDischarge.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble)
                }
            };

            var network = new HydroNetwork();
            //Now createInstance for the pump definition.
            var generator = new SewerConnectionOrificeGenerator();
            var createdElement = generator.Generate(structureOrificeGwswElement, network);
            Assert.IsNotNull(createdElement);

            var createdPump = createdElement as SewerConnectionOrifice;
            Assert.IsNotNull(createdPump);
            Assert.AreEqual(orificeId, createdPump.Name);
            Assert.AreEqual(bottomLevel, createdPump.Bottom_Level);
            Assert.AreEqual(contractionCoef, createdPump.Contraction_Coefficent);
            Assert.AreEqual(maxDischarge, createdPump.Max_Discharge);

        }

        [Test]
        public void TestCreateOrificeAsStructureThenCreateAsSewerConnectionExtendsValues()
        {
            //Create network
            var network = new HydroNetwork();

            #region GwswElements
            var structureId = "structure123";
            var bottomLevel = 30.0;
            var structureGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.UniqueId, structureId, string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StructureType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerStructureMapping.StructureType.Orifice), string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.BottomLevel, bottomLevel.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                }
            };

            var startNode = "node001";
            var endNode = "node002";
            var sewerConnectionGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.UniqueId, structureId, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionMapping.ConnectionType.Orifice), string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode, string.Empty)
                }
            };


            #endregion

            //Generate the Orifice from the Gwsw STRUCTURE
            var generator = new SewerConnectionOrificeGenerator();
            var structureElement = generator.Generate(structureGwswElement, network);
            Assert.IsNotNull(structureElement);

            var structureOrifice = structureElement as SewerConnectionOrifice;
            Assert.IsNotNull(structureOrifice);
            Assert.IsTrue(network.SewerConnections.Any(p => p.Name.Equals(structureId)));
            Assert.AreEqual(bottomLevel, structureOrifice.Bottom_Level, "The given attribute has been overriden with the connection Gwsw element.");

            //Generate the Orifice from the Gwsw SEWER CONNECTION
            var connectionElement = generator.Generate(sewerConnectionGwswElement, network);
            Assert.IsNotNull(connectionElement);

            var connectionOrifice = connectionElement as SewerConnectionOrifice;
            Assert.IsNotNull(connectionOrifice);

            Assert.IsTrue(network.SewerConnections.Any(p => p.Name.Equals(structureId)));

            var replacedStructure = network.SewerConnections.FirstOrDefault(s => s.Name.Equals(structureElement.Name));
            Assert.AreEqual(structureElement, replacedStructure, "the attributes from the element do not match");

            Assert.AreEqual(bottomLevel, connectionOrifice.Bottom_Level, "The given attribute has been overriden with the connection Gwsw element.");
        }

        [Test]
        public void AfterAddingAConnectionOrificeYouCanExtendItsDefinitionWithTheStructure()
        {
            var orificeId = "orifice123";
            var bottomLevel = 30.0;
            var contractionCoef = 0.5;
            var maxDischarge = 1;
            var structureOrificeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.UniqueId, orificeId, string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StructureType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionMapping.ConnectionType.Orifice), string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.BottomLevel, bottomLevel.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.ContractionCoefficient, contractionCoef.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.MaxDischarge, maxDischarge.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                }
            };

            //Create the pump, we know it works because of the previous tests.
            var network = new HydroNetwork();
            var orifice = new SewerConnectionOrifice();
            network.Branches.Add(orifice);
            Assert.IsTrue(network.Branches.Contains(orifice));
            //it should be found under sewerconnections as well
            Assert.IsTrue(network.SewerConnections.Contains(orifice));

            //Now createInstance for the pump definition.
            var generator = new SewerConnectionOrificeGenerator();
            var createdElement = generator.Generate(structureOrificeGwswElement, network);
            Assert.IsNotNull(createdElement);

            var createdPump = createdElement as SewerConnectionOrifice;
            Assert.IsNotNull(createdPump);
            Assert.AreEqual(orificeId, createdPump.Name);
            Assert.AreEqual(bottomLevel, createdPump.Bottom_Level);
            Assert.AreEqual(contractionCoef, createdPump.Contraction_Coefficent);
            Assert.AreEqual(maxDischarge, createdPump.Max_Discharge);
        }
    }
}