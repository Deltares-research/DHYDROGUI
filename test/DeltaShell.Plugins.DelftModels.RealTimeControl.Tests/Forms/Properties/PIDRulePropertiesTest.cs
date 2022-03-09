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

        [TestCase("ConstantSetpoint", PIDRule.PIDRuleSetpointTypes.Constant, false)]
        [TestCase("ConstantSetpoint", PIDRule.PIDRuleSetpointTypes.TimeSeries, true)]
        [TestCase("ConstantSetpoint", PIDRule.PIDRuleSetpointTypes.Signal, true)]
        [TestCase("Table", PIDRule.PIDRuleSetpointTypes.Constant, true)]
        [TestCase("Table", PIDRule.PIDRuleSetpointTypes.TimeSeries, false)]
        [TestCase("Table", PIDRule.PIDRuleSetpointTypes.Signal, true)]
        [TestCase("Interpolation", PIDRule.PIDRuleSetpointTypes.Constant, true)]
        [TestCase("Interpolation", PIDRule.PIDRuleSetpointTypes.TimeSeries, false)]
        [TestCase("Interpolation", PIDRule.PIDRuleSetpointTypes.Signal, true)]
        [TestCase("Extrapolation", PIDRule.PIDRuleSetpointTypes.Constant, true)]
        [TestCase("Extrapolation", PIDRule.PIDRuleSetpointTypes.TimeSeries, false)]
        [TestCase("Extrapolation", PIDRule.PIDRuleSetpointTypes.Signal, true)]
        [TestCase("Bla", PIDRule.PIDRuleSetpointTypes.Constant, true)]
        [TestCase("Bla", PIDRule.PIDRuleSetpointTypes.TimeSeries, true)]
        [TestCase("Bla", PIDRule.PIDRuleSetpointTypes.Signal, true)]
        public void GivenAPropertyName_WhenShowingThisPropertyInthePropertyBoxWindow_ThenThisOptionShouldBeReadOnlyOrNotBasedOnTheSetPointModeSetting(
            string propertyName, PIDRule.PIDRuleSetpointTypes pidRuleSetpointTypes, bool expectedBoolean)
        {
            // Given
            var pidRuleProperties = new PIDRuleProperties
            {
                Data = new PIDRule(),
                SetpointMode = pidRuleSetpointTypes
            };

            // When
            bool actualBoolean = pidRuleProperties.DynamicReadOnlyValidationMethod(propertyName);

            // Then
            Assert.AreEqual(expectedBoolean, actualBoolean,
                            $"For property \"{propertyName}\" of the PID rule, the readonly behaviour is not correct when the setpoint type is \"{pidRuleSetpointTypes.ToString()}\"");
        }
    }
}