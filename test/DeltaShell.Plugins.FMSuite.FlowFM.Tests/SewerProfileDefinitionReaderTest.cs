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
    public class SewerProfileDefinitionReaderTest
    {
        private static ILog Log = LogManager.GetLogger(typeof(SewerProfileDefinitionReaderTest));
        private const string ProfileId = "PRO1";

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
            var element = GetGwswElement(type, GetGwswKeyValuePairs(new List<string>(), new List<string>()));
            CreateCSDShapeAndCheckForNull<CsdCircleDefinitionReader>(element);
            CreateCSDShapeAndCheckForNull<CsdRectangleDefinitionReader>(element);
            CreateCSDShapeAndCheckForNull<CsdEggDefinitionReader>(element);
            CreateCSDShapeAndCheckForNull<CsdTrapezoidDefinitionReader>(element);
            CreateCSDShapeAndCheckForNull<CsdArchDefinitionReader>(element);
            CreateCSDShapeAndCheckForNull<CsdCunetteDefinitionReader>(element);
        }

        private static void CreateCSDShapeAndCheckForNull<T>(GwswElement element) where T : SewerProfileDefinitionReader, new()
        {
            var circleReader = new T();
            var csDefinition = circleReader.ReadSewerProfileDefinition(element);
            Assert.IsNull(csDefinition);
        }

        #region Circle shape cross section

        [Test]
        public void GivenGwswElement_WhenReadingSewerProfileCircleDefinition_ThenReturnCircleShapeWithCorrectPropertyValues()
        {
            var keys = new List<string> { SewerProfileMapping.PropertyKeys.SewerProfileWidth };
            var values = new List<string> { "1250" };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            CreateCSDCircleShapeAndCheckProperties(element, 1.25);
        }

        [Test]
        public void GivenSewerProfileGwswElementWithoutWidthDefined_WhenReadingSewerProfileCircleDefinition_ThenReturnDefaultCircleSewerProfileAndLogMessage()
        {
            var keys = new List<string> { SewerProfileMapping.PropertyKeys.SewerProfileId };
            var values = new List<string> { ProfileId };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => CreateCSDCircleShapeAndCheckProperties(element, 0.160d),
                "Sewer profile 'PRO1' is missing its width. Default profile property values are used for this profile.");
        }

        private static void CreateCSDCircleShapeAndCheckProperties(GwswElement element, double diameter)
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
            var keys = new List<string> { SewerProfileMapping.PropertyKeys.SewerProfileWidth, SewerProfileMapping.PropertyKeys.SewerProfileHeight };
            var values = new List<string> { "2000", "1200" };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            CreateCSDShapeAndCheckProperties<CsdRectangleDefinitionReader, CrossSectionStandardShapeRectangle>(element, 2.0, 1.2);
        }

        [TestCase(SewerProfileMapping.PropertyKeys.SewerProfileWidth)]
        [TestCase(SewerProfileMapping.PropertyKeys.SewerProfileHeight)]
        [TestCase("fakeKey")]
        public void GivenSewerProfileGwswElementWithoutHeightOrWidthDefined_WhenReadingSewerProfileRectangleDefinition_ThenReturnDefaultRectangleSewerProfileAndLogMessage(string key)
        {
            var keys = new List<string> { key, SewerProfileMapping.PropertyKeys.SewerProfileId };
            var values = new List<string> { "2000", ProfileId };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => CreateCSDShapeAndCheckProperties<CsdRectangleDefinitionReader, CrossSectionStandardShapeRectangle>(element, 1.0, 1.0),
                "Sewer profile 'PRO1' is missing its width and/or height. Default profile property values are used for this profile.");
        }

        #endregion

        #region Egg shape cross section
        
        [Test]
        public void GivenGwswElement_WhenReadingSewerProfileEggDefinition_ThenReturnEggShapeWithCorrectPropertyValues()
        {
            var keys = new List<string> { SewerProfileMapping.PropertyKeys.SewerProfileWidth };
            var values = new List<string> { "250" };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            CreateCSDShapeAndCheckProperties<CsdEggDefinitionReader, CrossSectionStandardShapeEgg>(element, 0.25, 0.375);
        }

        [Test]
        public void GivenSewerProfileGwswElementWithoutWidthDefined_WhenReadingSewerProfileEggDefinition_ThenReturnDefaultEggShapeAndLogMessage()
        {
            var keys = new List<string> { SewerProfileMapping.PropertyKeys.SewerProfileId };
            var values = new List<string> { ProfileId };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => CreateCSDShapeAndCheckProperties<CsdEggDefinitionReader, CrossSectionStandardShapeEgg>(element, 2.0, 3.0),
                "Sewer profile 'PRO1' is missing its width. Default profile property values are used for this profile.");
        }

        [Test]
        public void GivenGwswElementWithWidthAndHeightInCorrectProportion_WhenReadingSewerProfileEggDefinition_ThenReturnEggShapeWithCorrectPropertyValues()
        {
            var keys = new List<string> { SewerProfileMapping.PropertyKeys.SewerProfileWidth, SewerProfileMapping.PropertyKeys.SewerProfileHeight };
            var values = new List<string> { "250", "375" };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            CreateCSDShapeAndCheckProperties<CsdEggDefinitionReader, CrossSectionStandardShapeEgg>(element, 0.25, 0.375);
        }

        [Test]
        public void GivenSewerProfileGwswElementWithWidthAndHeightNotInCorrectProportion_WhenReadingSewerProfileEggDefinition_ThenLogMessageIsGivenToUser()
        {
            var keys = new List<string>
            {
                SewerProfileMapping.PropertyKeys.SewerProfileWidth,
                SewerProfileMapping.PropertyKeys.SewerProfileHeight,
                SewerProfileMapping.PropertyKeys.SewerProfileId
            };
            var values = new List<string> { "250", "400", "PRO1" };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));

            var reader = new CsdEggDefinitionReader();
            TestHelper.AssertAtLeastOneLogMessagesContains(() => reader.ReadSewerProfileDefinition(element), 
                "The width and height of sewer profile 'PRO1' are not in the right proportion (2:3). Width is now 250 mm and height is now 375 mm.");
        }

        [Test]
        public void GivenSewerProfileGwswElementWithWidthAndHeightNotInCorrectProportion_WhenReadingSewerProfileEggDefinition_ThenHeightFromGwswElementIsIgnored()
        {
            var keys = new List<string>
            {
                SewerProfileMapping.PropertyKeys.SewerProfileWidth,
                SewerProfileMapping.PropertyKeys.SewerProfileHeight,
                SewerProfileMapping.PropertyKeys.SewerProfileId
            };
            var values = new List<string> {"250", "400", "PRO1"};
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));

            var reader = new CsdEggDefinitionReader();
            var csDefinition = reader.ReadSewerProfileDefinition(element) as CrossSectionDefinitionStandard;

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
            var keys = new List<string> { SewerProfileMapping.PropertyKeys.SewerProfileWidth };
            var values = new List<string> { "2000" };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            CreateCSDShapeAndCheckProperties<CsdCunetteDefinitionReader, CrossSectionStandardShapeCunette>(element, 2.0, 1.268);
        }

        [Test]
        public void GivenGwswElementWithWidthAndHeightInCorrectProportion_WhenReadingSewerProfileCunetteDefinition_ThenReturnCunetteShapeWithCorrectPropertyValues()
        {
            var keys = new List<string> { SewerProfileMapping.PropertyKeys.SewerProfileWidth, SewerProfileMapping.PropertyKeys.SewerProfileHeight };
            var values = new List<string> { "2000", "1268" };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            CreateCSDShapeAndCheckProperties<CsdCunetteDefinitionReader, CrossSectionStandardShapeCunette>(element, 2.0, 1.268);
        }

        [Test]
        public void GivenSewerProfileGwswElementWithoutWidthDefined_WhenReadingSewerProfileCunetteDefinition_ThenReturnDefaultCunetteShapeAndLogMessage()
        {
            var keys = new List<string> { SewerProfileMapping.PropertyKeys.SewerProfileId };
            var values = new List<string> { ProfileId };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => CreateCSDShapeAndCheckProperties<CsdCunetteDefinitionReader, CrossSectionStandardShapeCunette>(element, 1.0, 0.634),
                "Sewer profile 'PRO1' is missing its width. Default profile property values are used for this profile.");
        }

        [Test]
        public void GivenSewerProfileGwswElementWithWidthAndHeightNotInCorrectProportion_WhenReadingSewerProfileCunetteDefinition_ThenLogMessageIsGivenToUser()
        {
            var keys = new List<string>
            {
                SewerProfileMapping.PropertyKeys.SewerProfileWidth,
                SewerProfileMapping.PropertyKeys.SewerProfileHeight,
                SewerProfileMapping.PropertyKeys.SewerProfileId
            };
            var values = new List<string> { "250", "400", "PRO1" };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));

            var reader = new CsdCunetteDefinitionReader();
            TestHelper.AssertAtLeastOneLogMessagesContains(() => reader.ReadSewerProfileDefinition(element),
                "The width and height of sewer profile 'PRO1' are not in the right proportion (1:0.634). Width is now 250 mm and height is now 158.5 mm.");
        }

        [Test]
        public void GivenSewerProfileGwswElementWithWidthAndHeightNotInCorrectProportion_WhenReadingSewerProfileCunetteDefinition_ThenHeightFromGwswElementIsIgnored()
        {
            var keys = new List<string>
            {
                SewerProfileMapping.PropertyKeys.SewerProfileWidth,
                SewerProfileMapping.PropertyKeys.SewerProfileHeight,
                SewerProfileMapping.PropertyKeys.SewerProfileId
            };
            var values = new List<string> { "250", "400", "PRO1" };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));

            var reader = new CsdCunetteDefinitionReader();
            var csDefinition = reader.ReadSewerProfileDefinition(element) as CrossSectionDefinitionStandard;

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
            var keys = new List<string> { SewerProfileMapping.PropertyKeys.SewerProfileWidth, SewerProfileMapping.PropertyKeys.SewerProfileHeight };
            var values = new List<string> { "1200", "2500" };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            CreateCSDArchShapeAndCheckProperties(element, 1.2, 2.5, 2.5);
        }

        [TestCase(SewerProfileMapping.PropertyKeys.SewerProfileWidth)]
        [TestCase(SewerProfileMapping.PropertyKeys.SewerProfileHeight)]
        public void GivenGwswElementWithMissingValues_WhenReadingSewerProfileArchDefinition_ThenReturnDefaultArchShapeAndLogMessage(string key)
        {
            var keys = new List<string> { key, SewerProfileMapping.PropertyKeys.SewerProfileId };
            var values = new List<string> { "1000", ProfileId };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => CreateCSDArchShapeAndCheckProperties(element, 1.0, 2.0, 1.0),
                "Sewer profile 'PRO1' is missing its width and/or height. Default profile property values are used for this profile.");
        }

        private static void CreateCSDArchShapeAndCheckProperties(GwswElement element, double expectedWidth, double expectedHeight, double expectedArcHeight)
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
            var keys = new List<string>
            {
                SewerProfileMapping.PropertyKeys.SewerProfileWidth,
                SewerProfileMapping.PropertyKeys.SewerProfileHeight,
                SewerProfileMapping.PropertyKeys.Slope1,
                SewerProfileMapping.PropertyKeys.Slope2
            };
            var values = new List<string> { "1000", "1000", slope1, slope2 };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            CreateCSDTrapezoidShapeAndCheckProperties(element, 1.0, 2.0, 2.0);
        }

        [TestCase(SewerProfileMapping.PropertyKeys.Slope1)]
        [TestCase(SewerProfileMapping.PropertyKeys.Slope2)]
        public void GivenGwswElementWithOneMissingSlope_WhenReadingSewerProfileTrapezoidDefinition_ThenReturnTrapezoidWithPresentSlope(string slopeKey)
        {
            var keys = new List<string>
            {
                SewerProfileMapping.PropertyKeys.SewerProfileWidth,
                SewerProfileMapping.PropertyKeys.SewerProfileHeight,
                slopeKey
            };
            var values = new List<string> { "1000", "1000", "2" };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            CreateCSDTrapezoidShapeAndCheckProperties(element, 1.0, 2.0, 2.0);
        }

        [Test]
        public void GivenSewerProfileGwswElementWithMissingWidth_WhenReadingSewerProfileTrapezoidDefinition_ThenReturnDefaultTrapezoidSewerProfileAndLogMessage()
        {
            var keys = new List<string>
            {
                SewerProfileMapping.PropertyKeys.SewerProfileId,
                SewerProfileMapping.PropertyKeys.SewerProfileHeight,
                SewerProfileMapping.PropertyKeys.Slope1,
                SewerProfileMapping.PropertyKeys.Slope2
            };
            var values = new List<string> { ProfileId, "1000", "2,5", "1,5" };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => CreateCSDTrapezoidShapeAndCheckProperties(element, 10.0, 2.0, 20.0),
                "Sewer profile 'PRO1' is missing its width, height and/or slope. Default profile property values are used for this profile.");
        }

        [Test]
        public void GivenSewerProfileGwswElementWithMissingHeight_WhenReadingSewerProfileTrapezoidDefinition_ThenReturnDefaultTrapezoidSewerProfile()
        {
            var keys = new List<string>
            {
                SewerProfileMapping.PropertyKeys.SewerProfileId,
                SewerProfileMapping.PropertyKeys.SewerProfileWidth,
                SewerProfileMapping.PropertyKeys.Slope1,
                SewerProfileMapping.PropertyKeys.Slope2
            };
            var values = new List<string> { ProfileId, "1000", "2,5", "1,5" };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => CreateCSDTrapezoidShapeAndCheckProperties(element, 10.0, 2.0, 20.0),
                "Sewer profile 'PRO1' is missing its width, height and/or slope. Default profile property values are used for this profile.");
        }

        [Test]
        public void GivenSewerProfileGwswElementWithMissingSlopes_WhenReadingSewerProfileTrapezoidDefinition_ThenReturnDefaultTrapezoidSewerProfile()
        {
            var keys = new List<string>
            {
                SewerProfileMapping.PropertyKeys.SewerProfileId,
                SewerProfileMapping.PropertyKeys.SewerProfileWidth,
                SewerProfileMapping.PropertyKeys.SewerProfileHeight
            };
            var values = new List<string> { ProfileId, "1000", "1000" };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => CreateCSDTrapezoidShapeAndCheckProperties(element, 10.0, 2.0, 20.0),
                "Sewer profile 'PRO1' is missing its width, height and/or slope. Default profile property values are used for this profile.");
        }

        private static void CreateCSDTrapezoidShapeAndCheckProperties(GwswElement element, double expectedBottomWidth, double expectedSlope, double expectedMaxFlowWidth)
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
        private GwswElement GetGwswElement(SewerFeatureType sewerFeatureType, Dictionary<string, string> keyValuePairs)
        {
            var element = new GwswElement
            {
                ElementTypeName = sewerFeatureType.ToString()
            };

            foreach (var keyValuePair in keyValuePairs)
            {
                element.GwswAttributeList.Add(new GwswAttribute
                {
                    ValueAsString = keyValuePair.Value,
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                        keyValuePair.Key, "MyDescription", null, null)
                });
            }

            return element;
        }

        private Dictionary<string, string> GetGwswKeyValuePairs(List<string> keys, List<string> values)
        {
            Assert.That(keys.Count, Is.EqualTo(values.Count));
            var keyValues = new Dictionary<string, string>();
            for (var i = 0; i < keys.Count; i++) keyValues.Add(keys[i], values[i]);
            return keyValues;
        }

        private static void CreateCSDShapeAndCheckProperties<TReader, TShape>(GwswElement gwswElement, double expectedWidth, double expectedHeight) 
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

        [TestCase("2", 2.0)]
        [TestCase("2.0", 2.0)]
        [TestCase("2,0", 2.0)]
        [TestCase("-2,0", -2.0)]
        [TestCase("-99.3560", -99.356)]
        public void GivenCorrectStringValue_WhenTryingToGetDoubleValueFromDictionary_ThenRetrievalIsSuccessful(string stringValue, double expectedResult)
        {
            bool successfulRetrieval;
            var doubleValue = CreateDictionaryAndRetrieveDoubleValue(stringValue, out successfulRetrieval);
            Assert.True(successfulRetrieval);
            Assert.That(doubleValue, Is.EqualTo(expectedResult));
        }

        [TestCase("AB")]
        [TestCase("2.0.0")]
        [TestCase("2,0,0")]
        [TestCase("(2.5)")]
        public void GivenIncorrectStringValue_WhenTryingToGetDoubleValueFromDictionary_ThenRetrievalIsNotSuccessful(string stringValue)
        {
            bool successfulRetrieval;
            CreateDictionaryAndRetrieveDoubleValue(stringValue, out successfulRetrieval);
            Assert.False(successfulRetrieval);
        }

        [Test]
        public void GivenLogger_WhenInvokingMessageForMissingValues_ThenMessageIsLogged()
        {
            var missingValuesText = "key values";
            var dict = new Dictionary<string, string>
            {
                { SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId }
            };
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => Log.MessageForMissingValues(dict, missingValuesText),
                "Sewer profile 'PRO1' is missing its " + missingValuesText + ". Default profile property values are used for this profile.");
        }

        [Test]
        public void GivenLogger_WhenInvokingLogMessageInCaseSewerShapeWidthHeightAreNotInCorrectProportion_ThenMessageIsLogged()
        {
            var dict = new Dictionary<string, string>
            {
                { SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId },
                { SewerProfileMapping.PropertyKeys.SewerProfileHeight, "2500" }
            };
            var csShape = new CrossSectionStandardShapeEgg { Width = 2.0 };
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => Log.LogMessageInCaseSewerShapeWidthHeightAreNotInCorrectProportion(dict, 2000.0, 1.5, "(2:3)", csShape),
                "The width and height of sewer profile 'PRO1' are not in the right proportion (2:3). Width is now 2000 mm and height is now 3000 mm.");
        }

        private static double CreateDictionaryAndRetrieveDoubleValue(string stringValue, out bool successfulRetrieval)
        {
            var key = "myKey";
            var dictionary = new Dictionary<string, string>
            {
                {key, stringValue}
            };

            double doubleValue;
            successfulRetrieval = dictionary.TryGetDoubleValueFromDictionary(key, out doubleValue);
            return doubleValue;
        }

        #endregion
    }
}