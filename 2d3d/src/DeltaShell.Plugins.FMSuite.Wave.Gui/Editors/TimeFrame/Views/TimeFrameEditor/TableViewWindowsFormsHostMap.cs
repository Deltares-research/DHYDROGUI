using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Table;
using DelftTools.Functions.Binding;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.Views.TimeFrameEditor
{
    /// <summary>
    /// Attached properties for a <see cref="WindowsFormsHost"/> with a
    /// <see cref="TableView"/> child item. Allows for one way binding of
    /// properties in WPF controls.
    /// </summary>
    public static class TableViewWindowsFormsHostMap
    {
        public static readonly DependencyProperty DockProperty =
            DependencyProperty.RegisterAttached(nameof(TableView.Dock),
                                                typeof(DockStyle),
                                                typeof(TableViewWindowsFormsHostMap),
                                                new PropertyMetadata(PropertyChanged));

        public static readonly DependencyProperty ShowImportExportToolBarProperty =
            DependencyProperty.RegisterAttached(nameof(TableView.ShowImportExportToolbar),
                                                typeof(bool),
                                                typeof(TableViewWindowsFormsHostMap),
                                                new PropertyMetadata(PropertyChanged));

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.RegisterAttached(nameof(TableView.Data),
                                                typeof(IFunctionBindingList),
                                                typeof(TableViewWindowsFormsHostMap),
                                                new PropertyMetadata(PropertyChanged));

        public static readonly DependencyProperty ColumnVisibilitiesProperty =
            DependencyProperty.RegisterAttached("ColumnVisibilities",
                                                typeof(IList<bool>),
                                                typeof(TableViewWindowsFormsHostMap),
                                                new PropertyMetadata(PropertyChanged));

        public static DockStyle GetDock(DependencyObject element) =>
            (DockStyle)element.GetValue(DockProperty);

        public static void SetDock(DependencyObject element, DockStyle value) =>
            element.SetValue(DockProperty, value);

        public static bool GetShowImportExportToolBar(DependencyObject element) =>
            (bool)element.GetValue(ShowImportExportToolBarProperty);

        public static void SetShowImportExportToolBar(DependencyObject element, bool value) =>
            element.SetValue(ShowImportExportToolBarProperty, value);

        public static IFunctionBindingList GetData(DependencyObject element) =>
            (IFunctionBindingList)element.GetValue(DataProperty);

        public static void SetData(DependencyObject element, IFunctionBindingList value) =>
            element.SetValue(DataProperty, value);

        public static IList<bool> GetColumnVisibilities(DependencyObject element) =>
            (IList<bool>)element.GetValue(ColumnVisibilitiesProperty);

        public static void SetColumnVisibilities(DependencyObject element, IList<bool> value) =>
            element.SetValue(ColumnVisibilitiesProperty, value);

        private static void PropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!((sender as WindowsFormsHost)?.Child is TableView tableView))
            {
                return;
            }

            if (e.Property == DockProperty)
            {
                tableView.Dock = (DockStyle)e.NewValue;
            }
            else if (e.Property == ShowImportExportToolBarProperty)
            {
                tableView.ShowImportExportToolbar = (bool)e.NewValue;
            }
            else if (e.Property == DataProperty)
            {
                tableView.Data = (IFunctionBindingList)e.NewValue;

                // Currently this is a direct requirement for the time frame
                // editor table view. Ideally the columns would be managed 
                // separately, however due to the winforms element this 
                // introduces unnecessary complexity. If this map is 
                // reused in other places, it would be advised to adjust 
                // this.
                tableView.BestFitColumns();
                tableView.Columns[0].Width = 160;
            }
            else if (e.Property == ColumnVisibilitiesProperty)
            {
                var visibilities = (IList<bool>)e.NewValue;
                UpdateTableViewVisibilities(tableView, visibilities);
            }
        }

        private static void UpdateTableViewVisibilities(ITableView tableView,
                                                        IList<bool> columnVisibilities)
        {
            var displayIndex = 1;
            int upperBound = Math.Min(tableView.Columns.Count,
                                      columnVisibilities.Count);

            for (var i = 0; i < upperBound; i++)
            {
                tableView.Columns[i].Visible = columnVisibilities[i];

                if (columnVisibilities[i])
                {
                    tableView.Columns[i].DisplayIndex = displayIndex;
                    displayIndex += 1;
                }
            }
        }
    }
}