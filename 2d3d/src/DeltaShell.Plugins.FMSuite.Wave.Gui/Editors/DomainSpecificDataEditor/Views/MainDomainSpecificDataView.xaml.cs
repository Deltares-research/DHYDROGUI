using System;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.Views
{
    /// <summary>
    /// Interaction logic for MainDomainSpecificDataView.xaml
    /// </summary>
    public sealed partial class MainDomainSpecificDataView : IDisposable
    {
        private bool disposed = false;

        public MainDomainSpecificDataView(MainDomainSpecificDataViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing && DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }

            disposed = true;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="MainDomainSpecificDataView"/> class.
        /// </summary>
        ~MainDomainSpecificDataView()
        {
            Dispose(false);
        }
    }
}