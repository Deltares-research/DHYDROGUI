using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.Properties;
using GeoAPI.Extensions.Feature;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Forms.Properties
{
    [TestFixture]
    public class RelativeTimeRulePropertiesTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowProperties()
        {
            WindowsFormsTestHelper.ShowPropertyGridForObject(new RelativeTimeRuleProperties {Data = new RelativeTimeRule()});
        }

        [TestCaseSource(nameof(GetTableTestCases))]
        public void GetTable_ReturnsCorrectResult(RelativeTimeRule rule, string expectedVariableName)
        {
            // Setup
            var properties = new RelativeTimeRuleProperties {Data = rule};

            // Call
            Function function = properties.Table;

            // Assert
            Assert.That(function, Is.SameAs(rule.Function));
            Assert.That(function.Arguments[0].Name, Is.EqualTo(expectedVariableName));
        }

        private static IEnumerable<TestCaseData> GetTableTestCases()
        {
            var ruleWithoutInput = new RelativeTimeRule();

            yield return new TestCaseData(ruleWithoutInput, "seconds");

            var ruleWithUnconnectedInput = new RelativeTimeRule();
            ruleWithUnconnectedInput.Inputs.Add(new Input());

            yield return new TestCaseData(ruleWithUnconnectedInput, "seconds");

            const string parameterName = "parameter_name";
            var connectedInput = new Input
            {
                Feature = Substitute.For<IFeature>(),
                ParameterName = parameterName
            };
            var ruleWithConnectedInput = new RelativeTimeRule();
            ruleWithConnectedInput.Inputs.Add(connectedInput);

            yield return new TestCaseData(ruleWithConnectedInput, $"{parameterName} [i]");

            const string expressionName = "expression_name";
            var ruleWithMathExpression = new RelativeTimeRule();
            ruleWithMathExpression.Inputs.Add(new MathematicalExpression {Name = expressionName});

            yield return new TestCaseData(ruleWithMathExpression, $"{expressionName} [i]");
        }
    }
}