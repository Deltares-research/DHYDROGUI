using System;
using System.Collections.Generic;
using System.Globalization;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    public class SewerFeatureFactoryTestHelper
    {
        public static void CheckManholeNodePropertyValues(Manhole manhole, string manholeId, double xCoordinate, double yCoordinate, int numberOfCompartments)
        {
            Assert.That(manhole.Name, Is.EqualTo(manholeId));
            Assert.That(manhole.XCoordinate, Is.EqualTo(xCoordinate));
            Assert.That(manhole.YCoordinate, Is.EqualTo(yCoordinate));
            Assert.That(manhole.Geometry, Is.EqualTo(new Point(xCoordinate, yCoordinate)));
            Assert.NotNull(manhole.Compartments);
            Assert.That(manhole.Compartments.Count, Is.EqualTo(numberOfCompartments));
        }

        public void CheckCompartmentPropertyValues(Compartment compartment, string uniqueId, string manholeId, double manholeLength, double manholeWidth, CompartmentShape shape, double floodableArea, double bottomLevel, double surfaceLevel, Coordinate coords, int numberOfParentManholeCompartments)
        {
            Assert.NotNull(compartment.ParentManhole);
            CheckManholeNodePropertyValues(compartment.ParentManhole, manholeId, coords?.X ?? 0.0, coords?.Y ?? 0.0, numberOfParentManholeCompartments);

            Assert.That(compartment.Name, Is.EqualTo(uniqueId));
            Assert.That(compartment.ManholeLength, Is.EqualTo(manholeLength));
            Assert.That(compartment.ManholeWidth, Is.EqualTo(manholeWidth));
            Assert.That(compartment.Shape, Is.EqualTo(shape));
            Assert.That(compartment.FloodableArea, Is.EqualTo(floodableArea));
            Assert.That(compartment.BottomLevel, Is.EqualTo(bottomLevel));
            Assert.That(compartment.SurfaceLevel, Is.EqualTo(surfaceLevel));
            if (compartment.Geometry != null)
            {
                Assert.That(compartment.Geometry.Coordinates.Length, Is.EqualTo(1));
                Assert.That(compartment.Geometry.Coordinate, Is.EqualTo(coords));
            }
        }

        public static void TryCreateFeatureAndCheckForLogMessageAndFeatureIsNull(GwswElement badGwswElement, string expectedPartOfMessage)
        {
            INetworkFeature feature = null;
            TestHelper.AssertAtLeastOneLogMessagesContains(() => feature = SewerFeatureFactory.CreateInstance(badGwswElement), expectedPartOfMessage);
            Assert.IsNull(feature);
        }

        public static GwswAttribute GetDefaultGwswAttribute(string attributeName, string attributeValue, string attributeType = null)
        {
            if (attributeValue == null)
                attributeValue = string.Empty;

            return new GwswAttribute
            {
                GwswAttributeType = new GwswAttributeType("testFile", 5, "columnName", attributeType ?? "string", attributeName,
                    "unkownDefinition", "mandatoryMaybe", "noRemarks"),
                ValueAsString = attributeValue
            };
        }

        public static GwswElement GetNodeGwswElement(string uniqueId, string manholeId, string nodeType, double xCoordinate, double yCoordinate, double nodeLength, double nodeWidth, string nodeShape, double floodableArea, double bottomLevel, double surfaceLevel)
        {
            var typeDouble = "double";
            GwswElement nodeGwswElement = new GwswElement();
            try
            {
                nodeGwswElement = new GwswElement
                {
                    ElementTypeName = SewerFeatureType.Node.ToString(),
                    GwswAttributeList =
                    {
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.UniqueId, uniqueId),
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.ManholeId, manholeId),
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeType, nodeType),
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.XCoordinate, xCoordinate.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.YCoordinate, yCoordinate.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeLength, nodeLength.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeWidth, nodeWidth.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeShape, nodeShape),
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.FloodableArea, floodableArea.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.BottomLevel, bottomLevel.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.SurfaceLevel, surfaceLevel.ToString(CultureInfo.InvariantCulture), typeDouble),
                    }
                };
            }
            catch (Exception e)
            {
                Assert.Fail("Gwsw element was not created, thus the test can't go ahead. {0}", e.Message);
            }

            Assert.IsNotNull(nodeGwswElement);

            return nodeGwswElement;
        }

        public static GwswElement GetSewerConnectionGwswElement(string uniqueId, string startNode, string endNode, string sewerConnectionTypeString , double startLevel, double endLevel, string flowDirectionString, double length,
            string crossSectionDef, string pipeIndicator, string sewerConnectionWaterType, double inletLossStart, double inletLossEnd, double outletLossStart, double outletLossEnd)
        {
            var typeDouble = "double";

            GwswElement nodeGwswElement = new GwswElement();
            try
            {
                nodeGwswElement = new GwswElement
                {
                    ElementTypeName = SewerFeatureType.Connection.ToString(),
                    GwswAttributeList = new List<GwswAttribute>
                    {
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.UniqueId, uniqueId),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd,endNode),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType,sewerConnectionTypeString),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.LevelStart, startLevel.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.LevelEnd, endLevel.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.Length, length.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.FlowDirection, flowDirectionString),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.CrossSectionDef, crossSectionDef),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeIndicator, pipeIndicator),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.WaterType, sewerConnectionWaterType),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.InletLossStart, inletLossStart.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.InletLossEnd, inletLossEnd.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.OutletLossStart, outletLossStart.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.OutletLossEnd, outletLossEnd.ToString(CultureInfo.InvariantCulture), typeDouble),
                    }
                };
            }
            catch (Exception e)
            {
                Assert.Fail("Gwsw element was not created, thus the test can't go ahead. {0}", e.Message);
            }

            Assert.IsNotNull(nodeGwswElement);
            
            return nodeGwswElement;
        }

        public static GwswElement GetStructureGwswElement(string uniqueId, string structureType, double pumpCapacity, double startLevelDownstreams, double stopLevelDownstreams, double startLevelUpstreams, double stopLevelUpstreams, double surfaceWaterLevel)
        {
            var typeDouble = "double";

            GwswElement nodeGwswElement = new GwswElement();
            try
            {
                nodeGwswElement = new GwswElement
                {
                    ElementTypeName = SewerFeatureType.Structure.ToString(),
                    GwswAttributeList = new List<GwswAttribute>
                    {
                        GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.UniqueId, uniqueId),
                        GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StructureType, structureType),
                        GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.PumpCapacity, pumpCapacity.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StartLevelDownstreams, startLevelDownstreams.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StopLevelDownstreams, stopLevelDownstreams.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StartLevelUpstreams, startLevelUpstreams.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StopLevelUpstreams, stopLevelUpstreams.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.SurfaceWaterLevel, surfaceWaterLevel.ToString(CultureInfo.InvariantCulture), typeDouble),
                    }
                };
            }
            catch (Exception e)
            {
                Assert.Fail("Gwsw element was not created, thus the test can't go ahead. {0}", e.Message);
            }

            Assert.IsNotNull(nodeGwswElement);

            return nodeGwswElement;
        }

        public static GwswElement GetSewerProfileGwswElement(string profileId, string profileShape, string profileWidth, string profileHeight, string slope1, string slope2)
        {
            var typeDouble = "double";

            GwswElement nodeGwswElement = new GwswElement();
            try
            {
                nodeGwswElement = new GwswElement
                {
                    ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                    GwswAttributeList = new List<GwswAttribute>
                    {
                        GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, profileId),
                        GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileShape, profileShape),
                        GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, profileWidth, typeDouble),
                        GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, profileHeight, typeDouble),
                        GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.Slope1, slope1, typeDouble),
                        GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.Slope2, slope2, typeDouble)
                    }
                };
            }
            catch (Exception e)
            {
                Assert.Fail("Gwsw element was not created, thus the test can't go ahead. {0}", e.Message);
            }

            Assert.IsNotNull(nodeGwswElement);

            return nodeGwswElement;
        }
    }
}