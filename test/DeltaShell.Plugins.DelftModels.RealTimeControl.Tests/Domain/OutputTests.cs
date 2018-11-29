using System.Xml.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Domain
{
    [TestFixture]
    public class OutputTests
    {
        private static readonly XNamespace Fns = "http://www.wldelft.nl/fews";

        private const string elementName = "Weir";
        private const string parameterName = "CrestLevel";
        private const string IntegralPart = "integralPart";
        private const string HsRule1 = "H-2030_S.rule1";

        private Output output;
       
        [SetUp]
        public void SetUp()
        {
            output = new Output 
            {
                ParameterName = parameterName,
                Feature = new RtcTestFeature { Name = elementName },
                IntegralPart = IntegralPart
            };
        }

        [Test]
        public void CheckXmlGeneration()
        {
            Assert.AreEqual(OriginXml(), output.ToXml(Fns, "y", "integralPart").ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void CheckXmlGenerationWithFilledValuesOnly()
        {
            output.IntegralPart = string.Empty;
            Assert.AreEqual(OriginXmlSingleVariableFilled(), output.ToXml(Fns, "y", "integralPart").ToString(SaveOptions.DisableFormatting));
        }

        private static string OriginXml()
        {
            return "<output xmlns=\"http://www.wldelft.nl/fews\">" +
                   "<y>" + RtcXmlTag.Output + elementName + "/" + parameterName + "</y>" +
                   "<integralPart>" + IntegralPart + "</integralPart>" +
                   //"<active>" + HsRule1 + "</active>" +
                   "</output>";
        }

        private static string OriginXmlSingleVariableFilled()
        {
            return "<output xmlns=\"http://www.wldelft.nl/fews\">" +
                   "<y>" + RtcXmlTag.Output + elementName + "/" + parameterName + "</y>" +
                   "</output>";
        }
    
        [Test]
        public void CopyFromAndCreateClone()
        {
            var source = new Output()
            {
                ParameterName = parameterName,
                Feature = new RtcTestFeature { Name = elementName },
                IntegralPart = "test"

            };

            var newInput = new Output();
            newInput.CopyFrom(source);
            Assert.AreEqual(source.Name, newInput.Name);
            var clone = (Output)source.Clone();
            Assert.IsFalse(ReferenceEquals(source, clone));
            Assert.AreEqual(source.Name, clone.Name);
            Assert.IsTrue(ReferenceEquals(source.Feature, clone.Feature));
            Assert.AreEqual(source.IntegralPart, clone.IntegralPart);
        }
    }
}
