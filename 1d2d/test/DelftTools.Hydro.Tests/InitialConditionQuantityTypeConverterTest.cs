using System;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class InitialConditionQuantityTypeConverterTest
    {
        [Test]
        [TestCase("WaTeRdEpTh", InitialConditionQuantity.WaterDepth)]
        [TestCase("waterDepth", InitialConditionQuantity.WaterDepth)]
        [TestCase("WATERLEVEL", InitialConditionQuantity.WaterLevel)]
        [TestCase("waterlevel", InitialConditionQuantity.WaterLevel)]
        public void GivenInitialConditionQuantityString_WhenCallingInitialConditionQuantityConverter_ThenReturnsCorrectInitialConditionQuantity(
            string initialConditionQuantityString, 
            InitialConditionQuantity expectedInitialConditionQuantity)
        {
            var actualInitialConditionQuantity = InitialConditionQuantityTypeConverter.ConvertStringToInitialConditionQuantity(initialConditionQuantityString);
            Assert.That(actualInitialConditionQuantity, Is.EqualTo(expectedInitialConditionQuantity));
        }

        [Test]
        public void GivenInvalidInitialConditionQuantityString_WhenCallingConverter_ThrowsException()
        {
            string invalidQuantity = "InvalidQuantity";
            TestDelegate action = () => InitialConditionQuantityTypeConverter.ConvertStringToInitialConditionQuantity(invalidQuantity);
            Assert.Throws<InvalidOperationException>(action);
        }

        [Test]
        [TestCase( InitialConditionQuantity.WaterDepth, "Water depth")]
        [TestCase( InitialConditionQuantity.WaterLevel, "Water level")]
        public void GivenInitialConditionQuantity_WhenCallingInitialConditionQuantityConverter_ThenReturnsCorrectInitialConditionQuantityString(
            InitialConditionQuantity initialConditionQuantity,
            string expectedInitialConditionQuantityString
            )
        {
            var actualInitialConditionQuantityString = InitialConditionQuantityTypeConverter.ConvertInitialConditionQuantityToString(initialConditionQuantity);
            Assert.That(actualInitialConditionQuantityString, Is.EqualTo(expectedInitialConditionQuantityString));
        }
    }
}