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
        }

        private static void CreateCSDShapeAndCheckForNull<T>(GwswElement element) where T : SewerCrossSectionDefinitionReader, new()
        {
            var circleReader = new T();
            var csDefinition = circleReader.ReadCrossSectionDefinition(element);
            Assert.IsNull(csDefinition);
        }

        #region Circle cross section

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

        #region Rectangle cross section

        [Test]
        public void GivenGwswElement_WhenReadingCrossSectionRectangleDefinition_ThenReturnCorrectValue()
        {
            var keys = new List<string> { CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionWidth, CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionHeight };
            var values = new List<string> { "2000", "1200" };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            CreateCSDRectangleShapeAndCheckProperties(element, 2.0, 1.2);
        }

        [TestCase(CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionWidth)]
        [TestCase(CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionHeight)]
        [TestCase("fakeKey")]
        public void GivenCrossSectionGwswElementWithoutHeightOrWidthDefined_WhenReadingCrossSectionRectangleDefinition_ThenReturnDefaultRectangleCrossSection(string key)
        {
            var keys = new List<string> { key };
            var values = new List<string> { "2000" };
            var element = GetGwswElement(SewerFeatureType.Crosssection, GetGwswKeyValuePairs(keys, values));
            CreateCSDRectangleShapeAndCheckProperties(element, 1.0, 1.0);
        }

        private static void CreateCSDRectangleShapeAndCheckProperties(GwswElement gwswElement, double width, double length)
        {
            var rectangleReader = new CsdRectangleDefinitionReader();
            var csDefinition = rectangleReader.ReadCrossSectionDefinition(gwswElement) as CrossSectionDefinitionStandard;
            Assert.NotNull(csDefinition);

            var csRectangleShape = csDefinition.Shape as CrossSectionStandardShapeRectangle;
            Assert.NotNull(csRectangleShape);
            Assert.That(csRectangleShape.Width, Is.EqualTo(width));
            Assert.That(csRectangleShape.Height, Is.EqualTo(length));
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

        #endregion
    }
}