using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Controls.Swf.Charting.Series;
using DelftTools.Functions;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Charting;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;

namespace DeltaShell.NGHS.Common.Gui.WPF
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
            DependencyProperty.RegisterAttached(nameof(MultipleFunctionView.Dock),
                                                typeof(DockStyle),
                                                typeof(MultipleFunctionViewWindowsFormsHostMap),
                                                new PropertyMetadata(PropertyChanged));

        public static readonly DependencyProperty FunctionsProperty =
            DependencyProperty.RegisterAttached(nameof(MultipleFunctionView.Functions),
                                                typeof(IEnumerable<IFunction>),
                                                typeof(MultipleFunctionViewWindowsFormsHostMap),
                                                new PropertyMetadata(PropertyChanged));

        public static readonly DependencyProperty OnCreateBindingListProperty =
            DependencyProperty.RegisterAttached(nameof(MultipleFunctionView.OnCreateBindingList),
                                                typeof(MultipleFunctionView.CreateBindingListDelegate),
                                                typeof(MultipleFunctionViewWindowsFormsHostMap), 
                                                new PropertyMetadata(PropertyChanged));

        public static readonly DependencyProperty AllowColumnSortingProperty =
            DependencyProperty.RegisterAttached(nameof(MultipleFunctionView.TableView.AllowColumnSorting),
                                                typeof(bool),
                                                typeof(MultipleFunctionViewWindowsFormsHostMap), 
                                                new PropertyMetadata(PropertyChanged));

        public static readonly DependencyProperty TableSelectionChangedChangedEventHandlerProperty =
            DependencyProperty.RegisterAttached("TableSelectionChanged", 
                                                typeof(System.EventHandler<TableSelectionChangedEventArgs>), 
                                                typeof(MultipleFunctionViewWindowsFormsHostMap), 
                                                new PropertyMetadata(PropertyChanged));
        
        public static readonly DependencyProperty ShowYearsProperty =
            DependencyProperty.RegisterAttached("ShowYears",
                                                typeof(bool),
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

        public static MultipleFunctionView.CreateBindingListDelegate GetOnCreateBindingList(DependencyObject element) =>
            (MultipleFunctionView.CreateBindingListDelegate)element.GetValue(OnCreateBindingListProperty);

        public static void SetOnCreateBindingList(DependencyObject element, MultipleFunctionView.CreateBindingListDelegate value) =>
            element.SetValue(OnCreateBindingListProperty, value);

        public static bool GetAllowColumnSorting(DependencyObject element) =>
            (bool)element.GetValue(AllowColumnSortingProperty);

        public static void SetAllowColumnSorting(DependencyObject element, bool value) =>
            element.SetValue(AllowColumnSortingProperty, value);

        public static System.EventHandler<TableSelectionChangedEventArgs> GetTableSelectionChanged(DependencyObject element) =>
            (System.EventHandler<TableSelectionChangedEventArgs>) element.GetValue(TableSelectionChangedChangedEventHandlerProperty);

        public static void SetTableSelectionChanged(DependencyObject element, System.EventHandler<TableSelectionChangedEventArgs> value) =>
            element.SetValue(TableSelectionChangedChangedEventHandlerProperty, value);

        public static bool GetShowYears(DependencyObject element) =>
            (bool)element.GetValue(ShowYearsProperty);

        public static void SetShowYears(DependencyObject element, bool value) =>
            element.SetValue(ShowYearsProperty, value);
        
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

                DetermineShowYears(GetShowYears((WindowsFormsHost) sender), multipleFunctionView);
            }
            else if (e.Property == OnCreateBindingListProperty)
            {
                multipleFunctionView.OnCreateBindingList = (MultipleFunctionView.CreateBindingListDelegate)e.NewValue;
            }
            else if (e.Property == AllowColumnSortingProperty)
            {
                multipleFunctionView.TableView.AllowColumnSorting = (bool)e.NewValue;
            } 
            else if (e.Property == TableSelectionChangedChangedEventHandlerProperty)
            {
                if (e.OldValue != null)
                {
                    multipleFunctionView.TableView.SelectionChanged -= 
                        (System.EventHandler<TableSelectionChangedEventArgs>)e.OldValue;
                }

                if (e.NewValue != null)
                {
                    multipleFunctionView.TableView.SelectionChanged += 
                        (System.EventHandler<TableSelectionChangedEventArgs>)e.NewValue;
                }
            }
            else if (e.Property == ShowYearsProperty)
            {
                DetermineShowYears((bool)e.NewValue, multipleFunctionView);
            }
        }

        private static void DetermineShowYears(bool showYears, MultipleFunctionView multipleFunctionView)
        {
            if (showYears)
            {
                YearPatternHelper.ShowYears(multipleFunctionView.TableView, multipleFunctionView.ChartView as ChartView);
                multipleFunctionView.RefreshChartView();
            }
            else
            {
                YearPatternHelper.HideYears(multipleFunctionView.TableView, multipleFunctionView.ChartView as ChartView);
            }
        }
    }
}