using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
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
            var connection = new SewerOrificeGenerator().Generate(nodeGwswElement, network) as SewerConnection;

            Assert.NotNull(connection);

            var orifice = connection.BranchFeatures.FirstOrDefault(bf => bf.GetType() == typeof(Orifice));
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
            var generator = new SewerOrificeGenerator();
            var createdElement = generator.Generate(structureOrificeGwswElement, network);
            Assert.IsNotNull(createdElement);

            var createdPump = createdElement as Orifice;
            Assert.IsNotNull(createdPump);
            Assert.AreEqual(orificeId, createdPump.Name);
            Assert.AreEqual(bottomLevel, createdPump.BottomLevel);
            Assert.AreEqual(contractionCoef, createdPump.ContractionCoefficent);
            Assert.AreEqual(maxDischarge, createdPump.MaxDischarge);

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
            var generator = new SewerOrificeGenerator();
            var structureElement = generator.Generate(structureGwswElement, network);
            Assert.IsNotNull(structureElement);

            var structureOrifice = structureElement as Orifice;
            Assert.IsNotNull(structureOrifice);
            Assert.IsTrue(network.SewerConnections.Any(p => p.Name.Equals(structureId)));
            Assert.AreEqual(bottomLevel, structureOrifice.BottomLevel, "The given attribute has been overriden with the connection Gwsw element.");

            //Generate the Orifice from the Gwsw SEWER CONNECTION
            var connection = generator.Generate(sewerConnectionGwswElement, network) as SewerConnection;
            Assert.IsNotNull(connection);

            var orifice = connection.GetStructuresFromBranchFeatures<Orifice>().FirstOrDefault();
            Assert.NotNull(orifice);

            Assert.IsTrue(network.SewerConnections.Any(sc => sc.Name.Equals(structureId)));

            var replacedStructure = network.SewerConnections.FirstOrDefault(s => s.Name.Equals(structureElement.Name));
            Assert.NotNull(replacedStructure);
            Assert.AreEqual(structureElement, replacedStructure.GetStructuresFromBranchFeatures<Orifice>().FirstOrDefault(), "the attributes from the element do not match");

            Assert.AreEqual(bottomLevel, orifice.BottomLevel, "The given attribute has been overriden with the connection Gwsw element.");
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
            var orifice = new Orifice();
            var sewerConnection = new SewerConnection("");
            sewerConnection.AddStructureToBranch(orifice);
            network.Branches.Add(sewerConnection);
            Assert.IsTrue(network.Branches.Contains(sewerConnection));
            //it should be found under sewerconnections as well
            Assert.IsTrue(network.SewerConnections.Contains(sewerConnection));

            //Now createInstance for the pump definition.
            var generator = new SewerOrificeGenerator();
            var createdElement = generator.Generate(structureOrificeGwswElement, network) as Orifice;
            Assert.IsNotNull(createdElement);

            var createdOrifice = createdElement;
            Assert.IsNotNull(createdOrifice);
            Assert.AreEqual(orificeId, createdOrifice.Name);
            Assert.AreEqual(bottomLevel, createdOrifice.BottomLevel);
            Assert.AreEqual(contractionCoef, createdOrifice.ContractionCoefficent);
            Assert.AreEqual(maxDischarge, createdOrifice.MaxDischarge);
        }
    }
}