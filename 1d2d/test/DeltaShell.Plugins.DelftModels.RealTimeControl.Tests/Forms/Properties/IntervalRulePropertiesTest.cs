using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.Properties;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Forms.Properties
{
    [TestFixture]
    public class IntervalRulePropertiesTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowProperties()
        {
            WindowsFormsTestHelper.ShowPropertyGridForObject(new IntervalRuleProperties {Data = new IntervalRule()});
        }

        [TestCase("ConstantSetpoint", IntervalRule.IntervalRuleSetPointType.Fixed, false)]
        [TestCase("ConstantSetpoint", IntervalRule.IntervalRuleSetPointType.Variable, true)]
        [TestCase("ConstantSetpoint", IntervalRule.IntervalRuleSetPointType.Signal, true)]
        [TestCase("TimeSeries", IntervalRule.IntervalRuleSetPointType.Fixed, true)]
        [TestCase("TimeSeries", IntervalRule.IntervalRuleSetPointType.Variable, false)]
        [TestCase("TimeSeries", IntervalRule.IntervalRuleSetPointType.Signal, true)]
        [TestCase("Interpolation", IntervalRule.IntervalRuleSetPointType.Fixed, true)]
        [TestCase("Interpolation", IntervalRule.IntervalRuleSetPointType.Variable, false)]
        [TestCase("Interpolation", IntervalRule.IntervalRuleSetPointType.Signal, true)]
        [TestCase("Extrapolation", IntervalRule.IntervalRuleSetPointType.Fixed, true)]
        [TestCase("Extrapolation", IntervalRule.IntervalRuleSetPointType.Variable, false)]
        [TestCase("Extrapolation", IntervalRule.IntervalRuleSetPointType.Signal, true)]
        [TestCase("Bla", IntervalRule.IntervalRuleSetPointType.Fixed, true)]
        [TestCase("Bla", IntervalRule.IntervalRuleSetPointType.Variable, true)]
        [TestCase("Bla", IntervalRule.IntervalRuleSetPointType.Signal, true)]
        public void GivenAPropertyName_WhenShowingThisPropertyInthePropertyBoxWindow_ThenThisOptionShouldBeReadOnlyOrNotBasedOnTheSetpointTypeSetting(
            string propertyName, IntervalRule.IntervalRuleSetPointType intervalRuleSetpointType,
            bool expectedBoolean)
        {
            // Given
            var intervalRuleProperties = new IntervalRuleProperties
            {
                Data = new IntervalRule(),
                SetPointType = intervalRuleSetpointType
            };

            // When
            bool actualBoolean = intervalRuleProperties.DynamicReadOnlyValidationMethod(propertyName);

            // Then
            Assert.AreEqual(expectedBoolean, actualBoolean,
                            $"For property \"{propertyName}\" of the interval rule, the readonly behaviour is not correct when the setpoint type is \"{intervalRuleSetpointType.ToString()}\"");
        }

        [TestCase("FixedInterval", IntervalRule.IntervalRuleSetPointType.Fixed, false)]
        [TestCase("FixedInterval", IntervalRule.IntervalRuleSetPointType.Variable, true)]
        [TestCase("FixedInterval", IntervalRule.IntervalRuleSetPointType.Signal, true)]
        [TestCase("MaxSpeed", IntervalRule.IntervalRuleSetPointType.Fixed, true)]
        [TestCase("MaxSpeed", IntervalRule.IntervalRuleSetPointType.Variable, false)]
        [TestCase("MaxSpeed", IntervalRule.IntervalRuleSetPointType.Signal, false)]
        public void GivenAPropertyName_WhenShowingThisPropertyInthePropertyBoxWindow_ThenThisOptionShouldBeReadOnlyOrNotBasedOnTheIntervalTypeSetting(
            string propertyName, IntervalRule.IntervalRuleIntervalType intervalRuleIntervalType,
            bool expectedBoolean)
        {
            // Given
            var intervalRuleProperties = new IntervalRuleProperties
            {
                Data = new IntervalRule(),
                IntervalType = intervalRuleIntervalType
            };

            // When
            bool actualBoolean = intervalRuleProperties.DynamicReadOnlyValidationMethod(propertyName);

            // Then
            Assert.AreEqual(expectedBoolean, actualBoolean,
                            $"For property \"{propertyName}\" of the interval rule, the readonly behaviour is not correct when the setpoint type is \"{intervalRuleIntervalType.ToString()}\"");
        }
    }
}