using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Controls.Swf.Table;
using DelftTools.Utils.Globalization;
using DeltaShell.NGHS.Common.Gui.WPF;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Gui.Tests.WPF
{
    [TestFixture]
    public class YearPatternHelperTest
    {
        private MultipleFunctionView multipleFunctionView;
        private DateTimeFormatInfo dateTimeFormatInfo;
        private ITableViewColumn tableViewColumn;
        
        [SetUp]
        public void SetUp()
        {
            multipleFunctionView = new MultipleFunctionView();
            dateTimeFormatInfo = new DateTimeFormatInfo();
            tableViewColumn = Substitute.For<ITableViewColumn>();
            multipleFunctionView.TableView.Columns.Add(tableViewColumn);
            ((ChartView) multipleFunctionView.ChartView).DateTimeLabelFormatProvider.CustomDateTimeFormatInfo = new DateTimeFormatInfo();
        }

        [TearDown]
        public void TearDown()
        {
            ((ChartView) multipleFunctionView.ChartView).DateTimeLabelFormatProvider.CustomDateTimeFormatInfo = null;
            multipleFunctionView = null;
            dateTimeFormatInfo = null;
        }
        
        [Test]
        public void WhenShowYears_ThenRetrievedDataIsShowingYears()
        {
            //Arrange
            string expectedDateTimeFormat = RegionalSettingsManager.DateTimeFormat;
            
            //Act
            YearPatternHelper.ShowYears(multipleFunctionView.TableView, multipleFunctionView.ChartView as ChartView);

            //Assert
            ViewTestData retrievedData = RetrieveViewData();
            Assert.That(retrievedData.TableDisplayFormat, Is.EqualTo(expectedDateTimeFormat));
            Assert.That(retrievedData.BottomAxisLabelsFormat, Is.EqualTo(expectedDateTimeFormat));
            Assert.That(retrievedData.RangeLabelVisible, Is.True);
            Assert.That(retrievedData.YearMonthPattern, Is.EqualTo(dateTimeFormatInfo.YearMonthPattern));
        }

        [Test]
        [TestCaseSource(nameof(NullExceptionArguments))]
        public void WhenShowYears_GivenNull_ThenThrowArgumentNullException(ITableView tableView, ChartView chartView)
        {
            //Arrange & Act
            void Call() => YearPatternHelper.ShowYears(tableView, chartView);

            //Assert
            Assert.Throws<System.ArgumentNullException>(Call);
        }

        [Test]
        public void WhenHideYears_ThenRetrievedDataIsNotShowingYears()
        {
            //Arrange
            DateTimeFormatInfo dateTimeFormat = RegionalSettingsManager.CurrentCulture.DateTimeFormat;
            
            string expectedDateTimeFormatWithoutYear = string.Format("MM{0}dd HH{1}mm{1}ss",
                                                                     dateTimeFormat.DateSeparator,
                                                                     dateTimeFormat.TimeSeparator);
            string expectedYearMonthPatternWithoutYear = " MMMM";
            
            //Act
            YearPatternHelper.HideYears(multipleFunctionView.TableView, multipleFunctionView.ChartView as ChartView);

            //Assert
            ViewTestData retrievedData = RetrieveViewData();
            Assert.That(retrievedData.TableDisplayFormat, Is.EqualTo(expectedDateTimeFormatWithoutYear));
            Assert.That(retrievedData.BottomAxisLabelsFormat, Is.EqualTo(expectedDateTimeFormatWithoutYear));
            Assert.That(retrievedData.BottomAxisTitle, Is.EqualTo(string.Empty));
            Assert.That(retrievedData.RangeLabelVisible, Is.False);
            Assert.That(retrievedData.YearMonthPattern, Is.EqualTo(expectedYearMonthPatternWithoutYear));
        }
        
        [Test]
        [TestCaseSource(nameof(NullExceptionArguments))]
        public void WhenHideYears_GivenNull_ThenThrowArgumentNullException(ITableView tableView, ChartView chartView)
        {
            //Arrange & Act
            void Call() => YearPatternHelper.HideYears(tableView, chartView);

            //Assert
            Assert.Throws<System.ArgumentNullException>(Call);
        }

        private ViewTestData RetrieveViewData()
        {
            ViewTestData viewTestData = new ViewTestData();
            viewTestData.TableDisplayFormat = multipleFunctionView.TableView.Columns.First().DisplayFormat;

            IChartAxis bottomAxis = multipleFunctionView.ChartView.Chart.BottomAxis;
            viewTestData.BottomAxisTitle = bottomAxis.Title;
            viewTestData.BottomAxisLabelsFormat = bottomAxis.LabelsFormat;

            var view = multipleFunctionView.ChartView as ChartView;
            Assert.NotNull(view);
            viewTestData.RangeLabelVisible = view.DateTimeLabelFormatProvider.ShowRangeLabel;
            viewTestData.YearMonthPattern = view.DateTimeLabelFormatProvider.CustomDateTimeFormatInfo.YearMonthPattern;

            return viewTestData;
        }
        
        private static IEnumerable<TestCaseData> NullExceptionArguments()
        {
            yield return new TestCaseData(null, new ChartView());
            yield return new TestCaseData(new TableView(), null);
        }

        private class ViewTestData
        {
            public string TableDisplayFormat;
            public string BottomAxisLabelsFormat;
            public string BottomAxisTitle;
            public string YearMonthPattern;
            public bool RangeLabelVisible;
        }
    }
}