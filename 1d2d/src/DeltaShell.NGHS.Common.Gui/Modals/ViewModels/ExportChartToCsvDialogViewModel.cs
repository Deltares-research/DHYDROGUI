using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Controls.Wpf.Commands;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.Utils;

namespace DeltaShell.NGHS.Common.Gui.Modals.ViewModels
{
    public class ExportChartToCsvDialogViewModel : INotifyPropertyChanged
    {
        private char separator = ';';
        private bool useDecimalPlaces = true;
        private int numberOfDigits=1;
        private string previewText;
        private ExportFormatType formatType = ExportFormatType.Number;
        private int previewTextLength = 20;
        private bool combineSeries = true;
        private string path;

        public ExportChartToCsvDialogViewModel()
        {
            SelectFileCommand = new RelayCommand(o =>
            {
                Path = GetFilePath?.Invoke();
            });
            ExportToFileCommand = new RelayCommand(o =>
            {
                FileUtils.DeleteIfExists(Path);
                File.WriteAllText(Path, CombineSeries
                                            ? CreateCombinedText()
                                            : CreateText());

                CloseView?.Invoke();
            }, o => !string.IsNullOrEmpty(Path) && Series.Any(s => s.Selected));
        }

        /// <summary>
        /// Function to get a file path
        /// </summary>
        public Func<string> GetFilePath { get; set; }

        /// <summary>
        /// Action to close view after export
        /// </summary>
        public Action CloseView { get; set; }

        /// <summary>
        /// Separator to use in csv export
        /// </summary>
        public char Separator
        {
            get
            {
                return separator;
            }
            set
            {
                separator = value;
                OnPropertyChanged();
                GeneratePreview();
            }
        }

        /// <summary>
        /// Specify number of decimals or use default parsing
        /// </summary>
        public bool UseDecimalPlaces
        {
            get
            {
                return useDecimalPlaces;
            }
            set
            {
                useDecimalPlaces = value;
                OnPropertyChanged();
                GeneratePreview();
            }
        }
        
        /// <summary>
        /// Collection of available chart series to export
        /// </summary>
        public ObservableCollection<ChartSeriesInfo> Series
        {
            get;
        } = new ObservableCollection<ChartSeriesInfo>();

        /// <summary>
        /// Number of digits to use for numbers when exporting (0 = default)
        /// </summary>
        public int NumberOfDigits
        {
            get
            {
                return numberOfDigits;
            }
            set
            {
                numberOfDigits = value;
                OnPropertyChanged();
                GeneratePreview();
            }
        }

