using System;
using System.Windows.Controls;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Views.WaveBoundaryConditionEditor.SupportPoints
{
    /// <summary>
    /// Interaction logic for SupportPointEditorView.xaml
    /// </summary>
    public sealed partial class SupportPointEditorView : UserControl, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SupportPointEditorView"/> class.
        /// </summary>
        public SupportPointEditorView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing,
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources;
        /// <c>false</c> to release only unmanaged resources.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (disposing && DataContext is IDisposable disposableDataContext)
            {
                disposableDataContext.Dispose();
            }
        }
    }
}