using System;
using DelftTools.Hydro.Roughness;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class RoughnessHelperTest
    {
        [Test]
        [TestCase("CHEZY", RoughnessType.Chezy)]
        [TestCase("manning", RoughnessType.Manning)]
        [TestCase("stricklerNikuradsE", RoughnessType.StricklerNikuradse)]
        [TestCase("striCKler", RoughnessType.Strickler)]
        [TestCase("whiteColeBrOoK", RoughnessType.WhiteColebrook)]
        [TestCase("deBosBijkerk", RoughnessType.DeBosBijkerk)]
        [TestCase("waLLLawNikuradse", RoughnessType.WallLawNikuradse)]
        public void GivenRoughnessTypeString_WhenCallingStringToRoughnessTypeConverter_ThenReturnsCorrectRoughnessType(
            string roughnessTypeString, 
            RoughnessType expectedRoughnessType)
        {
            var actualRoughnessType = RoughnessHelper.ConvertStringToRoughnessType(roughnessTypeString);
            Assert.That(actualRoughnessType, Is.EqualTo(expectedRoughnessType));
        }

        [Test]
        public void GivenInvalidRoughnessTypeString_WhenCallingStringToRoughnessTypeConverter_ThenExceptionIsThrown()
        {
            var invalidRoughnessType = "InvalidRoughnessType";
            TestDelegate action = () => RoughnessHelper.ConvertStringToRoughnessType(invalidRoughnessType);
            Assert.Throws<InvalidOperationException>(action);

        }

        [Test]
        [TestCase("CONSTANT", RoughnessFunction.Constant)]
        [TestCase("absdischarge", RoughnessFunction.FunctionOfQ)]
        [TestCase("FunctionOfQ", RoughnessFunction.FunctionOfQ)]
        [TestCase("functionOfH", RoughnessFunction.FunctionOfH)]
        [TestCase("waTerLevel", RoughnessFunction.FunctionOfH)]
        public void GivenRoughnessFunctionString_WhenCallingConvertStringToRoughnessFunction_ThenReturnsCorrectRoughnessFunction(
                string roughnessFunctionString,
                RoughnessFunction expectedRoughnessFunction)
        {
            var actualRoughnessFunction = RoughnessHelper.ConvertStringToRoughnessFunction(roughnessFunctionString);
            Assert.That(actualRoughnessFunction, Is.EqualTo(expectedRoughnessFunction));
        }

        [Test]
        public void GivenInvalidRoughnessFunctionString_WhenCallingStringToRoughnessFunctionConverter_ThenExceptionIsThrown()
        {
            var invalidRoughnessFunction = "InvalidRoughnessFunction";
            TestDelegate action = () => RoughnessHelper.ConvertStringToRoughnessFunction(invalidRoughnessFunction);
            Assert.Throws<InvalidOperationException>(action);

        }
    }
}