using System;
using System.Windows.Controls;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Views.WaveBoundaryConditionEditor.BoundaryParametersSpecific
{
    /// <summary>
    /// Interaction logic for TimeDependentParametersView.xaml
    /// </summary>
    public partial class TimeDependentParametersView : UserControl, IDisposable
    {
        public TimeDependentParametersView()
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
            if (disposing)
            {
                WindowsFormsHost?.Dispose();
            }
        }
    }
}