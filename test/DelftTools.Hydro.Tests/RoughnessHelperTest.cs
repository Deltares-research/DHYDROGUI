using System;
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
    }
}