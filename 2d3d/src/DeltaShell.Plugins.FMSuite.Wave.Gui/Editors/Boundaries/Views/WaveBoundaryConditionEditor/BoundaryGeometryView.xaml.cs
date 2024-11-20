using System;
using System.Windows.Controls;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Views.WaveBoundaryConditionEditor
{
    /// <summary>
    /// Interaction logic for BoundaryGeometryView.xaml
    /// </summary>
    public partial class BoundaryGeometryView : UserControl, IDisposable
    {
        public BoundaryGeometryView()
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
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources;
        /// <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            GeometryPreviewView.Dispose();
            SupportPointEditorView.Dispose();
        }
    }
}