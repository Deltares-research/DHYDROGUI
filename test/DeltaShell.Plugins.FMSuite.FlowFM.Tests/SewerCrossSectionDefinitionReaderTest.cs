using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class SewerCrossSectionDefinitionReaderTest
    {
        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
        }

        [TearDown]
        public void TearDown()
        {
            mocks.VerifyAll();
        }

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

        [Test]
        public void GivenGwswElement_WhenReadingCrossSectionDefinition_ThenReturnCorrectValue()
        {
            var element = GetGwswElement(SewerFeatureType.Crosssection);
            var circleReader = new CsdCircleDefinitionReader();
            var csDefinition = circleReader.ReadCrossSectionDefinition(element);

            Assert.That(csDefinition.Width, Is.EqualTo(1250.0));
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
    }
}