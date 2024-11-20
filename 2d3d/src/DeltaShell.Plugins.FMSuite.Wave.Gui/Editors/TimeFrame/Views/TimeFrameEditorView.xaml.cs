using System;
using System.Windows.Controls;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Table;
using Image = System.Drawing.Image;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.Views
{
    /// <summary>
    /// Interaction logic for TimeFrameEditorView.xaml
    /// </summary>
    public sealed partial class TimeFrameEditorView : UserControl, IView
    {
        /// <summary>
        /// Creates a new <see cref="TimeFrameEditorView"/>.
        /// </summary>
        public TimeFrameEditorView()
        {
            InitializeComponent();

            // Because we need access to the view component we need to set this
            // up in the code-behind rather than the ViewModel
            int[] argumentColumns = { 0 };
            TableView.PasteController =
                new TableViewArgumentBasedPasteController(TableView,
                                                          argumentColumns)
                {
                    SkipRowsWithMissingArgumentValues = true
                };

            TableView.ShowImportExportToolbar = true;
        }

        public object Data { get; set; }
        public string Text { get; set; }
        public Image Image { get; set; }
        public ViewInfo ViewInfo { get; set; }

        public void EnsureVisible(object item)
        {
            // No specific object requires focus.
        }

        public void Dispose()
        {
            if (hasDisposed)
            {
                return;
            }

            (DataContext as IDisposable)?.Dispose();
            TableView?.Dispose();
            hasDisposed = true;
        }

        private bool hasDisposed = false;
    }
}
