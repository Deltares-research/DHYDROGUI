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

        [TestCase("ConstantSetpoint", IntervalRule.IntervalRuleIntervalType.Fixed, false)]
        [TestCase("ConstantSetpoint", IntervalRule.IntervalRuleIntervalType.Variable, true)]
        [TestCase("ConstantSetpoint", IntervalRule.IntervalRuleIntervalType.Signal, true)]
        [TestCase("TimeSeries", IntervalRule.IntervalRuleIntervalType.Fixed, true)]
        [TestCase("TimeSeries", IntervalRule.IntervalRuleIntervalType.Variable, false)]
        [TestCase("TimeSeries", IntervalRule.IntervalRuleIntervalType.Signal, true)]
        [TestCase("Interpolation", IntervalRule.IntervalRuleIntervalType.Fixed, true)]
        [TestCase("Interpolation", IntervalRule.IntervalRuleIntervalType.Variable, false)]
        [TestCase("Interpolation", IntervalRule.IntervalRuleIntervalType.Signal, true)]
        [TestCase("Extrapolation", IntervalRule.IntervalRuleIntervalType.Fixed, true)]
        [TestCase("Extrapolation", IntervalRule.IntervalRuleIntervalType.Variable, false)]
        [TestCase("Extrapolation", IntervalRule.IntervalRuleIntervalType.Signal, true)]
        [TestCase("FixedInterval", IntervalRule.IntervalRuleIntervalType.Fixed, false)]
        [TestCase("FixedInterval", IntervalRule.IntervalRuleIntervalType.Variable, true)]
        [TestCase("FixedInterval", IntervalRule.IntervalRuleIntervalType.Signal, true)]
        [TestCase("MaxSpeed", IntervalRule.IntervalRuleIntervalType.Fixed, true)]
        [TestCase("MaxSpeed", IntervalRule.IntervalRuleIntervalType.Variable, false)]
        [TestCase("MaxSpeed", IntervalRule.IntervalRuleIntervalType.Signal, false)]
        [TestCase("Bla", IntervalRule.IntervalRuleIntervalType.Fixed, true)]
        [TestCase("Bla", IntervalRule.IntervalRuleIntervalType.Variable, true)]
        [TestCase("Bla", IntervalRule.IntervalRuleIntervalType.Signal, true)]
        public void GivenAPropertyName_WhenShowingThisPropertyInthePropertyBoxWindow_ThenThisOptionShouldBeReadOnlyOrNotBasedOnTheIntervalTypeSetting(
            string propertyName, IntervalRule.IntervalRuleIntervalType intervalRuleSetpointType,
            bool expectedBoolean)
        {
            // Given
            var intervalRuleProperties = new IntervalRuleProperties
            {
                Data = new IntervalRule(),
                IntervalType = intervalRuleSetpointType
            };

            // When
            bool actualBoolean = intervalRuleProperties.DynamicReadOnlyValidationMethod(propertyName);

            // Then
            Assert.AreEqual(expectedBoolean, actualBoolean,
                            $"For property \"{propertyName}\" of the interval rule, the readonly behaviour is not correct when the setpoint type is \"{intervalRuleSetpointType.ToString()}\"");
        }
    }
}