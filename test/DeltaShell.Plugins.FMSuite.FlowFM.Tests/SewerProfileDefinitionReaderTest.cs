using System.Collections.Generic;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using log4net;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class SewerProfileDefinitionReaderTest : SewerFeatureFactoryTestHelper
    {
        private static ILog Log = LogManager.GetLogger(typeof(SewerProfileDefinitionReaderTest));
        private const string ProfileId = "PRO1";
        private const string TypeDouble = "double";

        [TestCase(SewerFeatureType.Node)]
        [TestCase(SewerFeatureType.Connection)]
        [TestCase(SewerFeatureType.Discharge)]
        [TestCase(SewerFeatureType.Distribution)]
        [TestCase(SewerFeatureType.Meta)]
        [TestCase(SewerFeatureType.Runoff)]
        [TestCase(SewerFeatureType.Structure)]
        [TestCase(SewerFeatureType.Surface)]
        public void GivenGwswElementWithElementTypeNameUnequalToSewerProfile_WhenReadingSewerProfileDefinition_ThenReturnNull(SewerFeatureType type)
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = type.ToString()
            };
            CreateCsdShapeAndCheckForNull<CsdCircleDefinitionReader>(profileGwswElement);
            CreateCsdShapeAndCheckForNull<CsdRectangleDefinitionReader>(profileGwswElement);
            CreateCsdShapeAndCheckForNull<CsdEggDefinitionReader>(profileGwswElement);
            CreateCsdShapeAndCheckForNull<CsdTrapezoidDefinitionReader>(profileGwswElement);
            CreateCsdShapeAndCheckForNull<CsdArchDefinitionReader>(profileGwswElement);
            CreateCsdShapeAndCheckForNull<CsdCunetteDefinitionReader>(profileGwswElement);
        }

        private static void CreateCsdShapeAndCheckForNull<T>(GwswElement element) where T : SewerProfileDefinitionReader, new()
        {
            var circleReader = new T();
            var csDefinition = circleReader.ReadSewerProfileDefinition(element);
            Assert.IsNull(csDefinition);
        }

        #region Circle shape cross section

        [Test]
        public void GivenGwswElement_WhenReadingSewerProfileCircleDefinition_ThenReturnCircleShapeWithCorrectPropertyValues()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "1250", string.Empty, TypeDouble)
                }
            };
            CreateCsdCircleShapeAndCheckProperties(profileGwswElement, 1.25);
        }

        [Test]
        public void GivenSewerProfileGwswElementWithoutWidthDefined_WhenReadingSewerProfileCircleDefinition_ThenReturnDefaultCircleSewerProfileAndLogMessage()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty)
                }
            };
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => CreateCsdCircleShapeAndCheckProperties(profileGwswElement, 0.160d),
                "Sewer profile 'PRO1' is missing its width. Default profile property values are used for this profile.");
        }

        private static void CreateCsdCircleShapeAndCheckProperties(GwswElement element, double diameter)
        {
            var circleReader = new CsdCircleDefinitionReader();

            var csDefinition = circleReader.ReadSewerProfileDefinition(element) as CrossSectionDefinitionStandard;
            Assert.NotNull(csDefinition);

            var csRoundShape = csDefinition.Shape as CrossSectionStandardShapeRound;
            Assert.NotNull(csRoundShape);
            Assert.That(csRoundShape.Diameter, Is.EqualTo(diameter));
        }

        #endregion

        #region Rectangle shape cross section

        [Test]
        public void GivenGwswElement_WhenReadingSewerProfileRectangleDefinition_ThenReturnRectangleShapeWithCorrectPropertyValues()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "2000", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "1200", string.Empty, TypeDouble)
                }
            };
            CreateCsdShapeAndCheckProperties<CsdRectangleDefinitionReader, CrossSectionStandardShapeRectangle>(profileGwswElement, 2.0, 1.2);
        }

        [TestCase(SewerProfileMapping.PropertyKeys.SewerProfileWidth)]
        [TestCase(SewerProfileMapping.PropertyKeys.SewerProfileHeight)]
        [TestCase("fakeKey")]
        public void GivenSewerProfileGwswElementWithoutHeightOrWidthDefined_WhenReadingSewerProfileRectangleDefinition_ThenReturnDefaultRectangleSewerProfileAndLogMessage(string key)
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(key, "2000", string.Empty, TypeDouble)
                }
            };
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => CreateCsdShapeAndCheckProperties<CsdRectangleDefinitionReader, CrossSectionStandardShapeRectangle>(profileGwswElement, 1.0, 1.0),
                "Sewer profile 'PRO1' is missing its width and/or height. Default profile property values are used for this profile.");
        }

        #endregion

        #region Egg shape cross section
        
        [Test]
        public void GivenGwswElement_WhenReadingSewerProfileEggDefinition_ThenReturnEggShapeWithCorrectPropertyValues()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "250", string.Empty, TypeDouble)
                }
            };
            CreateCsdShapeAndCheckProperties<CsdEggDefinitionReader, CrossSectionStandardShapeEgg>(profileGwswElement, 0.25, 0.375);
        }

        [Test]
        public void GivenSewerProfileGwswElementWithoutWidthDefined_WhenReadingSewerProfileEggDefinition_ThenReturnDefaultEggShapeAndLogMessage()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty)
                }
            };
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => CreateCsdShapeAndCheckProperties<CsdEggDefinitionReader, CrossSectionStandardShapeEgg>(profileGwswElement, 2.0, 3.0),
                "Sewer profile 'PRO1' is missing its width. Default profile property values are used for this profile.");
        }

        [Test]
        public void GivenGwswElementWithWidthAndHeightInCorrectProportion_WhenReadingSewerProfileEggDefinition_ThenReturnEggShapeWithCorrectPropertyValues()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "250", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "375", string.Empty, TypeDouble)
                }
            };
            CreateCsdShapeAndCheckProperties<CsdEggDefinitionReader, CrossSectionStandardShapeEgg>(profileGwswElement, 0.25, 0.375);
        }

        [Test]
        public void GivenSewerProfileGwswElementWithWidthAndHeightNotInCorrectProportion_WhenReadingSewerProfileEggDefinition_ThenLogMessageIsGivenToUser()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "250", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "400", string.Empty, TypeDouble)
                }
            };

            var reader = new CsdEggDefinitionReader();
            TestHelper.AssertAtLeastOneLogMessagesContains(() => reader.ReadSewerProfileDefinition(profileGwswElement), 
                "The width and height of sewer profile 'PRO1' are not in the right proportion (2:3). Width is now 250 mm and height is now 375 mm.");
        }

        [Test]
        public void GivenSewerProfileGwswElementWithWidthAndHeightNotInCorrectProportion_WhenReadingSewerProfileEggDefinition_ThenHeightFromGwswElementIsIgnored()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "250", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "375", string.Empty, TypeDouble)
                }
            };

            var reader = new CsdEggDefinitionReader();
            var csDefinition = reader.ReadSewerProfileDefinition(profileGwswElement) as CrossSectionDefinitionStandard;

            Assert.NotNull(csDefinition);

            var csShape = csDefinition.Shape as CrossSectionStandardShapeEgg;
            Assert.NotNull(csShape);
            Assert.That(csShape.Width, Is.EqualTo(0.25));
            Assert.That(csShape.Height, Is.EqualTo(0.375));
        }

        #endregion

        #region Cunette shape cross section

        [Test]
        public void GivenGwswElement_WhenReadingSewerProfileCunetteDefinition_ThenReturnCunetteShapeWithCorrectPropertyValues()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "2000", string.Empty, TypeDouble)
                }
            };
            CreateCsdShapeAndCheckProperties<CsdCunetteDefinitionReader, CrossSectionStandardShapeCunette>(profileGwswElement, 2.0, 1.268);
        }

        [Test]
        public void GivenGwswElementWithWidthAndHeightInCorrectProportion_WhenReadingSewerProfileCunetteDefinition_ThenReturnCunetteShapeWithCorrectPropertyValues()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "2000", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "1268", string.Empty, TypeDouble)
                }
            };
            CreateCsdShapeAndCheckProperties<CsdCunetteDefinitionReader, CrossSectionStandardShapeCunette>(profileGwswElement, 2.0, 1.268);
        }

        [Test]
        public void GivenSewerProfileGwswElementWithoutWidthDefined_WhenReadingSewerProfileCunetteDefinition_ThenReturnDefaultCunetteShapeAndLogMessage()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty)
                }
            };
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => CreateCsdShapeAndCheckProperties<CsdCunetteDefinitionReader, CrossSectionStandardShapeCunette>(profileGwswElement, 1.0, 0.634),
                "Sewer profile 'PRO1' is missing its width. Default profile property values are used for this profile.");
        }

        [Test]
        public void GivenSewerProfileGwswElementWithWidthAndHeightNotInCorrectProportion_WhenReadingSewerProfileCunetteDefinition_ThenLogMessageIsGivenToUser()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "250", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "400", string.Empty, TypeDouble)
                }
            };

            var reader = new CsdCunetteDefinitionReader();
            TestHelper.AssertAtLeastOneLogMessagesContains(() => reader.ReadSewerProfileDefinition(profileGwswElement),
                "The width and height of sewer profile 'PRO1' are not in the right proportion (1:0.634). Width is now 250 mm and height is now 158.5 mm.");
        }

        [Test]
        public void GivenSewerProfileGwswElementWithWidthAndHeightNotInCorrectProportion_WhenReadingSewerProfileCunetteDefinition_ThenHeightFromGwswElementIsIgnored()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "250", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "400", string.Empty, TypeDouble)
                }
            };

            var reader = new CsdCunetteDefinitionReader();
            var csDefinition = reader.ReadSewerProfileDefinition(profileGwswElement) as CrossSectionDefinitionStandard;

            Assert.NotNull(csDefinition);

            var csShape = csDefinition.Shape as CrossSectionStandardShapeCunette;
            Assert.NotNull(csShape);
            Assert.That(csShape.Width, Is.EqualTo(0.25));
            Assert.That(csShape.Height, Is.EqualTo(0.1585));
        }

        #endregion

        #region Arch shape cross section

        [Test]
        public void GivenGwswElement_WhenReadingSewerProfileArchDefinition_ThenReturnArchShapeWithCorrectPropertyValues()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "1200", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "2500", string.Empty, TypeDouble)
                }
            };
            
            CreateCsdArchShapeAndCheckProperties(profileGwswElement, 1.2, 2.5, 2.5);
        }

        [TestCase(SewerProfileMapping.PropertyKeys.SewerProfileWidth)]
        [TestCase(SewerProfileMapping.PropertyKeys.SewerProfileHeight)]
        public void GivenGwswElementWithMissingValues_WhenReadingSewerProfileArchDefinition_ThenReturnDefaultArchShapeAndLogMessage(string key)
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(key, "1000", string.Empty, TypeDouble)
                }
            };
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => CreateCsdArchShapeAndCheckProperties(profileGwswElement, 1.0, 2.0, 1.0),
                "Sewer profile 'PRO1' is missing its width and/or height. Default profile property values are used for this profile.");
        }

        private static void CreateCsdArchShapeAndCheckProperties(GwswElement element, double expectedWidth, double expectedHeight, double expectedArcHeight)
        {
            var archReader = new CsdArchDefinitionReader();

            var csDefinition = archReader.ReadSewerProfileDefinition(element) as CrossSectionDefinitionStandard;
            Assert.NotNull(csDefinition);

            var csShape = csDefinition.Shape as CrossSectionStandardShapeArch;
            Assert.NotNull(csShape);
            Assert.That(csShape.Width, Is.EqualTo(expectedWidth));
            Assert.That(csShape.Height, Is.EqualTo(expectedHeight));
            Assert.That(csShape.ArcHeight, Is.EqualTo(expectedArcHeight));
        }

        #endregion

        #region Trapezoid shape cross section

        [TestCase("2,5", "1,5")]
        [TestCase("2.5", "1.5")]
        public void GivenGwswElement_WhenReadingSewerProfileTrapezoidDefinition_ThenReturnTrapezoidShapeWithCorrectPropertyValues(string slope1, string slope2)
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "1000", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "1000", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.Slope1, slope1, string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.Slope2, slope2, string.Empty, TypeDouble)
                }
            };
            CreateCsdTrapezoidShapeAndCheckProperties(profileGwswElement, 1.0, 2.0, 2.0);
        }

        [TestCase(SewerProfileMapping.PropertyKeys.Slope1)]
        [TestCase(SewerProfileMapping.PropertyKeys.Slope2)]
        public void GivenGwswElementWithOneMissingSlope_WhenReadingSewerProfileTrapezoidDefinition_ThenReturnTrapezoidWithPresentSlope(string slopeKey)
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "1000", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "1000", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(slopeKey, "2", string.Empty, TypeDouble)
                }
            };
            CreateCsdTrapezoidShapeAndCheckProperties(profileGwswElement, 1.0, 2.0, 2.0);
        }

        [Test]
        public void GivenSewerProfileGwswElementWithMissingWidth_WhenReadingSewerProfileTrapezoidDefinition_ThenReturnDefaultTrapezoidSewerProfileAndLogMessage()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "1000", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.Slope1, "2,5", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.Slope2, "1,5", string.Empty, TypeDouble)
                }
            };
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => CreateCsdTrapezoidShapeAndCheckProperties(profileGwswElement, 10.0, 2.0, 20.0),
                "Sewer profile 'PRO1' is missing its width, height and/or slope. Default profile property values are used for this profile.");
        }

        [Test]
        public void GivenSewerProfileGwswElementWithMissingHeight_WhenReadingSewerProfileTrapezoidDefinition_ThenReturnDefaultTrapezoidSewerProfile()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "1000", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.Slope1, "2,5", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.Slope2, "1,5", string.Empty, TypeDouble)
                }
            };
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => CreateCsdTrapezoidShapeAndCheckProperties(profileGwswElement, 10.0, 2.0, 20.0),
                "Sewer profile 'PRO1' is missing its width, height and/or slope. Default profile property values are used for this profile.");
        }

        [Test]
        public void GivenSewerProfileGwswElementWithMissingSlopes_WhenReadingSewerProfileTrapezoidDefinition_ThenReturnDefaultTrapezoidSewerProfile()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "1000", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "1000", string.Empty, TypeDouble)
                }
            };
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => CreateCsdTrapezoidShapeAndCheckProperties(profileGwswElement, 10.0, 2.0, 20.0),
                "Sewer profile 'PRO1' is missing its width, height and/or slope. Default profile property values are used for this profile.");
        }

        private static void CreateCsdTrapezoidShapeAndCheckProperties(GwswElement element, double expectedBottomWidth, double expectedSlope, double expectedMaxFlowWidth)
        {
            var reader = new CsdTrapezoidDefinitionReader();
            var csDefinition = reader.ReadSewerProfileDefinition(element) as CrossSectionDefinitionStandard;
            Assert.NotNull(csDefinition);

            var csShape = csDefinition.Shape as CrossSectionStandardShapeTrapezium;
            Assert.NotNull(csShape);
            Assert.That(csShape.BottomWidthB, Is.EqualTo(expectedBottomWidth));
            Assert.That(csShape.Slope, Is.EqualTo(expectedSlope));
            Assert.That(csShape.MaximumFlowWidth, Is.EqualTo(expectedMaxFlowWidth));
        }

        #endregion

        #region Test helpers
        
        private static void CreateCsdShapeAndCheckProperties<TReader, TShape>(GwswElement gwswElement, double expectedWidth, double expectedHeight) 
            where TReader : SewerProfileDefinitionReader, new() 
            where TShape : CrossSectionStandardShapeWidthHeightBase, new()
        {
            var reader = new TReader();
            var csDefinition = reader.ReadSewerProfileDefinition(gwswElement) as CrossSectionDefinitionStandard;
            Assert.NotNull(csDefinition);

            var csShape = csDefinition.Shape as TShape;
            Assert.NotNull(csShape);
            Assert.That(csShape.Width, Is.EqualTo(expectedWidth));
            Assert.That(csShape.Height, Is.EqualTo(expectedHeight));
        }

        #endregion

        #region SewerDictionaryExtensions

        [Test]
        public void GivenLogger_WhenInvokingMessageForMissingValues_ThenMessageIsLogged()
        {
            var missingValuesText = "key values";
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty)
                }
            };
            
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => Log.MessageForMissingValues(profileGwswElement, missingValuesText),
                "Sewer profile 'PRO1' is missing its " + missingValuesText + ". Default profile property values are used for this profile.");
        }

        [Test]
        public void GivenLogger_WhenInvokingLogMessageInCaseSewerShapeWidthHeightAreNotInCorrectProportion_ThenMessageIsLogged()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "2500", string.Empty)
                }
            };

            var csShape = new CrossSectionStandardShapeEgg { Width = 2.0 };
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => Log.LogMessageInCaseSewerShapeWidthHeightAreNotInCorrectProportion(profileGwswElement, 2000.0, 1.5, "(2:3)", csShape),
                "The width and height of sewer profile 'PRO1' are not in the right proportion (2:3). Width is now 2000 mm and height is now 3000 mm.");
        }

        #endregion
    }
}