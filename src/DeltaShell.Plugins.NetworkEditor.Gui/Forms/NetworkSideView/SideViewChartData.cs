using System;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Controls.Swf.Charting.Series;
using DelftTools.Functions;
using DelftTools.Functions.Binding;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView
{
    internal class SideViewChartData : IDisposable
    {
        private IFunction function;
        private FunctionBindingList bindingList;

        public SideViewChartData(IFunction func, Color color, ChartSeriesType style)
        {
            Style = style;
            Color = color;
            Function = func;
        }

        public FunctionBindingList FunctionBindingList
        {
            get { return bindingList; }
        }

        public IFunction Function
        {
            get { return function; }
            set
            {
                function = value;
                if (bindingList != null)
                {
                    bindingList.Clear();
                    bindingList.Dispose();
                }

                bindingList = function != null ? new FunctionBindingList(function) { SynchronizeWaitMethod = Application.DoEvents } : null;
            }
        }

        public Color Color { get; set; }

        public ChartSeriesType Style { get; set; }

        public Action<IPointChartSeries> PointStyleCustomizer { get; set; }

        public Action<ILineChartSeries> LineStyleCustomizer { get; set; }

        public Action<IAreaChartSeries> AreaStyleCustomizer { get; set; }

        public void CustomizeChart(IChartSeries series)
        {
            switch (series)
            {
                case IPointChartSeries pointChartSeries when PointStyleCustomizer != null:
                    PointStyleCustomizer(pointChartSeries);
                    break;
                case ILineChartSeries lineChartSeries when LineStyleCustomizer != null:
                    LineStyleCustomizer(lineChartSeries);
                    break;
                case IAreaChartSeries areaChartSeries when AreaStyleCustomizer != null:
                    AreaStyleCustomizer(areaChartSeries);
                    break;
            }
        }

        public void Dispose()
        {
            if (bindingList == null)
            {
                return;
            }

            bindingList.Clear();
            bindingList.Dispose();
        }
    }
}