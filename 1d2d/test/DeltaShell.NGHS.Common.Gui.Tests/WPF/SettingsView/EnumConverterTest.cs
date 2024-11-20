using System;
using System.Globalization;
using System.Windows;
using DeltaShell.NGHS.Common.Gui.WPF.ValueConverters;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Gui.Tests.WPF.SettingsView
{
    [TestFixture]
    public class EnumConverterTest
    {
        [Test]
        public void WpfEnumConverter_Test()
        {
            var converter = new EnumConverter();
            Assert.IsNotNull(converter);
        }

        private enum DummyEnum
        {
            First = 1,
            Second = 2,
            Third = 3,
        }

        [Test]
        public void WpfEnumConverter_Convert_Given_EnumType_Value_Returns_EnumArray()
        {
            var converter = new EnumConverter();
            Assert.IsNotNull(converter);

            var convertedValue = converter.Convert(typeof(DummyEnum), typeof(DummyEnum), null, CultureInfo.InvariantCulture);
            Assert.IsNotNull(convertedValue);

            var arrayResult = convertedValue as Enum[];
            var expectedResult = new[] {DummyEnum.First, DummyEnum.Second, DummyEnum.Third};
            Assert.IsNotNull(arrayResult);
            Assert.AreEqual(3, arrayResult.Length);
            Assert.AreEqual(expectedResult, arrayResult);
            
        }

        [Test]
        public void WpfEnumConverter_Convert_Given_WrongValue_Returns_EnumArray()
        {
            var converter = new EnumConverter();
            Assert.IsNotNull(converter);

            object result = null;
            Assert.DoesNotThrow( () => result = converter.Convert(null, null, null, CultureInfo.InvariantCulture));
            Assert.IsNotNull(result);
            Assert.AreEqual( DependencyProperty.UnsetValue, result);
        }

        [Test]
        [TestCase(null)]
        [TestCase(true)]
        [TestCase(4.2)]
        [TestCase("dummyString")]
        public void WpfEnumConverter_ConvertBack_ReturnsGivenValue(object valueToConvert)
        {
            var converter = new EnumConverter();
            Assert.IsNotNull(converter);

            object result = null;
            Assert.DoesNotThrow(() => result = converter.ConvertBack(valueToConvert, null, null, CultureInfo.InvariantCulture));
            Assert.AreEqual(valueToConvert, result);
        }
    }
}