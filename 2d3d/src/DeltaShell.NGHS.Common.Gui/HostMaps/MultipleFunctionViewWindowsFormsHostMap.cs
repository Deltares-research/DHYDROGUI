using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using DelftTools.Controls.Swf.Charting.Series;
using DelftTools.Functions;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Charting;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;

namespace DeltaShell.NGHS.Common.Gui.HostMaps
{
    /// <summary>
    /// Attached properties for WindowsFormsHost with a MultipleFunction child
    /// item. Allows for one way binding of properties in WPF controls
    /// </summary>
    public static class MultipleFunctionViewWindowsFormsHostMap
    {
        public static readonly DependencyProperty ChartViewOptionProperty =
            DependencyProperty.RegisterAttached(nameof(MultipleFunctionView.ChartViewOption),
                                                typeof(ChartViewOptions),
                                                typeof(MultipleFunctionViewWindowsFormsHostMap),
                                                new PropertyMetadata(PropertyChanged));

        public static readonly DependencyProperty ChartSeriesTypeProperty =
            DependencyProperty.RegisterAttached(nameof(MultipleFunctionView.ChartSeriesType),
                                                typeof(ChartSeriesType),
                                                typeof(MultipleFunctionViewWindowsFormsHostMap),
                                                new PropertyMetadata(PropertyChanged));

        public static readonly DependencyProperty DockProperty =
            DependencyProperty.RegisterAttached(nameof(Control.Dock),
                                                typeof(DockStyle),
                                                typeof(MultipleFunctionViewWindowsFormsHostMap),
                                                new PropertyMetadata(PropertyChanged));

        public static readonly DependencyProperty FunctionsProperty =
            DependencyProperty.RegisterAttached(nameof(MultipleFunctionView.Functions),
                                                typeof(IEnumerable<IFunction>),
                                                typeof(MultipleFunctionViewWindowsFormsHostMap),
                                                new PropertyMetadata(PropertyChanged));

        public static ChartViewOptions GetChartViewOption(DependencyObject element) =>
            (ChartViewOptions)element.GetValue(ChartViewOptionProperty);

        public static void SetChartViewOption(DependencyObject element, ChartViewOptions value) =>
            element.SetValue(ChartViewOptionProperty, value);

        public static ChartSeriesType GetChartSeriesType(DependencyObject element) =>
            (ChartSeriesType)element.GetValue(ChartSeriesTypeProperty);

        public static void SetChartSeriesType(DependencyObject element, ChartSeriesType value) =>
            element.SetValue(ChartSeriesTypeProperty, value);

        public static DockStyle GetDock(DependencyObject element) =>
            (DockStyle)element.GetValue(DockProperty);

        public static void SetDock(DependencyObject element, DockStyle value) =>
            element.SetValue(DockProperty, value);

        public static IEnumerable<IFunction> GetFunctions(DependencyObject element) =>
            (IEnumerable<IFunction>)element.GetValue(FunctionsProperty);

        public static void SetFunctions(DependencyObject element, IEnumerable<IFunction> value) =>
            element.SetValue(FunctionsProperty, value);

        private static void PropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!((sender as WindowsFormsHost)?.Child is MultipleFunctionView multipleFunctionView))
            {
                return;
            }

            if (e.Property == ChartViewOptionProperty)
            {
                multipleFunctionView.ChartViewOption = (ChartViewOptions)e.NewValue;
            }
            else if (e.Property == ChartSeriesTypeProperty)
            {
                multipleFunctionView.ChartSeriesType = (ChartSeriesType)e.NewValue;
            }
            else if (e.Property == DockProperty)
            {
                multipleFunctionView.Dock = (DockStyle)e.NewValue;
            }
            else if (e.Property == FunctionsProperty)
            {
                // Reset the current content of the MultipleFunctionView, such that is cleared, 
                // otherwise the e.NewValue is just added to the existing functions.
                multipleFunctionView.Functions = null;
                multipleFunctionView.Functions = (IEnumerable<IFunction>)e.NewValue;
            }
        }
    }
}