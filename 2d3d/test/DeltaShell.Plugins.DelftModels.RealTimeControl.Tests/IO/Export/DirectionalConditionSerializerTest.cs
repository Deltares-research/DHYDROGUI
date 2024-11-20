using System.Linq;
using System.Xml.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO.Export
{
    [TestFixture]
    public class DirectionalConditionSerializerTest
    {
        private const string @implicit = StandardCondition.ReferenceType.Implicit;
        private const string name = "Trigger31";
        private const string inputName = "AlarmREGEN";
        private const string inputParameterName = "DeadBandTime";
        private const string trueReference = "REGEN-ORANGE";
        private const string falseReference = "REGEN-ROT";
        private static readonly XNamespace fns = "http://www.wldelft.nl/fews";

        private PIDRule trueRule;
        private PIDRule falseRule;
        private DirectionalCondition directionalCondition;

        [SetUp]
        public void SetUp()
        {
            trueRule = new PIDRule {Name = trueReference};
            falseRule = new PIDRule {Name = falseReference};

            directionalCondition = new DirectionalCondition
            {
                Name = name,
                Reference = @implicit,
                Operation = Operation.Greater,
                Input = new Input
                {
                    ParameterName = inputParameterName,
                    Feature = new RtcTestFeature {Name = inputName}
                }
            };
        }

        [Test]
        public void CheckXmlGeneration()
        {
            directionalCondition.TrueOutputs.Add(trueRule);
            directionalCondition.FalseOutputs.Add(falseRule);

            var directionalConditionSerializer = new DirectionalConditionSerializer(directionalCondition);
            Assert.AreEqual(ExpectedXml(),
                            directionalConditionSerializer.ToXml(fns, "").Single().ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void GivenStandardConditionWithoutConnectedObjects_WhenSerializedToXml_ThenExpectedXmlReturned()
        {
            var serializer = new DirectionalConditionSerializer(new DirectionalCondition());
            Assert.AreEqual(XmlWithoutConnectedObjects(), serializer.ToXml(fns, "").Single().ToString(SaveOptions.DisableFormatting));
        }

        private string XmlWithoutConnectedObjects()
        {
            return "<trigger xmlns=\"http://www.wldelft.nl/fews\">" +
                   "<standard id=\"[DirectionalCondition]Standard Differential Condition\">" +
                   "<condition>" +
                   "<x1Series ref=\"EXPLICIT\">|no input|</x1Series>" +
                   "<relationalOperator>Equal</relationalOperator>" +
                   "<x2Series ref=\"EXPLICIT\">|no input|-1</x2Series>" +
                   "</condition>" +
                   "<output>" +
                   "<status>" + RtcXmlTag.Status + "Standard Differential Condition</status>" +
                   "</output>" +
                   "</standard>" +
                   "</trigger>";
        }

        private string ExpectedXml()
        {
            const string seriesOne = RtcXmlTag.Input + inputName + "/" + inputParameterName;
            const string previousTimeStep = seriesOne + "-1";

            return
                "<trigger xmlns=\"" + fns + "\">" +
                "<standard id=\"" + "[DirectionalCondition]" + directionalCondition.Name + "\">" +
                "<condition>" +
                "<x1Series ref=\"" + @implicit + "\">" + seriesOne + "</x1Series>" +
                "<relationalOperator>Greater</relationalOperator>" +
                "<x2Series ref=\"" + @implicit + "\">" + previousTimeStep + "</x2Series>" +
                "</condition>" +
                "<true>" +
                "<trigger><ruleReference>" + "[PID]" + trueReference + "</ruleReference></trigger>" +
                "</true>" +
                "<false>" +
                "<trigger><ruleReference>" + "[PID]" + falseReference + "</ruleReference></trigger>" +
                "</false>" +
                "<output>" +
                "<status>" + RtcXmlTag.Status + directionalCondition.Name + "</status>" +
                "</output>" +
                "</standard>" +
                "</trigger>";
        }
    }
}