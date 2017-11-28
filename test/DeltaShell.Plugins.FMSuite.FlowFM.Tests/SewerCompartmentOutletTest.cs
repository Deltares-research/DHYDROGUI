using System.Collections.Generic;
using System.Globalization;
using DelftTools.Hydro;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Geometries;
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
                EnumDescriptionAttributeTypeConverter.GetEnumDescription(StructureMapping.StructureType.Outlet);
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Node.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.UniqueId, uniqueId),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.ManholeId, manholeId),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.XCoordinate, xCoord.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.YCoordinate, yCoord.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeLength, nodeLength.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeWidth, nodeWidth.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeShape, nodeShapeAsString),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.FloodableArea, floodableArea.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.BottomLevel, bottomLevel.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.SurfaceLevel, surfaceLevel.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeType, nodeType),
                }
            };

            var network = new HydroNetwork();
            var createdElement = SewerFeatureFactory.CreateInstance(nodeGwswElement, network);
            Assert.NotNull(createdElement);

            //Check it can be casted into a compartment.
            Assert.NotNull(createdElement as Compartment);
            CheckCompartmentPropertyValues(createdElement as Compartment, uniqueId, manholeId, nodeLength, nodeWidth, nodeShape, floodableArea, bottomLevel, surfaceLevel, new Coordinate(xCoord, yCoord), 1);
            //Check it can be casted into an outlet.
            var outlet = createdElement as OutletCompartment;
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
                EnumDescriptionAttributeTypeConverter.GetEnumDescription(StructureMapping.StructureType.Outlet);
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Node.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.UniqueId, uniqueId),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.ManholeId, manholeId),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.XCoordinate, xCoord.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.YCoordinate, yCoord.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeLength, nodeLength.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeWidth, nodeWidth.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeShape, nodeShapeAsString),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.FloodableArea, floodableArea.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.BottomLevel, bottomLevel.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.SurfaceLevel, surfaceLevel.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeType, nodeType),
                }
            };

            var createdElement = SewerFeatureFactory.CreateInstance(nodeGwswElement);
            Assert.NotNull(createdElement);

            //Check it can be casted into a compartment.
            Assert.NotNull(createdElement as Compartment);
            CheckCompartmentPropertyValues(createdElement as Compartment, uniqueId, manholeId, nodeLength, nodeWidth, nodeShape, floodableArea, bottomLevel, surfaceLevel, new Coordinate(xCoord, yCoord), 1);
            //Check it can be casted into an outlet.
            var outlet = createdElement as OutletCompartment;
            Assert.IsNotNull(outlet);
        }

        [Test]
        public void CreateOutletCompartmentFromGwswElementStructureType()
        {
            var uniqueId = "outlet123";
            var surfaceWaterLevel = 15.0;
            var structureType =
                EnumDescriptionAttributeTypeConverter.GetEnumDescription(StructureMapping.StructureType.Outlet);

            var defaultDouble = 0.0;
            var structureGwswElement = GetStructureGwswElement(uniqueId, structureType, defaultDouble, defaultDouble,
                defaultDouble, defaultDouble, defaultDouble, surfaceWaterLevel);

            var network = new HydroNetwork();
            var createdElement = SewerFeatureFactory.CreateInstance(structureGwswElement, network);
            Assert.NotNull(createdElement);

            //Check it can be casted into a compartment.
            Assert.NotNull(createdElement as Compartment);

            //Check it can be casted into an outlet.
            var outlet = createdElement as OutletCompartment;
            Assert.IsNotNull(outlet);
            Assert.AreEqual(surfaceWaterLevel, outlet.SurfaceWaterLevel);
        }

        [Test]
        public void CreateOutletCompartmentFromGwswElementStructureTypeWithoutNetwork()
        {
            var uniqueId = "outlet123";
            var surfaceWaterLevel = 15.0;
            var structureType =
                EnumDescriptionAttributeTypeConverter.GetEnumDescription(StructureMapping.StructureType.Outlet);

            var defaultDouble = 0.0;
            var structureGwswElement = GetStructureGwswElement(uniqueId, structureType, defaultDouble, defaultDouble,
                defaultDouble, defaultDouble, defaultDouble, surfaceWaterLevel);

            var createdElement = SewerFeatureFactory.CreateInstance(structureGwswElement);
            Assert.NotNull(createdElement);

            //Check it can be casted into a compartment.
            Assert.NotNull(createdElement as Compartment);

            //Check it can be casted into an outlet.
            var outlet = createdElement as OutletCompartment;
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
                EnumDescriptionAttributeTypeConverter.GetEnumDescription(StructureMapping.StructureType.Outlet);

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
            var outletFromStructure = createdStructureElement as OutletCompartment;
            Assert.IsNotNull(outletFromStructure);
            Assert.AreEqual(surfaceWaterLevel, outletFromStructure.SurfaceWaterLevel);

            //Create node element and make sure it still has the surface water level value.
            var createdNodeElement = SewerFeatureFactory.CreateInstance(nodeGwswElement, network);
            Assert.NotNull(createdNodeElement);

            //Check it can be casted into an outlet.
            var outletFromNode = createdNodeElement as OutletCompartment;
            Assert.IsNotNull(outletFromNode);
            Assert.AreEqual(surfaceWaterLevel, outletFromNode.SurfaceWaterLevel);
        }
        #endregion
    }
}