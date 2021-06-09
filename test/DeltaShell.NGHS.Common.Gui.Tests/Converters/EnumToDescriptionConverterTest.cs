using System;
using System.Collections.Generic;
using System.Windows;
using DeltaShell.NGHS.Common.Gui.Converters;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Gui.Tests.Converters
{
    [TestFixture]
    public class EnumToDescriptionConverterTest
    {
        private EnumToDescriptionConverter converter;

        [SetUp]
        public void Setup()
        {
            converter = new EnumToDescriptionConverter();
        }

        [Test]
        public void ConvertBack_ThenNotSupportedExceptionIsThrown()
        {
            // Call
            void Call() => converter.ConvertBack(null, null, null, null);

            // Assert
            Assert.That(Call, Throws.TypeOf<NotSupportedException>());
        }

        [TestCaseSource(nameof(TestCaseData))]
        public void Convert_ThenExpectedValueIsReturned(object value, Type targetType, object expectedReturnValue)
        {
            // Call
            object result = converter.Convert(value, targetType, null, null);

            // Assert
            Assert.That(result, Is.EqualTo(expectedReturnValue));
        }

        private static IEnumerable<TestCaseData> TestCaseData()
        {
            yield return new TestCaseData(TestEnum.Test1, typeof(string), "This is the first description");
            yield return new TestCaseData(TestEnum.Test2, typeof(string), "This is the second description");
            yield return new TestCaseData(TestEnum.Test3, typeof(string), "Test3");

            yield return new TestCaseData(new object(), typeof(string), DependencyProperty.UnsetValue);

            yield return new TestCaseData(TestEnum.Test1, typeof(object), DependencyProperty.UnsetValue);
            yield return new TestCaseData(TestEnum.Test2, typeof(object), DependencyProperty.UnsetValue);
            yield return new TestCaseData(TestEnum.Test3, typeof(object), DependencyProperty.UnsetValue);
        }

        private enum TestEnum
        {
            [System.ComponentModel.Description("This is the first description")]
            Test1,

            [System.ComponentModel.Description("This is the second description")]
            Test2,
            Test3
        }
    }
}