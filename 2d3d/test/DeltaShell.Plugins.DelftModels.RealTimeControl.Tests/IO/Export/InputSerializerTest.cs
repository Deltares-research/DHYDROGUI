using System.Xml.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO.Export
{
    [TestFixture]
    public class InputSerializerTest
    {
        private const string featureName = "InputName";
        private const string name = "ParameterName";
        private const string setPoint = "Test";
        private static readonly XNamespace fns = "http://www.wldelft.nl/fews";

        private Input input;

        [SetUp]
        public void SetUp()
        {
            input = new Input
            {
                ParameterName = name,
                Feature = new RtcTestFeature {Name = featureName},
                SetPoint = setPoint
            };
        }

        [Test]
        public void CheckXmlGeneration()
        {
            var serializer = new InputSerializer(input);

            Assert.AreEqual(OriginXml(),
                            serializer.ToXmlInputReference(fns, "x", "setpoint").ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void CheckXmlGenerationWithFilledValuesOnly()
        {
            input.SetPoint = string.Empty;

            var serializer = new InputSerializer(input);

            Assert.AreEqual(OriginXmlSingleVariableFilled(),
                            serializer.ToXmlInputReference(fns, "x").ToString(SaveOptions.DisableFormatting));
        }

        private static string OriginXml()
        {
            return "<input xmlns=\"http://www.wldelft.nl/fews\">" +
                   "<x>" + RtcXmlTag.Input + featureName + "/" + name + "</x>" +
                   "<setpoint>" + setPoint + "</setpoint>" + //Not sure what setpoint means yet ... Set by the pidRule!!
                   "</input>";
        }

        private static string OriginXmlSingleVariableFilled()
        {
            return "<input xmlns=\"http://www.wldelft.nl/fews\">" +
                   "<x>" + RtcXmlTag.Input + featureName + "/" + name + "</x>" +
                   "</input>";
        }
    }
}