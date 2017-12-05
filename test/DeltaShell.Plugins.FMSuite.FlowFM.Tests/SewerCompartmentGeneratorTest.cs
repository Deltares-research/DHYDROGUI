using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class SewerCompartmentGeneratorTest: SewerFeatureFactoryTestHelper
    {
        #region Manhole

        [Test]
        public void GivenSimpleManholeData_WhenCreatingWithFactory_ThenManholeIsCorrectlyReturned()
        {
            #region expectedVariables
            var uniqueId = "put1";
            var manholeId = "01001";
            var manholeLength = 7071;
            var manholeWidth = 7071;
            var compartmentShape = CompartmentShape.Square;
            var compartmentShapeAsString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(compartmentShape);
            var nodeType = string.Empty;
            var floodableArea = 45.67;
            var bottomLevel = 0.01;
            var surfaceLevel = 2.75;
            var xCoordinate = 400.0;
            var yCoordinate = 50.0;
            var numberOfParentManholeCompartments = 1;
            #endregion

            var nodeGwswElement = GetNodeGwswElement(uniqueId, manholeId, nodeType, xCoordinate, yCoordinate, manholeLength, manholeWidth, compartmentShapeAsString, floodableArea, bottomLevel, surfaceLevel);
            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement);
            var manhole = element as Manhole;
            Assert.NotNull(manhole);
            Assert.IsTrue(manhole.Compartments.Count == 1);
            var compartment = manhole.Compartments.FirstOrDefault();
            Assert.NotNull(compartment);
            // Check Compartment properties
            CheckCompartmentAndManholePropertyValues(compartment, uniqueId, manholeId, manholeLength, manholeWidth, compartmentShape, floodableArea, bottomLevel, surfaceLevel, xCoordinate, yCoordinate, numberOfParentManholeCompartments);
        }

        [Test]
        public void GivenSimpleGwswCompartmentWithManholeIdAndNoNetwork_WhenCreatingWithGenerator_ThenManholeWithNewCoordinatesIsGiven()
        {
            #region expectedVariables
            var uniqueId = "put1";
            var manholeId = "01001";
            var manholeLength = 7071;
            var manholeWidth = 7071;
            var compartmentShape = CompartmentShape.Square;
            var compartmentShapeAsString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(compartmentShape);
            var nodeType = string.Empty;
            var floodableArea = 45.67;
            var bottomLevel = 0.01;
            var surfaceLevel = 2.75;
            var xCoordinate = 400.0;
            var yCoordinate = 50.0;
            var numberOfParentManholeCompartments = 1;
            #endregion

            var nodeGwswElement = GetNodeGwswElement(uniqueId, manholeId, nodeType, xCoordinate, yCoordinate, manholeLength, manholeWidth, compartmentShapeAsString, floodableArea, bottomLevel, surfaceLevel);
            var manhole = new SewerCompartmentGenerator().Generate(nodeGwswElement, null) as Manhole;
            Assert.NotNull(manhole);
            Assert.IsTrue(manhole.Compartments.Count == 1);

            var compartment = manhole.Compartments.FirstOrDefault();
            Assert.NotNull(compartment);
            
            // Check Properties
            CheckCompartmentAndManholePropertyValues(compartment, uniqueId, manholeId, manholeLength, manholeWidth, compartmentShape, floodableArea, bottomLevel, surfaceLevel, xCoordinate, yCoordinate, numberOfParentManholeCompartments);
            Assert.AreEqual(xCoordinate, manhole.XCoordinate);
            Assert.AreEqual(yCoordinate, manhole.YCoordinate);
        }

        [Test]
        public void GivenSimpleGwswCompartmentWithManholeIdNetworkAndExistingManhole_WhenCreatingWithGenerator_ThenManholeWithoutNewCoordinatesIsGiven()
        {
            #region expectedVariables
            var uniqueId = "put1";
            var manholeId = "01001";
            var manholeLength = 7071;
            var manholeWidth = 7071;
            var compartmentShape = CompartmentShape.Square;
            var compartmentShapeAsString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(compartmentShape);
            var nodeType = string.Empty;
            var floodableArea = 45.67;
            var bottomLevel = 0.01;
            var surfaceLevel = 2.75;
            var xCoordinate = 400.0;
            var yCoordinate = 50.0;
            var numberOfParentManholeCompartments = 1;
            #endregion

            var nodeGwswElement = GetNodeGwswElement(uniqueId, manholeId, nodeType, xCoordinate, yCoordinate, manholeLength, manholeWidth, compartmentShapeAsString, floodableArea, bottomLevel, surfaceLevel);

            var network = new HydroNetwork();
            var existingXCoord = 5;
            var existingYCoord = 3;
            network.Nodes.Add(new Manhole(manholeId){Geometry = new Point(existingXCoord,existingYCoord)});

            var manhole = new SewerCompartmentGenerator().Generate(nodeGwswElement, network) as Manhole;
            Assert.NotNull(manhole);
            Assert.IsTrue(manhole.Compartments.Count == 1);

            var compartment = manhole.Compartments.FirstOrDefault();
            Assert.NotNull(compartment);
            
            // Check Compartment properties
            CheckCompartmentAndManholePropertyValues(compartment, uniqueId, manholeId, manholeLength, manholeWidth, compartmentShape, floodableArea, bottomLevel, surfaceLevel, existingXCoord, existingYCoord, numberOfParentManholeCompartments);
        }

        [Test]
        public void GivenSimpleGwswCompartmentWithManholeIdAndNetworkWithoutExistingManhole_WhenCreatingWithGenerator_ThenManholeWithNewCoordinatesIsGiven()
        {
            #region expectedVariables
            var uniqueId = "put1";
            var manholeId = "01001";
            var manholeLength = 7071;
            var manholeWidth = 7071;
            var compartmentShape = CompartmentShape.Square;
            var compartmentShapeAsString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(compartmentShape);
            var nodeType = string.Empty;
            var floodableArea = 45.67;
            var bottomLevel = 0.01;
            var surfaceLevel = 2.75;
            var xCoordinate = 400.0;
            var yCoordinate = 50.0;
            var numberOfParentManholeCompartments = 1;
            #endregion

            var nodeGwswElement = GetNodeGwswElement(uniqueId, manholeId, nodeType, xCoordinate, yCoordinate, manholeLength, manholeWidth, compartmentShapeAsString, floodableArea, bottomLevel, surfaceLevel);

            var network = new HydroNetwork();
            var manhole = new SewerCompartmentGenerator().Generate(nodeGwswElement, network) as Manhole;
            Assert.NotNull(manhole);
            Assert.IsTrue(manhole.Compartments.Count == 1);

            var compartment = manhole.Compartments.FirstOrDefault();
            Assert.NotNull(compartment);
            
            // Check Compartment properties
            CheckCompartmentAndManholePropertyValues(compartment, uniqueId, manholeId, manholeLength, manholeWidth, compartmentShape, floodableArea, bottomLevel, surfaceLevel, xCoordinate, yCoordinate, numberOfParentManholeCompartments);
            Assert.AreEqual(xCoordinate, manhole.XCoordinate);
            Assert.AreEqual(yCoordinate, manhole.YCoordinate);
            Assert.IsTrue(network.Manholes.Contains(manhole));
        }

        [Test]
        public void GivenGwswElementWithNotAllAttributesDefined_WhenCreatingManhole_ThenNoExceptionAndMissingPropertiesAreNotDefinedOrHaveDefaultValues()
        {
            var uniqueId = "put1";
            var manholeId = "01001";
            var gwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Node.ToString(),
                GwswAttributeList =
                {
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.UniqueId, uniqueId, string.Empty),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.ManholeId, manholeId, string.Empty)
                }
            };

            var element = SewerFeatureFactory.CreateInstance(gwswElement);
            var manhole = element as Manhole;
            Assert.NotNull(manhole);
            Assert.IsTrue(manhole.Compartments.Count == 1);
            var compartment = manhole.Compartments.FirstOrDefault();
            Assert.NotNull(compartment);
            CheckCompartmentAndManholePropertyValues(compartment, uniqueId, manholeId, 0, 0, CompartmentShape.Unknown, 0.0, 0.0, 0.0, 0.0, 0.0, 1);
        }

        [Test]
        public void GivenGwswElementWithBadlyFormattedStringForShape_WhenCreatingWithFactory_ThenLogMessageIsShownAndDefaultShapeIsAssigned()
        {
            var compartmentId = "put1";
            var manholeId = "01001";
            var unknownShapeValue = "UnkownValue";
            var badGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Node.ToString(),
                GwswAttributeList =
                {
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.UniqueId, compartmentId, string.Empty),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.ManholeId, manholeId, string.Empty),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeShape, unknownShapeValue, string.Empty)
                }
            };

            var expectedLogMsg = string.Format(Resources.SewerFeatureFactory_GetValueFromDescription_Type__0__is_not_recognized__please_check_the_syntax, unknownShapeValue);
            var compartment = TryCreateCompartmentAndCheckForLogMessageAndCheckCompartmentValidity(manholeId, badGwswElement, compartmentId, expectedLogMsg);
            Assert.AreEqual(default(CompartmentShape), compartment.Shape);
        }

        [TestCase("01FA", ManholeMapping.PropertyKeys.NodeWidth)]
        [TestCase("01FA", ManholeMapping.PropertyKeys.NodeLength)]
        [TestCase("01FA", ManholeMapping.PropertyKeys.FloodableArea)]
        [TestCase("01FA", ManholeMapping.PropertyKeys.BottomLevel)]
        [TestCase("01FA", ManholeMapping.PropertyKeys.SurfaceLevel)]
        public void GivenGwswElementWithBadlyFormattedStringForDoubleValue_WhenCreatingWithFactory_ThenLogMessageIsShownAndNullValueIsReturned(string badlyFormattedEntry, string keyValue)
        {
            var compartmentId = "put1";
            var manholeId = "01001";
            var badGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Node.ToString(),
                GwswAttributeList =
                {
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.UniqueId, compartmentId, string.Empty),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.ManholeId, manholeId, string.Empty),
                    GetDefaultGwswAttribute(keyValue, badlyFormattedEntry, string.Empty)
                }
            };
            var expectedPartOfMessage = "It was not possible to parse attribute";
            TryCreateCompartmentAndCheckForLogMessageAndCheckCompartmentValidity(manholeId, badGwswElement, compartmentId, expectedPartOfMessage);
        }

        [Test]
        public void GivenManhole_WhenGeneratingNewCompartmentWithSameName_ThenReplaceOldCompartmentWithNewCompartment()
        {
            /* This test should be SewerCompartment responsibility.*/
            var oldBottomLevel = 10;
            var newBottomLevel = 33;
            var uniqueId = "testCompartment";
            var oldCompartment = new Compartment(uniqueId) { BottomLevel = oldBottomLevel };

            var manholeId = "testManhole";
            var manhole = new Manhole(manholeId);
            manhole.Compartments.Add(oldCompartment);

            Assert.That(manhole.Compartments.Count, Is.EqualTo(1));
            Assert.AreEqual(uniqueId, manhole.Compartments.FirstOrDefault()?.Name);
            Assert.AreEqual(oldCompartment, manhole.Compartments.FirstOrDefault());
            Assert.AreEqual(oldBottomLevel, manhole.Compartments.FirstOrDefault()?.BottomLevel);

            #region MyRegion

            var gwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Node.ToString(),
                GwswAttributeList =
                {
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.UniqueId, uniqueId, string.Empty),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.ManholeId, manholeId, string.Empty),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.BottomLevel, newBottomLevel.ToString(), string.Empty, TypeDouble)
                }
            };

            #endregion

            var network = new HydroNetwork();
            network.Nodes.Add(manhole);
            Assert.IsTrue(network.Manholes.Contains(manhole));

            new SewerCompartmentGenerator().Generate(gwswElement, network);
            
            Assert.That(manhole.Compartments.Count, Is.EqualTo(1));
            Assert.AreEqual(uniqueId, manhole.Compartments.FirstOrDefault()?.Name);
            Assert.AreEqual(newBottomLevel, manhole.Compartments.FirstOrDefault()?.BottomLevel);
        }

        [Test]
        public void GivenGwswElementWithMissingUniqueId_WhenCreatingWithSewerCompartmentGenerator_ThenLogMessageIsShownAndManholeWithCompartmentIsReturned()
        {
            var manholeId = "01001";
            var lineNumber = 2;
            var badGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Node.ToString(),
                GwswAttributeList =
                {
                    new GwswAttribute
                    {
                        ValueAsString = manholeId,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", lineNumber, "MyColumnName", "string",
                            ManholeMapping.PropertyKeys.ManholeId, "MyDescription",  null, null, null)
                    }
                }
            };

            var manhole = GenerateManholeWithLogMessages(badGwswElement, "Compartment", lineNumber, string.Empty);

            Assert.AreEqual(manholeId, manhole.Name);
            Assert.IsTrue(manhole.Compartments.Any());
            Assert.AreEqual(1, manhole.Compartments.Count);
        }

        [Test]
        public void GivenGwswElementWithMissingManholeId_WhenCreatingWithSewerCompartmentGenerator_ThenLogMessageIsShownAndManholeWithCompartmentIsReturned()
        {
            var compartmentName = "put1";
            var lineNumber = 2;
            var badGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Node.ToString(),
                GwswAttributeList =
                {
                    new GwswAttribute
                    {
                        ValueAsString = compartmentName,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", lineNumber, "MyColumnName", "string",
                            ManholeMapping.PropertyKeys.UniqueId, "MyDescription", null, "", null)
                    }
                }
            };

            var manhole = GenerateManholeWithLogMessages(badGwswElement, "Manhole", lineNumber, string.Empty);
            Assert.IsTrue(manhole.Compartments.Any());
            Assert.AreEqual(1, manhole.Compartments.Count);
            Assert.IsTrue(manhole.ContainsCompartment(compartmentName));
        }

        [Test]
        public void GivenGwswElementWithMissingUniqueIdAndManholeId_WhenCreatingWithSewerCompartmentGenerator_ThenLogMessageAreShownAndManholeWithCompartmentIsReturned()
        {
            var badGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Node.ToString()
            };

            GenerateManholeWithLogMessages(badGwswElement, "Compartment", 0, string.Empty);
            var manhole = GenerateManholeWithLogMessages(badGwswElement, "Manhole", 0, string.Empty);

            Assert.IsTrue(manhole.Compartments.Any());
            Assert.AreEqual(1, manhole.Compartments.Count);
        }

        [TestCase(ManholeMapping.PropertyKeys.NodeLength)]
        [TestCase(ManholeMapping.PropertyKeys.NodeWidth)]
        [TestCase(ManholeMapping.PropertyKeys.FloodableArea)]
        [TestCase(ManholeMapping.PropertyKeys.BottomLevel)]
        [TestCase(ManholeMapping.PropertyKeys.SurfaceLevel)]
        [TestCase(ManholeMapping.PropertyKeys.NodeShape)]
        public void GivenGwswElementWithEmptyValue_WhenCreatingWithFactory_ThenDefaultValuesAreGivenToTheCorrespondingCompartmentProperty(string manholePropertyKey)
        {
            var uniqueId = "put1";
            var manholeId = "01001";
            var gwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Node.ToString(),
                GwswAttributeList =
                {
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.UniqueId, uniqueId, string.Empty),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.ManholeId, manholeId, string.Empty),
                    GetDefaultGwswAttribute(manholePropertyKey, string.Empty, string.Empty)
                }
            };

            var manhole = SewerFeatureFactory.CreateInstance(gwswElement) as Manhole;
            Assert.NotNull(manhole);
            Assert.IsTrue(manhole.Compartments.Any());

            CheckCompartmentAndManholePropertyValues(manhole.Compartments.FirstOrDefault(), uniqueId, manholeId, 0.0, 0.0, CompartmentShape.Unknown, 0.0, 0.0, 0.0, 0.0, 0.0, 1);
        }

        private static Manhole GenerateManholeWithLogMessages(GwswElement badGwswElement, string componentType, int lineNumber, string newName)
        {
            INetworkFeature feature = null;
            var message =
                string.Format(
                    Resources
                        .SewerCompartmentGenerator_FindOrGetNewCompartment__0__in_line__1__does_not_have_a_name_and_has_been_created_as__2_,
                    componentType, lineNumber, newName);
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => feature = new SewerCompartmentGenerator().Generate(badGwswElement, null), message);
            Assert.IsNotNull(feature);

            var manhole = feature as Manhole;
            Assert.IsNotNull(manhole);
            return manhole;
        }

        #endregion

        private static Compartment TryCreateCompartmentAndCheckForLogMessageAndCheckCompartmentValidity(string manholeId, GwswElement badGwswElement, string compartmentId, string expectedMsg)
        {
            INetworkFeature feature = null;

            feature = SewerFeatureFactory.CreateInstance(badGwswElement);
            //TestHelper.AssertAtLeastOneLogMessagesContains(() => feature = SewerFeatureFactory.CreateInstance(badGwswElement),
            //    expectedMsg);

            // Check compartment
            var manhole = feature as Manhole;
            Assert.NotNull(manhole);

            var compartment = manhole.Compartments.FirstOrDefault();
            Assert.IsNotNull(compartment);
            Assert.That(compartment.Name, Is.EqualTo(compartmentId));
            Assert.NotNull(compartment.ParentManhole);
            Assert.That(compartment.ParentManhole.Name, Is.EqualTo(manholeId));

            return compartment;
        }
    }
}