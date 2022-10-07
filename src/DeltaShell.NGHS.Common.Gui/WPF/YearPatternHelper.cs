using System;
using System.Globalization;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Utils;
using DelftTools.Utils.Globalization;
using DelftTools.Utils.Guards;

namespace DeltaShell.NGHS.Common.Gui.WPF
{
    /// <summary>
    /// Helper class to show/hide year pattern in table and graph view.
    /// </summary>
    public static class YearPatternHelper
    {
        private const string yearCharacter = "y";

        /// <summary>
        /// Show years in table and graph view.
        /// </summary>
        /// <param name="tableView">Graphical representation of tabular data.</param>
        /// <param name="chartView">Graphical representation of chart data.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tableView"/>, <paramref name="chartView"/> or their respective inner objects which are used are null.</exception>
        public static void ShowYears(ITableView tableView, ChartView chartView)
        {
            Ensure.NotNull(tableView, nameof(tableView));
            Ensure.NotNull(chartView, nameof(chartView));
            
            SetTableViewData(tableView, RegionalSettingsManager.DateTimeFormat);
            SetLabels(chartView.Chart.BottomAxis, chartView.DateTimeLabelFormatProvider, true, RegionalSettingsManager.DateTimeFormat);
            ShowYearFromBottomAxisChartView(chartView.DateTimeLabelFormatProvider);
        }
        
        /// <summary>
        /// Hide years in table and graph view.
        /// </summary>
        /// <param name="tableView">Graphical representation of tabular data.</param>
        /// <param name="chartView">Graphical representation of chart data.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tableView"/>, <paramref name="chartView"/> or their respective inner objects which are used are null.</exception>
        public static void HideYears(ITableView tableView, ChartView chartView)
        {
            Ensure.NotNull(tableView, nameof(tableView));
            Ensure.NotNull(chartView, nameof(chartView));
            
            string dateTimeWithoutYear = RemoveYearFromPattern(RegionalSettingsManager.DateTimeFormat);
            SetTableViewData(tableView, dateTimeWithoutYear);
            SetLabels(chartView.Chart.BottomAxis, chartView.DateTimeLabelFormatProvider, false, dateTimeWithoutYear);
            RemoveYearInBottomAxisChartView(chartView.DateTimeLabelFormatProvider);
        }

        private static void SetTableViewData(ITableView tableView, string timeFormat)
        {
            if (tableView.Columns.Any())
            {
                ITableViewColumn timeColumn = tableView.Columns.First();
                timeColumn.DisplayFormat = timeFormat;
            }
        }
        
        private static void SetLabels(IChartAxis bottomAxis, TimeNavigatableLabelFormatProvider dateTimeLabelFormatProvider, bool showRange,  string timeFormat)
        {
            Ensure.NotNull(bottomAxis, nameof(bottomAxis));
            Ensure.NotNull(dateTimeLabelFormatProvider, nameof(dateTimeLabelFormatProvider));
            bottomAxis.LabelsFormat = timeFormat;
            bottomAxis.Title = string.Empty;
            dateTimeLabelFormatProvider.ShowRangeLabel = showRange;
        }

        private static void RemoveYearInBottomAxisChartView(TimeNavigatableLabelFormatProvider dateTimeLabelFormatProvider)
        {
            Ensure.NotNull(dateTimeLabelFormatProvider, nameof(dateTimeLabelFormatProvider));
            if (dateTimeLabelFormatProvider.CustomDateTimeFormatInfo.Clone() is DateTimeFormatInfo customDateTimeFormatInfo)
            {
                customDateTimeFormatInfo.YearMonthPattern = RemoveYearFromPattern(customDateTimeFormatInfo.YearMonthPattern);
                dateTimeLabelFormatProvider.CustomDateTimeFormatInfo = customDateTimeFormatInfo;
            }
        }

        private static void ShowYearFromBottomAxisChartView(TimeNavigatableLabelFormatProvider dateTimeLabelFormatProvider)
        {
            Ensure.NotNull(dateTimeLabelFormatProvider, nameof(dateTimeLabelFormatProvider));
            dateTimeLabelFormatProvider.CustomDateTimeFormatInfo = new DateTimeFormatInfo();
        }

        private static string RemoveYearFromPattern(string timePattern)
        {
            return timePattern
                   .Replace(yearCharacter, string.Empty)
                   .Trim(Convert.ToChar(RegionalSettingsManager.CurrentCulture.DateTimeFormat.DateSeparator));
        }
    }
}