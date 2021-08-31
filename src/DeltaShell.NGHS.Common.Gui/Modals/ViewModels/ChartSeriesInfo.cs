using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Controls.Swf.Charting.Series;
using DelftTools.Utils;
using DelftTools.Utils.Reflection;
using Steema.TeeChart.Styles;

namespace DeltaShell.NGHS.Common.Gui.Modals.ViewModels
{
    /// <summary>
    /// Wrapper class for chart series export
    /// </summary>
    public class ChartSeriesInfo : INotifyPropertyChanged
    {
        private readonly IChartSeries series;
        private bool selected;
        
        public ChartSeriesInfo(IChartSeries series)
        {
            this.series = series;
        }

        /// <summary>
        /// Is series selected for export
        /// </summary>
        public bool Selected
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Name of the series
        /// </summary>
        public string Name
        {
            get { return series.Title; }
        }

        /// <summary>
        /// X values of the series
        /// </summary>
        public double[] XValues
        {
            get { return GetValueListValues(s => s?.XValues); }
        }

        /// <summary>
        /// Y values of the series
        /// </summary>
        public double[] YValues
        {
            get { return GetValueListValues(s => s?.YValues); }
        }

        /// <inheritdoc cref="INotifyPropertyChanged"/>
        public event PropertyChangedEventHandler PropertyChanged;

        private double[] GetValueListValues(Func<Series,ValueList> getValueList)
        {
            var field = TypeUtils.GetField<ChartSeries, Series>(series, "series");
            var valueList = getValueList(field);

            if (valueList == null)
            {
                return Array.Empty<double>();
            }

            var resultValues = new double[valueList.Count];

            Array.Copy(valueList.Value, resultValues, valueList.Count);
            return resultValues;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}