        /// <summary>
        /// Preview of export result
        /// </summary>
        public string PreviewText
        {
            get
            {
                return previewText;
            }
            set
            {
                previewText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Type of export format (G or E)
        /// </summary>
        public ExportFormatType FormatType
        {
            get
            {
                return formatType;
            }
            set
            {
                formatType = value;
                OnPropertyChanged();
                GeneratePreview();
            }
        }

        /// <summary>
        /// Number of lines to show in preview
        /// </summary>
        public int PreviewTextLength
        {
            get
            {
                return previewTextLength;
            }   
            set
            {
                previewTextLength = value;
                OnPropertyChanged();
                GeneratePreview();
            }
        }

        /// <summary>
        /// Combine all series in one table or export separately
        /// </summary>
        public bool CombineSeries
        {
            get
            {
                return combineSeries;
            }
            set
            {
                combineSeries = value;
                OnPropertyChanged();
                GeneratePreview();
            }
        }

        /// <summary>
        /// Path to export to
        /// </summary>
        public string Path
        {
            get
            {
                return path;
            }
            set
            {
                path = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Command to select a file (set Path)
        /// </summary>
        public ICommand SelectFileCommand { get; }

        /// <summary>
        /// Command to export data to file
        /// </summary>
        public ICommand ExportToFileCommand { get; }

        /// <summary>
        /// Sets the chart (series) for export
        /// </summary>
        /// <param name="chart"></param>
        public void SetChart(IChart chart)
        {
            Series.Clear();

            foreach (var chartSeries in chart.Series)
            {
                var chartSeriesInfo = new ChartSeriesInfo(chartSeries)
                {
                    Selected = true
                };
                chartSeriesInfo.PropertyChanged += (s, e) =>
                {
                    GeneratePreview();
                };
                Series.Add(chartSeriesInfo);
            }

            GeneratePreview();
        }

        /// <inheritdoc cref="INotifyPropertyChanged"/>
        public event PropertyChangedEventHandler PropertyChanged;

        private void GeneratePreview()
        {
            PreviewText = CombineSeries
                            ? CreateCombinedText(PreviewTextLength)
                            : CreateText(PreviewTextLength);
        }

        private string CreateText(int limit = -1)
        {
            string format = GetFormat();
            var chartSeriesInfos = Series.Where(s => s.Selected).ToArray();

            var text = "";
            foreach (var chartSeriesInfo in chartSeriesInfos)
            {
                text += $"{chartSeriesInfo.Name}{Environment.NewLine}";

                var xValues = limit == -1
                                  ? chartSeriesInfo.XValues
                                  : chartSeriesInfo.XValues.Take(limit).ToArray();

                var yValues = limit == -1
                                  ? chartSeriesInfo.YValues
                                  : chartSeriesInfo.YValues.Take(limit).ToArray();

                for (int i = 0; i < xValues.Length; i++)
                {
                    var xValue = xValues[i].ToString(format,CultureInfo.InvariantCulture);
                    var yValue = yValues[i].ToString(format, CultureInfo.InvariantCulture);
                    text += $"{xValue}"+ $"{separator}{yValue}{Environment.NewLine}";
                }

                if (limit != -1 && xValues.Length == limit)
                {
                    text += $"...{Environment.NewLine}";
                }

                text += Environment.NewLine;
            }

            return text;
        }

        private string CreateCombinedText(int limit = -1)
        {
            string format = GetFormat();
            var chartSeriesInfos = Series.Where(s => s.Selected).ToArray();

            var chartValues = chartSeriesInfos.Select(i => new
            {
                xValues = (limit == -1 ? i.XValues : i.XValues.Take(limit).ToArray()).ToIndexDictionary(true),
                yValues = limit == -1 ? i.YValues : i.YValues.Take(limit).ToArray()
            }).ToArray();

            var xValuesTotal = limit == -1 
                                   ? chartValues.SelectMany(v => v.xValues.Keys).Distinct().ToArray()
                                   : chartValues.SelectMany(v => v.xValues.Keys).Distinct().Take(limit).ToArray();
            var separatorString = new string(separator, 1);

            var headers = new []{ "X" }.Concat(chartSeriesInfos.Select(i => i.Name));
            var text = string.Join(separatorString, headers) + Environment.NewLine;
            
            foreach (double xValue in xValuesTotal)
            {
                var yValues = chartValues
                    .Select(v => v.xValues.TryGetValue(xValue, out var index) ? v.yValues[index] : double.NaN)
                    .Select(y => y.ToString(format, CultureInfo.InvariantCulture))
                    .ToArray();

                text += $"{xValue.ToString(format, CultureInfo.InvariantCulture)}{separator}{string.Join(separatorString, yValues)}{Environment.NewLine}";
            }
            
            if (limit != -1 && xValuesTotal.Length == limit)
            {
                text += $"...{Environment.NewLine}";
            }

            return text;
        }

        private string GetFormat()
        {
            var formatString = FormatType == ExportFormatType.Number
                                   ? "N"
                                   : "E";

            var formatDecimals = UseDecimalPlaces ? numberOfDigits.ToString(CultureInfo.InvariantCulture) : "";

            return $"{formatString}{formatDecimals}";
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}