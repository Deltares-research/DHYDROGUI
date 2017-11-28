using System;
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
    public class SewerFeatureFactoryTest: SewerFeatureFactoryTestHelper
    {
        [Test]
        public void SewerFeatureGetsAllHydrObjects()
        {
            var network = new HydroNetwork();
            try
            {
                var allHydroObjectsInNetwork = network.AllHydroObjects.ToList();
                network.Nodes.Add(new Manhole("ManholeTest"));
                allHydroObjectsInNetwork = network.AllHydroObjects.ToList();
            }
            catch (Exception e)
            {
                Assert.Fail("Could not cast HydroOjbects: {0}", e.Message);
            }
        }

        [Test]
        public void SewerFeatureTypeCanBeRetrievedWithAStringValue()
        {
            SewerFeatureType testValue;
            Assert.IsFalse(Enum.TryParse("failValue", out testValue));
            Assert.IsTrue(Enum.TryParse(SewerFeatureType.Connection.ToString(), out testValue));
        }

        [Test]
        [TestCase(SewerFeatureType.Structure)]
        [TestCase(SewerFeatureType.Surface)]
        [TestCase(SewerFeatureType.Runoff)]
        [TestCase(SewerFeatureType.Discharge)]
        [TestCase(SewerFeatureType.Distribution)]
        [TestCase(SewerFeatureType.Meta)]
        public void NotKnownSewerFeaturesDoNotInstantiate(SewerFeatureType type)
        {
            /* When the above features are added to the object model they can remove from this test. */
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = type.ToString()
            };

            try
            {
                var element = SewerFeatureFactory.CreateInstance(nodeGwswElement);
                Assert.IsNull(element);
            }
            catch (Exception e)
            {
                Assert.Fail("There was a problem while instantiating. {0}", e.Message);
            }
        }

        [Test]
        public void SewerFeatureFactoryReturnsNullStructuresWhenNotGivingNameForStructure()
        {
            var structureGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StructureType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerStructureMapping.StructureType.Pump)),
                }
            };

            var network = new HydroNetwork();
            var createdElement = SewerFeatureFactory.CreateInstance(structureGwswElement, network);
            Assert.IsNull(createdElement);
        }

        [Test]
        [TestCase(SewerStructureMapping.StructureType.Crest, false)]
        [TestCase(SewerStructureMapping.StructureType.Orifice, true)]
        [TestCase(SewerStructureMapping.StructureType.Outlet, true)]
        [TestCase(SewerStructureMapping.StructureType.Pump, true)]
        public void SewerFeatureFactoryReturnsStructuresWhenGivingNameForStructure(SewerStructureMapping.StructureType structureType, bool mapped)
        {
            var structureId = "structure123";
            var structurePumpGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.UniqueId, structureId),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StructureType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(structureType)),
                }
            };

            var network = new HydroNetwork();
            var createdElement = SewerFeatureFactory.CreateInstance(structurePumpGwswElement, network);
            Assert.AreEqual(mapped, createdElement != null);
        }

        [Test]
        [TestCase(SewerStructureMapping.StructureType.Crest, false, false, false, false)]
        [TestCase(SewerStructureMapping.StructureType.Orifice, true, false, true, false)]
        [TestCase(SewerStructureMapping.StructureType.Outlet, true, true, false, false)]
        [TestCase(SewerStructureMapping.StructureType.Pump, true, false, true, false)]
        public void SewerFeatureFactoryCreatesStructureAndSewerConnectionIfNeitherExists(SewerStructureMapping.StructureType structureType, bool mapped, bool isNode, bool isBranch, bool isStructure)
        {
            var structureId = "structure123";
            var structureGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.UniqueId, structureId),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StructureType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(structureType)),
                }
            };

            var network = new HydroNetwork();
            var createdElement = SewerFeatureFactory.CreateInstance(structureGwswElement, network);
            Assert.AreEqual(mapped, createdElement != null);
            if (!mapped) return;

            if (isNode)
            {
                Assert.IsTrue(network.Nodes.Any());
                Assert.IsTrue(network.Manholes.Any(m => m.Name.Equals(structureId) || m.ContainsCompartment(structureId)));
            }

            if (isBranch)
            {
                Assert.IsTrue(network.SewerConnections.Any());
                Assert.IsTrue(network.SewerConnections.Any(s => s.Name.Equals(structureId)));
            }

            if (isStructure)
            {
                Assert.IsTrue(network.Structures.Any());
                Assert.IsTrue(network.Structures.Any(s => s.Name.Equals(structureId)));

                Assert.IsTrue(network.CompositeBranchStructures.Any());
                Assert.IsTrue(network.CompositeBranchStructures.Any(cb => cb.Structures.Any(s => s.Name.Equals(structureId))));
            }
        }
    }
}