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
        }

        private static void CreateCSDShapeAndCheckForNull<T>(GwswElement element) where T : SewerCrossSectionDefinitionReader, new()
        {
            var circleReader = new T();
            var csDefinition = circleReader.ReadCrossSectionDefinition(element);
            Assert.IsNull(csDefinition);
        }

        #region Circle shape cross section

        [Test]
        public void GivenGwswElement_WhenReadingCrossSectionCircleDefinition_ThenReturnCorrectValue()
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
        public void GivenGwswElement_WhenReadingCrossSectionRectangleDefinition_ThenReturnCorrectValue()
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
        public void GivenGwswElement_WhenReadingCrossSectionEggDefinition_ThenReturnCorrectValue()
        {
            var keys = new List<string> { CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionWidth };
            var values = new List<string> { "250" };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            CreateCSDShapeAndCheckProperties<CsdEggDefinitionReader, CrossSectionStandardShapeEgg>(element, 0.25, 0.375);
        }

        // TODO: Maybe check also when height is available and NOT width

        // Check for default shape when there is missing data
        [Test]
        public void GivenCrossSectionGwswElementWithoutWidthDefined_WhenReadingCrossSectionEggDefinition_ThenReturnDefaultRectangleCrossSection()
        {
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(new List<string>(), new List<string>()));
            CreateCSDShapeAndCheckProperties<CsdEggDefinitionReader, CrossSectionStandardShapeEgg>(element, 2.0, 3.0);
        }

        // 

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

        private static void CreateCSDShapeAndCheckProperties<TReader, TShape>(GwswElement gwswElement, double width, double length) 
            where TReader : SewerCrossSectionDefinitionReader, new() 
            where TShape : CrossSectionStandardShapeWidthHeightBase, new()
        {
            var rectangleReader = new TReader();
            var csDefinition = rectangleReader.ReadCrossSectionDefinition(gwswElement) as CrossSectionDefinitionStandard;
            Assert.NotNull(csDefinition);

            var csRectangleShape = csDefinition.Shape as TShape;
            Assert.NotNull(csRectangleShape);
            Assert.That(csRectangleShape.Width, Is.EqualTo(width));
            Assert.That(csRectangleShape.Height, Is.EqualTo(length));
        }

        #endregion
    }
}