using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Converters;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.Converters
{
    [TestFixture]
    public class NotNullToBooleanConverterTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var converter = new NotNullToBooleanConverter();

            // Assert
            Assert.That(converter, Is.InstanceOf<IValueConverter>());
        }

        [Test]
        [TestCaseSource(nameof(GetConvertData))]
        public void Convert_ExpectedValues(object obj, bool expectedValue)
        {
            // Setup
            var converter = new NotNullToBooleanConverter();

            // Call
            object result = converter.Convert(obj, typeof(bool), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        [TestCaseSource(nameof(GetConvertData))]
        public void Convert_InvalidTargetType_ReturnsUnsetValue(object obj, bool expectedValue)
        {
            // Setup
            var converter = new NotNullToBooleanConverter();

            // Call
            object result = converter.Convert(obj, typeof(object), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.That(result, Is.EqualTo(DependencyProperty.UnsetValue));
        }

        [Test]
        public void ConvertBack_ThrowsNotSupportedException()
        {
            // Setup
            var converter = new NotNullToBooleanConverter();

            // Call | Assert
            void Call() => converter.ConvertBack(true, typeof(object), null, CultureInfo.InvariantCulture);
            Assert.Throws<NotSupportedException>(Call);
        }

        private static IEnumerable<TestCaseData> GetConvertData()
        {
            yield return new TestCaseData(new object(), true);
            yield return new TestCaseData(null, false);
        }
    }
}