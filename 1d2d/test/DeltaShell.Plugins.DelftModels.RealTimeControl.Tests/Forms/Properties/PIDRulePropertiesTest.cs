using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.Properties;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Forms.Properties
{
    [TestFixture]
    public class PIDRulePropertiesTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowProperties()
        {
            WindowsFormsTestHelper.ShowPropertyGridForObject(new PIDRuleProperties {Data = new PIDRule()});
        }

        [TestCase("ConstantSetpoint", PIDRule.PIDRuleSetpointType.Constant, false)]
        [TestCase("ConstantSetpoint", PIDRule.PIDRuleSetpointType.TimeSeries, true)]
        [TestCase("ConstantSetpoint", PIDRule.PIDRuleSetpointType.Signal, true)]
        [TestCase("Table", PIDRule.PIDRuleSetpointType.Constant, true)]
        [TestCase("Table", PIDRule.PIDRuleSetpointType.TimeSeries, false)]
        [TestCase("Table", PIDRule.PIDRuleSetpointType.Signal, true)]
        [TestCase("Interpolation", PIDRule.PIDRuleSetpointType.Constant, true)]
        [TestCase("Interpolation", PIDRule.PIDRuleSetpointType.TimeSeries, false)]
        [TestCase("Interpolation", PIDRule.PIDRuleSetpointType.Signal, true)]
        [TestCase("Extrapolation", PIDRule.PIDRuleSetpointType.Constant, true)]
        [TestCase("Extrapolation", PIDRule.PIDRuleSetpointType.TimeSeries, false)]
        [TestCase("Extrapolation", PIDRule.PIDRuleSetpointType.Signal, true)]
        [TestCase("Bla", PIDRule.PIDRuleSetpointType.Constant, true)]
        [TestCase("Bla", PIDRule.PIDRuleSetpointType.TimeSeries, true)]
        [TestCase("Bla", PIDRule.PIDRuleSetpointType.Signal, true)]
        public void GivenAPropertyName_WhenShowingThisPropertyInthePropertyBoxWindow_ThenThisOptionShouldBeReadOnlyOrNotBasedOnTheSetPointModeSetting(
            string propertyName, PIDRule.PIDRuleSetpointType pidRuleSetpointType, bool expectedBoolean)
        {
            // Given
            var pidRuleProperties = new PIDRuleProperties
            {
                Data = new PIDRule(),
                SetpointMode = pidRuleSetpointType
            };

            // When
            bool actualBoolean = pidRuleProperties.DynamicReadOnlyValidationMethod(propertyName);

            // Then
            Assert.AreEqual(expectedBoolean, actualBoolean,
                            $"For property \"{propertyName}\" of the PID rule, the readonly behaviour is not correct when the setpoint type is \"{pidRuleSetpointType.ToString()}\"");
        }
    }
}