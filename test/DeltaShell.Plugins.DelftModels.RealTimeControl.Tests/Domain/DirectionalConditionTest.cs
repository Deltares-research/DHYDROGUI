using System.Xml.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Domain
{
    public class DirectionalConditionTest
    {
        private static readonly XNamespace Fns = "http://www.wldelft.nl/fews";

        private const string Implicit = "IMPLICIT";
        private const string Name = "Trigger31";
        private const string InputName = "AlarmREGEN";
        private const string InputParameterName = "DeadBandTime";
        private const string TrueReference = "REGEN-ORANGE";
        private const string TrueAndFalseReference = "Thunersee Messung";
        private const string FalseReference = "REGEN-ROT";

        private MockRepository mocks;
        private IControlGroup controlGroup;
        private PIDRule trueRule;
        private PIDRule falseRule;
        private PIDRule trueAndFalseRule;
        private DirectionalCondition directionalCondition;

        [SetUp]
        public void SetUp()
        {
            mocks = new MockRepository();
            controlGroup = mocks.StrictMock<IControlGroup>();
            trueRule = new PIDRule { Name = TrueReference };
            falseRule = new PIDRule { Name = FalseReference };
            trueAndFalseRule = new PIDRule { Name = TrueAndFalseReference };

            directionalCondition = new DirectionalCondition
            {
                Name = Name,
                Reference = Implicit,
                Operation = Operation.Greater,
                Input = new Input
                    {
                        ParameterName = InputParameterName,
                        Feature = new RtcTestFeature { Name = InputName }
                    },
            };
        }

        [Test]
        public void CheckXmlGeneration()
        {
            directionalCondition.TrueOutputs.Add(trueRule);
            directionalCondition.FalseOutputs.Add(falseRule);

            Assert.AreEqual(ExpectedXml(), directionalCondition.ToXml(Fns, "").ToString(SaveOptions.DisableFormatting));
        }

        private string ExpectedXml()
        {
            var seriesOne = RtcXmlTag.Input + InputName + "/" + InputParameterName;
            var previousTimeStep = seriesOne + DirectionalCondition.TimeLagPostFix;

            return
                "<trigger xmlns=\"" + Fns.ToString() + "\">" +
                "<standard id=\"" + "/" + directionalCondition.Name + "\">" +
                "<condition>" +
                "<x1Series ref=\"" + Implicit + "\">" + seriesOne + "</x1Series>" +
                "<relationalOperator>Greater</relationalOperator>" +
                "<x2Series ref=\"" + Implicit + "\">" + previousTimeStep + "</x2Series>" +
                "</condition>" +
                "<true>" +
                "<trigger><ruleReference>" + "/" + TrueReference + "</ruleReference></trigger>" +
                "</true>" +
                "<false>" +
                "<trigger><ruleReference>" + "/" + FalseReference + "</ruleReference></trigger>" +
                "</false>" +
                "<output>" +
                "<status>" + RtcXmlTag.DirectionalCondition + RtcXmlTag.Status + "/" +
                directionalCondition.Name + "</status>" +
                "</output>" +
                "</standard>" +
                "</trigger>";
        }
    }
}
