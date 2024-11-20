using System;
using System.Threading;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Controls.Swf.Charting.Series;
using DelftTools.TestUtils;
using DeltaShell.NGHS.Common.Gui.Modals.Views;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Gui.Tests.Modals.Views
{
    [TestFixture]
    public class ExportChartToCsvDialogTest
    {
        [Test, Apartment(ApartmentState.STA), Category(TestCategory.WindowsForms)]
        public void GivenExportChartToCsvDialog_ShowingChartData_ShouldBeCorrect()
        {
            //Arrange
            var chart = new Chart();
            var lineSeries = new LineChartSeries { Title = "Line series" };
            var areaSeries = new AreaChartSeries { Title = "Area series" };
            var barSeries = new AreaChartSeries { Title = "Bar series" };

            for (int i = 0; i < 100; i++)
            {
                lineSeries.Add(i, Math.Sin(i) * 3);
                areaSeries.Add(i, Math.Cos(i) * 5);
                if (i % 2 == 0)
                {
                    barSeries.Add(i, Math.Atan(i) * 9);
                }
            }
            
            chart.Series.AddRange(new IChartSeries[]{lineSeries, areaSeries, barSeries});

            // Act
            var dialog = new ExportChartToCsvDialog();

            dialog.SetChart(chart);
            
            // Assert
            WpfTestHelper.ShowModal(dialog);
        }
    }
}