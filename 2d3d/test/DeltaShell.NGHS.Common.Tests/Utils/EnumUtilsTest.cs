using System;
using DeltaShell.NGHS.Common.Utils;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests.Utils
{
    [TestFixture]
    public class EnumUtilsTest
    {
        [Test]
        public void GetEnumValueByDescription_DescriptionNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => EnumUtils.GetEnumValueByDescription<TestEnum>(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("description"));
        }

        [TestCase("first_value", TestEnum.A)]
        [TestCase("First_value", TestEnum.A)]
        [TestCase("second_value", TestEnum.B)]
        [TestCase("SECOND_value", TestEnum.B)]
        [TestCase("third_value", TestEnum.C)]
        [TestCase("THIRD_VALUE", TestEnum.C)]
        public void GetEnumValueByDescription_ReturnsCorrectResult(string description,
                                                                   object expectedResult)
        {
            // Call
            var result = EnumUtils.GetEnumValueByDescription<TestEnum>(description);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [TestCase("fourth_value")]
        [TestCase("1")]
        [TestCase("2")]
        [TestCase("3")]
        [TestCase("")]
        public void GetEnumValueByDescription_InvalidDescription_ThrowsNotSupportedException(string description)
        {
            Assert.That(() => EnumUtils.GetEnumValueByDescription<TestEnum>(description), Throws.InstanceOf<NotSupportedException>());
        }

        private enum TestEnum
        {
            [System.ComponentModel.Description("first_value")]
            A,

            [System.ComponentModel.Description("second_value")]
            B,

            [System.ComponentModel.Description("third_value")]
            C
        }
    }
}