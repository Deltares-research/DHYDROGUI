using System;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Concepts.Nwrw
{
    [TestFixture]
    public class DryweatherFlowDistributionTypeConverterTest
    {
        [Test]
        [TestCase("CST", DryweatherFlowDistributionType.Constant)]
        [TestCase("cSt", DryweatherFlowDistributionType.Constant)]
        [TestCase("VaR", DryweatherFlowDistributionType.Variable)]
        [TestCase("vaR", DryweatherFlowDistributionType.Variable)]
        [TestCase("dag", DryweatherFlowDistributionType.Daily)]
        [TestCase("DAg", DryweatherFlowDistributionType.Daily)]
        public void GivenDryweatherDistributionTypeString_WhenCallingConverter_ThenReturnsCorrectDryweatherFlowDistributionType(
            string dryweatherFlowDistributionTypeString, DryweatherFlowDistributionType expectedDryweatherFlowDistributionType)
        {
            var actualDryweatherFlowDistributionType = 
                DryweatherFlowDistributionTypeConverter.ConvertStringToDryweatherFlowDistributionType(dryweatherFlowDistributionTypeString);
            Assert.That(actualDryweatherFlowDistributionType, Is.EqualTo(expectedDryweatherFlowDistributionType));
        }

        [Test]
        public void GivenInvalidDryweatherFlowDistributionTypeString_WhenCallingConvert_ThenThrowsException()
        {
            string invalidTypeString = "InvalidTypeString";
            TestDelegate action = () => 
                DryweatherFlowDistributionTypeConverter.ConvertStringToDryweatherFlowDistributionType(invalidTypeString);
            Assert.Throws<InvalidOperationException>(action);
        }
    }
}