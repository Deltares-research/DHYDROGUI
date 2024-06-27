using System.Globalization;
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
    public class StandardConditionSerializerTest
    {
        private const string @implicit = StandardCondition.ReferenceType.Implicit;
        private const string name = "Trigger31";
        private const string inputName = "AlarmREGEN";
        private const string inputParameterName = "DeadBandTime";
        private const double value = 1.5;
        private const string trueReference = "REGEN-ORANGE";
        private const string falseReference = "REGEN-ROT";
        private static readonly XNamespace fns = "http://www.wldelft.nl/fews";

        private PIDRule trueRule;
        private PIDRule falseRule;
        private StandardCondition standardCondition;

        [SetUp]
        public void SetUp()
        {
            trueRule = new PIDRule {Name = trueReference};
            falseRule = new PIDRule {Name = falseReference};

            standardCondition = new StandardCondition
            {
                Name = name,
                Reference = @implicit,
                Operation = Operation.Greater,
                Input =
                    new Input
                    {
                        ParameterName = inputParameterName,
                        Feature = new RtcTestFeature {Name = inputName}
                    },
                Value = value
            };
        }

        [Test]
        public void CheckXmlGeneration()
        {
            standardCondition.TrueOutputs.Add(trueRule);
            standardCondition.FalseOutputs.Add(falseRule);

            var serializer = new StandardConditionSerializer(standardCondition);

            Assert.AreEqual(OriginXml(), serializer.ToXml(fns, "").Single().ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void GivenStandardConditionWithoutConnectedObjects_WhenSerializedToXml_ThenExpectedXmlReturned()
        {
            var serializer = new StandardConditionSerializer(new StandardCondition());
            Assert.AreEqual(XmlWithoutConnectedObjects(), serializer.ToXml(fns, "").Single().ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void CheckXmlGenerationOtherBooleanCheck()
        {
            standardCondition.Operation = Operation.Less;

            standardCondition.TrueOutputs.Add(trueRule);
            standardCondition.FalseOutputs.Add(falseRule);

            var serializer = new StandardConditionSerializer(standardCondition);

            Assert.AreEqual(SecondaryConditionLessThanXml(),
                            serializer.ToXml(fns, "").Single().ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void GenerateXmlConditionToCondition()
        {
            var trueCondition = new StandardCondition
            {
                Name = "trueCondition",
                Reference = @implicit,
                Operation = Operation.Greater,
                Input =
                    new Input
                    {
                        ParameterName = inputParameterName,
                        Feature = new RtcTestFeature {Name = inputName}
                    },
                Value = value
            };

            var falseCondition = new StandardCondition
            {
                Name = "falseCondition",
                Reference = @implicit,
                Operation = Operation.Greater,
                Input =
                    new Input
                    {
                        ParameterName = inputParameterName,
                        Feature = new RtcTestFeature {Name = inputName}
                    },
                Value = value
            };

            standardCondition.TrueOutputs.Add(trueCondition);
            standardCondition.FalseOutputs.Add(falseCondition);

            var serializer = new StandardConditionSerializer(standardCondition);

            Assert.AreEqual(XmlConditionToCondition(),
                            serializer.ToXml(fns, "").Single().ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void ToXml_ShouldReturnConditionWithInlineExpressionDefinitionsIfDefined()
        {
            var condition = new StandardCondition();
            var inputCondition = new Input
            {
                ParameterName = "WaterLevel1",
                Feature = new RtcTestFeature {Name = "ObservationPoint1"}
            };
            condition.Input = inputCondition;

            var expression = new MathematicalExpression();
            var inputExpression = new Input
            {
                ParameterName = "WaterLevel2",
                Feature = new RtcTestFeature {Name = "ObservationPoint2"}
            };
            expression.Inputs.Add(inputExpression);
            expression.Expression = "A+1+2";
            expression.Name = "expression";

            var expression2 = new MathematicalExpression();
            var inputExpression2 = new Input
            {
                ParameterName = "WaterLevel3",
                Feature = new RtcTestFeature {Name = "ObservationPoint3"}
            };
            expression2.Inputs.Add(inputExpression2);
            expression2.Expression = "A+1+2";
            expression2.Name = "expression2";

            condition.TrueOutputs.Add(expression);
            condition.FalseOutputs.Add(expression2);

            var serializer = new StandardConditionSerializer(condition);
            var retrievedXml = serializer.ToXml(fns, "Group1/").Single().ToString(SaveOptions.DisableFormatting);

            string expectedXml =
                "<trigger xmlns=\"http://www.wldelft.nl/fews\">" +
                "<standard id=\"[StandardCondition]Group1/Standard Condition\">" +
                "<condition>" +
                "<x1Series ref=\"EXPLICIT\">[Input]ObservationPoint1/WaterLevel1</x1Series>" +
                "<relationalOperator>Equal</relationalOperator>" +
                "<x2Value>0</x2Value>" +
                "</condition>" +
                "<true>" +
                "<trigger>" +
                "<expression id=\"Group1/expression/([Input]ObservationPoint2/WaterLevel2 + 1)\">" +
                "<x1Series ref=\"IMPLICIT\">[Input]ObservationPoint2/WaterLevel2</x1Series>" +
                "<mathematicalOperator>+</mathematicalOperator>" +
                "<x2Value>1</x2Value>" +
                "<y>expression/([Input]ObservationPoint2/WaterLevel2 + 1)</y>" +
                "</expression>" +
                "</trigger>" +
                "<trigger>" +
                "<expression id=\"Group1/expression\">" +
                "<x1Series ref=\"IMPLICIT\">expression/([Input]ObservationPoint2/WaterLevel2 + 1)</x1Series>" +
                "<mathematicalOperator>+</mathematicalOperator>" +
                "<x2Value>2</x2Value>" +
                "<y>expression</y>" +
                "</expression>" +
                "</trigger>" +
                "</true>" +
                "<false>" +
                "<trigger>" +
                "<expression id=\"Group1/expression2/([Input]ObservationPoint3/WaterLevel3 + 1)\">" +
                "<x1Series ref=\"IMPLICIT\">[Input]ObservationPoint3/WaterLevel3</x1Series>" +
                "<mathematicalOperator>+</mathematicalOperator>" +
                "<x2Value>1</x2Value>" +
                "<y>expression2/([Input]ObservationPoint3/WaterLevel3 + 1)</y>" +
                "</expression>" +
                "</trigger>" +
                "<trigger>" +
                "<expression id=\"Group1/expression2\">" +
                "<x1Series ref=\"IMPLICIT\">expression2/([Input]ObservationPoint3/WaterLevel3 + 1)</x1Series>" +
                "<mathematicalOperator>+</mathematicalOperator>" +
                "<x2Value>2</x2Value>" +
                "<y>expression2</y>" +
                "</expression>" +
                "</trigger>" +
                "</false>" +
                "<output>" +
                "<status>[Status]Group1/Standard Condition" + "</status>" +
                "</output>" +
                "</standard>" +
                "</trigger>";

            Assert.AreEqual(expectedXml, retrievedXml);
        }

        private string XmlWithoutConnectedObjects()
        {
            return "<trigger xmlns=\"http://www.wldelft.nl/fews\">" +
                   "<standard id=\"[StandardCondition]Standard Condition\">" +
                   "<condition>" +
                   "<x1Series ref=\"EXPLICIT\">|no input|</x1Series>" +
                   "<relationalOperator>Equal</relationalOperator>" +
                   "<x2Value>0</x2Value>" +
                   "</condition>" +
                   "<output>" +
                   "<status>" + RtcXmlTag.Status + "Standard Condition</status>" +
                   "</output>" +
                   "</standard>" +
                   "</trigger>";
        }

        private string OriginXml()
        {
            return "<trigger xmlns=\"http://www.wldelft.nl/fews\">" +
                   "<standard id=\"[StandardCondition]" + standardCondition.Name + "\">" +
                   "<condition>" +
                   "<x1Series ref=\"" + @implicit + "\">" + RtcXmlTag.Input + inputName + "/" + inputParameterName +
                   "</x1Series>" +
                   "<relationalOperator>Greater</relationalOperator>" +
                   "<x2Value>" + value.ToString(CultureInfo.InvariantCulture) + "</x2Value>" +
                   "</condition>" +
                   "<true>" +
                   "<trigger><ruleReference>[PID]" + trueReference + "</ruleReference></trigger>" +
                   "</true>" +
                   "<false>" +
                   "<trigger><ruleReference>[PID]" + falseReference + "</ruleReference></trigger>" +
                   "</false>" +
                   "<output>" +
                   "<status>" + RtcXmlTag.Status + standardCondition.Name + "</status>" +
                   "</output>" +
                   "</standard>" +
                   "</trigger>";
        }

        private string SecondaryConditionLessThanXml()
        {
            return "<trigger xmlns=\"http://www.wldelft.nl/fews\">" +
                   "<standard id=\"[StandardCondition]" + standardCondition.Name + "\">" +
                   "<condition>" +
                   "<x1Series ref=\"" + @implicit + "\">" + RtcXmlTag.Input + inputName + "/" + inputParameterName +
                   "</x1Series>" +
                   "<relationalOperator>Less</relationalOperator>" +
                   "<x2Value>" + value.ToString(CultureInfo.InvariantCulture) + "</x2Value>" +
                   "</condition>" +
                   "<true>" +
                   "<trigger><ruleReference>[PID]" + trueReference + "</ruleReference></trigger>" +
                   "</true>" +
                   "<false>" +
                   "<trigger><ruleReference>[PID]" + falseReference + "</ruleReference></trigger>" +
                   "</false>" +
                   "<output>" +
                   "<status>" + RtcXmlTag.Status + standardCondition.Name + "</status>" +
                   "</output>" +
                   "</standard>" +
                   "</trigger>";
        }

        private string XmlConditionToCondition()
        {
            return "<trigger xmlns=\"http://www.wldelft.nl/fews\">" +
                   "<standard id=\"[StandardCondition]" + standardCondition.Name + "\">" +
                   "<condition>" +
                   "<x1Series ref=\"" + @implicit + "\">" + RtcXmlTag.Input + inputName + "/" + inputParameterName +
                   "</x1Series>" +
                   "<relationalOperator>Greater</relationalOperator>" +
                   "<x2Value>" + value.ToString(CultureInfo.InvariantCulture) + "</x2Value>" +
                   "</condition>" +
                   "<true>" +
                   "<trigger>" +
                   "<standard id=\"[StandardCondition]trueCondition\">" +
                   "<condition>" +
                   "<x1Series ref=\"" + @implicit + "\">" + RtcXmlTag.Input + inputName + "/" + inputParameterName +
                   "</x1Series>" +
                   "<relationalOperator>Greater</relationalOperator>" +
                   "<x2Value>" + value.ToString(CultureInfo.InvariantCulture) + "</x2Value>" +
                   "</condition>" +
                   "<output>" +
                   "<status>" + RtcXmlTag.Status + "trueCondition</status>" +
                   "</output>" +
                   "</standard>" +
                   "</trigger>" +
                   "</true>" +
                   "<false>" +
                   "<trigger>" +
                   "<standard id=\"[StandardCondition]falseCondition\">" +
                   "<condition>" +
                   "<x1Series ref=\"" + @implicit + "\">" + RtcXmlTag.Input + inputName + "/" + inputParameterName +
                   "</x1Series>" +
                   "<relationalOperator>Greater</relationalOperator>" +
                   "<x2Value>" + value.ToString(CultureInfo.InvariantCulture) + "</x2Value>" +
                   "</condition>" +
                   "<output>" +
                   "<status>" + RtcXmlTag.Status + "falseCondition</status>" +
                   "</output>" +
                   "</standard>" +
                   "</trigger>" +
                   "</false>" +
                   "<output>" +
                   "<status>" + RtcXmlTag.Status + standardCondition.Name +
                   "</status>" +
                   "</output>" +
                   "</standard>" +
                   "</trigger>";
        }
    }
}