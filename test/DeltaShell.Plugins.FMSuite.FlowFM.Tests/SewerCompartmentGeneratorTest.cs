using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Networks;
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
            CheckCompartmentPropertyValues(compartment, uniqueId, manholeId, manholeLength, manholeWidth, compartmentShape, floodableArea, bottomLevel, surfaceLevel, numberOfParentManholeCompartments);
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
                    new GwswAttribute
                    {
                        ValueAsString = uniqueId,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholeMapping.PropertyKeys.UniqueId, "MyDescription", null, null)
                    },
                    new GwswAttribute
                    {
                        ValueAsString = manholeId,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholeMapping.PropertyKeys.ManholeId, "MyDescription", null, null)
                    }
                }
            };

            var element = SewerFeatureFactory.CreateInstance(gwswElement);
            var manhole = element as Manhole;
            Assert.NotNull(manhole);
            Assert.IsTrue(manhole.Compartments.Count == 1);
            var compartment = manhole.Compartments.FirstOrDefault();
            Assert.NotNull(compartment);
            CheckCompartmentPropertyValues(compartment, uniqueId, manholeId, 0, 0, CompartmentShape.Unknown, 0.0, 0.0, 0.0, 1);
        }

        [Test]
        public void GivenGwswElementWithBadlyFormattedStringForShape_WhenCreatingWithFactory_ThenLogMessageIsShownAndDefaultShapeIsAssigned()
        {
            var compartmentId = "put1";
            var manholeId = "01001";
            var valueAsString = "UnkownValue";
            var badGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Node.ToString(),
                GwswAttributeList =
                {
                    new GwswAttribute
                    {
                        ValueAsString = compartmentId,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholeMapping.PropertyKeys.UniqueId, "MyDescription", null, null)
                    },
                    new GwswAttribute
                    {
                        ValueAsString = manholeId,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholeMapping.PropertyKeys.ManholeId, "MyDescription", null, null)
                    },
                    new GwswAttribute
                    {
                        ValueAsString = valueAsString,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholeMapping.PropertyKeys.NodeShape, "MyDescription", null, null)
                    }
                }
            };

            var expectedLogMsg = string.Format(Resources.SewerFeatureFactory_GetValueFromDescription_Type__0__is_not_recognized__please_check_the_syntax, valueAsString);
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
                    new GwswAttribute
                    {
                        ValueAsString = compartmentId,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholeMapping.PropertyKeys.UniqueId, "MyDescription", null, null)
                    },
                    new GwswAttribute
                    {
                        ValueAsString = manholeId,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholeMapping.PropertyKeys.ManholeId, "MyDescription", null, null)
                    },
                    new GwswAttribute
                    {
                        ValueAsString = badlyFormattedEntry,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "double",
                            keyValue, "MyDescription", null, null)
                    }
                }
            };
            var expectedPartOfMessage = "It was not possible to parse attribute";
            TryCreateCompartmentAndCheckForLogMessageAndCheckCompartmentValidity(manholeId, badGwswElement, compartmentId, expectedPartOfMessage);
        }

        [Test]
        public void GivenGwswElementWithMissingUniqueId_WhenCreatingWithFactory_ThenLogMessageIsShownAndNullValueIsReturned()
        {
            var badGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Node.ToString(),
                GwswAttributeList =
                {
                    new GwswAttribute
                    {
                        ValueAsString = "01001",
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholeMapping.PropertyKeys.ManholeId, "MyDescription", null, null)
                    }
                }
            };

            var manholeId = badGwswElement.GwswAttributeList.Where(attr => attr.GwswAttributeType.Key == ManholeMapping.PropertyKeys.ManholeId)
                .Select(attr => attr.ValueAsString).FirstOrDefault();
            TryCreateFeatureAndCheckForLogMessageAndFeatureIsNull(badGwswElement, "Manhole with manhole id '" + manholeId + "' could not be created, because one of its compartments misses its unique id.");
        }

        [Test]
        public void GivenGwswElementWithMissingManholeId_WhenCreatingWithFactory_ThenLogMessageIsShownAndNullValueIsReturned()
        {
            var badGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Node.ToString(),
                GwswAttributeList =
                {
                    new GwswAttribute
                    {
                        ValueAsString = "put1",
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholeMapping.PropertyKeys.UniqueId, "MyDescription", null, null)
                    }
                }
            };

            TryCreateFeatureAndCheckForLogMessageAndFeatureIsNull(badGwswElement, "There are lines in 'Knooppunt.csv' that do not contain a Manhole Id. These lines are not imported.");
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
                    new GwswAttribute
                    {
                        ValueAsString = uniqueId,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholeMapping.PropertyKeys.UniqueId, "MyDescription", null, null)
                    },
                    new GwswAttribute
                    {
                        ValueAsString = manholeId,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholeMapping.PropertyKeys.ManholeId, "MyDescription", null, null)
                    },
                    new GwswAttribute
                    {
                        ValueAsString = string.Empty,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "double",
                            manholePropertyKey, "MyDescription", null, null)
                    }
                }
            };

            var manhole = SewerFeatureFactory.CreateInstance(gwswElement) as Manhole;
            Assert.NotNull(manhole);
            Assert.IsTrue(manhole.Compartments.Any());

            CheckCompartmentPropertyValues(manhole.Compartments.FirstOrDefault(), uniqueId, manholeId, 0.0, 0.0, CompartmentShape.Unknown, 0.0, 0.0, 0.0, 1);
        }

        #endregion

        private static Compartment TryCreateCompartmentAndCheckForLogMessageAndCheckCompartmentValidity(string manholeId, GwswElement badGwswElement, string compartmentId, string expectedMsg)
        {
            INetworkFeature feature = null;

            TestHelper.AssertAtLeastOneLogMessagesContains(() => feature = SewerFeatureFactory.CreateInstance(badGwswElement),
                expectedMsg);

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