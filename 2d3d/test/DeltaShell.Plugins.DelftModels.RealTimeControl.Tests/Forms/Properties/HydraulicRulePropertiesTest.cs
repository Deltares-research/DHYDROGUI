using System.Collections.Generic;
using System.Reflection;
using DelftTools.Functions;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.Properties;
using GeoAPI.Extensions.Feature;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Forms.Properties
{
    [TestFixture]
    public class HydraulicRulePropertiesTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowProperties()
        {
            WindowsFormsTestHelper.ShowPropertyGridForObject(new HydraulicRuleProperties {Data = new HydraulicRule()});
        }

        [Test]
        [SetCulture("en-US")]
        public void ResourcesCategoryAttributeOfInterpolationProperty_ShouldHaveCorrectText()
        {
            string interpolationPropertyName = nameof(HydraulicRuleProperties.Interpolation);
            PropertyInfo interpolationPropertyInfo = TypeUtils.GetPropertyInfo(typeof(HydraulicRuleProperties),
                                                                               interpolationPropertyName);

            string categoryPropertyName = nameof(ResourcesCategoryAttribute.Category);
            Assert.That(interpolationPropertyInfo,
                        Has.Attribute<ResourcesCategoryAttribute>()
                           .Property(categoryPropertyName)
                           .EqualTo("Interpolation"));
        }

        [TestCaseSource(nameof(GetTableTestCases))]
        public void GetTable_ReturnsCorrectResult(HydraulicRule rule, string expectedVariableName)
        {
            // Setup
            var properties = new HydraulicRuleProperties {Data = rule};

            // Call
            Function function = properties.Table;

            // Assert
            Assert.That(function, Is.SameAs(rule.Function));
            Assert.That(function.Arguments[0].Name, Is.EqualTo(expectedVariableName));
        }

        private static IEnumerable<TestCaseData> GetTableTestCases()
        {
            var ruleWithoutInput = new HydraulicRule();

            yield return new TestCaseData(ruleWithoutInput, "<input undefined>");

            var ruleWithUnconnectedInput = new HydraulicRule();
            ruleWithUnconnectedInput.Inputs.Add(new Input());

            yield return new TestCaseData(ruleWithUnconnectedInput, "<input undefined>");

            const string parameterName = "parameter_name";
            var connectedInput = new Input
            {
                Feature = Substitute.For<IFeature>(),
                ParameterName = parameterName
            };
            var ruleWithConnectedInput = new HydraulicRule();
            ruleWithConnectedInput.Inputs.Add(connectedInput);

            yield return new TestCaseData(ruleWithConnectedInput, $"{parameterName} [i]");

            const string expressionName = "expression_name";
            var ruleWithMathExpression = new HydraulicRule();
            ruleWithMathExpression.Inputs.Add(new MathematicalExpression {Name = expressionName});

            yield return new TestCaseData(ruleWithMathExpression, $"{expressionName} [i]");
        }
    }
}