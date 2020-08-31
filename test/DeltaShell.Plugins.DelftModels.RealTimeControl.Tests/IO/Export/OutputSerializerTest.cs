using System.Xml.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO.Export
{
    [TestFixture]
    public class OutputSerializerTest
    {
        private const string elementName = "Weir";
        private const string parameterName = "CrestLevel";
        private const string integralPart = "integralPart";
        private static readonly XNamespace fns = "http://www.wldelft.nl/fews";

        private Output output;

        [SetUp]
        public void SetUp()
        {
            output = new Output
            {
                ParameterName = parameterName,
                Feature = new RtcTestFeature {Name = elementName},
                IntegralPart = integralPart
            };
        }

        [Test]
        public void CheckXmlGeneration()
        {
            var serializer = new OutputSerializer(output);

            Assert.AreEqual(OriginXml(),
                            serializer.ToXmlOutputReference(fns, "y", "integralPart")
                                      .ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void CheckXmlGenerationWithFilledValuesOnly()
        {
            output.IntegralPart = string.Empty;

            var serializer = new OutputSerializer(output);

            Assert.AreEqual(OriginXmlSingleVariableFilled(),
                            serializer.ToXmlOutputReference(fns, "y", "integralPart")
                                      .ToString(SaveOptions.DisableFormatting));
        }

        private static string OriginXml()
        {
            return "<output xmlns=\"http://www.wldelft.nl/fews\">" +
                   "<y>" + RtcXmlTag.Output + elementName + "/" + parameterName + "</y>" +
                   "<integralPart>" + integralPart + "</integralPart>" +
                   //"<active>" + HsRule1 + "</active>" +
                   "</output>";
        }

        private static string OriginXmlSingleVariableFilled()
        {
            return "<output xmlns=\"http://www.wldelft.nl/fews\">" +
                   "<y>" + RtcXmlTag.Output + elementName + "/" + parameterName + "</y>" +
                   "</output>";
        }
    }
}