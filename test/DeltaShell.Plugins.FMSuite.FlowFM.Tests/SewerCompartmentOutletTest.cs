using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class SewerCompartmentOutletTest: SewerFeatureFactoryTestHelper
    {
        #region Outlets

        [Test]
        public void CreateOutletCompartmentFromGwswElementNodeType()
        {
            var uniqueId = "outlet123";
            var manholeId = "man123";
            var typeDouble = "double";
            var xCoord = 30.0;
            var yCoord = 15.0;
            var nodeLength = 14.0;
            var nodeWidth = 13.0;
            var nodeShape = CompartmentShape.Square;
            var nodeShapeAsString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(nodeShape);
            var floodableArea = 11.0;
            var bottomLevel = 10.0;
            var surfaceLevel = 5.0;
            var nodeType =
                EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerStructureMapping.StructureType.Outlet);
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Node.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.UniqueId, uniqueId, string.Empty),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.ManholeId, manholeId, string.Empty),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.XCoordinate, xCoord.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.YCoordinate, yCoord.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeLength, nodeLength.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeWidth, nodeWidth.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeShape, nodeShapeAsString, string.Empty),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.FloodableArea, floodableArea.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.BottomLevel, bottomLevel.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.SurfaceLevel, surfaceLevel.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeType, nodeType, string.Empty),
                }
            };

            var network = new HydroNetwork();
            var createdElement = SewerFeatureFactory.CreateInstance(nodeGwswElement, network);
            var manhole = createdElement as Manhole;
            Assert.NotNull(manhole);
            Assert.IsTrue(manhole.Compartments.Count == 1);
            var compartment = manhole.Compartments.FirstOrDefault();
            Assert.NotNull(compartment);
            
            CheckCompartmentAndManholePropertyValues(compartment, uniqueId, manholeId, nodeLength, nodeWidth, nodeShape, floodableArea, bottomLevel, surfaceLevel, xCoord, yCoord, 1);
            //Check it can be casted into an outlet.
            var outlet = compartment as OutletCompartment;
            Assert.IsNotNull(outlet);
        }

        [Test]
        public void CreateOutletCompartmentFromGwswElementNodeTypeWithoutNetwork()
        {
            var uniqueId = "outlet123";
            var manholeId = "man123";
            var typeDouble = "double";
            var xCoord = 30.0;
            var yCoord = 15.0;
            var nodeLength = 14.0;
            var nodeWidth = 13.0;
            var nodeShape = CompartmentShape.Square;
            var nodeShapeAsString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(nodeShape);
            var floodableArea = 11.0;
            var bottomLevel = 10.0;
            var surfaceLevel = 5.0;
            var nodeType =
                EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerStructureMapping.StructureType.Outlet);
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Node.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.UniqueId, uniqueId, string.Empty),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.ManholeId, manholeId, string.Empty),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.XCoordinate, xCoord.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.YCoordinate, yCoord.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeLength, nodeLength.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeWidth, nodeWidth.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeShape, nodeShapeAsString, string.Empty),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.FloodableArea, floodableArea.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.BottomLevel, bottomLevel.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.SurfaceLevel, surfaceLevel.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeType, nodeType, string.Empty)
                }
            };

            var createdElement = SewerFeatureFactory.CreateInstance(nodeGwswElement);
            var manhole = createdElement as Manhole;
            Assert.NotNull(manhole);
            Assert.IsTrue(manhole.Compartments.Count == 1);
            var compartment = manhole.Compartments.FirstOrDefault();
            Assert.NotNull(compartment);

            CheckCompartmentAndManholePropertyValues(compartment, uniqueId, manholeId, nodeLength, nodeWidth, nodeShape, floodableArea, bottomLevel, surfaceLevel, xCoord, yCoord, 1);
            //Check it can be casted into an outlet.
            var outlet = compartment as OutletCompartment;
            Assert.IsNotNull(outlet);
        }

        [Test]
        public void CreateOutletCompartmentFromGwswElementStructureType()
        {
            var uniqueId = "outlet123";
            var surfaceWaterLevel = 15.0;
            var structureType =
                EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerStructureMapping.StructureType.Outlet);

            var defaultDouble = 0.0;
            var structureGwswElement = GetStructureGwswElement(uniqueId, structureType, defaultDouble, defaultDouble,
                defaultDouble, defaultDouble, defaultDouble, surfaceWaterLevel);

            var network = new HydroNetwork();
            var createdElement = SewerFeatureFactory.CreateInstance(structureGwswElement, network);
            var manhole = createdElement as Manhole;
            Assert.NotNull(manhole);
            Assert.IsTrue(manhole.Compartments.Count == 1);
            var compartment = manhole.Compartments.FirstOrDefault();
            Assert.NotNull(compartment);

            //Check it can be casted into an outlet.
            var outlet = compartment as OutletCompartment;
            Assert.IsNotNull(outlet);
            Assert.AreEqual(surfaceWaterLevel, outlet.SurfaceWaterLevel);
        }

        [Test]
        public void CreateOutletCompartmentFromGwswElementStructureTypeWithoutNetwork()
        {
            var uniqueId = "outlet123";
            var surfaceWaterLevel = 15.0;
            var structureType =
                EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerStructureMapping.StructureType.Outlet);

            var defaultDouble = 0.0;
            var structureGwswElement = GetStructureGwswElement(uniqueId, structureType, defaultDouble, defaultDouble,
                defaultDouble, defaultDouble, defaultDouble, surfaceWaterLevel);

            var createdElement = SewerFeatureFactory.CreateInstance(structureGwswElement);
            Assert.NotNull(createdElement);

            //Check it can be casted into a compartment.
            var manhole = createdElement as Manhole;
            Assert.NotNull(manhole);
            Assert.IsTrue(manhole.Compartments.Count == 1);
            var compartment = manhole.Compartments.FirstOrDefault();
            Assert.NotNull(compartment);

            //Check it can be casted into an outlet.
            var outlet = compartment as OutletCompartment;
            Assert.IsNotNull(outlet);
            Assert.AreEqual(surfaceWaterLevel, outlet.SurfaceWaterLevel);
        }

        [Test]
        public void CreateOutletFromGwswStructureThenCreateSameOutletFromGwswNodeShouldAddAttributesNotRemove()
        {
            var uniqueId = "outlet123";
            var manholeId = "manhole1";
            var surfaceWaterLevel = 15.0;
            var structureType =
                EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerStructureMapping.StructureType.Outlet);

            var defaultString = string.Empty;
            var defaultDouble = 0.0;
            //Gwsw elements
            var structureGwswElement = GetStructureGwswElement(uniqueId, structureType, defaultDouble, defaultDouble,
                defaultDouble, defaultDouble, defaultDouble, surfaceWaterLevel);
            var nodeGwswElement = GetNodeGwswElement(uniqueId, manholeId, structureType, defaultDouble, defaultDouble, defaultDouble,
                defaultDouble, defaultString, defaultDouble, defaultDouble, defaultDouble);

            //Create structure element and add it to the network.
            var network = new HydroNetwork();
            var createdStructureElement = SewerFeatureFactory.CreateInstance(structureGwswElement, network);
            Assert.NotNull(createdStructureElement);

            //Check it can be casted into an outlet.
            var manhole = createdStructureElement as Manhole;
            Assert.NotNull(manhole);
            Assert.IsTrue(manhole.Compartments.Count == 1);
            var compartment = manhole.Compartments.FirstOrDefault();
            Assert.NotNull(compartment);

            var outletFromStructure = compartment as OutletCompartment;
            Assert.IsNotNull(outletFromStructure);
            Assert.AreEqual(surfaceWaterLevel, outletFromStructure.SurfaceWaterLevel);

            //Create node element and make sure it still has the surface water level value.
            var createdNodeElement = SewerFeatureFactory.CreateInstance(nodeGwswElement, network);
            Assert.NotNull(createdNodeElement);

            manhole = createdStructureElement as Manhole;
            Assert.NotNull(manhole);
            Assert.IsTrue(manhole.Compartments.Any());
            compartment = manhole.Compartments.FirstOrDefault();
            Assert.NotNull(compartment);

            //Check it can be casted into an outlet.
            var outletFromNode = compartment as OutletCompartment;
            Assert.IsNotNull(outletFromNode);
            Assert.AreEqual(surfaceWaterLevel, outletFromNode.SurfaceWaterLevel);
        }

        [Test]
        public void
            CreateOutletCompartmentFromGwswElementStructureWhenCompartmentWithSameNameExistsOnNetworkShouldCreateNewOutletCompartment()
        {
            var uniqueId = "outlet123";
            var surfaceWaterLevel = 15.0;
            var structureType =
                EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerStructureMapping.StructureType.Outlet);

            var defaultDouble = 0.0;
            //Gwsw elements
            var structureGwswElement = GetStructureGwswElement(uniqueId, structureType, defaultDouble, defaultDouble,
                defaultDouble, defaultDouble, defaultDouble, surfaceWaterLevel);

            var network = new HydroNetwork();
            var compartment = new Compartment(uniqueId);
            Assert.IsFalse(compartment.IsOutletCompartment());

            var manhole = new Manhole("manholeName"){ Compartments = new EventedList<Compartment>(){compartment}};
            network.Nodes.Add(manhole);
            Assert.IsTrue(network.Manholes.Any( m => m.ContainsCompartmentWithName(uniqueId)));

            var manholeForOutlet = new SewerCompartmentOutletGenerator().Generate(structureGwswElement, network) as IManhole;
            Assert.IsNotNull(manholeForOutlet);
            Assert.AreEqual(manhole, manholeForOutlet);

            Assert.AreEqual(2, manholeForOutlet.Compartments.Count);
            var outletCompartment = manholeForOutlet.Compartments.OfType<OutletCompartment>().FirstOrDefault();
            Assert.IsNotNull(outletCompartment);
            Assert.IsTrue(outletCompartment.IsOutletCompartment());

            Assert.AreEqual(compartment.Name, outletCompartment.Name);
            Assert.IsTrue(manhole.Compartments.Contains(compartment));
            Assert.IsTrue(manhole.Compartments.Contains(outletCompartment));
        }
        #endregion
    }
}