using System.Linq;
using System.Xml.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Domain
{
    public class DirectionalConditionTest
    {
        private const string Implicit = StandardCondition.ReferenceType.Implicit;
        private const string Name = "Trigger31";
        private const string InputName = "AlarmREGEN";
        private const string InputParameterName = "DeadBandTime";
        private const string TrueReference = "REGEN-ORANGE";
        private const string FalseReference = "REGEN-ROT";
        private static readonly XNamespace Fns = "http://www.wldelft.nl/fews";

        private PIDRule trueRule;
        private PIDRule falseRule;
        private DirectionalCondition directionalCondition;

        [SetUp]
        public void SetUp()
        {
            trueRule = new PIDRule {Name = TrueReference};
            falseRule = new PIDRule {Name = FalseReference};

            directionalCondition = new DirectionalCondition
            {
                Name = Name,
                Reference = Implicit,
                Operation = Operation.Greater,
                Input = new Input
                {
                    ParameterName = InputParameterName,
                    Feature = new RtcTestFeature {Name = InputName}
                }
            };
        }

        [Test]
        public void CheckXmlGeneration()
        {
            directionalCondition.TrueOutputs.Add(trueRule);
            directionalCondition.FalseOutputs.Add(falseRule);

            var serializer = new DirectionalConditionSerializer(directionalCondition);
            Assert.AreEqual(ExpectedXml(), serializer.ToXml(Fns, "").First().ToString(SaveOptions.DisableFormatting));
        }

        private string ExpectedXml()
        {
            string seriesOne = RtcXmlTag.Input + InputName + "/" + InputParameterName;
            string previousTimeStep = seriesOne + "-1";

            return
                "<trigger xmlns=\"" + Fns.ToString() + "\">" +
                "<standard id=\"" + "[DirectionalCondition]" + directionalCondition.Name + "\">" +
                "<condition>" +
                "<x1Series ref=\"" + Implicit + "\">" + seriesOne + "</x1Series>" +
                "<relationalOperator>Greater</relationalOperator>" +
                "<x2Series ref=\"" + Implicit + "\">" + previousTimeStep + "</x2Series>" +
                "</condition>" +
                "<true>" +
                "<trigger><ruleReference>" + "[PID]" + TrueReference + "</ruleReference></trigger>" +
                "</true>" +
                "<false>" +
                "<trigger><ruleReference>" + "[PID]" + FalseReference + "</ruleReference></trigger>" +
                "</false>" +
                "<output>" +
                "<status>" + RtcXmlTag.Status + directionalCondition.Name + "</status>" +
                "</output>" +
                "</standard>" +
                "</trigger>";
        }
    }
}