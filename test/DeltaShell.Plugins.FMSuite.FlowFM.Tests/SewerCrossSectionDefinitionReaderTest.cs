using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class SewerCrossSectionDefinitionReaderTest
    {
        #region Circle cross section

        [Test]
        public void GivenGwswElement_WhenReadingCrossSectionCircleDefinition_ThenReturnCorrectValue()
        {
            var element = GetGwswElement(SewerFeatureType.Crosssection);
            var circleReader = new CsdCircleDefinitionReader();
            var csDefinition = circleReader.ReadCrossSectionDefinition(element);

            Assert.That(csDefinition.Width, Is.EqualTo(1.25));
        }

        [Test]
        public void GivenCrossSectionGwswElementWithoutWidthDefined_WhenReadingCrossSectionCircleDefinition_ThenReturnDefaultCircleCrossSection()
        {
            var element = GetGwswElement(SewerFeatureType.Crosssection);
            element.GwswAttributeList.Clear();
            var circleReader = new CsdCircleDefinitionReader();
            var csDefinition = circleReader.ReadCrossSectionDefinition(element);

            Assert.That(csDefinition.Width, Is.EqualTo(0.160d));
        }

        [TestCase(SewerFeatureType.Node)]
        [TestCase(SewerFeatureType.Connection)]
        [TestCase(SewerFeatureType.Discharge)]
        [TestCase(SewerFeatureType.Distribution)]
        [TestCase(SewerFeatureType.Meta)]
        [TestCase(SewerFeatureType.Runoff)]
        [TestCase(SewerFeatureType.Structure)]
        [TestCase(SewerFeatureType.Surface)]
        public void GivenGwswElementWithElementTypeNameUnequelToCrossSection_WhenReadingCrossSectionDefinition_ThenReturnNull(SewerFeatureType type)
        {
            var element = GetGwswElement(type);
            var circleReader = new CsdCircleDefinitionReader();
            var csDefinition = circleReader.ReadCrossSectionDefinition(element);

            Assert.IsNull(csDefinition);
        }

        #endregion

        #region Test helpers
        private GwswElement GetGwswElement(SewerFeatureType sewerFeatureType)
        {
            return new GwswElement
            {
                ElementTypeName = sewerFeatureType.ToString(),
                GwswAttributeList =
                {
                    new GwswAttribute
                    {
                        ValueAsString = "1250",
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionWidth, "MyDescription", null, null)
                    }
                }
            };
        }
        
        #endregion
    }
}