using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Charting;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Editors.MeteoDataEditor.Converters;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI.Editors.MeteoDataEditor.Converters
{
    [TestFixture]
    public class MeteoDataDistributionTypeToChartViewOptionConverterTest
    {
        private MeteoDataDistributionTypeToChartViewOptionConverter Converter { get; set; }

        [SetUp]
        public void SetUp()
        {
            Converter = new MeteoDataDistributionTypeToChartViewOptionConverter();
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
            yield return ToData(MeteoDataDistributionType.Global, ChartViewOptions.AllSeries, nameof(MeteoDataDistributionType.Global));
            yield return ToData(MeteoDataDistributionType.PerFeature, ChartViewOptions.SelectedColumns, nameof(MeteoDataDistributionType.PerFeature));
            yield return ToData(MeteoDataDistributionType.PerStation, ChartViewOptions.SelectedColumns, nameof(MeteoDataDistributionType.PerStation));
        }

        [Test]
        [TestCaseSource(nameof(ConvertData))]
        public void Convert_ExpectedResult(object input, object expectedResult)
        {
            object result = Converter.Convert(input, typeof(MeteoDataDistributionType), null, CultureInfo.InvariantCulture);
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        public void ConvertBack_ThrowsNotSupportedException([Values]bool input)
        {
            void Call() => Converter.ConvertBack(input, typeof(MeteoDataDistributionType), null, CultureInfo.InvariantCulture);
            Assert.Throws<NotSupportedException>(Call);
        }
    }
}