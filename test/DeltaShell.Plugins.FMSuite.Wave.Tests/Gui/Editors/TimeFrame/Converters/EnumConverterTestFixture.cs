using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.Converters;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.TimeFrame.Converters
{
    /// <summary>
    /// <see cref="EnumConverterTestFixture{TEnum}"/> is a test fixture meant to
    /// ease the testing of classes inheriting from <see cref="EnumConverterTestFixture{TEnum}"/>.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enum.</typeparam>
    [TestFixture]
    public abstract class EnumConverterTestFixture<TEnum> where TEnum : struct
    {
        /// <summary>
        /// Gets a value indicating whether collapse hidden is true.
        /// </summary>
        protected abstract bool CollapseHidden { get; }

        /// <summary>
        /// Gets a value indicating whether invert visibility is true.
        /// </summary>
        protected abstract bool InvertVisibility { get; }

        /// <summary>
        /// Creates the converter.
        /// </summary>
        /// <returns>A new converter which should be used in the tests.</returns>
        protected abstract EnumToVisibilityConverter<TEnum> CreateConverter();

        protected static IEnumerable<TEnum> GetEnumValues() =>
            Enum.GetValues(typeof(TEnum)).Cast<TEnum>();

        public static IEnumerable<TestCaseData> GetValidParameters() =>
            from enumValueCurrent in GetEnumValues()
            from enumValueParameter in GetEnumValues()
            select new TestCaseData(enumValueCurrent, enumValueParameter);

        [Test]
        [TestCaseSource(nameof(GetValidParameters))]

        public void Convert_ValidParameter_ReturnsExpectedResult(TEnum currentValue, TEnum parameter)
        {
            EnumToVisibilityConverter<TEnum> converter = CreateConverter();
            object result = converter.Convert(currentValue, typeof(Visibility), parameter, CultureInfo.InvariantCulture);

            Visibility expectedVisibility = Equals(currentValue, parameter) ^ InvertVisibility ? Visibility.Visible : CollapseHidden ? Visibility.Collapsed : Visibility.Hidden;
            Assert.That(result, Is.EqualTo(expectedVisibility));
        }

        public static IEnumerable<TestCaseData> GetInvalidParameters()
        {
            TEnum validEnum = GetEnumValues().First();

            yield return new TestCaseData(new object(), typeof(Visibility), validEnum);
            yield return new TestCaseData(validEnum, typeof(object), validEnum);
            yield return new TestCaseData(validEnum, typeof(Visibility), new object());
        }

        [Test]
        [TestCaseSource(nameof(GetInvalidParameters))]
        public void Convert_InvalidParameter_ReturnsDependencyPropertyUnsetValue(object currentValue,
                                                                                 Type t,
                                                                                 object parameter)
        {
            EnumToVisibilityConverter<TEnum> converter = CreateConverter();
            object result = converter.Convert(currentValue, t, parameter, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(DependencyProperty.UnsetValue));
        }

        [Test]
        public void ConvertBack_ThrowsNotSupportedException()
        {
            EnumToVisibilityConverter<TEnum> converter = CreateConverter();
            void Call() => converter.ConvertBack(Visibility.Collapsed,
                                                 typeof(Visibility),
                                                 GetEnumValues().First(),
                                                 CultureInfo.InvariantCulture);

            Assert.Throws<System.NotSupportedException>(Call);
        }
    }
}