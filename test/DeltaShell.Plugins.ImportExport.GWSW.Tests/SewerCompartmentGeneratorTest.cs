using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.ImportExport.GWSW.Properties;
using DHYDRO.Common.Logging;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.GWSW.Tests
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
            var compartmentShape = CompartmentShape.Round;
            var compartmentShapeAsString = compartmentShape.GetDescription();
            var nodeType = string.Empty;
            var floodableArea = 45.67;
            var bottomLevel = 0.01;
            var surfaceLevel = 2.75;
            var xCoordinate = 400.0;
            var yCoordinate = 50.0;
            var numberOfParentManholeCompartments = 1;
            #endregion

            var nodeGwswElement = GetNodeGwswElement(uniqueId, manholeId, nodeType, xCoordinate, yCoordinate, manholeLength, manholeWidth, compartmentShapeAsString, floodableArea, bottomLevel, surfaceLevel);
            var compartment = CreateSewerFeature<Compartment>(nodeGwswElement);
            Assert.IsNotNull(compartment);

            // Check Compartment properties
            CheckCompartmentPropertyValues(compartment, uniqueId, manholeLength, manholeWidth, compartmentShape, floodableArea, bottomLevel, surfaceLevel);
        }

        [Test]
        public void GivenSimpleGwswCompartmentWithManholeIdAndNoNetwork_WhenCreatingWithGenerator_ThenManholeWithNewCoordinatesIsGiven()
        {
            #region expectedVariables
            var uniqueId = "put1";
            var manholeId = "01001";
            var manholeLength = 7071;
            var manholeWidth = 7071;
            var compartmentShape = CompartmentShape.Round;
            var compartmentShapeAsString = compartmentShape.GetDescription();
            var nodeType = string.Empty;
            var floodableArea = 45.67;
            var bottomLevel = 0.01;
            var surfaceLevel = 2.75;
            var xCoordinate = 400.0;
            var yCoordinate = 50.0;
            var numberOfParentManholeCompartments = 1;
            #endregion

            var nodeGwswElement = GetNodeGwswElement(uniqueId, manholeId, nodeType, xCoordinate, yCoordinate, manholeLength, manholeWidth, compartmentShapeAsString, floodableArea, bottomLevel, surfaceLevel);
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var compartment = new SewerCompartmentGenerator(logHandler).Generate(nodeGwswElement) as Compartment;
            Assert.IsNotNull(compartment);
            
            // Check Properties
            CheckCompartmentPropertyValues(compartment, uniqueId, manholeLength, manholeWidth, compartmentShape, floodableArea, bottomLevel, surfaceLevel);
        }

        [Test]
        public void GivenSimpleGwswCompartmentWithManholeIdNetworkAndExistingManhole_WhenCreatingWithGenerator_NewCoordinatesOfAverageCompartmentsIsGiven()
        {
            #region expectedVariables
            var uniqueId = "put1";
            var manholeId = "01001";
            var manholeLength = 7071;
            var manholeWidth = 7071;
            var compartmentShape = CompartmentShape.Round;
            var compartmentShapeAsString = compartmentShape.GetDescription();
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
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var compartment = new SewerCompartmentGenerator(logHandler).Generate(nodeGwswElement) as Compartment;
            Assert.IsNotNull(compartment);

            // Check Compartment properties
            CheckCompartmentPropertyValues(compartment, uniqueId, manholeLength, manholeWidth, compartmentShape, floodableArea, bottomLevel, surfaceLevel);
        }

        [Test]
        public void GivenSimpleGwswCompartmentWithManholeIdAndNetworkWithoutExistingManhole_WhenCreatingWithGenerator_ThenManholeWithNewCoordinatesIsGiven()
        {
            #region expectedVariables
            var uniqueId = "put1";
            var manholeId = "01001";
            var manholeLength = 7071;
            var manholeWidth = 7071;
            var compartmentShape = CompartmentShape.Round;
            var compartmentShapeAsString = compartmentShape.GetDescription();
            var nodeType = string.Empty;
            var floodableArea = 45.67;
            var bottomLevel = 0.01;
            var surfaceLevel = 2.75;
            var xCoordinate = 400.0;
            var yCoordinate = 50.0;
            var numberOfParentManholeCompartments = 1;
            #endregion

            var nodeGwswElement = GetNodeGwswElement(uniqueId, manholeId, nodeType, xCoordinate, yCoordinate, manholeLength, manholeWidth, compartmentShapeAsString, floodableArea, bottomLevel, surfaceLevel);
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var compartment = new SewerCompartmentGenerator(logHandler).Generate(nodeGwswElement) as Compartment;
            Assert.IsNotNull(compartment);

            // Check Compartment properties
            CheckCompartmentPropertyValues(compartment, uniqueId, manholeLength, manholeWidth, compartmentShape, floodableArea, bottomLevel, surfaceLevel);
        }

        [Test]
        public void GivenCompartmentGwswElementWithOnlyUniqueIdAndManholeIdDefined_WhenCreatingCompartment_ThenDefaultValuesAreGivenToCompartmentProperties()
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

            var compartment = CreateSewerFeature<Compartment>(gwswElement);
        
            Assert.IsNotNull(compartment);
            Assert.IsNotNull(compartment.ParentManholeName);
            CheckCompartmentPropertyValues(compartment, uniqueId, 800, 800, CompartmentShape.Unknown, 500, -10.0, 0.0);
        }

        [Test]
        public void GivenGwswElementWithBadlyFormattedStringForShape_WhenCreatingWithFactory_ThenLogMessageIsShownAndDefaultShapeIsAssigned()
        {
            var compartmentId = "put1";
            var manholeId = "01001";
            var unknownShapeValue = "UnknownValue";
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

            ILogHandler logHandler = Substitute.For<ILogHandler>();

            Compartment compartment = CreateSewerFeature<Compartment>(badGwswElement, logHandler);

            logHandler.Received().ReportWarningFormat(Resources.Shape__0__is_not_a_valid_shape_Setting_shape_to_unknown, unknownShapeValue);

            Assert.IsNotNull(compartment);
            Assert.That(compartment.Name, Is.EqualTo(compartmentId));
            Assert.That(compartment.ParentManholeName, Is.EqualTo(manholeId));
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
            var expectedPartOfMessage = "using default value:";
            TryCreateCompartmentAndCheckForLogMessageAndCheckCompartmentValidity(manholeId, badGwswElement, compartmentId, expectedPartOfMessage);
        }

        [Test]
        public void GivenCompartmentGwswElementWithMissingUniqueId_WhenGeneratingCompartment_ThenLogMessageIsShownAndCompartmentWithParentManholeIdIsReturned()
        {
            var manholeId = "01001";
            var lineNumber = 2;
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var badGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Node.ToString(),
                GwswAttributeList =
                {
                    new GwswAttribute
                    {
                        LineNumber = lineNumber,
                        ValueAsString = manholeId,
                        GwswAttributeType = GetGwswAttributeType("Knooppunt.csv", 0, "MyColumnName", "string",
                            ManholeMapping.PropertyKeys.ManholeId, "MyDescription",  null, null, null, logHandler)
                    }
                }
            };

            var compartment = GenerateCompartmentAndCheckForLogMessages(badGwswElement, "Compartment", lineNumber, string.Empty);
            Assert.IsNotNull(compartment.ParentManholeName);
        }

        [Test]
        public void GivenGwswElementWithMissingCompartmentIdAndManholeId_WhenGeneratingSewerCompartment_ThenLogMessageAreShown()
        {
            var badGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Node.ToString()
            };

            GenerateCompartmentAndCheckForLogMessages(badGwswElement, "Compartment", 0, string.Empty);
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

            var compartment = CreateSewerFeature<Compartment>(gwswElement);
            Assert.IsNotNull(compartment);
            Assert.IsNotNull(compartment.ParentManholeName);

            CheckCompartmentPropertyValues(compartment, uniqueId, 800, 800, CompartmentShape.Unknown, 500, -10.0, 0.0);
        }

        private static Compartment GenerateCompartmentAndCheckForLogMessages(GwswElement badGwswElement, string componentType, int lineNumber, string newName)
        {
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            Compartment compartment = new SewerCompartmentGenerator(logHandler).Generate(badGwswElement) as Compartment;
            Assert.IsNotNull(compartment);
            string logMessage = string.Format(Resources.SewerCompartmentGenerator_FindOrGetNewCompartment__0__in_line__1__does_not_have_a_name_and_will_be_added_to_the_network_with_a_unique_name, componentType, lineNumber, newName);
            logHandler.Received().ReportWarningFormat(logMessage);
            
            return compartment;
        }

        #endregion

        private static Compartment TryCreateCompartmentAndCheckForLogMessageAndCheckCompartmentValidity(string manholeId, GwswElement badGwswElement, string compartmentId, string expectedMsg)
        {
            Compartment compartment = null;
            ILogHandler logHandler = Substitute.For<ILogHandler>();

            compartment = CreateSewerFeature<Compartment>(badGwswElement, logHandler);
            logHandler.Received().ReportWarningFormat(Arg.Is<string>(m => m.Contains(expectedMsg)));
            
            Assert.IsNotNull(compartment);
            Assert.That(compartment.Name, Is.EqualTo(compartmentId));
            Assert.That(compartment.ParentManholeName, Is.EqualTo(manholeId));

            return compartment;
        }
    }
}