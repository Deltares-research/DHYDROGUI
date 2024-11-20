using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.Converters;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.TimeFrame.Converters
{
    [TestFixture]
    public class SpiderWebPathVisibilityConverterTest
    {
        public static IEnumerable<TestCaseData> GetValidParameters()
        {
            yield return new TestCaseData(new object[] { true, WindInputType.SpiderWebGrid }, Visibility.Visible);
            yield return new TestCaseData(new object[] { true, WindInputType.WindVector }, Visibility.Visible);
            yield return new TestCaseData(new object[] { true, WindInputType.XYComponents }, Visibility.Visible);
            yield return new TestCaseData(new object[] { false, WindInputType.SpiderWebGrid }, Visibility.Visible);
            yield return new TestCaseData(new object[] { false, WindInputType.WindVector }, Visibility.Collapsed);
            yield return new TestCaseData(new object[] { false, WindInputType.XYComponents }, Visibility.Collapsed);
            yield return new TestCaseData(new object[] { WindInputType.SpiderWebGrid, true }, Visibility.Visible);
            yield return new TestCaseData(new object[] { WindInputType.WindVector, true }, Visibility.Visible);
            yield return new TestCaseData(new object[] { WindInputType.XYComponents, true }, Visibility.Visible);
            yield return new TestCaseData(new object[] { WindInputType.SpiderWebGrid, false }, Visibility.Visible);
            yield return new TestCaseData(new object[] { WindInputType.WindVector, false }, Visibility.Collapsed);
            yield return new TestCaseData(new object[] { WindInputType.XYComponents, false }, Visibility.Collapsed);
        }

        [Test]
        [TestCaseSource(nameof(GetValidParameters))]

        public void Convert_ValidParameter_ReturnsExpectedResult(object[] currentValues, Visibility expectedVisibility)
        {
            var converter = new SpiderWebPathVisibilityConverter();
            object result = converter.Convert(currentValues, typeof(Visibility), null, CultureInfo.InvariantCulture);
            Assert.That(result, Is.EqualTo(expectedVisibility));
        }

        public static IEnumerable<TestCaseData> GetInvalidParameters()
        {
            yield return new TestCaseData(null, typeof(Visibility));

            yield return new TestCaseData(new object[0], typeof(Visibility));
            yield return new TestCaseData(new object[1], typeof(Visibility));
            yield return new TestCaseData(new object[2], typeof(Visibility));
            yield return new TestCaseData(new object[3], typeof(Visibility));

            yield return new TestCaseData(new object[] { new object() }, typeof(Visibility));
            yield return new TestCaseData(new object[] { false }, typeof(Visibility));
            yield return new TestCaseData(new object[] { WindInputType.SpiderWebGrid }, typeof(Visibility));

            yield return new TestCaseData(new object[] { new object(), new object() }, typeof(Visibility));
            yield return new TestCaseData(new object[] { new object(), true }, typeof(Visibility));
            yield return new TestCaseData(new object[] { new object(), WindInputType.SpiderWebGrid }, typeof(Visibility));
            yield return new TestCaseData(new object[] { false, false }, typeof(Visibility));
            yield return new TestCaseData(new object[] { WindInputType.SpiderWebGrid, WindInputType.SpiderWebGrid }, typeof(Visibility));

            yield return new TestCaseData(new object[] { new object(), new object(), new object() }, typeof(Visibility));
            yield return new TestCaseData(new object[] { false, WindInputType.SpiderWebGrid, new object() }, typeof(Visibility));
            yield return new TestCaseData(new object[] { false, WindInputType.SpiderWebGrid }, typeof(object));
        }

        [Test]
        [TestCaseSource(nameof(GetInvalidParameters))]
        public void Convert_InvalidParameter_ReturnsDependencyPropertyUnsetValue(object[] currentValues,
                                                                                 Type t)
        {
            var converter = new SpiderWebPathVisibilityConverter();
            object result = converter.Convert(currentValues, t, null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(DependencyProperty.UnsetValue));
        }

        [Test]
        public void ConvertBack_ThrowsNotSupportedException()
        {
            var converter = new SpiderWebPathVisibilityConverter();
            void Call() => converter.ConvertBack(Visibility.Collapsed,
                                                 new[] { typeof(WindInputType) },
                                                 null,
                                                 CultureInfo.InvariantCulture);

            Assert.Throws<System.NotSupportedException>(Call);
        }
    }
}