using DeltaShell.NGHS.Common.Utils;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests.Utils
{
    [TestFixture]
    public class EnumUtilsTest
    {
        [TestCase("first_value", TestEnum.A)]
        [TestCase("second_value", TestEnum.B)]
        [TestCase("third_value", TestEnum.C)]
        [TestCase("fourth_value", default(TestEnum))]
        [TestCase(null, default(TestEnum))]
        [TestCase("", default(TestEnum))]
        public void GetEnumValueByDescription_ReturnsCorrectResult(string description,
                                                                   object expectedResult)
        {
            // Call
            var result = EnumUtils.GetEnumValueByDescription<TestEnum>(description);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        private enum TestEnum
        {
            [System.ComponentModel.Description("first_value")]
            A,

            [System.ComponentModel.Description("second_value")]
            B,

            [System.ComponentModel.Description("third_value")]
            C,
        }
    }
}
