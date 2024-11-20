using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Editors.MeteoDataEditor.Converters;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI.Editors.MeteoDataEditor.Converters
{
    [TestFixture]
    public class MeteoDataDistributionTypeToIsEnabledConverterTest
    {
        private MeteoDataDistributionTypeToIsEnabledConverter Converter { get; set; }

        [SetUp]
        public void SetUp()
        {
            Converter = new MeteoDataDistributionTypeToIsEnabledConverter();
        }

        [Test]
        public void Constructor_ExpectedResults()
        {
            Assert.That(Converter, Is.InstanceOf<IValueConverter>());
        }

        private static IEnumerable<TestCaseData> ConvertData()
        {
            TestCaseData ToData(object input, object expectedResult, string name) =>
                new TestCaseData(input, expectedResult).SetName(name);

            yield return ToData(null, DependencyProperty.UnsetValue, "null");
            yield return ToData(new object(), DependencyProperty.UnsetValue, "incorrect type");
            yield return ToData(MeteoDataSource.UserDefined, true, nameof(MeteoDataSource.UserDefined));
            yield return ToData(MeteoDataSource.GuidelineSewerSystems, false, nameof(MeteoDataSource.GuidelineSewerSystems));
            yield return ToData(MeteoDataSource.LongTermAverage, false, nameof(MeteoDataSource.LongTermAverage));
        }

        [Test]
        [TestCaseSource(nameof(ConvertData))]
        public void Convert_ExpectedResult(object input, object expectedResult)
        {
            object result = Converter.Convert(input, typeof(MeteoDataSource), null, CultureInfo.InvariantCulture);
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        public void ConvertBack_ThrowsNotSupportedException([Values]bool input)
        {
            void Call() => Converter.ConvertBack(input, typeof(MeteoDataSource), null, CultureInfo.InvariantCulture);
            Assert.Throws<NotSupportedException>(Call);
        }
    }
}