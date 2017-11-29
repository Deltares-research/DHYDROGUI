using System.Collections.Generic;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class SewerCrossSectionDefinitionReaderTest
    {
        [TestCase(SewerFeatureType.Node)]
        [TestCase(SewerFeatureType.Connection)]
        [TestCase(SewerFeatureType.Discharge)]
        [TestCase(SewerFeatureType.Distribution)]
        [TestCase(SewerFeatureType.Meta)]
        [TestCase(SewerFeatureType.Runoff)]
        [TestCase(SewerFeatureType.Structure)]
        [TestCase(SewerFeatureType.Surface)]
        public void GivenGwswElementWithElementTypeNameUnequalToCrossSection_WhenReadingCrossSectionDefinition_ThenReturnNull(SewerFeatureType type)
        {
            var element = GetGwswElement(type, GetGwswKeyValuePairs(new List<string>(), new List<string>()));
            CreateCSDShapeAndCheckForNull<CsdCircleDefinitionReader>(element);
            CreateCSDShapeAndCheckForNull<CsdRectangleDefinitionReader>(element);
            CreateCSDShapeAndCheckForNull<CsdEggDefinitionReader>(element);
            CreateCSDShapeAndCheckForNull<CsdTrapezoidDefinitionReader>(element);
            CreateCSDShapeAndCheckForNull<CsdHeulDefinitionReader>(element);
            CreateCSDShapeAndCheckForNull<CsdMuilDefinitionReader>(element);
        }

        private static void CreateCSDShapeAndCheckForNull<T>(GwswElement element) where T : SewerCrossSectionDefinitionReader, new()
        {
            var circleReader = new T();
            var csDefinition = circleReader.ReadCrossSectionDefinition(element);
            Assert.IsNull(csDefinition);
        }

        #region Circle shape cross section

        [Test]
        public void GivenGwswElement_WhenReadingCrossSectionCircleDefinition_ThenReturnCircleShapeWithCorrectPropertyValues()
        {
            var keys = new List<string> { CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionWidth };
            var values = new List<string> { "1250" };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            CreateCSDCircleShapeAndCheckProperties(element, 1.25);
        }

        [Test]
        public void GivenCrossSectionGwswElementWithoutWidthDefined_WhenReadingCrossSectionCircleDefinition_ThenReturnDefaultCircleCrossSection()
        {
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(new List<string>(), new List<string>()));
            CreateCSDCircleShapeAndCheckProperties(element, 0.160d);
        }

        private static void CreateCSDCircleShapeAndCheckProperties(GwswElement element, double diameter)
        {
            var circleReader = new CsdCircleDefinitionReader();

            var csDefinition = circleReader.ReadCrossSectionDefinition(element) as CrossSectionDefinitionStandard;
            Assert.NotNull(csDefinition);

            var csRoundShape = csDefinition.Shape as CrossSectionStandardShapeRound;
            Assert.NotNull(csRoundShape);
            Assert.That(csRoundShape.Diameter, Is.EqualTo(diameter));
        }

        #endregion

        #region Rectangle shape cross section

        [Test]
        public void GivenGwswElement_WhenReadingCrossSectionRectangleDefinition_ThenReturnRectangleShapeWithCorrectPropertyValues()
        {
            var keys = new List<string> { CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionWidth, CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionHeight };
            var values = new List<string> { "2000", "1200" };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            CreateCSDShapeAndCheckProperties<CsdRectangleDefinitionReader, CrossSectionStandardShapeRectangle>(element, 2.0, 1.2);
        }

        [TestCase(CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionWidth)]
        [TestCase(CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionHeight)]
        [TestCase("fakeKey")]
        public void GivenCrossSectionGwswElementWithoutHeightOrWidthDefined_WhenReadingCrossSectionRectangleDefinition_ThenReturnDefaultRectangleCrossSection(string key)
        {
            var keys = new List<string> { key };
            var values = new List<string> { "2000" };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            CreateCSDShapeAndCheckProperties<CsdRectangleDefinitionReader, CrossSectionStandardShapeRectangle>(element, 1.0, 1.0);
        }

        #endregion

        #region Egg shape cross section
        
        [Test]
        public void GivenGwswElement_WhenReadingCrossSectionEggDefinition_ThenReturnEggShapeWithCorrectPropertyValues()
        {
            var keys = new List<string> { CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionWidth };
            var values = new List<string> { "250" };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            CreateCSDShapeAndCheckProperties<CsdEggDefinitionReader, CrossSectionStandardShapeEgg>(element, 0.25, 0.375);
        }

        [Test]
        public void GivenCrossSectionGwswElementWithoutWidthDefined_WhenReadingCrossSectionEggDefinition_ThenReturnDefaultEggShape()
        {
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(new List<string>(), new List<string>()));
            CreateCSDShapeAndCheckProperties<CsdEggDefinitionReader, CrossSectionStandardShapeEgg>(element, 2.0, 3.0);
        }

        #endregion

        #region Cunette shape cross section

        [Test]
        public void GivenGwswElement_WhenReadingCrossSectionCunetteDefinition_ThenReturnCunetteShapeWithCorrectPropertyValues()
        {
            var keys = new List<string> { CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionWidth };
            var values = new List<string> { "2000" };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            CreateCSDShapeAndCheckProperties<CsdMuilDefinitionReader, CrossSectionStandardShapeCunette>(element, 2.0, 1.268);
        }

        [Test]
        public void GivenCrossSectionGwswElementWithoutWidthDefined_WhenReadingCrossSectionCunetteDefinition_ThenReturnDefaultCunetteShape()
        {
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(new List<string>(), new List<string>()));
            CreateCSDShapeAndCheckProperties<CsdMuilDefinitionReader, CrossSectionStandardShapeCunette>(element, 1.0, 0.634);
        }

        #endregion

        #region Arch shape cross section

        [Test]
        public void GivenGwswElement_WhenReadingCrossSectionArchDefinition_ThenReturnArchShapeWithCorrectPropertyValues()
        {
            var keys = new List<string> { CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionWidth, CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionHeight };
            var values = new List<string> { "1200", "2500" };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            CreateCSDArchShapeAndCheckProperties(element, 1.2, 2.5, 2.5);
        }

        [TestCase(CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionWidth)]
        [TestCase(CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionHeight)]
        public void GivenGwswElementWithMissingValues_WhenReadingCrossSectionArchDefinition_ThenReturnDefaultArchShape(string key)
        {
            var keys = new List<string> { key };
            var values = new List<string> { "1000" };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            CreateCSDArchShapeAndCheckProperties(element, 1.0, 2.0, 1.0);
        }

        private static void CreateCSDArchShapeAndCheckProperties(GwswElement element, double expectedWidth, double expectedHeight, double expectedArcHeight)
        {
            var archReader = new CsdHeulDefinitionReader();

            var csDefinition = archReader.ReadCrossSectionDefinition(element) as CrossSectionDefinitionStandard;
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
        public void GivenGwswElement_WhenReadingCrossSectionTrapezoidDefinition_ThenReturnTrapezoidShapeWithCorrectPropertyValues(string slope1, string slope2)
        {
            var keys = new List<string>
            {
                CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionWidth,
                CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionHeight,
                CrossSectionMapping.CrossSectionPropertyKeys.Slope1,
                CrossSectionMapping.CrossSectionPropertyKeys.Slope2
            };
            var values = new List<string> { "1000", "1000", slope1, slope2 };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            CreateCSDTrapezoidShapeAndCheckProperties(element, 1.0, 2.0, 2.0);
        }

        [TestCase(CrossSectionMapping.CrossSectionPropertyKeys.Slope1)]
        [TestCase(CrossSectionMapping.CrossSectionPropertyKeys.Slope2)]
        public void GivenGwswElementWithOneMissingSlope_WhenReadingCrossSectionTrapezoidDefinition_ThenReturnTrapezoidWithPresentSlope(string slopeKey)
        {
            var keys = new List<string>
            {
                CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionWidth,
                CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionHeight,
                slopeKey
            };
            var values = new List<string> { "1000", "1000", "2" };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            CreateCSDTrapezoidShapeAndCheckProperties(element, 1.0, 2.0, 2.0);
        }

        [Test]
        public void GivenCrossSectionGwswElementWithMissingWidth_WhenReadingCrossSectionTrapezoidDefinition_ThenReturnDefaultTrapezoidCrossSection()
        {
            var keys = new List<string>
            {
                CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionHeight,
                CrossSectionMapping.CrossSectionPropertyKeys.Slope1,
                CrossSectionMapping.CrossSectionPropertyKeys.Slope2
            };
            var values = new List<string> { "1000", "2,5", "1,5" };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            CreateCSDTrapezoidShapeAndCheckProperties(element, 10.0, 2.0, 20.0);
        }

        [Test]
        public void GivenCrossSectionGwswElementWithMissingHeight_WhenReadingCrossSectionTrapezoidDefinition_ThenReturnDefaultTrapezoidCrossSection()
        {
            var keys = new List<string>
            {
                CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionWidth,
                CrossSectionMapping.CrossSectionPropertyKeys.Slope1,
                CrossSectionMapping.CrossSectionPropertyKeys.Slope2
            };
            var values = new List<string> { "1000", "2,5", "1,5" };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            CreateCSDTrapezoidShapeAndCheckProperties(element, 10.0, 2.0, 20.0);
        }

        [Test]
        public void GivenCrossSectionGwswElementWithMissingSlopes_WhenReadingCrossSectionTrapezoidDefinition_ThenReturnDefaultTrapezoidCrossSection()
        {
            var keys = new List<string>
            {
                CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionWidth,
                CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionHeight
            };
            var values = new List<string> { "1000", "1000" };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            CreateCSDTrapezoidShapeAndCheckProperties(element, 10.0, 2.0, 20.0);
        }

        private static void CreateCSDTrapezoidShapeAndCheckProperties(GwswElement element, double expectedBottomWidth, double expectedSlope, double expectedMaxFlowWidth)
        {
            var reader = new CsdTrapezoidDefinitionReader();
            var csDefinition = reader.ReadCrossSectionDefinition(element) as CrossSectionDefinitionStandard;
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
            where TReader : SewerCrossSectionDefinitionReader, new() 
            where TShape : CrossSectionStandardShapeWidthHeightBase, new()
        {
            var reader = new TReader();
            var csDefinition = reader.ReadCrossSectionDefinition(gwswElement) as CrossSectionDefinitionStandard;
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