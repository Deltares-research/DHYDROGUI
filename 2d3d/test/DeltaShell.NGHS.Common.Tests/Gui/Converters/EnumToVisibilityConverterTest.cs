using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using DeltaShell.NGHS.Common.Gui.Converters;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests.Gui.Converters
{
    [TestFixture]
    public class EnumToVisibilityConverterTest
    {
        [Test]
        public void Convert_ValidParameter_ReturnsExpectedResult([Values] TestEnum currentValue, [Values] TestEnum parameter, [Values] bool invertVisibility, [Values] bool collapseHidden)
        {
            var converter = new EnumToVisibilityConverter
            {
                InvertVisibility = invertVisibility,
                CollapseHidden = collapseHidden
            };
            object result = converter.Convert(currentValue, typeof(Visibility), parameter, CultureInfo.InvariantCulture);

            object expectedVisibility = GetExpectedVisibility(currentValue, parameter, invertVisibility, collapseHidden);
            Assert.That(result, Is.EqualTo(expectedVisibility));
        }

        [Test]
        [TestCaseSource(nameof(GetInvalidParameters))]
        public void Convert_InvalidParameter_ReturnsDependencyPropertyUnsetValue(object currentValue,
                                                                                 Type t,
                                                                                 object parameter)
        {
            var converter = new EnumToVisibilityConverter();
            object result = converter.Convert(currentValue, t, parameter, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(DependencyProperty.UnsetValue));
        }

        [Test]
        public void ConvertBack_ThrowsNotSupportedException()
        {
            var converter = new EnumToVisibilityConverter();

            void Call() => converter.ConvertBack(Visibility.Collapsed,
                                                 typeof(Visibility),
                                                 TestEnum.Two,
                                                 CultureInfo.InvariantCulture);

            Assert.Throws<NotSupportedException>(Call);
        }

        public enum TestEnum
        {
            One,
            Two,
        }

        private static object GetExpectedVisibility(Enum currentEnum, Enum expectedEnum, bool invertVisibility, bool collapseHidden)
        {
            if (IsVisible(currentEnum, expectedEnum, invertVisibility))
            {
                return Visibility.Visible;
            }

            return collapseHidden ? Visibility.Collapsed : Visibility.Hidden;
        }

        private static bool IsVisible(Enum currentValue, Enum expectedEnumValue, bool invertVisibility) =>
            Equals(currentValue, expectedEnumValue) ^ invertVisibility;

        public static IEnumerable<TestCaseData> GetInvalidParameters()
        {
            yield return new TestCaseData(new object(), typeof(Visibility), TestEnum.One);
            yield return new TestCaseData(TestEnum.One, typeof(object), TestEnum.One);
            yield return new TestCaseData(TestEnum.One, typeof(Visibility), new object());
        }
    }
}