using System;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Controls.Swf.Charting.Series;
using DeltaShell.NGHS.Common.Gui.Modals.ViewModels;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Gui.Tests.Modals.ViewModels
{
    [TestFixture]
    public class ExportChartToCsvDialogViewModelTest
    {
        private readonly string[] defaultExpectedPreviewText = new[]
        {
            "X;Line series;Area series;Bar series",
            "0.0;3.0;0.0;0.0",
            "1.0;4.0;0.0;NaN",
            "2.0;5.0;0.0;18.0",
            "3.0;6.0;0.0;NaN",
            "4.0;7.0;0.0;36.0",
            "5.0;8.0;0.1;NaN",
            "6.0;9.0;0.1;54.0",
            "7.0;10.0;0.1;NaN",
            "8.0;11.0;0.1;72.0",
            "9.0;12.0;0.1;NaN",
            "10.0;13.0;0.1;90.0",
            "11.0;14.0;0.1;NaN",
            "12.0;15.0;0.1;108.0",
            "13.0;16.0;0.1;NaN",
            "14.0;17.0;0.1;126.0",
            "15.0;18.0;0.2;NaN",
            "16.0;19.0;0.2;144.0",
            "17.0;20.0;0.2;NaN",
            "18.0;21.0;0.2;162.0",
            "19.0;22.0;0.2;NaN",
            "..."
        };

        [Test]
        public void GivenExportChartToCsvDialogViewModel_SettingChart_ShouldUpdateSeries()
        {
            //Arrange
            var viewModel = new ExportChartToCsvDialogViewModel();
            var testChart = CreateTestChart();

            // Act
            Assert.AreEqual(0, viewModel.Series.Count);
            viewModel.SetChart(testChart);

            // Assert
            Assert.AreEqual(3, viewModel.Series.Count);
            Assert.AreEqual(defaultExpectedPreviewText, viewModel.PreviewText.Split(new []{Environment.NewLine},StringSplitOptions.RemoveEmptyEntries));
        }

        [Test]
        public void GivenExportChartToCsvDialogViewModel_SettingPreviewTextLength_ShouldWork()
        {
            //Arrange
            var viewModel = new ExportChartToCsvDialogViewModel();
            var testChart = CreateTestChart();

            // Act
            viewModel.SetChart(testChart);

            // Assert
            Assert.AreEqual(defaultExpectedPreviewText, viewModel.PreviewText.Split(new[]
            {
                Environment.NewLine
            }, StringSplitOptions.RemoveEmptyEntries));

            viewModel.PreviewTextLength = 3;

            var newExpectedPreviewText = new[]
            {
                "X;Line series;Area series;Bar series",
                "0.0;3.0;0.0;0.0",
                "1.0;4.0;0.0;NaN",
                "2.0;5.0;0.0;18.0",
                "..."
            };
            Assert.AreEqual(newExpectedPreviewText, viewModel.PreviewText.Split(new[]
            {
                Environment.NewLine
            }, StringSplitOptions.RemoveEmptyEntries));
        }

        [Test]
        public void GivenExportChartToCsvDialogViewModel_SettingSeparator_ShouldWork()
        {
            //Arrange
            var viewModel = new ExportChartToCsvDialogViewModel{PreviewTextLength = 3};
            viewModel.SetChart(CreateTestChart());

            // Act
            viewModel.Separator = '|';

            // Assert
            var newExpectedPreviewText = new[]
            {
                "X|Line series|Area series|Bar series",
                "0.0|3.0|0.0|0.0",
                "1.0|4.0|0.0|NaN",
                "2.0|5.0|0.0|18.0",
                "..."
            };
            Assert.AreEqual(newExpectedPreviewText, viewModel.PreviewText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
        }

        [Test]
        public void GivenExportChartToCsvDialogViewModel_SettingNumberOfDigits_ShouldWork()
        {
            //Arrange
            var viewModel = new ExportChartToCsvDialogViewModel { PreviewTextLength = 3 };

            viewModel.SetChart(CreateTestChart());

            // Act
            viewModel.NumberOfDigits = 3;

            // Assert
            var newExpectedPreviewText = new[]
            {
                "X;Line series;Area series;Bar series",
                "0.000;3.000;0.000;0.000",
                "1.000;4.000;0.010;NaN",
                "2.000;5.000;0.020;18.000",
                "..."
            };
            Assert.AreEqual(newExpectedPreviewText, viewModel.PreviewText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
        }

        [Test]
        public void GivenExportChartToCsvDialogViewModel_SettingFormatType_ShouldWork()
        {
            //Arrange
            var viewModel = new ExportChartToCsvDialogViewModel
            {
                PreviewTextLength = 3,
                NumberOfDigits = 3
            };

            viewModel.SetChart(CreateTestChart());

            // Act
            viewModel.FormatType = ExportFormatType.Exponential;

            // Assert
            var newExpectedPreviewText = new[]
            {
                "X;Line series;Area series;Bar series",
                "0.000E+000;3.000E+000;0.000E+000;0.000E+000",
                "1.000E+000;4.000E+000;1.000E-002;NaN",
                "2.000E+000;5.000E+000;2.000E-002;1.800E+001",
                "..."
            };
            Assert.AreEqual(newExpectedPreviewText, viewModel.PreviewText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
        }

        [Test]
        public void GivenExportChartToCsvDialogViewModel_SettingCombineSeries_ShouldWork()
        {
            //Arrange
            var viewModel = new ExportChartToCsvDialogViewModel{PreviewTextLength = 3};
            var testChart = CreateTestChart();

            // Act
            viewModel.SetChart(testChart);

            // Assert
            viewModel.CombineSeries = false;

            var newExpectedPreviewText = new[]
            {
                "Line series",
                "0.0;3.0",
                "1.0;4.0",
                "2.0;5.0",
                "...",
                "",
                "Area series",
                "0.0;0.0",
                "1.0;0.0",
                "2.0;0.0",
                "...",
                "",
                "Bar series",
                "0.0;0.0",
                "2.0;18.0",
                "4.0;36.0",
                "...",
                "",
                ""
            };
            Assert.AreEqual(newExpectedPreviewText, viewModel.PreviewText.Split(new[]
            {
                Environment.NewLine
            },StringSplitOptions.None));
        }

        private static Chart CreateTestChart()
        {
            var chart = new Chart();
            var lineSeries = new LineChartSeries { Title = "Line series" };
            var areaSeries = new AreaChartSeries { Title = "Area series" };
            var barSeries = new AreaChartSeries { Title = "Bar series" };

            for (int i = 0; i < 100; i++)
            {
                lineSeries.Add(i, i + 3);
                areaSeries.Add(i, i /100.0);
                if (i % 2 == 0)
                {
                    barSeries.Add(i, i * 9);
                }
            }

            chart.Series.AddRange(new IChartSeries[] { lineSeries, areaSeries, barSeries });
            return chart;
        }
    }
}